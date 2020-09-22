using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using MongoDB.Driver.Linq;
using MongoDbAsyncQueryableSample.Database.Adapters;
using MongoDbAsyncQueryableSample.Database.QueryProviders;

namespace MongoDbAsyncQueryableSample.Database.Queryables
{
    public class MongoQueryable<TDocument> : IQueryable<TDocument>, IAsyncEnumerable<TDocument>
    {
        private readonly Expression _expression;
        private readonly MongoQueryProvider _mongoQueryProvider;

        public MongoQueryable(MongoQueryProvider mongoQueryProvider, Expression expression = null)
        {
            _mongoQueryProvider = mongoQueryProvider;

            if (expression == null)
            {
                var (instance, type) = _mongoQueryProvider.GetInstanceAndOutputType();

                expression = Expression.Constant(instance, type);
            }

            _expression = expression;
        }

        public Type ElementType => typeof(TDocument);

        public Expression Expression => _expression;

        public IQueryProvider Provider => _mongoQueryProvider;

        public IAsyncEnumerator<TDocument> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            var asyncCursorSource = _mongoQueryProvider.GetAsyncCursorSource<TDocument>(this);

            return new MongoAsyncEnumeratorAdapter<TDocument>(asyncCursorSource, cancellationToken);
        }

        public IEnumerator<TDocument> GetEnumerator()
        {
            var results = (IEnumerable<TDocument>)_mongoQueryProvider.Execute(_expression);

            return results.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}