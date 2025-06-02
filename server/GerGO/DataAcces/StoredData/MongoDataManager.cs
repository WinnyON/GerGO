using GerGO.Models;
using GerGO.Utils;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GerGO.DataAcces.StoredData
{
    class MongoDataManager : IStoredDataManager
    {
        private readonly MongoClient _client;
        private readonly string connectionString = "mongodb://localhost:27017";
        public MongoDataManager()
        {
            _client = new MongoClient(connectionString);
        }

        public void AddColumn(string dbName, string tableName, string value)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(tableName);

            var documents = collection.Find(FilterDefinition<BsonDocument>.Empty).ToList();
            if (documents.Count == 0)
                return;

            var updates = new List<WriteModel<BsonDocument>>();

            foreach (var document in documents)
            {
                var id = document["_id"];
                var currentVal = document["Value"].AsString;

                string newVal = $"{currentVal}^{value}";
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                var update = Builders<BsonDocument>.Update.Set("Value", newVal);

                updates.Add(new UpdateOneModel<BsonDocument>(filter, update));
            }

            if (updates.Count > 0)
            {
                try
                {
                    collection.BulkWrite(updates);
                } catch (Exception ex)
                {
                    throw new DataAccesException($"Failed bulk write at add column: {ex.Message}");
                }
            }
        }
        public void RemoveColumn(string dbName, string tableName, int index)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(tableName);
            var documents = collection.Find(FilterDefinition<BsonDocument>.Empty).ToList();
            if (documents.Count == 0)
                return;

            var updates = new List<WriteModel<BsonDocument>>();

            foreach (var document in documents)
            {
                var id = document["_id"];
                var currentVal = document["Value"].AsString;

                List<string> data = currentVal.Split('^').ToList();
                data.RemoveAt(index);
                string newVal = string.Join('^', data);
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                var update = Builders<BsonDocument>.Update.Set("Value", newVal);

                updates.Add(new UpdateOneModel<BsonDocument>(filter, update));
            }

            if (updates.Count > 0)
            {
                try
                {
                    collection.BulkWrite(updates);
                }
                catch (Exception ex)
                {
                    throw new DataAccesException($"Failed bulk write at add column: {ex.Message}");
                }
            }
        }

        public void Delete(string dbName, string tableName, List<string> pKeys)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(tableName);

            var filter = Builders<BsonDocument>.Filter.In("_id", pKeys);

            try
            {
                var result = collection.DeleteMany(filter);
            }
            catch (Exception)
            {
                throw new DataAccesException("Failed to delete pKeys!");
            }

        }

        public void DropDatabase(string dbName)
        {
            _client.DropDatabase(dbName);
        }

        public void DropTable(string dbName, string tableName)
        {
            var db = _client.GetDatabase(dbName);

            db.DropCollection(tableName);
        }

        public List<string> GetAllRows(string dbName, string tableName)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(tableName);

            var documents = collection.Find(FilterDefinition<BsonDocument>.Empty).ToList();
            var result = documents.Select(doc => $"{doc["_id"]}^{doc["Value"]}").ToList();

            return result;
        }

        public void Insert(string dbName, string collectionName, List<MongoEntity> data)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);

            List<BsonDocument> rows = data.Select(x => new BsonDocument { { "_id", x.Key }, { "Value", x.Value } }).ToList();

            try
            {
                collection.InsertMany(rows);
            }
            catch (Exception)
            {
                throw new DataAccesException("Failed to insert data!");
            }
        }
        public void DeleteUniqueIndexData(string dbName, string collectionName, List<string> pKeys)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);

            var filter = Builders<BsonDocument>.Filter.In("Value", pKeys);

            try
            {
                var result = collection.DeleteMany(filter);
            }
            catch (MongoException)
            {
                throw new DataAccesException("Error at deleting documents!");
            }
        }

        public bool ExistsKey(string dbName, string collectionName, string key)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", key);
            var document = collection.Find(filter).FirstOrDefault();

            if (document == null)
            {
                return false;
            }
            return true;
        }

        public void InsertIndexData(string dbName, string collectionName, List<MongoEntity> data)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);

            Dictionary<string, string> dict = [];
            data.ForEach(d =>
            {
                if (dict.ContainsKey(d.Key))
                {
                    string val = dict[d.Key];
                    dict[d.Key] = $"{val}#{d.Value}";
                }
                else
                {
                    dict.Add(d.Key, d.Value);
                }
            });

            List<BsonDocument> docsToInsert = [];
            List<WriteModel<BsonDocument>> docsToUpdate = [];

            if (dict.Count == 0)
                return;

            foreach (var entity in dict)
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", entity.Key);
                var document = collection.Find(filter).FirstOrDefault();

                if (document != null)
                {
                    var id = document["_id"];
                    var currentVal = document["Value"].AsString;

                    string newVal = $"{currentVal}#{entity.Value}";
                    var filterDoc = Builders<BsonDocument>.Filter.Eq("_id", id);
                    var update = Builders<BsonDocument>.Update.Set("Value", newVal);

                    docsToUpdate.Add(new UpdateOneModel<BsonDocument>(filterDoc, update));
                }
                else
                {
                    docsToInsert.Add(new BsonDocument { { "_id", entity.Key }, { "Value", entity.Value } });
                }
            }

            try
            {
                if (docsToInsert.Count > 0)
                    collection.InsertMany(docsToInsert);
                if (docsToUpdate.Count > 0)
                    collection.BulkWrite(docsToUpdate);
            }
            catch (MongoException)
            {
                throw new DataAccesException("Failed to insert data!");
            }
        }


        public void DeleteIndexData(string dbName, string collectionName, List<string> pKeys)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);

            var filterDocs = Builders<BsonDocument>.Filter.Empty;
            var documents = collection.Find(FilterDefinition<BsonDocument>.Empty).ToList();
            foreach (var key in pKeys)
            {
                var updates = new List<WriteModel<BsonDocument>>();
                List<string> keysToDelete = [];
                foreach (var doc in documents)
                {
                    List<string> vals = doc["Value"].AsString.Split('#').ToList();
                    if (vals.Contains(key))
                    {
                        if (vals.Count == 1)
                        {
                            keysToDelete.Add(doc["_id"].AsString);
                            continue;
                        }

                        vals.Remove(key);

                        var filter = Builders<BsonDocument>.Filter.Eq("_id", doc["_id"].AsString);
                        var update = Builders<BsonDocument>.Update.Set("Value", string.Join('#', vals));

                        updates.Add(new UpdateOneModel<BsonDocument>(filter, update));
                    }
                }
                try
                {
                    if (updates.Count > 0)
                        collection.BulkWrite(updates);
                    if (keysToDelete.Count == 0)
                    {
                        var filter = Builders<BsonDocument>.Filter.In("_id", keysToDelete);
                        var result = collection.DeleteMany(filter);
                    }
                } catch (Exception)
                {
                    throw new DataAccesException("Failed to delete index data!");
                }
            }
        }

        //public string GetFullRow(string dbName, string mongoID, string key)
        //{
        //    var collection = _coreDB.GetCollection<BsonDocument>(dbName);
        //    ObjectId objId = ObjectId.Parse(mongoID);
        //    var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

        //    var document = collection.Find(filter).FirstOrDefault();

        //    return $"{key}^{document[key].AsString}";
        //}

        //public string GetValue(string dbName, string mongoID, string key)
        //{
        //    var collection = _coreDB.GetCollection<BsonDocument>(dbName);
        //    ObjectId objId = ObjectId.Parse(mongoID);
        //    var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

        //    var document = collection.Find(filter).FirstOrDefault();

        //    return document[key].AsString;
        //}

        //public List<string> GetValues(string dbName, string mongoID, List<string> keys)
        //{
        //    var collection = _coreDB.GetCollection<BsonDocument>(dbName);
        //    ObjectId objId = ObjectId.Parse(mongoID);
        //    var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

        //    var document = collection.Find(filter).FirstOrDefault();

        //    List<string> res = [];

        //    keys.ForEach(key => res.Add($"{key}^{document[key].AsString}"));

        //    return res;
        //}

        //public List<string> GetPrimaryKeysWhere(string dbName, string mongoId, int colIndex, string type, string op, string val)
        //{
        //    var collection = _coreDB.GetCollection<BsonDocument>(dbName);
        //    ObjectId objId = ObjectId.Parse(mongoId);
        //    var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

        //    var document = collection.Find(filter).FirstOrDefault();

        //    List<string> result = [];
        //    val = Validator.TrimApostrpohes(val);
        //    switch (op)
        //    {
        //        case "=":
        //        case "==":
        //            if (document.Names.Any(name => name.Split('^')[colIndex] == val))
        //                result = document[val].AsString.Split('#').ToList();
        //            break;
        //        case ">":
        //            foreach (var item in document)
        //            {
        //                if (item.Name != "_id" && Validator.IsGreater(item.Name.Split('^')[colIndex], val, type))
        //                    result.AddRange(item.Value.AsString.Split("#"));
        //            }
        //            break;
        //        case ">=":
        //            foreach (var item in document)
        //            {
        //                if (item.Name != "_id" && Validator.IsGreaterOrEqual(item.Name.Split('^')[colIndex], val, type))
        //                    result.AddRange(item.Value.AsString.Split("#"));
        //            }
        //            break;
        //        case "<":
        //            foreach (var item in document)
        //            {
        //                if (item.Name != "_id" && Validator.IsLess(item.Name.Split('^')[colIndex], val, type))
        //                    result.AddRange(item.Value.AsString.Split("#"));
        //            }
        //            break;
        //        case "<=":
        //            foreach (var item in document)
        //            {
        //                if (item.Name != "_id" && Validator.IsLessOrEqual(item.Name.Split('^')[colIndex], val, type))
        //                    result.AddRange(item.Value.AsString.Split("#"));
        //            }
        //            break;
        //        default:
        //            break;
        //    }

        //    return result;
        //}

        //public List<string> GetPrimaryKeysWhereAllRow(string dbName, string mongoId, int colIndex, string type, string op, string val)
        //{
        //    var collection = _coreDB.GetCollection<BsonDocument>(dbName);
        //    ObjectId objId = ObjectId.Parse(mongoId);
        //    var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

        //    var document = collection.Find(filter).FirstOrDefault();

        //    List<string> result = [];
        //    val = Validator.TrimApostrpohes(val);
        //    foreach (var item in document.Elements)
        //    {
        //        if (item.Name.Equals("_id"))
        //            continue;

        //        string row = $"{item.Name}^{item.Value}";
        //        switch (op)
        //        {
        //            case "=":
        //            case "==":
        //                if (row.Split('^')[colIndex] == val)
        //                    result.Add(item.Name);
        //                break;
        //            case ">":
        //                if (Validator.IsGreater(row.Split('^')[colIndex], val, type))
        //                    result.Add(item.Name);
        //                break;
        //            case ">=":
        //                if (Validator.IsGreaterOrEqual(row.Split('^')[colIndex], val, type))
        //                    result.Add(item.Name);
        //                break;
        //            case "<":
        //                if (Validator.IsLess(row.Split('^')[colIndex], val, type))
        //                    result.Add(item.Name);
        //                break;
        //            case "<=":
        //                if (Validator.IsLessOrEqual(row.Split('^')[colIndex], val, type))
        //                    result.Add(item.Name);
        //                break;
        //            default:
        //                break;
        //        }
        //    }

        //    return result;
        //}

        //public List<string> GetPrimaryKeys(string dbName, string mongoID)
        //{
        //    var collection = _coreDB.GetCollection<BsonDocument>(dbName);
        //    ObjectId objId = ObjectId.Parse(mongoID);
        //    var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

        //    var document = collection.Find(filter).FirstOrDefault();

        //    if (document == null)
        //        throw new DataAccesException("No matching document!");

        //    List<string> pKeys = document.Names.ToList();
        //    pKeys.Remove("_id");

        //    return pKeys;
        //}
    }
}
