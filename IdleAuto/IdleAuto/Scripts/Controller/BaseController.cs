using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class BaseController
{
    protected ChromiumWebBrowser _browser;

    protected int _broSeed;

    static string[] JsNames = new string[] { "ah", "char", "init", "map" };

    protected delegate void OnJsInitCallBack(bool result);
    protected OnJsInitCallBack onJsInitCallBack;

    /// <summary>
    /// 等待特定信号的委托
    /// </summary>
    /// <param name="signal"></param>
    protected delegate void OnSignalCallBack(string signal);
    protected OnSignalCallBack onSignalCallBack;


    public void OnSignalCallback(params object[] args)
    {
        string t = args[0] as string;
        onSignalCallBack?.Invoke(t);
    }

    /// <summary>
    /// 执行一个js或者跳转页面 等待js使用特定信号回调
    /// </summary>
    /// <param name="signal"></param>
    /// <param name="js"></param>
    /// <param name="urlToJump"></param>
    /// <returns></returns>
    public async Task SignalCallback(string signal, Action act)
    {

        var tcs2 = new TaskCompletionSource<bool>();
        if (onSignalCallBack != null)
        {
            throw new Exception("重复添加信号事件方法");
        }
        onSignalCallBack = (result) =>
        {
            if (result == signal)
            {
                tcs2.SetResult(true);
                onSignalCallBack = null;
            }
        };
        act.Invoke();
        await tcs2.Task;

    }

    /// <summary>
    /// 一个方法多种结果 只需等待其中一种结果的情况用这个 比如切图可能会异常可能会直接切过去
    /// </summary>
    /// <param name="signals"></param>
    /// <param name="act"></param>
    /// <returns></returns>
    public async Task SignalRaceCallBack(string[] signals, Action act)
    {
        var tcs2 = new TaskCompletionSource<bool>();
        if (onSignalCallBack != null)
        {
            throw new Exception("重复添加信号事件方法");
        }
        onSignalCallBack = (result) =>
        {
            if (signals.Contains(result))
            {
                tcs2.SetResult(true);
                onSignalCallBack = null;
            }
        };
        act.Invoke();
        await tcs2.Task;

    }








    //该方法要写在会执行刷新页面操作之后
    protected async Task JsInit()
    {

        //var tcs2 = new TaskCompletionSource<bool>();
        //onJsInitCallBack = (result) => tcs2.SetResult(result);
        //await tcs2.Task;
        using (var cts = new CancellationTokenSource())
        {
            cts.CancelAfter(TimeSpan.FromSeconds(50));

            var tcs2 = new TaskCompletionSource<bool>();


            onJsInitCallBack = (result) => tcs2.SetResult(result);

            var completedTask = await Task.WhenAny(tcs2.Task, Task.Delay(Timeout.Infinite, cts.Token));

            if (completedTask == tcs2.Task)
            {
                await Task.Delay(2000);
                // Task completed successfully
                await tcs2.Task; // Ensure any exceptions/cancellation are observed
            }
            else
            {
                // Task was cancelled

            }
        }
    }

}
