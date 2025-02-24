using CefSharp;
using CefSharp.WinForms;
using IdleAuto.Scripts;
using IdleAuto.Scripts.Wrap;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

/// <summary>
/// 1.确保name+url的窗口只会添加一个浏览器实例
/// </summary>
public class BroTabManager
{
    static object LOCKOBJECT = new object();

    public static BroTabManager Instance;

    private TabControl Tab;

    public static string Proxy;

    /// <summary>
    /// 返回的流水号
    /// </summary>
    private int _seed = 0;
    /// <summary>
    /// 流水号->浏览器
    /// </summary>
    private ConcurrentDictionary<int, ChromiumWebBrowser> BroDic;

    /// <summary>
    /// 流水号->选项卡 用于选中选项卡
    /// </summary>
    private ConcurrentDictionary<int, TabPage> TabPageDic;

    /// <summary>
    /// 账号名+地址 存浏览器流水号
    /// </summary>
    private ConcurrentDictionary<string, int> NameUrlDic;

    private delegate void OnJsInitCallBack(string jsName);
    private OnJsInitCallBack onJsInitCallBack;

    private void SeedIncrease()
    {
        lock (LOCKOBJECT)
        {
            _seed++;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index">浏览器的流水号</param>
    /// <returns></returns>
    private ChromiumWebBrowser InitializeChromium(string name, string url, string proxy = "")
    {
        // 创建 CefRequestContextSettings 并指定缓存路径
        var requestContextSettings = new RequestContextSettings
        {
            CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"cache/{name}")
        };
        // 创建 CefRequestContext
        var requestContext = new RequestContext(requestContextSettings);


        // 创建 ChromiumWebBrowser 实例，并指定 RequestContext
        var browser = new ChromiumWebBrowser(url, requestContext) { Dock = DockStyle.Fill };

        if (!string.IsNullOrWhiteSpace(Proxy))
        {
            browser.IsBrowserInitializedChanged += OnIsBrowserInitializedChanged;

        }
        // 绑定对象
        browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
        browser.JavascriptObjectRepository.Register("Bridge", new Bridge(), isAsync: true, options: BindingOptions.DefaultBinder);
        browser.KeyboardHandler = new CEFKeyBoardHandler();
        // 等待页面加载完成后执行脚本
        browser.FrameLoadEnd += (sender, e) => OnFrameLoadEnd(sender, e, name, url);
        browser.FrameLoadStart += (sender, e) => OnFrameLoadStart(sender, e, name, url);
        browser.IsBrowserInitializedChanged += (sender, e) =>
        {
            if (browser.IsBrowserInitialized)
            {
                P.Log($"Start Load {name} CookieAndCache");
                PageLoadHandler.LoadCookieAndCache(browser, name, url);
            }
        };

        BroDic.TryAdd(_seed, browser);

        return browser;
    }

    private void OnIsBrowserInitializedChanged(object sender, EventArgs e)
    {
        var bro = sender as ChromiumWebBrowser;
        // 在浏览器初始化完成后设置代理
        SetProxy(bro);

    }
    public void SetProxy(ChromiumWebBrowser browser)
    {
        var proxySettings = new Dictionary<string, object>
        {
            {"mode", "fixed_servers"},
            {"server", Proxy}
        };
        Cef.UIThreadTaskFactory.StartNew(() =>
        {
            browser.GetBrowser().GetHost().RequestContext.SetPreference("proxy", proxySettings, out string s);
            // 重新设置RequestHandler以更新代理账号密码
            browser.RequestHandler = new MyRequestHandler("d2011731996", "v84dygqx");
            Proxy = "";
        });

    }
    private void RemoveProxy(ChromiumWebBrowser browser)
    {
        // 在 CEF UI 线程上移除代理
        Cef.UIThreadTaskFactory.StartNew(() =>
        {
            var proxyOptions = new Dictionary<string, object>
            {
                ["mode"] = "direct"
            };
            browser.GetBrowser().GetHost().RequestContext.SetPreference("proxy", proxyOptions, out string error);
        });
    }

    public BroTabManager(TabControl tab)
    {
        this.Tab = tab;
        // 确保控件的句柄已经创建
        var handle = Tab.Handle;
        BroDic = new ConcurrentDictionary<int, ChromiumWebBrowser>();
        TabPageDic = new ConcurrentDictionary<int, TabPage>();
        NameUrlDic = new ConcurrentDictionary<string, int>();
        Instance = this;
    }

    public ChromiumWebBrowser GetBro(int seed)
    {
        return BroDic[seed];
    }
    public int GetFocusID()
    {
        int seed = 0;
        TabPage tab = null;
        if (Tab.InvokeRequired)
        {
            Tab.Invoke(new Action(() =>
           {

               tab = Tab.SelectedTab;

           }));
        }
        else tab = Tab.SelectedTab;
        seed = TabPageDic.First(x => x.Value == tab).Key;
        return seed;
    }

    private async Task<int> AddTabPage(string name, string url, string jsName = "", string proxy = "")
    {

        ChromiumWebBrowser bro;
        var key = name + url;
        if (NameUrlDic.ContainsKey(key))
        {
            int seed = NameUrlDic[key];
            Tab.SelectedTab = TabPageDic[seed];
            return seed;
        }
        SeedIncrease();

        var jsTask = new TaskCompletionSource<bool>();
        onJsInitCallBack = (result) =>
        {
            if (jsName == string.Empty || jsName == result) { jsTask.SetResult(true); onJsInitCallBack = null; }
        };
        EventManager.Instance.SubscribeEvent(emEventType.OnJsInited, OnJsInited);

        var broWindow = new BroWindow(0, name, url);
        bro = InitializeChromium(name, url, proxy);

        // 创建 TabPage
        TabPage tabPage = new TabPage(name);
        tabPage.Controls.Add(bro);
        // 将 TabPage 添加到 TabControl
        Tab.TabPages.Add(tabPage);
        Tab.SelectedTab = tabPage;
        NameUrlDic.TryAdd(key, _seed);
        TabPageDic.TryAdd(_seed, tabPage);

        await jsTask.Task;

        EventManager.Instance.UnsubscribeEvent(emEventType.OnJsInited, OnJsInited);
        return _seed;
    }

    private async Task LoadUrl(int seed, string name, string url, string jsName = "")
    {
        Tab.SelectedTab = TabPageDic[seed];
        NameUrlDic[name + url] = seed;
        var bro = BroDic[seed];
        var jsTask = new TaskCompletionSource<bool>();
        onJsInitCallBack = (result) =>
        {
            if (jsName == string.Empty || jsName == result) { jsTask.SetResult(true); onJsInitCallBack = null; }
        };
        EventManager.Instance.SubscribeEvent(emEventType.OnJsInited, OnJsInited);

        bro.LoadUrl(url);

        await jsTask.Task;

        EventManager.Instance.UnsubscribeEvent(emEventType.OnJsInited, OnJsInited);
        return;
    }

    public async Task<int> TriggerAddTabPage(string title, string url, string jsName = "", string proxy = "")
    {
        // 触发事件
        //AddTabPageEvent?.Invoke(title, url);
        // 检查是否需要调用 Invoke
        if (Tab.InvokeRequired)
        {
            // 使用 TaskCompletionSource<int> 来等待 Invoke 的结果
            var tcs = new TaskCompletionSource<int>();
            Tab.Invoke(new Action(async () =>
            {
                try
                {
                    var result = await AddTabPage(title, url, jsName, proxy);
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }));
            return await tcs.Task;
        }
        else
        {
            return await AddTabPage(title, url, jsName, proxy);
        }
    }

    public async Task TriggerLoadUrl(string title, string url, int index, string jsName = "")
    {
        // 触发事件
        //AddTabPageEvent?.Invoke(title, url);
        // 检查是否需要调用 Invoke
        if (Tab.InvokeRequired)
        {
            // 使用 TaskCompletionSource 来等待 Invoke 的结果
            var tcs = new TaskCompletionSource<bool>();
            Tab.Invoke(new Action(async () =>
            {
                try
                {
                    await LoadUrl(index, title, url, jsName);
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }));
            await tcs.Task;
        }
        else
        {
            await LoadUrl(index, title, url, jsName);
        }
    }

    public void DisposePage(int seed)
    {
        var list = NameUrlDic.Values.ToList();
        foreach (var item in NameUrlDic.ToList())
        {
            if (item.Value == seed)
            {
                NameUrlDic.TryRemove(item.Key, out _);
            }
        }
        var tabPage = TabPageDic[seed];
        if (Tab.InvokeRequired)
        {
            Tab.Invoke(new Action(() =>
            {
                Tab.TabPages.Remove(tabPage);
            }));
        }
        else
        {
            Tab.TabPages.Remove(tabPage);
        }
        BroDic.TryRemove(seed, out _);
        TabPageDic.TryRemove(seed, out _);
        GC.Collect();
    }

    /// <summary>
    /// 异步调用js方法，无需等待页面重载
    /// </summary>
    public async Task<JavascriptResponse> TriggerCallJs(int seed, string jsFunc)
    {
        var bro = BroDic[seed];
        var response2 = await bro.EvaluateScriptAsync(jsFunc);
        if (!response2.Success) P.Log($"Success = {response2.Success}, Message = {response2.Message}, Result = {response2.Result}", emLogType.Error);
        return response2;
    }
    /// <summary>
    /// 异步调用js方法，需等待页面重载
    /// </summary>
    public async Task<JavascriptResponse> TriggerCallJsWithReload(int seed, string jsFunc, string jsName)
    {
        var bro = BroDic[seed];
        EventManager.Instance.SubscribeEvent(emEventType.OnJsInited, OnJsInited);
        var jsTask = new TaskCompletionSource<bool>();
        onJsInitCallBack = (result) =>
        {
            if (jsName == string.Empty || jsName == result) { jsTask.SetResult(true); onJsInitCallBack = null; }
        };
        var response2 = await bro.EvaluateScriptAsync(jsFunc);
        //if (!response2.Success)
        //{
        //    jsTask.SetResult(false); onJsInitCallBack = null;
        //}
        //else
        {
            await Task.Delay(1000);
            bro.Reload();
        }

        await jsTask.Task;
        EventManager.Instance.UnsubscribeEvent(emEventType.OnJsInited, OnJsInited);
        return response2;
    }

    public void ClearBrowsers()
    {
        foreach (var item in BroDic)
        {
            DisposePage(item.Key);
        }
    }

    private void OnFrameLoadStart(object sender, FrameLoadStartEventArgs e, string name, string jumpToUrl)
    {
        P.Log($"On {name} FrameLoadStart URL:{jumpToUrl} Time:{DateTime.Now}", emLogType.Warning);

        var bro = sender as ChromiumWebBrowser;
        EventManager.Instance.InvokeEvent(emEventType.OnBrowserFrameLoadStart, bro.Address);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <param name="name">账户名称 南宫</param>
    /// <param name="jumpTo">需要条状的地址 在载入cookie之后跳转</param>
    private void OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e, string name, string jumpToUrl)
    {
        var bro = sender as ChromiumWebBrowser;
        string url = bro.Address;
        //EventManager.Instance.SubscribeEvent(emEventType.OnAccountCheck, CheckAccount);
        //if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.LoginPage))
        //{
        //    Task.Run(async () =>
        //    {
        //        await PageLoadHandler.LoadCookieAndCache(bro, name, jumpToUrl);
        //    });
        //}
        Task.Run(async () =>
        {
            await PageLoadHandler.LoadJsByUrl(bro);
        });

        if (!PageLoadHandler.ContainsUrl(url, PageLoadHandler.LoginPage))
        {
            P.Log($"Start Save {name} CookieAndCache");
            PageLoadHandler.SaveCookieAndCache(bro, name);
            RemoveProxy(bro);
        }

        EventManager.Instance.InvokeEvent(emEventType.OnBrowserFrameLoadEnd, bro.Address);
    }


    private void OnJsInited(params object[] args)
    {
        string jsName = args[0] as string;
        P.Log($"OnJsInited:{jsName}");
        onJsInitCallBack?.Invoke(jsName);
    }

    private void CheckAccount(params object[] args)
    {
        string name = args[0] as string;
        P.Log($"当前选择账号：{AccountController.Instance.User.AccountName}--当前读取缓存账户：{name}");
        //if (AccountController.Instance.User.AccountName != name)
        //{
        //    int id = GetFocusID();
        //    var bro = BroDic[id];
        //    PageLoadHandler.DeleteCookie(bro, AccountController.Instance.User.AccountName);
        //    bro.Reload();
        //}
    }


}
public class MyRequestHandler : CefSharp.Handler.RequestHandler
{
    private string _proxyUserName;
    private string _proxyUserPwd;

    public MyRequestHandler(string proxyUser, string proxyPwd)
    {
        _proxyUserName = proxyUser;
        _proxyUserPwd = proxyPwd;
    }

    protected override bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
    {
        if (isProxy)
        {
            callback.Continue(_proxyUserName, _proxyUserPwd);
            return true;
        }
        return base.GetAuthCredentials(chromiumWebBrowser, browser, originUrl, isProxy, host, port, realm, scheme, callback);
    }
}

