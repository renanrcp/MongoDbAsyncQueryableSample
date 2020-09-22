using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace MongoDbAsyncQueryableSample.Database
{
    public class MongoDbContext : INoSqlDbContext
    {
        private readonly ConcurrentDictionary<string, INoSqlCollection> _collections;
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoDatabase database)
        {
            _database = database;
            _collections = new ConcurrentDictionary<string, INoSqlCollection>();
        }

        public string Name => _database.DatabaseNamespace.DatabaseName;

        public INoSqlCollection<T> GetCollection<T>(string name)
        {
            if (_collections.TryGetValue(name, out var collection))
                return collection as INoSqlCollection<T>;

            var mongoCollection = _database.GetCollection<T>(name);

            collection = new MongoCollection<T>(this, mongoCollection);

            return collection as INoSqlCollection<T>;
        }

        public void Dispose()
        {
            var collections = _collections.Values.ToList();

            foreach (var collection in collections)
                collection.Dispose();

            collections.Clear();
        }
    }
}