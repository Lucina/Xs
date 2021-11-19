using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Xs;

/// <summary>
/// Represents an operation waiting for a certain response.
/// </summary>
public sealed class ResponseWaiter : IDisposable
{
    private readonly XsClient _client;
    private readonly CoreWebView2 _core;
    private readonly Predicate<Uri> _predicate;
    private readonly ConcurrentQueue<CoreWebView2WebResourceResponseReceivedEventArgs> _results;
    private bool _disposed;

    internal ResponseWaiter(XsClient client, CoreWebView2 core, Predicate<Uri> predicate)
    {
        _client = client;
        _core = core;
        _predicate = predicate;
        _results = new();
        _core.WebResourceResponseReceived += Core_WebResourceResponseReceived;
    }

    private void Core_WebResourceResponseReceived(object? sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
    {
        if (_disposed) return;
        if (_predicate(new Uri(e.Request.Uri)))
            _results.Enqueue(e);
    }

    private async Task<CoreWebView2WebResourceResponseReceivedEventArgs> GetResultAsync(CancellationToken? cancellationToken)
    {
        CoreWebView2WebResourceResponseReceivedEventArgs? result;
        if (cancellationToken is { } ct)
            while (!_results.TryDequeue(out result)) await Task.Delay(50, ct);
        else
            while (!_results.TryDequeue(out result)) await Task.Delay(50);
        return result;
    }

    /// <summary>
    /// True if unprocessed inputs exist.
    /// </summary>
    public bool ResultAvailable => !_results.IsEmpty;

    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="action">Action on result.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public async Task ProcessResultAsync(Action<CoreWebView2WebResourceResponseReceivedEventArgs> action, CancellationToken? cancellationToken = null)
    {
        var result = await GetResultAsync(cancellationToken);
        await _client.ExecuteAsync(_ => action(result));
    }

    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="action">Action on result.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public async Task ProcessResultAsync(Action<CoreWebView2, CoreWebView2WebResourceResponseReceivedEventArgs> action, CancellationToken? cancellationToken = null)
    {
        var result = await GetResultAsync(cancellationToken);
        await _client.ExecuteAsync(v => action(v, result));
    }

    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="action">Action on result.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public async Task ProcessResultAsync(Func<CoreWebView2WebResourceResponseReceivedEventArgs, Task> action, CancellationToken? cancellationToken = null)
    {
        var result = await GetResultAsync(cancellationToken);
        await _client.ExecuteAsync(_ => action(result));
    }

    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="action">Action on result.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public async Task ProcessResultAsync(Func<CoreWebView2, CoreWebView2WebResourceResponseReceivedEventArgs, Task> action, CancellationToken? cancellationToken = null)
    {
        var result = await GetResultAsync(cancellationToken);
        await _client.ExecuteAsync(v => action(v, result));
    }

    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="func">Function on result.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning value.</returns>
    public async Task<T> ProcessResultAsync<T>(Func<CoreWebView2WebResourceResponseReceivedEventArgs, T> func, CancellationToken? cancellationToken = null)
    {
        var result = await GetResultAsync(cancellationToken);
        return await _client.ExecuteAsync(_ => func(result));
    }

    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="func">Function on result.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning value.</returns>
    public async Task<T> ProcessResultAsync<T>(Func<CoreWebView2, CoreWebView2WebResourceResponseReceivedEventArgs, T> func, CancellationToken? cancellationToken = null)
    {
        var result = await GetResultAsync(cancellationToken);
        return await _client.ExecuteAsync(v => func(v, result));
    }


    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="func">Function on result.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning value.</returns>
    public async Task<T> ProcessResultAsync<T>(Func<CoreWebView2WebResourceResponseReceivedEventArgs, Task<T>> func, CancellationToken? cancellationToken = null)
    {
        var result = await GetResultAsync(cancellationToken);
        return await _client.ExecuteAsync(_ => func(result));
    }

    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="func">Function on result.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning value.</returns>
    public async Task<T> ProcessResultAsync<T>(Func<CoreWebView2, CoreWebView2WebResourceResponseReceivedEventArgs, Task<T>> func, CancellationToken? cancellationToken = null)
    {
        var result = await GetResultAsync(cancellationToken);
        return await _client.ExecuteAsync(v => func(v, result));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _client.ExecuteAsync(_ => _core.WebResourceResponseReceived -= Core_WebResourceResponseReceived);
    }
}
