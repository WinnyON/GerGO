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
                    result.Add($"1^{item.Value}");
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

        public bool IsValidRow(string dbName, string tableName, List<string[]> columns, string key, ref string value)
        {
            string[] insertedRow = value.Split('^');
            if (insertedRow.Length != columns.Count)
                return false;

            for (int i = 0; i < columns.Count; i++)
            {
                // if the column is the primary key
                if (!columns[i][2].Equals("--"))
                {
                    insertedRow[i] = key;
                    continue;
                }

                try
                {
                    // type check
                    switch (columns[i][1])
                    {
                        case "int":
                            _ = int.Parse(insertedRow[i]);
                            break;
                        case "float":
                            _ = float.Parse(insertedRow[i]);
                            break;
                        case "bit":
                            _ = bool.Parse(insertedRow[i]);
                            break;
                        case "date":
                            _ = DateTime.Parse(insertedRow[i]);
                            break;
                        case "datetime":
                            _ = TimeSpan.Parse(insertedRow[i]);
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

                // not null check
                if (!columns[i][3].Equals("--"))
                {
                    if (insertedRow[i].Equals("0") || insertedRow[i].Equals("null") || insertedRow[i].Equals(string.Empty))
                        return false;
                }

                // defaultval check
                if (!columns[i][4].Equals("--") && (insertedRow[i].Equals("0") || insertedRow[i].Equals("null") || insertedRow[i].Equals(string.Empty)))
                    insertedRow[i] = columns[i][3];

                // unique check
                if (!columns[i][7].Equals("--"))
                {
                    List<string> values = GetAllRows(dbName, tableName).Select(row => row.Split('^')[i+2]).ToList();

                    if (values.Contains(insertedRow[i]))
                        return false;
                }

                // check condition
            }

            value = string.Join('^', insertedRow);

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
