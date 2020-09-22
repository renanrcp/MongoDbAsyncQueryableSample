using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoDbAsyncQueryableSample.Database.Adapters
{
    internal class MongoAsyncEnumeratorAdapter<T> : IAsyncEnumerator<T>
    {
        private readonly IAsyncCursorSource<T> _asyncCursorSource;
        private readonly CancellationToken _cts;

        private IAsyncCursor<T> _asyncCursor;
        private IEnumerator<T> _batchEnumerator;

        public MongoAsyncEnumeratorAdapter(IAsyncCursorSource<T> asyncCursorSource, CancellationToken cancellationToken)
        {
            _asyncCursorSource = asyncCursorSource;
            _cts = cancellationToken;
        }

        public T Current => _batchEnumerator.Current;

        public ValueTask DisposeAsync()
        {
            _asyncCursor?.Dispose();
            _asyncCursor = null;

            return default;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (_asyncCursor == null)
                _asyncCursor = await _asyncCursorSource.ToCursorAsync(_cts);

            if (_batchEnumerator != null && _batchEnumerator.MoveNext())
                return true;

            if (_asyncCursor != null && await _asyncCursor.MoveNextAsync(_cts))
            {
                _batchEnumerator?.Dispose();
                _batchEnumerator = _asyncCursor.Current.GetEnumerator();

                return _batchEnumerator.MoveNext();
            }

            return false;
        }
    }
}