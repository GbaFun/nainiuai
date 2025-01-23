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

    static string[] JsNames = new string[] {"ah","char","init" };
    protected delegate void OnJsInitCallBack(bool result);
    protected OnJsInitCallBack onJsInitCallBack;

    public BaseController()
    {
    }


    protected void OnAhJsInited(params object[] args)
    {
        string jsName = args[0] as string;
        if (JsNames.Contains(jsName))
        {
            onJsInitCallBack?.Invoke(true);
            onJsInitCallBack = null;
        }
    }

    //该方法要写在会执行刷新页面操作之后
    protected async Task JsInit()
    {

        //var tcs2 = new TaskCompletionSource<bool>();
        //onJsInitCallBack = (result) => tcs2.SetResult(result);
        //await tcs2.Task;
        using (var cts = new CancellationTokenSource())
        {
            cts.CancelAfter(TimeSpan.FromMinutes(1));

            var tcs2 = new TaskCompletionSource<bool>();

            onJsInitCallBack = (result) => tcs2.SetResult(result);

            var completedTask = await Task.WhenAny(tcs2.Task, Task.Delay(Timeout.Infinite, cts.Token));

            if (completedTask == tcs2.Task)
            {
                await Task.Delay(3000);
                // Task completed successfully
                await tcs2.Task; // Ensure any exceptions/cancellation are observed
            }
            else
            {
                // Task was cancelled
                throw new OperationCanceledException("The operation was cancelled.");
            }
        }
    }

}
