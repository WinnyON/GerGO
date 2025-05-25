using GerGO.Utils;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GerGO.DataAcces.StoredData
{
    class MongoDataManager : IStoredDataManager
    {
        private readonly MongoClient _client;
        private readonly string connectionString = "mongodb://localhost:27017";
        private readonly IMongoDatabase _coreDB;
        public MongoDataManager()
        {
            _client = new MongoClient(connectionString);
            // Connect to the 'GerGOStorage' database
            _coreDB = _client.GetDatabase("GerGOStorage");
        }

        public void AddColumn(string dbName, string tableID, string value)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(tableID));
            var document = collection.Find(filter).FirstOrDefault();

            if (document.Elements.Count() == 1)
                return;

            var updateDef = new List<UpdateDefinition<BsonDocument>>();

            foreach (var element in document.Elements)
            {
                if (element.Name != "_id")
                {
                    updateDef.Add(Builders<BsonDocument>.Update.Set(element.Name, $"{element.Value}^{value}"));
                }
            }

            var combinedUpdate = Builders<BsonDocument>.Update.Combine(updateDef);
            var result = collection.UpdateOne(filter, combinedUpdate);

            if (result.ModifiedCount == 0)
            {
                throw new DataAccesException("Document was not updated.");
            }
        }
        public void RemoveColumn(string dbName, string tableID, bool isPkKey, int index)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(tableID));
            var document = collection.Find(filter).FirstOrDefault();

            if (document.Elements.Count() == 1)
                return;

            var updateDef = new List<UpdateDefinition<BsonDocument>>();

            if (isPkKey)
            {
                var removeOldPKeysDef = new List<UpdateDefinition<BsonDocument>>();
                foreach (var element in document.Elements)
                {
                    if (element.Name == "_id")
                        continue;

                    List<string> data = element.Name.Split('^').ToList();
                    data.RemoveAt(index);
                    string newName = string.Join('^', data);
                    updateDef.Add(Builders<BsonDocument>.Update.Set(newName, element.Value));
                    removeOldPKeysDef.Add(Builders<BsonDocument>.Update.Unset(element.Name));
                }

                var combinedDelUpdate = Builders<BsonDocument>.Update.Combine(removeOldPKeysDef);
                var res = collection.UpdateOne(filter, combinedDelUpdate);

                if (res.ModifiedCount == 0)
                {
                    throw new DataAccesException("Document was not updated.");
                }
            }
            else
            {
                foreach (var element in document.Elements)
                {
                    if (element.Name == "_id")
                        continue;

                    List<string> data = element.Value.AsString.Split('^').ToList();
                    data.RemoveAt(index);
                    string newVal = string.Join('^', data);
                    updateDef.Add(Builders<BsonDocument>.Update.Set(element.Name, newVal));
                }
            }

            var combinedUpdate = Builders<BsonDocument>.Update.Combine(updateDef);
            var result = collection.UpdateOne(filter, combinedUpdate);

            if (result.ModifiedCount == 0)
            {
                throw new DataAccesException("Document was not updated.");
            }
        }

        public void RemoveIndexAttribute(string colName, string mongoID, int index)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(colName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(mongoID));
            var document = collection.Find(filter).FirstOrDefault();

            if (document.Elements.Count() == 1)
                return;

            var updateDef = new List<UpdateDefinition<BsonDocument>>();
            var removeOldPKeysDef = new List<UpdateDefinition<BsonDocument>>();

            foreach (var element in document.Elements)
            {
                if (element.Name == "_id")
                    continue;

                List<string> data = element.Name.Split('^').ToList();
                data.RemoveAt(index);
                string newName = string.Join('^', data);
                updateDef.Add(Builders<BsonDocument>.Update.Set(newName, element.Value));
                removeOldPKeysDef.Add(Builders<BsonDocument>.Update.Unset(element.Name));
            }

            var combinedDelUpdate = Builders<BsonDocument>.Update.Combine(removeOldPKeysDef);
            var res = collection.UpdateOne(filter, combinedDelUpdate);

            if (res.ModifiedCount == 0)
            {
                throw new DataAccesException("Index was not updated.");
            }

            var combinedUpdate = Builders<BsonDocument>.Update.Combine(updateDef);
            var result = collection.UpdateOne(filter, combinedUpdate);

            if (result.ModifiedCount == 0)
            {
                throw new DataAccesException("Index was not updated.");
            }
        }

        public void UpdateIndexPKeyData(string colName, string mongoID, int index)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(colName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(mongoID));
            var document = collection.Find(filter).FirstOrDefault();

            if (document.Elements.Count() == 1)
                return;

            var updateDef = new List<UpdateDefinition<BsonDocument>>();

            foreach (var element in document.Elements)
            {
                if (element.Name == "_id")
                    continue;

                List<string> data = element.Value.AsString.Split('#').ToList();
                data = data.Select(pKey =>
                {
                    List<string> tmp = pKey.Split('^').ToList();
                    tmp.RemoveAt(index);
                    return string.Join('^', tmp);
                }).ToList();
                string newVal = string.Join('#', data);
                updateDef.Add(Builders<BsonDocument>.Update.Set(element.Name, newVal));
            }

            var combinedUpdate = Builders<BsonDocument>.Update.Combine(updateDef);
            var result = collection.UpdateOne(filter, combinedUpdate);

            if (result.ModifiedCount == 0)
            {
                throw new DataAccesException("Index was not updated.");
            }
        }

        public void Delete(string dbName, string tableID, string key)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);

            ObjectId objId = ObjectId.Parse(tableID);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

            var deletedRow = Builders<BsonDocument>.Update.Unset(key);
            var result = collection.UpdateOne(filter, deletedRow);

            if (result.ModifiedCount == 0)
            {
                throw new DataAccesException("No matching key!");
            }
        }

        public void DropDatabase(string dbName)
        {
            _coreDB.DropCollection(dbName);
        }

        public void DropTable(string dbName, string mongoId)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(mongoId));

            var result = collection.DeleteOne(filter);

            if (result.DeletedCount == 0)
            {
                throw new DataAccesException("No document found to delete.");
            }
        }

        public List<string> GetAllRows(string dbName, string tableID)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);
            ObjectId objId = ObjectId.Parse(tableID);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

            var table = collection.Find(filter).First();
            
            var result = new List<string>();

            foreach (var item in table)
            {
                if (!item.Name.Equals("_id"))
                    result.Add($"{item.Name}^{item.Value}");
            }

            return result;
        }

        public void Insert(string dbName, string tableID, string key, string value)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);

            ObjectId objId = ObjectId.Parse(tableID);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

            var insertedRow = Builders<BsonDocument>.Update.Set(key, value);

            var result = collection.UpdateOne(filter, insertedRow);

            if (result.ModifiedCount == 0)
            {
                throw new DataAccesException("Failed to insert!");
            }
        }

        public string PrepareTable(string dbName, string tableName)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);
            var newTable = new BsonDocument();
            collection.InsertOne(newTable);

            string id = newTable["_id"].AsObjectId.ToString();

            return id;
        }

        public string AddUniqueFile(string dbName, string tableName)
        {
            var collection = _coreDB.GetCollection<BsonDocument>($"{dbName}_{tableName}_uniquekeys");
            var newUniqueKey = new BsonDocument();
            collection.InsertOne(newUniqueKey);

            string id = newUniqueKey["_id"].AsObjectId.ToString();

            return id;
        }

        public void InsertToUniqueFile(string dbName, string tableName, string mongoId, string pKey, string uKey)
        {
            var collection = _coreDB.GetCollection<BsonDocument>($"{dbName}_{tableName}_uniquekeys");
            ObjectId objId = ObjectId.Parse(mongoId);
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("_id", objId),
                Builders<BsonDocument>.Filter.Exists(uKey));
            var document = collection.Find(filter).FirstOrDefault();

            if (document != null)
            {
                throw new DataAccesException("Unique value already exists!");
            }

            var filterToUpdate = Builders<BsonDocument>.Filter.Eq("_id", objId);
            var updateStatement = Builders<BsonDocument>.Update.Set(uKey, pKey);

            var res = collection.UpdateOne(filterToUpdate, updateStatement);
            if (res.ModifiedCount == 0)
            {
                throw new DataAccesException("Failed to insert unique key data!");
            }
        }

        public void DeleteFromUniqueFile(string dbName, string tableName, string mongoID, string uKey)
        {
            var collection = _coreDB.GetCollection<BsonDocument>($"{dbName}_{tableName}_uniquekeys");

            ObjectId objId = ObjectId.Parse(mongoID);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

            var deletedRow = Builders<BsonDocument>.Update.Unset(uKey);
            var result = collection.UpdateOne(filter, deletedRow);

            if (result.ModifiedCount == 0)
            {
                throw new DataAccesException("No matching key!");
            }
        }

        public string AddForeignKeyFile(string dbName, string tableName)
        {
            var collection = _coreDB.GetCollection<BsonDocument>($"{dbName}_{tableName}_foreignkeys");
            var newForeignKey = new BsonDocument();
            collection.InsertOne(newForeignKey);

            string id = newForeignKey["_id"].AsObjectId.ToString();

            return id;
        }

        public void InsertToForeignKeyFile(string dbName, string tableName, string mongoID, string pKey, string fKey)
        {
            var collection = _coreDB.GetCollection<BsonDocument>($"{dbName}_{tableName}_foreignkeys");
            ObjectId objId = ObjectId.Parse(mongoID);
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("_id", objId),
                Builders<BsonDocument>.Filter.Exists(fKey));
            var document = collection.Find(filter).FirstOrDefault();

            var filterToUpdate = Builders<BsonDocument>.Filter.Eq("_id", objId);
            UpdateDefinition<BsonDocument> updateStatement;
            if (document != null)
            {
                updateStatement = Builders<BsonDocument>.Update.Set(fKey, $"{document[fKey]}#{pKey}");
            }
            else
            {
                updateStatement = Builders<BsonDocument>.Update.Set(fKey, pKey);
            }

            var res = collection.UpdateOne(filterToUpdate, updateStatement);
            if (res.ModifiedCount == 0)
            {
                throw new DataAccesException("Failed to insert index data!");
            }
        }

        public void DeleteFromForeignKeyFile(string dbName, string tableName, string mongoID, string pKey, string fKeyVal)
        {
            var collection = _coreDB.GetCollection<BsonDocument>($"{dbName}_{tableName}_foreignkeys");

            ObjectId objId = ObjectId.Parse(mongoID);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);
            var document = collection.Find(filter).FirstOrDefault();

            if (document == null)
            {
                throw new DataAccesException("Failed to delete foreign key value!");
            }

            string value = document[fKeyVal].AsString;
            List<string> values = value.Split('^').ToList();
            values.Remove(pKey);

            UpdateDefinition<BsonDocument> updateDef;
            if (values.Count > 0)
            {
                updateDef = Builders<BsonDocument>.Update.Set(fKeyVal, string.Join('#', values));
            }
            else
            {
                updateDef = Builders<BsonDocument>.Update.Unset(fKeyVal);
            }

            var result = collection.UpdateOne(filter, updateDef);

            if (result.ModifiedCount == 0)
            {
                throw new DataAccesException("No matching key!");
            }
        }

        public bool ExistsKey(string dbName, string mongoID, string key)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);
            ObjectId objId = ObjectId.Parse(mongoID);
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("_id", objId),
                Builders<BsonDocument>.Filter.Exists(key));
            var document = collection.Find(filter).FirstOrDefault();

            if (document == null)
            {
                return false;
            }
            return true;
        }

        public string AddIndexFile(string dbName, string tableName)
        {
            var collection = _coreDB.GetCollection<BsonDocument>($"{dbName}_{tableName}_indexfiles");
            var newIndex = new BsonDocument();
            collection.InsertOne(newIndex);

            string id = newIndex["_id"].AsObjectId.ToString();

            return id;
        }

        public void DropIndexFile(string colName, string mongoID)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(colName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(mongoID));

            var result = collection.DeleteOne(filter);

            if (result.DeletedCount == 0)
            {
                throw new DataAccesException("No document found to delete.");
            }
        }

        public void InsertToIndexFile(string dbName, string tableName, string mongoID, string pKey, string value)
        {
            var collection = _coreDB.GetCollection<BsonDocument>($"{dbName}_{tableName}_indexfiles");
            ObjectId objId = ObjectId.Parse(mongoID);
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("_id", objId),
                Builders<BsonDocument>.Filter.Exists(value));
            var document = collection.Find(filter).FirstOrDefault();

            var filterToUpdate = Builders<BsonDocument>.Filter.Eq("_id", objId);
            UpdateDefinition<BsonDocument> updateStatement;
            if (document != null)
            {
                updateStatement = Builders<BsonDocument>.Update.Set(value, $"{document[value]}#{pKey}");
            }
            else
            {
                updateStatement = Builders<BsonDocument>.Update.Set(value, pKey);
            }

            var res = collection.UpdateOne(filterToUpdate, updateStatement);
            if (res.ModifiedCount == 0)
            {
                throw new DataAccesException("Failed to insert index data!");
            }
        }

        public void DeleteFromIndexFile(string dbName, string tableName, string mongoID, string pKey)
        {
            string indexDocName = $"{dbName}_{tableName}_indexfiles";
            var collection = _coreDB.GetCollection<BsonDocument>(indexDocName);
            ObjectId objId = ObjectId.Parse(mongoID);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);
            var document = collection.Find(filter).FirstOrDefault();

            var updateDef = new List<UpdateDefinition<BsonDocument>>();
            var removeOldPKeysDef = new List<UpdateDefinition<BsonDocument>>();

            foreach (var item in document.Elements)
            {
                if (item.Name == "_id")
                    continue;

                string row = item.Value.AsString;
                List<string> values = row.Split('#').ToList();
                if (values.Contains(pKey))
                {
                    values.Remove(pKey);
                    if (values.Count == 0)
                    {
                        removeOldPKeysDef.Add(Builders<BsonDocument>.Update.Unset(item.Name));
                        continue;
                    }
                    string tmp = string.Join('#', values);
                    updateDef.Add(Builders<BsonDocument>.Update.Set(item.Name, tmp));
                    
                }
            }

            var combinedDelUpdate = Builders<BsonDocument>.Update.Combine(removeOldPKeysDef);
            var res = collection.UpdateOne(filter, combinedDelUpdate);

            if (res.ModifiedCount == 0)
            {
                throw new DataAccesException("Index was not updated.");
            }

            var combinedUpdate = Builders<BsonDocument>.Update.Combine(updateDef);
            var result = collection.UpdateOne(filter, combinedUpdate);

            if (result.ModifiedCount == 0)
            {
                throw new DataAccesException("Index was not updated.");
            }

        }

        public bool ContainsValue(string dbName, string tableID, int index, string value)
        {
            return GetAllRows(dbName, tableID).Select(row => row.Split('^')[index]).Contains(value);
        }

        public string GetValue(string dbName, string mongoID, string key)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);
            ObjectId objId = ObjectId.Parse(mongoID);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

            var document = collection.Find(filter).FirstOrDefault();

            return document[key].AsString;
        }

        public List<string> GetValues(string dbName, string mongoID, List<string> keys)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);
            ObjectId objId = ObjectId.Parse(mongoID);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

            var document = collection.Find(filter).FirstOrDefault();

            List<string> res = [];

            keys.ForEach(key => res.Add($"{key}^{document[key].AsString}"));

            return res;
        }

        public List<string> GetPrimaryKeysWhere(string dbName, string mongoId, int colIndex, string type, string op, string val)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);
            ObjectId objId = ObjectId.Parse(mongoId);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

            var document = collection.Find(filter).FirstOrDefault();

            List<string> result = [];
            val = Validator.TrimApostrpohes(val);
            switch (op)
            {
                case "=":
                case "==":
                    if (document.Names.Any(name => name.Split('^')[colIndex] == val))
                        result = document[val].AsString.Split('#').ToList();
                    break;
                case ">":
                    foreach (var item in document)
                    {
                        if (item.Name != "_id" && Validator.IsGreater(item.Name.Split('^')[colIndex], val, type))
                            result.AddRange(item.Value.AsString.Split("#"));
                    }
                    break;
                case ">=":
                    foreach (var item in document)
                    {
                        if (item.Name != "_id" && Validator.IsGreaterOrEqual(item.Name.Split('^')[colIndex], val, type))
                            result.AddRange(item.Value.AsString.Split("#"));
                    }
                    break;
                case "<":
                    foreach (var item in document)
                    {
                        if (item.Name != "_id" && Validator.IsLess(item.Name.Split('^')[colIndex], val, type))
                            result.AddRange(item.Value.AsString.Split("#"));
                    }
                    break;
                case "<=":
                    foreach (var item in document)
                    {
                        if (item.Name != "_id" && Validator.IsLessOrEqual(item.Name.Split('^')[colIndex], val, type))
                            result.AddRange(item.Value.AsString.Split("#"));
                    }
                    break;
                default:
                    break;
            }

            return result;
        }

        public List<string> GetPrimaryKeysWhereAllRow(string dbName, string mongoId, int colIndex, string type, string op, string val)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);
            ObjectId objId = ObjectId.Parse(mongoId);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

            var document = collection.Find(filter).FirstOrDefault();

            List<string> result = [];
            val = Validator.TrimApostrpohes(val);
            foreach (var item in document.Elements)
            {
                if (item.Name.Equals("_id"))
                    continue;

                string row = $"{item.Name}^{item.Value}";
                switch (op)
                {
                    case "=":
                    case "==":
                        if (row.Split('^')[colIndex] == val)
                            result.Add(item.Name);
                        break;
                    case ">":
                        if (Validator.IsGreater(row.Split('^')[colIndex], val, type))
                            result.Add(item.Name);
                        break;
                    case ">=":
                        if (Validator.IsGreaterOrEqual(row.Split('^')[colIndex], val, type))
                            result.Add(item.Name);
                        break;
                    case "<":
                        if (Validator.IsLess(row.Split('^')[colIndex], val, type))
                            result.Add(item.Name);
                        break;
                    case "<=":
                        if (Validator.IsLessOrEqual(row.Split('^')[colIndex], val, type))
                            result.Add(item.Name);
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        public List<string> GetPrimaryKeys(string dbName, string mongoID)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);
            ObjectId objId = ObjectId.Parse(mongoID);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

            var document = collection.Find(filter).FirstOrDefault();

            if (document == null)
                throw new DataAccesException("No matching document!");

            List<string> pKeys = document.Names.ToList();
            pKeys.Remove("_id");

            return pKeys;
        }
    }
}
