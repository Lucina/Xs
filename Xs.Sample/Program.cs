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
    using var waiter = await x.CreateResponseWaiterAsync(v => v.Host == "api.fanbox.cc");
    using var waiter2 = await x.CreateResponseWaiterAsync(v => v.Host == "api.fanbox.cc" && v.AbsolutePath == "/post.listCreator");
    await x.LoadAsync("https://fanbox.cc/@svchzz");
    var cts = new CancellationTokenSource(20_000);
    var ct = cts.Token;
    do
    {
        await waiter.ProcessResultAsync(r0 =>
        {
            Console.WriteLine(r0.Request.Uri);
        }, ct);
    } while (!waiter2.ResultAvailable);
    await waiter2.ProcessResultAsync(r0 =>
    {
        Console.WriteLine($"Main call: {r0.Request.Uri}");
    });
}
