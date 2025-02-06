using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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
    private ChromiumWebBrowser InitializeChromium(string name, string url)
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
        // 绑定对象
        browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
        browser.JavascriptObjectRepository.Register("Bridge", new Bridge(), isAsync: true, options: BindingOptions.DefaultBinder);
        browser.KeyboardHandler = new CEFKeyBoardHandler();
        // 等待页面加载完成后执行脚本
        browser.FrameLoadEnd += (sender, e) => OnFrameLoadEnd(sender, e, name, url);
        browser.FrameLoadStart += OnFrameLoadStart;
        BroDic.TryAdd(_seed, browser);

        return browser;
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
        var tab = Tab.SelectedTab;
        seed = TabPageDic.First(x => x.Value == tab).Key;
        return seed;
    }

    private async Task<int> AddTabPage(string name, string url, string jsName = "")
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
        onJsInitCallBack = (result) => jsTask.SetResult(jsName == string.Empty || jsName == result);
        EventManager.Instance.SubscribeEvent(emEventType.OnJsInited, OnJsInited);

        bro = InitializeChromium(name, url);
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
        onJsInitCallBack = (result) => jsTask.SetResult(jsName == string.Empty || jsName == result);
        EventManager.Instance.SubscribeEvent(emEventType.OnJsInited, OnJsInited);

        bro.LoadUrl(url);

        await jsTask.Task;
        EventManager.Instance.UnsubscribeEvent(emEventType.OnJsInited, OnJsInited);
        return;
    }

    public async Task<int> TriggerAddTabPage(string title, string url, string jsName = "")
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
                    var result = await AddTabPage(title, url, jsName);
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
            return await AddTabPage(title, url, jsName);
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

    public async Task<JavascriptResponse> TriggerCallJs(int seed, string jsFunc)
    {
        var bro = BroDic[seed];
        EventManager.Instance.SubscribeEvent(emEventType.OnJsInited, OnJsInited);
        var jsTask = new TaskCompletionSource<bool>();
        onJsInitCallBack = (result) => jsTask.SetResult(true);
        var response2 = await bro.EvaluateScriptAsync(jsFunc);
        await jsTask.Task;
        EventManager.Instance.UnsubscribeEvent(emEventType.OnJsInited, OnJsInited);
        return response2;
    }

    private void OnFrameLoadStart(object sender, FrameLoadStartEventArgs e)
    {
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
        Console.WriteLine(url);
        if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.LoginPage))
        {
            Task.Run(async () =>
            {
                await PageLoadHandler.LoadCookieAndCache(bro, name, jumpToUrl);
            });
        }
        Task.Run(async () =>
        {
            await PageLoadHandler.LoadJsByUrl(bro);
        });

        if (!PageLoadHandler.ContainsUrl(url, PageLoadHandler.LoginPage))
        {
            PageLoadHandler.SaveCookieAndCache(bro);
        }

        EventManager.Instance.InvokeEvent(emEventType.OnBrowserFrameLoadEnd, bro.Address);
    }

    private void OnJsInited(params object[] args)
    {
        string jsName = args[0] as string;
        onJsInitCallBack?.Invoke(jsName);
        onJsInitCallBack = null;
    }
}

