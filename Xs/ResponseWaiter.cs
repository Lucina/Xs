using Microsoft.Web.WebView2.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xs;

/// <summary>
/// Represents an operation waiting for a certain response.
/// </summary>
public class ResponseWaiter
{
    private readonly XsClient _client;
    private readonly CoreWebView2 _core;
    private readonly Predicate<Uri> _predicate;
    private readonly ManualResetEvent _mre;
    private readonly GimmeDaBlood<CoreWebView2WebResourceResponseReceivedEventArgs> _result;

    internal ResponseWaiter(XsClient client, CoreWebView2 core, Predicate<Uri> predicate, ManualResetEvent mre, GimmeDaBlood<CoreWebView2WebResourceResponseReceivedEventArgs> result)
    {
        _client = client;
        _core = core;
        _predicate = predicate;
        _mre = mre;
        _result = result;
        _core.WebResourceResponseReceived += _core_WebResourceResponseReceived;
    }

    private void _core_WebResourceResponseReceived(object? sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
    {
        if (_predicate(new Uri(e.Request.Uri)))
        {
            _core.WebResourceResponseReceived -= _core_WebResourceResponseReceived;
            _result.Result = e;
            _mre.Set();
        }
    }

    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="action">Action on result.</param>
    /// <returns>Task.</returns>
    public async Task ProcessResultAsync(Action<CoreWebView2WebResourceResponseReceivedEventArgs> action)
    {
        await Task.Yield();
        _mre.WaitOne();
        await _client.ExecuteAsync(_ => action(_result.Result));
    }

    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="action">Action on result.</param>
    /// <returns>Task.</returns>
    public async Task ProcessResultAsync(Action<CoreWebView2, CoreWebView2WebResourceResponseReceivedEventArgs> action)
    {
        await Task.Yield();
        _mre.WaitOne();
        await _client.ExecuteAsync(v => action(v, _result.Result));
    }

    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="action">Action on result.</param>
    /// <returns>Task.</returns>
    public async Task ProcessResultAsync(Func<CoreWebView2WebResourceResponseReceivedEventArgs, Task> action)
    {
        await Task.Yield();
        _mre.WaitOne();
        await _client.ExecuteAsync(_ => action(_result.Result));
    }

    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="action">Action on result.</param>
    /// <returns>Task.</returns>
    public async Task ProcessResultAsync(Func<CoreWebView2, CoreWebView2WebResourceResponseReceivedEventArgs, Task> action)
    {
        await Task.Yield();
        _mre.WaitOne();
        await _client.ExecuteAsync(v => action(v, _result.Result));
    }

    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="func">Function on result.</param>
    /// <returns>Task returning value.</returns>
    public async Task<T> ProcessResultAsync<T>(Func<CoreWebView2WebResourceResponseReceivedEventArgs, T> func)
    {
        await Task.Yield();
        _mre.WaitOne();
        return await _client.ExecuteAsync(_ => func(_result.Result));
    }

    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="func">Function on result.</param>
    /// <returns>Task returning value.</returns>
    public async Task<T> ProcessResultAsync<T>(Func<CoreWebView2, CoreWebView2WebResourceResponseReceivedEventArgs, T> func)
    {
        await Task.Yield();
        _mre.WaitOne();
        return await _client.ExecuteAsync(v => func(v, _result.Result));
    }


    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="func">Function on result.</param>
    /// <returns>Task returning value.</returns>
    public async Task<T> ProcessResultAsync<T>(Func<CoreWebView2WebResourceResponseReceivedEventArgs, Task<T>> func)
    {
        await Task.Yield();
        _mre.WaitOne();
        return await _client.ExecuteAsync(_ => func(_result.Result));
    }

    /// <summary>
    /// Process a result.
    /// </summary>
    /// <param name="func">Function on result.</param>
    /// <returns>Task returning value.</returns>
    public async Task<T> ProcessResultAsync<T>(Func<CoreWebView2, CoreWebView2WebResourceResponseReceivedEventArgs, Task<T>> func)
    {
        await Task.Yield();
        _mre.WaitOne();
        return await _client.ExecuteAsync(v => func(v, _result.Result));
    }
}
