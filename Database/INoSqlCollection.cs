using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MongoDbAsyncQueryableSample.Database
{
    public interface INoSqlCollection<T> : INoSqlCollection
    {
        Task AddAsync(T item);

        Task UpdateAsync(Expression<Func<T, bool>> predicate, T item);

        Task DeleteAsync(Expression<Func<T, bool>> predicate);

        IQueryable<T> AsQueryable();
    }

    public interface INoSqlCollection : IDisposable
    {
        string Name { get; }

        INoSqlDbContext Database { get; }
    }
}