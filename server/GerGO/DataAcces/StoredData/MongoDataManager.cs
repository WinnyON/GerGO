using GerGO.Models;
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

            if (!isPkKey)
            {
                var updateDef = new List<UpdateDefinition<BsonDocument>>();
                foreach (var element in document.Elements)
                {
                    if (element.Name != "_id")
                    {
                        List<string> values = element.Value.AsString.Split('^').ToList();
                        values.RemoveAt(index);
                        string res = string.Join('^', values);
                        updateDef.Add(Builders<BsonDocument>.Update.Set(element.Name, $"{res}"));
                    }
                }
                return;
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

        public bool IsValidRow(string dbName, Table table, List<string> columnNames, ref string key, ref string value, ref int innerSeed)
        {
            string[] insertedRow = value.Split('^');
            string row = "";
            key = "";

            foreach (var column in table.Columns)
            {
                if (!columnNames.Contains(column.Name) && !column.PrimaryKey)
                {
                    row = string.IsNullOrEmpty(row) ? "null" : row + "^null";
                    continue;
                }

                int index = columnNames.IndexOf(column.Name);

                if (column.PrimaryKey)
                {
                    string tmp = "";
                    if (index != -1)
                    {
                        tmp = insertedRow[index];
                    }
                    if (column.PKIdentity.Step > 0)
                    {
                        tmp = (column.PKIdentity.InnerSeed + column.PKIdentity.Step).ToString();
                        column.PKIdentity.InnerSeed += column.PKIdentity.Step;
                        innerSeed = column.PKIdentity.InnerSeed;
                        key = string.IsNullOrEmpty(key) ? tmp : (key + "^" + tmp);
                        continue;
                    }
                    key = string.IsNullOrEmpty(key) ? tmp : (key + "^" + tmp);
                }

                try
                {
                    // type check
                    switch (column.Type)
                    {
                        case "int":
                            _ = int.Parse(insertedRow[index]);
                            break;
                        case "float":
                            _ = float.Parse(insertedRow[index]);
                            break;
                        case "bit":
                            _ = bool.Parse(insertedRow[index]);
                            break;
                        case "date":
                            _ = DateTime.Parse(insertedRow[index]);
                            break;
                        case "datetime":
                            _ = TimeSpan.Parse(insertedRow[index]);
                            break;
                        case "string":
                            break;
                        default:
                            return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }

                if (column.NotNull)
                {
                    if (insertedRow[index].Equals("0") || insertedRow[index].Equals("null") || insertedRow[index].Equals(string.Empty))
                        return false;
                }

                if (!string.IsNullOrEmpty(column.DefaultVal) && (insertedRow[index].Equals("0") || insertedRow[index].Equals("null") || insertedRow[index].Equals(string.Empty)))
                    insertedRow[index] = column.DefaultVal;

                // unique check
                if (column.Unique)
                {
                    //List<string> values;

                    //if (values.Contains(insertedRow[i]))
                    //    return false;
                }

                // check condition
                if (!column.Check.Equals("--"))
                {
                    string[] checkConst = column.Check.Split('^');
                    switch (checkConst[0])
                    {
                        case "=":
                        case "==":
                            if (insertedRow[index] != checkConst[1]) return false;
                            break;
                        case ">":
                            if (insertedRow[index].CompareTo(checkConst[1]) <= 0) return false;
                            break;
                        case ">=":
                            if (insertedRow[index].CompareTo(checkConst[1]) < 0) return false;
                            break;
                        case "<":
                            if (insertedRow[index].CompareTo(checkConst[1]) >= 0) return false;
                            break;
                        case "<=":
                            if (insertedRow[index].CompareTo(checkConst[1]) > 0) return false;
                            break;
                        default:
                            break;
                    }
                }

                if (!column.PrimaryKey)
                    row = string.IsNullOrEmpty(row) ? insertedRow[index] : row + "^" + insertedRow[index];
            }

            if (string.IsNullOrEmpty(key))
                return false;

            value = row;

            return true;
        }

        public string PrepareTable(string dbName, string tableName)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);
            var newTable = new BsonDocument();
            collection.InsertOne(newTable);

            string id = newTable["_id"].AsObjectId.ToString();

            return id;
        }
    }
}
