using Microsoft.Web.WebView2.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Xs;

/// <summary>
/// Manages browser instances.
/// </summary>
public static class XsManager
{
    /// <summary>
    /// Path for blank page.
    /// </summary>
    public const string BlankPage = "about:blank";

    private static bool _init;
    private static bool _closed;
    private static Application? a;
    private static ManualResetEvent? mainMre;

    internal static void InitMre() => mainMre?.Set();

    /// <summary>
    /// Initializes WPF core for browser instances.
    /// </summary>
    public static void Init()
    {
        if (_init) return;
        _init = true;
        mainMre = new(false);
        ThreadStart ts = new(() =>
        {
            Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
            a = new()
            {
                StartupUri = new Uri("pack://application:,,,/Xs;component/MainWindow.xaml", UriKind.Absolute)
            };
            a.Run();
        });
        Thread tr = new(ts);
        tr.SetApartmentState(ApartmentState.STA);
        tr.Start();
        mainMre.WaitOne();
    }

    /// <summary>
    /// Asynchronously creates a new browser client.
    /// </summary>
    /// <returns>Task returning browser client.</returns>
    public static async Task<XsClient> CreateClientAsync()
    {
        Init();
        EnsureState();
        GimmeDaBlood<XsClient> res = new();
        await MainWindow._singleton!.Dispatcher.InvokeAsync(() => MainWindow._singleton!.CreateClient(res));
        await res.Result.ReadyAsync();
        return res.Result;
    }

    /// <summary>
    /// Asynchronously gets browser content for page.
    /// </summary>
    /// <param name="uri">Uri.</param>
    /// <returns>Text of content.</returns>
    public static async Task<string> GetContentAsync(string uri)
    {
        Init();
        EnsureState();
        using XsClient client = await CreateClientAsync();
        await client.LoadAsync(uri);
        return await client.GetContentAsync();
    }

    /// <summary>
    /// Asynchronously executes action for a page.
    /// </summary>
    /// <param name="uri">Uri.</param>
    /// <param name="action">Action to execute.</param>
    /// <returns>Text of content.</returns>
    public static async Task ExecuteAsync(string uri, Action<CoreWebView2> action)
    {
        Init();
        EnsureState();
        using XsClient client = await CreateClientAsync();
        await client.LoadAsync(uri);
        await client.ExecuteAsync(action);
    }

    /// <summary>
    /// Asynchronously executes action for a page.
    /// </summary>
    /// <param name="uri">Uri.</param>
    /// <param name="action">Action to execute.</param>
    /// <returns>Text of content.</returns>
    public static async Task ExecuteAsync(string uri, Func<CoreWebView2, Task> action)
    {
        Init();
        EnsureState();
        using XsClient client = await CreateClientAsync();
        await client.LoadAsync(uri);
        await client.ExecuteAsync(action);
    }

    /// <summary>
    /// Asynchronously executes function for a page.
    /// </summary>
    /// <param name="uri">Uri.</param>
    /// <param name="function">Function to execute.</param>
    /// <returns>Text of content.</returns>
    public static async Task<T> ExecuteAsync<T>(string uri, Func<CoreWebView2, T> function)
    {
        Init();
        EnsureState();
        using XsClient client = await CreateClientAsync();
        await client.LoadAsync(uri);
        return await client.ExecuteAsync(function);
    }

    /// <summary>
    /// Asynchronously executes action for a page.
    /// </summary>
    /// <param name="uri">Uri.</param>
    /// <param name="function">Function to execute.</param>
    /// <returns>Text of content.</returns>
    public static async Task<T> ExecuteAsync<T>(string uri, Func<CoreWebView2, Task<T>> function)
    {
        Init();
        EnsureState();
        using XsClient client = await CreateClientAsync();
        await client.LoadAsync(uri);
        return await client.ExecuteAsync(function);
    }

    /// <summary>
    /// Creates a new Xs quit handle.
    /// </summary>
    /// <returns>Quit handle.</returns>
    public static XsHandle CreateHandle() => new();

    /// <summary>
    /// Quits browser manager.
    /// </summary>
    public static void Quit()
    {
        if (_closed) return;
        EnsureState();
        _closed = true;
        MainWindow._singleton!.Dispatcher.InvokeAsync(() => MainWindow._singleton.Close());
    }

    private static void EnsureState()
    {
        if (_closed) throw new InvalidOperationException();
        if (a == null || MainWindow._singleton == null) throw new InvalidOperationException();
    }
}
