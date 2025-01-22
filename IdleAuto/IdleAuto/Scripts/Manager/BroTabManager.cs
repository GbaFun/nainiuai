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


public class BroTabManager
{
    private TabControl Tab;

    /// <summary>
    /// 登录账号调取cookie用
    /// </summary>
    private string _accountName { get; set; }

    /// <summary>
    /// 流水号存浏览器 流水号就是选项卡索引
    /// </summary>
    public ConcurrentDictionary<int, ChromiumWebBrowser> BroDic;

    /// <summary>
    /// 账号名+地址 存浏览器流水号
    /// </summary>
    public ConcurrentDictionary<string, int> NameUrlDic;



    //public event Action<string, string> AddTabPageEvent;

    //public event Action<int, string> LoadUrl;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index">浏览器的流水号</param>
    /// <returns></returns>
    private ChromiumWebBrowser InitializeChromium(int index, string name, string url)
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
        browser.FrameLoadEnd += OnFrameLoadEnd;
        browser.FrameLoadStart += OnFrameLoadStart;
        BroDic.TryAdd(index, browser);
        return browser;
    }


    public BroTabManager(TabControl tab)
    {
        this.Tab = tab;
        // 确保控件的句柄已经创建
        var handle = Tab.Handle;
        BroDic = new ConcurrentDictionary<int, ChromiumWebBrowser>();
        NameUrlDic = new ConcurrentDictionary<string, int>();
    }





    private int AddTabPage(string name, string url)
    {
        ChromiumWebBrowser bro;
        var key = name + url;
        if (NameUrlDic.ContainsKey(key))
        {
            Tab.SelectedIndex = NameUrlDic[key];
            return Tab.SelectedIndex;
        }
        _accountName = name;
        var index = Tab.TabPages.Count;
        bro = InitializeChromium(index, name, url);
        // 创建 TabPage
        TabPage tabPage = new TabPage(name);
        tabPage.Controls.Add(bro);
        // 将 TabPage 添加到 TabControl
        Tab.TabPages.Add(tabPage);
        Tab.SelectedIndex = index;
        return index;
    }

    private void LoadUrl(int index, string name, string url)
    {
        Tab.SelectedIndex = index;
        NameUrlDic[name + url] = index;
        var bro = BroDic[index];
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

    private void OnFrameLoadStart(object sender, FrameLoadStartEventArgs e)
    {
        var bro = sender as ChromiumWebBrowser;
        EventManager.Instance.InvokeEvent(emEventType.OnBrowserFrameLoadStart, bro.Address);

    }

    private void OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
    {
        var bro = sender as ChromiumWebBrowser;
        string url = bro.Address;
        Console.WriteLine(url);
        if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.LoginPage))
        {
            Task.Run(async () =>
            {
                await PageLoadHandler.LoadCookieAndCache(bro, _accountName);
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

