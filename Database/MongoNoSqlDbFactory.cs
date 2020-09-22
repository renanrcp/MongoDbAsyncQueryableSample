using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace MongoDbAsyncQueryableSample.Database
{
    public class MongoNoSqlDbFactory : INoSqlDbFactory, IDisposable
    {
        private readonly ConcurrentDictionary<string, INoSqlDbContext> _contexts;
        private readonly IMongoClient _mongoClient;

        public MongoNoSqlDbFactory(IMongoClient mongoClient)
        {
            _contexts = new ConcurrentDictionary<string, INoSqlDbContext>();
            _mongoClient = mongoClient;
        }

        public INoSqlDbContext Create(string databaseName)
        {
            if (_contexts.TryGetValue(databaseName, out var context))
                return context;

            var database = _mongoClient.GetDatabase(databaseName);

            context = new MongoDbContext(database);

            _contexts.TryAdd(databaseName, context);

            return context;
        }

        public void Dispose()
        {
            var contexts = _contexts.Values.ToList();

            foreach (var context in contexts)
                context.Dispose();

            _contexts?.Clear();
        }
    }
}