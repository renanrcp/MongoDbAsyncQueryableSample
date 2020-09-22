using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDbAsyncQueryableSample.Database.Queryables;
using MongoDbAsyncQueryableSample.Database.QueryProviders;

namespace MongoDbAsyncQueryableSample.Database
{
    internal class MongoCollection<T> : INoSqlCollection<T>
    {
        private readonly IMongoCollection<T> _mongoCollection;

        public MongoCollection(INoSqlDbContext database, IMongoCollection<T> mongoCollection)
        {
            _mongoCollection = mongoCollection;

            Database = database;
        }

        public string Name => _mongoCollection.CollectionNamespace.CollectionName;

        public INoSqlDbContext Database { get; }

        public Task AddAsync(T item)
            => _mongoCollection.InsertOneAsync(item);

        public Task DeleteAsync(Expression<Func<T, bool>> predicate)
            => _mongoCollection.DeleteOneAsync(predicate);

        public Task UpdateAsync(Expression<Func<T, bool>> predicate, T item)
            => _mongoCollection.ReplaceOneAsync(predicate, item);

        public void Dispose()
        { }

        public IQueryable<T> AsQueryable()
        {
            var mongoQueryable = _mongoCollection.AsQueryable();

            var mongoQueryProvider = new MongoQueryProvider(mongoQueryable);

            return new MongoQueryable<T>(mongoQueryProvider);
        }
    }
}