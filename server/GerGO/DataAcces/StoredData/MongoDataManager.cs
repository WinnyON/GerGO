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

        public void Delete<T>(string dbName, string tableName, List<T> pKeys)
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

        public void Insert<T>(string dbName, string collectionName, List<MongoEntity<T>> data)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);

            List<BsonDocument> rows = data.Select(x => new BsonDocument { { "_id", BsonValue.Create(x.Key) }, { "Value", x.Value } }).ToList();

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

        public bool ExistsKey<T>(string dbName, string collectionName, T key)
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

        public void InsertIndexData<T>(string dbName, string collectionName, List<MongoEntity<T>> data)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);

            Dictionary<T, string> dict = [];
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
                    docsToInsert.Add(new BsonDocument { { "_id", BsonValue.Create(entity.Key) }, { "Value", entity.Value } });
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
        public List<string> GetAllKeys(string dbName, string tableName)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(tableName);
            var projection = Builders<BsonDocument>.Projection.Include("_id");

            var result = collection.Find(FilterDefinition<BsonDocument>.Empty).Project(projection).ToList()
                .Select(doc => doc["_id"].AsString).ToList();

            return result;
        }
        public List<string> GetKeysWhere<T>(string dbName, string collectionName, string op, T val)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);

            FilterDefinition<BsonDocument> filterDef;

            switch (op)
            {
                case "=":
                case "==":
                    filterDef = Builders<BsonDocument>.Filter.Eq("_id", val);
                    break;
                case ">":
                    filterDef = Builders<BsonDocument>.Filter.Gt("_id", val);
                    break;
                case ">=":
                    filterDef = Builders<BsonDocument>.Filter.Gte("_id", val);
                    break;
                case "<":
                    filterDef = Builders<BsonDocument>.Filter.Lt("_id", val);
                    break;
                case "<=":
                    filterDef = Builders<BsonDocument>.Filter.Lte("_id", val);
                    break;
                default:
                    throw new DataAccesException("Invalid operator!");
            }

            var projection = Builders<BsonDocument>.Projection.Include("Value");
            try
            {
                var result = collection.Find(filterDef).Project(projection).ToList().SelectMany(doc => doc["Value"].ToString().Split('#')).ToList();
                return result;
            } catch (Exception ex)
            {
                throw new DataAccesException("Failed to get data!");
            }
        }


        public List<string> Get_idWhere<T>(string dbName, string collectionName, string op, T val)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);

            FilterDefinition<BsonDocument> filterDef;

            switch (op)
            {
                case "=":
                case "==":
                    filterDef = Builders<BsonDocument>.Filter.Eq("_id", val);
                    break;
                case ">":
                    filterDef = Builders<BsonDocument>.Filter.Gt("_id", val);
                    break;
                case ">=":
                    filterDef = Builders<BsonDocument>.Filter.Gte("_id", val);
                    break;
                case "<":
                    filterDef = Builders<BsonDocument>.Filter.Lt("_id", val);
                    break;
                case "<=":
                    filterDef = Builders<BsonDocument>.Filter.Lte("_id", val);
                    break;
                default:
                    throw new DataAccesException("Invalid operator!");
            }

            var projection = Builders<BsonDocument>.Projection.Include("_id");
            try
            {
                var result = collection.Find(filterDef).Project(projection).ToList().Select(doc => doc["_id"].ToString()).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw new DataAccesException("Failed to get data!");
            }
        }

        public List<string> GetKeysWhereIter(string dbName, string tableName, int colIndex, string type, string op, string val)
        {
            var db = _client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(tableName);

            val = Validator.TrimApostrpohes(val);
            
            List<string> result = collection.Find(FilterDefinition<BsonDocument>.Empty).ToList().
                FindAll(doc => Validator.Match(type, doc["Value"].AsString.Split('^')[colIndex], val, op)).
                Select(doc => doc["_id"].ToString()).ToList();

            return result;
        }

        //public string GetValue(string dbName, string tableName, string key)
        //{
        //    var db = _client.GetDatabase(dbName);
        //    var collection = db.GetCollection<BsonDocument>(tableName);
        //    ObjectId objId = ObjectId.Parse(key);
        //    var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);
        //    var document = collection.Find(filter).FirstOrDefault();

        //    return document[key].AsString;
        //}

        //public string GetFullRow(string dbName, string mongoID, string key)
        //{
        //    var collection = _coreDB.GetCollection<BsonDocument>(dbName);
        //    ObjectId objId = ObjectId.Parse(mongoID);
        //    var filter = Builders<BsonDocument>.Filter.Eq("_id", objId);

        //    var document = collection.Find(filter).FirstOrDefault();

        //    return $"{key}^{document[key].AsString}";
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
    }
}
