using MongoDB.Bson;
using MongoDB.Driver;

namespace GerGO.DataAcces.StoredData
{
    class MongoDataManager : IStoredDataManager
    {
        private MongoClient _client;
        private string connectionString = "mongodb://localhost:27017";
        public MongoDataManager()
        {
            _client = new MongoClient(connectionString);
            // Connect to the 'local' database
            var database = _client.GetDatabase("local");

            // List collections in 'local' database
            var collections = database.ListCollectionNames().ToList();
            Console.WriteLine("Collections in 'local' database:");
            foreach (var collection in collections)
            {
                Console.WriteLine(collection);
            }
        }
    }
}
