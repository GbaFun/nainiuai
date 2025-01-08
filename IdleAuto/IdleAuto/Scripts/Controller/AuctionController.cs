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
        EventManager.Instance.SubscribeEvent(emEventType.OnAhLastPageLoaded, OnLastPageLoad);
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
    /// 是否跳转到对应的下拉框
    /// </summary>
    public bool IsJumpEnd = false;

    /// <summary>
    /// 当前扫描到配置中的位置
    /// </summary>
    public int EqIndex = 0;


    private void OnLastPageLoad(params object[] args)
    {

    }

    public async void AutoScanAh()
    {
        if (!IsStart) return;
        Console.WriteLine($"{DateTime.Now}---开始一键扫货");
        List<DemandEquip> cfg = ScanAhCfg.Instance.data;
        var curCfg = cfg[EqIndex];
        do
        {
            Thread.Sleep(2500);
            var d = await JumpTo(curCfg);
            if (d.Result?.ToString() == "success")
            {
                IsJumpEnd = true;
            }

        } while (!IsJumpEnd);




    }
    public async Task<JavascriptResponse> JumpTo(DemandEquip config)
    {
        if (MainForm.Instance.browser.CanExecuteJavascriptInMainFrame)
        {
            var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"ah.jumpTo({JsonConvert.SerializeObject(config)});");
            return d;
        }
        else return new JavascriptResponse();
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
