using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

/// <summary>
/// 1.确保name+url的窗口只会添加一个浏览器实例
/// 2.销毁一个tabpage的时候需要对字典中的index作重新排序
/// 3.accountName目前不支持多线程 也需要一个方法通过
/// </summary>
public class BroTabManager
{
    static object LOCKOBJECT = new object();

    private TabControl Tab;

    /// <summary>
    /// 返回的流水号
    /// </summary>
    private int _seed = 0;
    /// <summary>
    /// 流水号->浏览器
    /// </summary>
    public ConcurrentDictionary<int, ChromiumWebBrowser> BroDic;

    /// <summary>
    /// 流水号->选项卡 用于选中选项卡
    /// </summary>
    public ConcurrentDictionary<int, TabPage> TabPageDic;

    /// <summary>
    /// 账号名+地址 存浏览器流水号
    /// </summary>
    public ConcurrentDictionary<string, int> NameUrlDic;



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
    private ChromiumWebBrowser InitializeChromium( string name, string url)
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
        browser.FrameLoadEnd += (sender,e)=> OnFrameLoadEnd(sender,e,name);
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
    }





    private int AddTabPage(string name, string url)
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
        bro = InitializeChromium(name, url);
        // 创建 TabPage
        TabPage tabPage = new TabPage(name);
        tabPage.Controls.Add(bro);
        // 将 TabPage 添加到 TabControl
        Tab.TabPages.Add(tabPage);
        Tab.SelectedTab = tabPage;
        TabPageDic.TryAdd(_seed, tabPage);
        return _seed;
    }

    private void LoadUrl(int seed, string name, string url)
    {
        Tab.SelectedTab = TabPageDic[seed] ;
        NameUrlDic[name + url] = seed;
        var bro = BroDic[seed];
        bro.LoadUrl(url);
    }
    public int TriggerAddTabPage(string title, string url)
    {
        // 触发事件
        //AddTabPageEvent?.Invoke(title, url);
        // 检查是否需要调用 Invoke
        if (Tab.InvokeRequired)
        {
            // 使用 Invoke 方法将操作委托给创建控件的线程
          var a=  Tab.Invoke(new Func<int>(() => {return AddTabPage(title, url); }));
            return int.Parse(a.ToString());
        }
        else
        {
          return AddTabPage(title, url);
        }

    }

    public void TriggerLoadUrl(string title, string url,int index)
    {
        // 触发事件
        //AddTabPageEvent?.Invoke(title, url);
        // 检查是否需要调用 Invoke
        if (Tab.InvokeRequired)
        {
            // 使用 Invoke 方法将操作委托给创建控件的线程
            Tab.Invoke(new Action(() => LoadUrl(index,title, url)));
        }
        else
        {
            LoadUrl(index,title, url);
        }

    }

    public void DisposePage(int seed)
    {
        var list=NameUrlDic.Values.ToList();
        foreach(var item in NameUrlDic.ToList())
        {
           if( item.Value == seed)
            {
                NameUrlDic.TryRemove(item.Key,out _);
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

    private void OnFrameLoadStart(object sender, FrameLoadStartEventArgs e)
    {
        var bro = sender as ChromiumWebBrowser;
        EventManager.Instance.InvokeEvent(emEventType.OnBrowserFrameLoadStart, bro.Address);

    }

    private void OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e,string name)
    {
        var bro = sender as ChromiumWebBrowser;
        string url = bro.Address;
        Console.WriteLine(url);
        if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.LoginPage))
        {
            Task.Run(async () =>
            {
                await PageLoadHandler.LoadCookieAndCache(bro, name);
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
}

