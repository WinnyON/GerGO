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
                    result.Add($"1^{item.Name}^{item.Value}");
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
    }
}
