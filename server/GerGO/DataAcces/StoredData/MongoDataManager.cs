using GerGO.Utils;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GerGO.DataAcces.StoredData
{
    class MongoDataManager : IStoredDataManager
    {
        private ILogger _logger = LoggerFactory.GetLogger();
        private MongoClient _client;
        private string connectionString = "mongodb://localhost:27017";
        private IMongoDatabase _coreDB;
        public MongoDataManager()
        {
            _client = new MongoClient(connectionString);
            // Connect to the 'local' database
            _coreDB = _client.GetDatabase("GerGOStorage");

            // List collections in 'GerGOStorage' database
            var collections = _coreDB.ListCollectionNames().ToList();
            _logger.Info("Collections in 'GerGOStorage' database:");
            foreach (var collection in collections)
            {
                _logger.Info(collection);
            }
        }

        public void Delete(string tableID, string key)
        {
        }

        public void Insert(string dbName, string tableID, string key, string value)
        {
            var collection = _coreDB.GetCollection<BsonDocument>(dbName);

            ObjectId objId = ObjectId.Parse(tableID);
            //var filter = Builders<BsonDocument>.Filter.And(Builders<BsonDocument>.Filter.Eq("_id", objId),
            //    Builders<BsonDocument>.Filter.Exists(key, false));
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

            _logger.Info($"Created table id: {id}");
            return id;
        }
    }
}
