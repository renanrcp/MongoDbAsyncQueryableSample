using System;

namespace MongoDbAsyncQueryableSample.Database
{
    public interface INoSqlDbContext : IDisposable
    {
        string Name { get; }

        INoSqlCollection<T> GetCollection<T>(string name);
    }
}