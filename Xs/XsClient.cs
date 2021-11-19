using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;

namespace Xs;

/// <summary>
/// Represents a request client.
/// </summary>
public class XsClient : IDisposable
{
    private readonly ICallManager _callManager;
    private bool _disposed;
    private bool _ready;

    internal XsClient(ICallManager mgr)
    {
        _callManager = mgr;
    }

    internal async Task ReadyAsync()
    {
        EnsureNotDisposed();
        if (_ready) return;
        await _callManager.ReadyAsync();
        _ready = true;
        return;
    }

    /// <summary>
    /// Asynchronously loads a page.
    /// </summary>
    /// <param name="page">Page to load.</param>
    /// <returns>Task returning when load is complete.</returns>
    public Task LoadAsync(string page)
    {
        EnsureState();
        return _callManager.LoadAsync(page);
    }

    /// <summary>
    /// Executes an action on the underlying <see cref="CoreWebView2"/>.
    /// </summary>
    /// <param name="action">Action.</param>
    /// <returns>Task.</returns>
    public Task ExecuteAsync(Action<CoreWebView2> action)
    {
        EnsureState();
        return _callManager.ExecuteAsync(action);
    }

    /// <summary>
    /// Executes an action on the underlying <see cref="CoreWebView2"/>.
    /// </summary>
    /// <param name="function">Action.</param>
    /// <returns>Task.</returns>
    public Task ExecuteAsync(Func<CoreWebView2, Task> function)
    {
        EnsureState();
        return _callManager.ExecuteAsync(function);
    }

    /// <summary>
    /// Executes a function on the underlying <see cref="CoreWebView2"/>.
    /// </summary>
    /// <param name="function">Function.</param>
    /// <returns>Task.</returns>
    public Task<T> ExecuteAsync<T>(Func<CoreWebView2, T> function)
    {
        EnsureState();
        return _callManager.ExecuteAsync(function);
    }

    /// <summary>
    /// Executes a function on the underlying <see cref="CoreWebView2"/>.
    /// </summary>
    /// <param name="function">Function.</param>
    /// <returns>Task.</returns>
    public Task<T> ExecuteAsync<T>(Func<CoreWebView2, Task<T>> function)
    {
        EnsureState();
        return _callManager.ExecuteAsync(function);
    }

    /// <summary>
    /// Asynchronously executes Javascript code on context.
    /// </summary>
    /// <param name="js">Code to execute.</param>
    /// <returns>Task returning result of script.</returns>
    public Task<string> ExecuteJsAsync(string js)
    {
        EnsureState();
        return _callManager.ExecuteJsAsync(js);
    }

    /// <summary>
    /// Asynchronously retrieves page content.
    /// </summary>
    /// <returns>Task returning content.</returns>
    public Task<string> GetContentAsync()
    {
        EnsureState();
        return _callManager.GetContentAsync();
    }

    /// <summary>
    /// Creates a response waiter.
    /// </summary>
    /// <param name="predicate">Response predicate.</param>
    /// <returns>Task returning waiter.</returns>
    public async Task<ResponseWaiter> CreateResponseWaiterAsync(Predicate<Uri> predicate)
        => await ExecuteAsync(v => CreateResponseWaiter(v, predicate));

    private ResponseWaiter CreateResponseWaiter(CoreWebView2 core, Predicate<Uri> predicate)
    {
        return new ResponseWaiter(this, core, predicate);
    }

    private void EnsureState()
    {
        EnsureNotDisposed();
        if (!_ready) throw new InvalidOperationException("Client not readied");
    }

    private void EnsureNotDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(XsClient));
    }

    /// <summary>
    /// Disposes instance.
    /// </summary>
    /// <param name="disposing">True if disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _callManager.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
