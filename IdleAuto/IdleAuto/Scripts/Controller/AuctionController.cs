using CefSharp;
using IdleAuto.Configs.CfgExtension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class AuctionController
{
    public AuctionController()
    {
        MainForm.Instance.browser.FrameLoadEnd += OnPageLoadEnd;
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
    /// 监听页面刷新状态
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnPageLoadEnd(object sender, FrameLoadEndEventArgs e)
    {
        if (IsStart)
        {
            await AutoJump();
            await AutoNextPage();
        }
    }
    /// <summary>
    /// 开始扫拍
    /// </summary>
    public async void StartScan()
    {
        IsStart = true;
        await AutoJump();
        await AutoNextPage();
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
        if (!isLast&&isJumpEnd)
        {
            await NextPage();
        }
    }

    public void StopScan()
    {
        IsStart = false;
    }

    /// <summary>
    /// 当前扫描的配置
    /// </summary>
    /// <returns></returns>
    private DemandEquip GetCurConfig()
    {
        return ScanAhCfg.Instance.data[EqIndex];
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
        return d.Result?.ToString()=="success";

    }

    private async Task<bool> IsLastPage()
    {
        if (MainForm.Instance.browser.CanExecuteJavascriptInMainFrame)
        {
            var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"ah.isLastPage();");
            return bool.Parse( d.Result?.ToString());
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
