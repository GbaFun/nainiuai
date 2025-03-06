using CefSharp.WinForms;
using IdleAuto.Scripts.Wrap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class TabManager
{
    static object LOCKOBJECT = new object();

    private TabControl Tab;
    /// <summary>
    /// 返回的流水号
    /// </summary>
    private int _seed = 0;


    /// <summary>
    /// js初始化的委托
    /// </summary>
    /// <param name="jsName"></param>
    private delegate void OnJsInited(string jsName);
    private OnJsInited onJsInitCallBack;

    /// <summary>
    /// 流水号->浏览器包装类
    /// </summary>
    private ConcurrentDictionary<int, BroWindow> BroWindowDic;

    /// <summary>
    /// 流水号->选项卡 用于选中选项卡
    /// </summary>
    private ConcurrentDictionary<int, TabPage> TabPageDic;

    public static TabManager Instance;
    public TabManager(TabControl tab)
    {
        this.Tab = tab;
        // 确保控件的句柄已经创建
        var handle = Tab.Handle;

        TabPageDic = new ConcurrentDictionary<int, TabPage>();

        BroWindowDic = new ConcurrentDictionary<int, BroWindow>();
        Instance = this;
    }

    private int AddTabPage(string name)
    {
        SeedIncrease();

        // 创建 TabPage
        TabPage tabPage = new TabPage(name);
        // tabPage.Controls.Add(bro);
        // 将 TabPage 添加到 TabControl
        Tab.TabPages.Add(tabPage);
        Tab.SelectedTab = tabPage;
        TabPageDic.TryAdd(_seed, tabPage);
        return _seed;
    }


    /// <summary>
    /// 载入指定地址,因为浏览器在被添加进控件渲染时才真正初始化 所以将bro单独new意义不大
    /// </summary>
    /// <param name="name">窗口名字</param>
    /// <param name="url">跳转url</param>
    /// <param name="jsName">必须等待某个js载入完成才能保证逻辑正确执行</param>
    /// <returns></returns>
    public async Task<BroWindow> AddBroToTap(UserModel user, string url)
    {
        var seed = AddTabPage(user.AccountName);
        var tabPage = TabPageDic[seed];

        var jsTask = new TaskCompletionSource<bool>();
        //等待特定js载入完成
        onJsInitCallBack = (result) =>
        {
            if ("login" == result) { jsTask.SetResult(true); onJsInitCallBack = null; }
        };
        var window = new BroWindow(seed, user, url);
        window.SubscribeEvent(emEventType.OnJsInited, OnJsinitCallBack);
        tabPage.Controls.Add(window.GetBro());
        await jsTask.Task;
        window.UnsubscribeEvent(emEventType.OnJsInited, OnJsinitCallBack);
        BroWindowDic.TryAdd(seed, window);
        return window;
    }
    public async Task<BroWindow> TriggerAddBroToTap(UserModel user)
    {
        // 触发事件
        //AddTabPageEvent?.Invoke(title, url);
        // 检查是否需要调用 Invoke
        if (Tab.InvokeRequired)
        {
            // 使用 TaskCompletionSource<int> 来等待 Invoke 的结果
            var tcs = new TaskCompletionSource<BroWindow>();
            Tab.Invoke(new Action(async () =>
            {
                try
                {
                    var result = await AddBroToTap(user, IdleUrlHelper.HomeUrl());
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
            return await AddBroToTap(user, IdleUrlHelper.HomeUrl());
        }
    }

    public BroWindow GetWindow(int seed)
    {
        return BroWindowDic[seed];
    }
    public BroWindow GetWindow()
    {
        return BroWindowDic[GetFocusID()];
    }

    private void OnJsinitCallBack(params object[] args)
    {
        string jsName = args[0] as string;
        P.Log($"OnJsInited:{jsName}");
        onJsInitCallBack?.Invoke(jsName);
    }

    private int GetFocusID()
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
    private void SeedIncrease()
    {
        lock (LOCKOBJECT)
        {
            _seed++;
        }
    }

    public void DisposePage(int seed)
    {

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
        BroWindowDic.TryRemove(seed, out _);
        TabPageDic.TryRemove(seed, out _);
        
    }
    public void DisposePage()
    {
        List<int> seeds = BroWindowDic.Keys.ToList();
        for (int i = 0; i < seeds.Count; i++)
        {
            DisposePage(seeds[i]);
        }
        seeds.Clear();
    }

}

