using CefSharp;
using IdleAuto.Configs.CfgExtension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CefSharp.WinForms;

public class AuctionController
{
    private ChromiumWebBrowser _browser;

    private int _broSeed;

    private delegate void OnJsInitCallBack(bool result);
    private OnJsInitCallBack onJsInitCallBack;
    public AuctionController()
    {
    }
    private static AuctionController instance;
    public static AuctionController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AuctionController();
            }
            return instance;
        }
    }

    /// <summary>
    /// 是否开始扫货
    /// </summary>
    public bool IsStart = false;

    /// <summary>
    /// 当前索引位置
    /// </summary>
    public int CurIndex = 0;




    /// <summary>
    /// 开始扫拍
    /// </summary>
    public async void StartScan()
    {
        IsStart = true;
        EventManager.Instance.SubscribeEvent(emEventType.OnJsInited, OnAhJsInited);
        await StartAutoJob();
    }

    /// <summary>
    /// 开始自动执行
    /// </summary>
    private async Task StartAutoJob()
    {
        _browser = await GetBrowserAsync();
        await AutoBuy();
        await AutoJump();
        await AutoNextPage();
        var isLastPage = await IsLastPage();
        var isJumpEnd = await IsJumpToEnd(GetCurNode());
        if (isLastPage && isJumpEnd)
        {
            if (CurIndex == ScanAhCfg.Instance.NodeList.Count - 1) CurIndex = 0;
            else CurIndex++;
            await AutoJump();
        }

    }


    private async Task<ChromiumWebBrowser> GetBrowserAsync()
    {
        var roleid = AccountController.Instance.User.FirstRole.RoleId;
        var seed = await MainForm.Instance.TabManager.TriggerAddTabPage("扫拍", $"https://www.idleinfinity.cn/Auction/Query?id={roleid}", "ah");
        _broSeed = seed;
        return MainForm.Instance.TabManager.BroDic[seed];
    }

    private async Task AutoBuy()
    {
        await Task.Delay(1500);
        var map = await getEqMap();
        foreach (var item in map.Values)
        {
            if (CanBuy(item))
            {
                await Buy(item.Eid);
                await AwaitJsInit();
                P.Log($@"购买到:【{item.ETitle}】,价格:{item.ToPriceStr()}", emLogType.AhScan);
            }
        }



    }
    /// <summary>
    /// 自动跳转到对应选项
    /// </summary>
    /// <returns></returns>
    private async Task AutoJump()
    {
        var node = GetCurNode();
        var isJumpEnd = await IsJumpToEnd(node);
        if (!isJumpEnd)
        {
            await JumpTo(node);
        }
    }

    private async Task AutoNextPage()
    {
        var node = GetCurNode();
        var isJumpEnd = await IsJumpToEnd(node);
        var isLast = await IsLastPage();
        if (!isLast && isJumpEnd)
        {
            await NextPage();
        }
    }

    public void StopScan()
    {
        IsStart = false;
        EventManager.Instance.UnsubscribeEvent(emEventType.OnJsInited, OnAhJsInited);
        Task.Run(() => {
            Thread.Sleep(60000);
            MainForm.Instance.TabManager.DisposePage(_broSeed);
        });
    }

    private void CalEqLogicPrice(AHItemModel eq)
    {
        var map = RuneLogicPriceCfg.Instance.data;
        for (int i = 0; i < eq.RunePriceArr.Length; i++)
        {
            var logicPrice = map[eq.RunePriceArr[i]] * eq.RuneCountArr[i];
            eq.LogicPrice += logicPrice;
        }
    }




    private Boolean CanBuy(AHItemModel item)
    {
        CalEqLogicPrice(item);
        var node = GetCurNode();
        foreach (var cfg in node.Configs)
        {
            if (cfg.Content == null) cfg.Content = new List<string>();
            if (item.ETitle != cfg.Name) return false;
            if (cfg.MinLv != 0 && item.Lv < cfg.MinLv) return false;
            if (!cfg.Content.All(p => item.Content.Contains(p))) return false;
            if (cfg.RegexList != null && !RegexUtil.Match(item.Content, cfg.RegexList)) return false;
            if (item.LogicPrice != 0 && item.LogicPrice <= cfg.Price) return true;//最后比较价格是否合适
            else
            {
                P.Log($@"太贵没买:【{item.ETitle}】,价格:{item.ToPriceStr()},地址:{_browser.Address}", emLogType.AhScan);
            }
        }
        return false;
    }

    private ScanAhTreeNode GetCurNode()
    {
        return ScanAhCfg.Instance.NodeList[CurIndex];
    }

    /// <summary>
    /// 一次跳转一个选项 
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public async Task<JavascriptResponse> JumpTo(ScanAhTreeNode node)
    {
        await Task.Delay(1500);
        if (_browser.CanExecuteJavascriptInMainFrame)
        {
            var data = node.ToLowerCamelCase();
            var d = await _browser.EvaluateScriptAsync($@"ah.jumpTo({data});");
            return d;
        }
        return new JavascriptResponse();

    }

    /// <summary>
    /// 是否跳转到最后对应选项
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public async Task<bool> IsJumpToEnd(ScanAhTreeNode node)
    {
        if (_browser.CanExecuteJavascriptInMainFrame)
        {
            var d = await _browser.EvaluateScriptAsync($@"ah.isJumpToEnd({node.ToLowerCamelCase()});");
            return d.Result?.ToString() == "success";
        }
        return false;

    }

    private async Task<bool> IsLastPage()
    {
        if (_browser.CanExecuteJavascriptInMainFrame)
        {
            var d = await _browser.EvaluateScriptAsync($@"ah.isLastPage();");
            return bool.Parse(d.Result?.ToString());
        }
        else return false;
    }

    private async Task NextPage()
    {
        await Task.Delay(1500);
        if (_browser.CanExecuteJavascriptInMainFrame)
        {
            var d = await _browser.EvaluateScriptAsync($@"ah.nextPage();");
        }
    }
    private async Task<Dictionary<int, AHItemModel>> getEqMap()
    {
        if (_browser.CanExecuteJavascriptInMainFrame)
        {
            var d = await _browser.EvaluateScriptAsync($@"ah.getEqMap();");
            return d.Result.ToObject<Dictionary<int, AHItemModel>>();
        }
        return null;
    }

    private async Task<JavascriptResponse> Buy(int eid)
    {
        await Task.Delay(3000);
        if (_browser.CanExecuteJavascriptInMainFrame)
        {
            var d = await _browser.EvaluateScriptAsync($@"ah.buy({eid});");
            return d;
        }
        return null;
    }

    private void OnAhJsInited(params object[] args)
    {
        string jsName = args[0] as string;
        if (jsName == "ah")
        {
            onJsInitCallBack?.Invoke(true);
            onJsInitCallBack = null;
        }
    }

    private async Task AwaitJsInit()
    {
        var tcs2 = new TaskCompletionSource<bool>();
        onJsInitCallBack = (result) => tcs2.SetResult(result);
        await tcs2.Task;
    }

}
