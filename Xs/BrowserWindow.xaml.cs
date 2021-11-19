using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Xs;

/// <summary>
/// Interaction logic for BrowserWindow.xaml
/// </summary>
public sealed partial class BrowserWindow : Window, ICallManager
{
    private readonly XsClient _xs;
    private readonly ManualResetEvent _readyMre;
    private bool _disposed;

    /// <summary>
    /// Creates a new instance of <see cref="BrowserWindow"/>.
    /// </summary>
    public BrowserWindow()
    {
        InitializeComponent();
        _readyMre = new(false);
        _xs = new XsClient(this);
        RegisterNav(() => _readyMre.Set());
        Closing += (_, _) => _xs.Dispose();
    }

    private class NavHandler
    {
        private readonly WebView2 _webView;
        private readonly Action _post;

        public NavHandler(WebView2 wv2, Action post)
        {
            _webView = wv2;
            _post = post;
            _webView.NavigationCompleted += Wv2_NavigationCompleted;
        }

        private void Wv2_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            _webView.NavigationCompleted -= Wv2_NavigationCompleted;
            _post();
        }
    }

    private void RegisterNav(Action post)
    {
        _ = new NavHandler(webView, post);
    }

    Task ICallManager.ReadyAsync() => Task.Run(() => _readyMre.WaitOne());

    async Task ICallManager.LoadAsync(string page)
    {
        using ManualResetEvent mre = new(false);
        RegisterNav(() => mre.Set());
        await Dispatcher.InvokeAsync(() => webView.CoreWebView2.Navigate(page));
        await Task.Run(() => mre.WaitOne());
    }

    async Task ICallManager.ExecuteAsync(Action<CoreWebView2> action)
    {
        using ManualResetEvent mre = new(false);
        await Dispatcher.InvokeAsync(() =>
        {
            try
            {
                action(webView.CoreWebView2);
            }
            finally
            {
                mre.Set();
            }
        });
        await Task.Run(() => mre.WaitOne());
    }

    async Task ICallManager.ExecuteAsync(Func<CoreWebView2, Task> action)
    {
        using ManualResetEvent mre = new(false);
        await Dispatcher.InvokeAsync(async () =>
        {
            try
            {
                await action(webView.CoreWebView2);
            }
            finally
            {
                mre.Set();
            }
        });
        await Task.Run(() => mre.WaitOne());
    }

    async Task<T> ICallManager.ExecuteAsync<T>(Func<CoreWebView2, T> function)
    {
        using ManualResetEvent mre = new(false);
        GimmeDaBlood<T> r = new();
        await Dispatcher.InvokeAsync(() =>
        {
            try
            {
                r.Result = function(webView.CoreWebView2);
            }
            finally
            {
                mre.Set();
            }
        });
        await Task.Run(() => mre.WaitOne());
        return r.Result;
    }

    async Task<T> ICallManager.ExecuteAsync<T>(Func<CoreWebView2, Task<T>> function)
    {
        using ManualResetEvent mre = new(false);
        GimmeDaBlood<T> r = new();
        await Dispatcher.InvokeAsync(async () =>
        {
            try
            {
                r.Result = await function(webView.CoreWebView2);
            }
            finally
            {
                mre.Set();
            }
        });
        await Task.Run(() => mre.WaitOne());
        return r.Result;
    }

    async Task<string> ICallManager.ExecuteJsAsync(string js)
    {
        using ManualResetEvent mre = new(false);
        GimmeDaBlood<string> gdb = new();
        await Dispatcher.InvokeAsync(() => Js_UIONLY(gdb, mre, js));
        await Task.Run(() => mre.WaitOne());
        return gdb.Result;
    }

    Task<string> ICallManager.GetContentAsync() => ((ICallManager)this).ExecuteJsAsync("document.documentElement.outerHTML;");

    private Task<string> Js_UIONLYAsync(string js) => Js_UIONLYAsync(webView, js);

    private static async Task<string> Js_UIONLYAsync(WebView2 view, string js)
    {
        var x = await view.CoreWebView2.ExecuteScriptAsync(js);
        return Regex.Unescape(x[1..^1]);
    }

    private void Js_UIONLY(GimmeDaBlood<string> res, ManualResetEvent mre, string js)
    {
        Js_UIONLYAsync(js).ContinueWith(v =>
        {
            res.Result = v.Result;
            mre.Set();
        });
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Dispatcher.Invoke(() => Close());
    }
}
