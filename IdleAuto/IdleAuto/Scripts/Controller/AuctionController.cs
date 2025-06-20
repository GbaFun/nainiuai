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
using IdleAuto.Scripts.Wrap;

public class AuctionController : BaseController
{




    public AuctionController(BroWindow win) : base(win)
    {
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
    public async Task StartScan(RoleModel role)
    {
        IsStart = true;
        await _win.LoadUrlWaitJsInit($"https://www.idleinfinity.cn/Auction/Query?id={role.RoleId}", "ah");
        await Task.Delay(1000);
        await StartAutoJob();
    }

    /// <summary>
    /// 开始自动执行
    /// </summary>
    private async Task StartAutoJob()
    {


        await AutoJump();
        await AutoNextPage();
        var isLastPage = await IsLastPage();
        var isJumpEnd = await IsJumpToEnd(GetCurNode());
        if (isLastPage && isJumpEnd)
        {
            if (CurIndex == ScanAhCfg.Instance.NodeList.Count - 1) CurIndex = 0;
            else
            {
                CurIndex++;
                await StartAutoJob();
            }

        }


    }




    private async Task AutoBuy()
    {
        var map = await getEqMap();
        foreach (var item in map.Values)
        {
            if (CanBuy(item))
            {
                await Buy(item.Eid);

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
            await AutoJump();
        }
        else
        {
            await AutoBuy();
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
            await AutoBuy();
            await AutoNextPage();
        }
    }

    public void StopScan()
    {
        IsStart = false;
        //  EventSystem.Instance.UnsubscribeEvent(emEventType.OnJsInited, OnAhJsInited);
        Task.Run(() =>
        {
            Thread.Sleep(60000);
            BroTabManager.Instance.DisposePage(_broSeed);
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
    public async Task JumpTo(ScanAhTreeNode node)
    {

        var data = node.ToLowerCamelCase();
        var d = await _win.CallJsWaitReload($@"ah.jumpTo({data});", "ah");


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

        var d = await _win.CallJsWaitReload($@"ah.nextPage();", "ah");


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

    private async Task Buy(int eid)
    {


        var d = await _win.CallJsWaitReload($@"ah.buy({eid});", "ah");


    }






}
