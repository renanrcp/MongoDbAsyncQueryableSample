using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDbAsyncQueryableSample.Database.Adapters;
using MongoDbAsyncQueryableSample.Database.Queryables;
using MongoDbAsyncQueryableSample.Extensions;

namespace MongoDbAsyncQueryableSample.Database.QueryProviders
{

    public class MongoQueryProvider : IQueryProvider
    {
        private readonly IMongoQueryable _mongoQueryable;

        public MongoQueryProvider(IMongoQueryable mongoQueryable)
        {
            _mongoQueryable = mongoQueryable;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetSequenceElementType();

            return (IQueryable)Activator.CreateInstance(typeof(MongoQueryable<>)
                                        .MakeGenericType(elementType), new object[] { this, expression });
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new MongoQueryable<TElement>(this, expression);
        }

        public object Execute(Expression expression)
            => _mongoQueryable.Provider.Execute(expression);

        public TResult Execute<TResult>(Expression expression)
            => _mongoQueryable.Provider.Execute<TResult>(expression);

        public IAsyncCursorSource<TElement> GetAsyncCursorSource<TElement>(IQueryable<TElement> queryable)
        {
            // This not optimized yet.
            var methodDefinition = _mongoQueryable.Provider
                                        .GetType()
                                        .GetMethod("ExecuteAsync");

            var method = methodDefinition.MakeGenericMethod(typeof(IAsyncCursor<TElement>));

            var expressionParam = Expression.Parameter(typeof(Expression), "expression");
            var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
            var queryableParam = Expression.Parameter(typeof(IQueryProvider), "mongo");

            var parameters = new List<ParameterExpression>()
            {
                expressionParam,
                cancellationTokenParam,
            };

            var instanceCast = Expression.Convert(queryableParam, _mongoQueryable.Provider.GetType());

            var callExpression = Expression.Call(instanceCast, method, parameters);

            var delegateParameters = new List<ParameterExpression>()
            {
                queryableParam,
            };

            delegateParameters.AddRange(parameters);

            var lambda = Expression.Lambda<ExecuteAsyncDelegate<IAsyncCursor<TElement>>>(callExpression, delegateParameters);
            var func = lambda.Compile();

            return new MongoAsyncCursorSource<TElement>(queryable, _mongoQueryable.Provider, func);
        }

        public (object, Type) GetInstanceAndOutputType()
        {
            var type = typeof(IMongoQueryable<>)
                        .MakeGenericType(_mongoQueryable.GetExecutionModel().OutputType);

            return (_mongoQueryable, type);
        }
    }

    public class MongoAsyncCursorSource<TElement> : IAsyncCursorSource<TElement>
    {
        private readonly IQueryable<TElement> _queryable;
        private readonly IQueryProvider _realQueryProvider;
        private readonly ExecuteAsyncDelegate<IAsyncCursor<TElement>> _executeAsyncFunction;

        public MongoAsyncCursorSource(IQueryable<TElement> queryable, IQueryProvider realQueryProvider, ExecuteAsyncDelegate<IAsyncCursor<TElement>> executeAsyncFunction)
        {
            _queryable = queryable;
            _realQueryProvider = realQueryProvider;
            _executeAsyncFunction = executeAsyncFunction;
        }

        public IAsyncCursor<TElement> ToCursor(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IAsyncCursor<TElement>> ToCursorAsync(CancellationToken cancellationToken = default)
        {
            return _executeAsyncFunction(_realQueryProvider, _queryable.Expression, cancellationToken);
        }
    }

    public delegate Task<TResult> ExecuteAsyncDelegate<TResult>(IQueryProvider mongo, Expression expression, CancellationToken cancellationToken);
}