using Xs;

namespace Art.Xs
{
    /// <summary>
    /// Represents a periodic page-bottom scroll action on an <see cref="XsClient"/>.
    /// </summary>
    public sealed class DownScroller : IDisposable
    {
        private readonly XsClient _client;
        private readonly int _msDelay;
        private CancellationTokenSource _cts;
        private CancellationToken _ct;
        private bool _disposed;

        /// <summary>
        /// Creates a new instance of <see cref="DownScroller"/>.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msDelay"></param>
        public DownScroller(XsClient client, int msDelay)
        {
            (_client, _msDelay, _cts) = (client, msDelay, new CancellationTokenSource());
            _ct = _cts.Token;
        }

        /// <summary>
        /// Start scrolling.
        /// </summary>
        public void Start()
        {
            NotDisposed();
            Task.Factory.StartNew(Execute(_ct), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Stop scrolling.
        /// </summary>
        public void Stop()
        {
            NotDisposed();
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            _ct = _cts.Token;
        }

        private Func<Task> Execute(CancellationToken ct) => async () =>
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(_msDelay, ct);
                await _client.ExecuteJsAsync("window.scrollTo(0, document.body.scrollHeight);");
            }
        };

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _cts.Cancel();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DownScroller()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }

        private void NotDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DownScroller));
        }
    }
}
