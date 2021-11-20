using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;

namespace Xs;

internal interface IWebView2Operator : IDisposable
{
    Task ReadyAsync();
    Task LoadAsync(string page);
    Task ExecuteAsync(Action<CoreWebView2> action);
    Task ExecuteAsync(Func<CoreWebView2, Task> action);
    Task<T> ExecuteAsync<T>(Func<CoreWebView2, T> action);
    Task<T> ExecuteAsync<T>(Func<CoreWebView2, Task<T>> action);
    Task<string> ExecuteJsAsync(string js);
    Task<string> GetContentAsync();
}
