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

    protected OnJsInitCallBack onSignal = null;

    private string _signal = "";
    public string Signal
    {
        get { return _signal; }
        set
        {
            //只能在js回调的委托里面更改信号量
            if (_signal != "") throw new Exception("上一个信号量未接受到回调:"+_signal);
            _signal = value;
        }
    }

    public BaseController()
    {
        EventManager.Instance.SubscribeEvent(emEventType.OnSignal, OnSignal);
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

    protected void OnSignal(params object[] args)
    {
        string t = args[0] as string;
        if (t == Signal)
        {
            onSignal?.Invoke(true);
            onSignal = null;
            _signal = "";
        }
    }

    public async Task SignalCallback()
    {
       
        var tcs2 = new TaskCompletionSource<bool>();
        if (onSignal != null)
        {
            throw new Exception("重复添加信号事件方法");
        }
        onSignal = (result) => tcs2.SetResult(result);
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
