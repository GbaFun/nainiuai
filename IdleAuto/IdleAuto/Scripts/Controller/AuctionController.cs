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

public class AuctionController
{
    public AuctionController()
    {
        EventManager.Instance.SubscribeEvent(emEventType.OnScanAh, OnScanAh);
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
        await StartAutoJob();
    }

    /// <summary>
    /// 开始自动执行
    /// </summary>
    private async Task StartAutoJob()
    {
        await AutoBuy();
        await AutoJump();
        await AutoNextPage();
        var isLastPage = await IsLastPage();
        var isJumpEnd = await IsJumpToEnd(GetCurNode());
        if (isLastPage&& isJumpEnd)
        {
            if (CurIndex == ScanAhCfg.Instance.NodeList.Count - 1) CurIndex = 0;
            else CurIndex++;
            await AutoJump();
        }

    }

    private async Task AutoBuy()
    {
        await Task.Delay(1500);
        var map = await getEqMap();
        foreach (var item in map.Values)
        {
            if (CanBuy(item))
            {
                await Buy(item.eid);
                P.Log($@"购买到:【{item.eTitle}】,价格:{item.ToPriceStr()}", emLogType.AhScan);
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
    }

    private void CalEqLogicPrice(AHItemModel eq)
    {
        var map = RuneLogicPriceCfg.Instance.data;
        for (int i = 0; i < eq.runePriceArr.Length; i++)
        {
            var logicPrice = map[eq.runePriceArr[i]] * eq.runeCountArr[i];
            eq.logicPrice += logicPrice;
        }
    }



    private async void OnScanAh(params object[] args)
    {

        if (IsStart)
        {
            await StartAutoJob();
        }
    }

    private Boolean CanBuy(AHItemModel item)
    {
        CalEqLogicPrice(item);
        var node = GetCurNode();
        foreach (var cfg in node.Configs)
        {
            if (cfg.content == null) cfg.content = new List<string>();
            if (item.eTitle != cfg.name) return false;
            if (cfg.minLv != 0 && item.lv < cfg.minLv) return false;
            if (!cfg.content.All(p => item.content.Contains(p))) return false;
            if (cfg.regexList!=null&& !RegexUtil.Match(item.content, cfg.regexList)) return false;
            if (item.logicPrice!=0&&item.logicPrice <= cfg.price) return true;//最后比较价格是否合适
            else
            {
                P.Log($@"太贵没买:【{item.eTitle}】,价格:{item.ToPriceStr()},地址:{MainForm.Instance.browser.Address}", emLogType.AhScan);
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
        if (MainForm.Instance.browser.CanExecuteJavascriptInMainFrame)
        {
            var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"ah.jumpTo({JsonConvert.SerializeObject(node)});");
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
        if (MainForm.Instance.browser.CanExecuteJavascriptInMainFrame)
        {
            var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"ah.isJumpToEnd({JsonConvert.SerializeObject(node)});");
            return d.Result?.ToString() == "success";
        }
        return false;

    }

    private async Task<bool> IsLastPage()
    {
        if (MainForm.Instance.browser.CanExecuteJavascriptInMainFrame)
        {
            var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"ah.isLastPage();");
            return bool.Parse(d.Result?.ToString());
        }
        else return false;
    }

    private async Task NextPage()
    {
        await Task.Delay(1500);
        if (MainForm.Instance.browser.CanExecuteJavascriptInMainFrame)
        {
            var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"ah.nextPage();");
        }
    }
    private async Task<Dictionary<int, AHItemModel>> getEqMap()
    {
        if (MainForm.Instance.browser.CanExecuteJavascriptInMainFrame)
        {
            var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"ah.getEqMap();");
            return d.Result.ToObject<Dictionary<int, AHItemModel>>();
        }
        return null;
    }

    private async Task<JavascriptResponse> Buy(int eid)
    {
        await Task.Delay(3000);
        if (MainForm.Instance.browser.CanExecuteJavascriptInMainFrame)
        {
            var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"ah.buy({eid});");
            return d;
        }
        return null;
    }

}
