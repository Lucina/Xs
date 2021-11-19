using Microsoft.Web.WebView2.Core;
using Xs;

using var h = XsManager.CreateHandle();
//await XsManager.ExecuteAsync(XsManager.BlankPage, async v =>
//{
//    var cookies = await v.CookieManager.GetCookiesAsync("https://fanbox.cc");
//    foreach (var c in cookies)
//        v.CookieManager.DeleteCookie(c);
//});
/*await XsManager.ExecuteAsync("https://fanbox.cc", async v =>
{
    Console.WriteLine(v.Settings.UserAgent);
    var cookies = await v.CookieManager.GetCookiesAsync("https://fanbox.cc");
    foreach (var c in cookies)
        Console.WriteLine($"{c.Domain}\t{true}\t{c.Path}\t{c.IsSecure}\t{c.Expires}\t{c.Name}\t{c.Value}");
});*/

using (XsClient x = await XsManager.CreateClientAsync())
{
    var waiter = await x.CreateResponseWaiterAsync(v => v.Host == "api.fanbox.cc" && v.AbsolutePath == "/post.listCreator");
    await x.LoadAsync("https://fanbox.cc/@svchzz");
    await waiter.ProcessResultAsync(r0 =>
    {
        Console.WriteLine(r0.Request.Uri);
    });
}


internal class GimmeDaBlood<T>
{
    public T Result = default!;
}

class Wf
{
    private readonly XsClient _client;
    private readonly CoreWebView2 _core;
    private readonly Predicate<Uri> _predicate;
    private readonly ManualResetEvent _mre;
    private readonly GimmeDaBlood<CoreWebView2WebResourceResponseReceivedEventArgs> _result;

    public Wf(XsClient client, CoreWebView2 core, Predicate<Uri> predicate, ManualResetEvent mre, GimmeDaBlood<CoreWebView2WebResourceResponseReceivedEventArgs> result)
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

    public async Task ProcessResultAsync(Action<CoreWebView2WebResourceResponseReceivedEventArgs> action)
    {
        await Task.Yield();
        _mre.WaitOne();
        await _client.ExecuteAsync(_ => action(_result.Result));
    }

    public async Task ProcessResultAsync(Action<CoreWebView2, CoreWebView2WebResourceResponseReceivedEventArgs> action)
    {
        await Task.Yield();
        _mre.WaitOne();
        await _client.ExecuteAsync(v => action(v, _result.Result));
    }

    public async Task ProcessResultAsync(Func<CoreWebView2WebResourceResponseReceivedEventArgs, Task> action)
    {
        await Task.Yield();
        _mre.WaitOne();
        await _client.ExecuteAsync(_ => action(_result.Result));
    }

    public async Task ProcessResultAsync(Func<CoreWebView2, CoreWebView2WebResourceResponseReceivedEventArgs, Task> action)
    {
        await Task.Yield();
        _mre.WaitOne();
        await _client.ExecuteAsync(v => action(v, _result.Result));
    }

    public async Task<T> ProcessResultAsync<T>(Func<CoreWebView2WebResourceResponseReceivedEventArgs, T> func)
    {
        await Task.Yield();
        _mre.WaitOne();
        return await _client.ExecuteAsync(_ => func(_result.Result));
    }

    public async Task<T> ProcessResultAsync<T>(Func<CoreWebView2, CoreWebView2WebResourceResponseReceivedEventArgs, T> func)
    {
        await Task.Yield();
        _mre.WaitOne();
        return await _client.ExecuteAsync(v => func(v, _result.Result));
    }

    public async Task<T> ProcessResultAsync<T>(Func<CoreWebView2WebResourceResponseReceivedEventArgs, Task<T>> func)
    {
        await Task.Yield();
        _mre.WaitOne();
        return await _client.ExecuteAsync(_ => func(_result.Result));
    }

    public async Task<T> ProcessResultAsync<T>(Func<CoreWebView2, CoreWebView2WebResourceResponseReceivedEventArgs, Task<T>> func)
    {
        await Task.Yield();
        _mre.WaitOne();
        return await _client.ExecuteAsync(v => func(v, _result.Result));
    }
}

static class Extensions
{
    public static async Task<Wf> CreateResponseWaiterAsync(this XsClient client, Predicate<Uri> predicate)
        => await client.ExecuteAsync(v => CreateResponseWaiter(client, v, predicate));

    private static Wf CreateResponseWaiter(XsClient client, CoreWebView2 core, Predicate<Uri> predicate)
    {
        ManualResetEvent mre = new(false);
        GimmeDaBlood<CoreWebView2WebResourceResponseReceivedEventArgs> res = new();
        return new Wf(client, core, predicate, mre, res);
    }
}