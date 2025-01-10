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
    /// 当前扫描到配置中的位置
    /// </summary>
    public int EqIndex = 0;


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

    }

    private async Task AutoBuy()
    {
        List<AHItemModel> matchList = new List<AHItemModel>();
        var map = await getEqMap();
        foreach (var item in map.Values)
        {
           if(CanBuy(item))
            {
                matchList.Add(item);
            }
        }
        CalEqLogicPrice(map.FirstOrDefault().Value);
    }
    /// <summary>
    /// 自动跳转到对应选项
    /// </summary>
    /// <returns></returns>
    private async Task AutoJump()
    {
        var curCfg = GetCurConfig();
        var isJumpEnd = await IsJumpToEnd(curCfg);
        if (!isJumpEnd)
        {
            await JumpTo(curCfg);
        }
    }

    private async Task AutoNextPage()
    {
        var isJumpEnd = await IsJumpToEnd(GetCurConfig());
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


    /// <summary>
    /// 当前扫描的配置
    /// </summary>
    /// <returns></returns>
    private DemandEquip GetCurConfig()
    {
        return ScanAhCfg.Instance.data[EqIndex];
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
        var cfg = GetCurConfig();
        if (item.eTitle != cfg.name) return false;
        if (cfg.minLv != 0 && item.lv < cfg.minLv) return false;
        if (cfg.content.Count == 0) return true;
        if (!cfg.content.All(p => item.content.Contains(p))) return false;
        if (!RegexUtil.Match(item.content, cfg.regexList)) return false;
        return true;
    }

    /// <summary>
    /// 一次跳转一个选项 
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public async Task<JavascriptResponse> JumpTo(DemandEquip config)
    {
        Thread.Sleep(1500);
        var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"ah.jumpTo({JsonConvert.SerializeObject(config)});");
        return d;

    }

    /// <summary>
    /// 是否跳转到最后对应选项
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public async Task<bool> IsJumpToEnd(DemandEquip config)
    {
        var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"ah.isJumpToEnd({JsonConvert.SerializeObject(config)});");
        return d.Result?.ToString() == "success";

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
        Thread.Sleep(1500);
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
    public static List<AHItemModel> BuyEquip(ExpandoObject data)
    {
        var d = data.ToObject<Dictionary<int, AHItemModel>>();
        foreach (var item in d)
        {
            item.Value.eid = item.Key;
        }

        return d.Values.ToList();

    }
}
