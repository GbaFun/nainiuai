using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

public class RuneController
{
    private static RuneController instance;
    public static RuneController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new RuneController();
            }
            return instance;
        }
    }

    private delegate void OnUpgradeRuneBack(bool result);
    private OnUpgradeRuneBack onUpgradeRuneCallBack;
    public async void AutoUpgradeRune()
    {
        //Console.WriteLine($"{DateTime.Now}---开始一键升级符文");
        long start = DateTime.Now.Ticks;
        MainForm.Instance.ShowLoadingPanel("开始一键升级符文");
        EventManager.Instance.SubscribeEvent(emEventType.OnUpgradeRuneBack, OnEventUpgradeRuneBack);
        //MainForm.Instance.browser.FrameLoadEnd += OnMainFormBrowseFrameLoad;
        List<RuneCompandData> cfg = RuneCompandCfg.Instance.RuneCompandData;
        foreach (var item in cfg)
        {
            long duration = (DateTime.Now.Ticks - start) / 10000;
            MainForm.Instance.SetLoadContent($"当前升级符文：{item.ID}#       耗时：{duration}ms");
            //如果配置保留数量为-1，则不处理
            if (item.CompandNum == -1)
            {
                MainForm.Instance.SetLoadContent($"{DateTime.Now}---升级符文：{item.ID}#--跳过");
                await Task.Delay(500);
                continue;
            }
            var response = await GetRuneNum(item.ID);
            if (response.Success)
            {
                int curNum = (int)response.Result;
                if (curNum >= item.CompandNum)
                {
                    int count = curNum - item.CompandNum;
                    count = count - count % 2;
                    if (count < 2)
                    {
                        MainForm.Instance.SetLoadContent($"{DateTime.Now}---升级符文：{item.ID}#--跳过");
                        await Task.Delay(500);
                        continue;
                    }
                    UpgradeRune(item.ID, count);
                    var tcs = new TaskCompletionSource<bool>();
                    onUpgradeRuneCallBack = (result) => { tcs.SetResult(result); onUpgradeRuneCallBack = null; };
                    await tcs.Task;
                    if (tcs.Task.Result == false)
                    {
                        break;
                    }
                }
            }
        }

        //MainForm.Instance.browser.FrameLoadEnd -= OnMainFormBrowseFrameLoad;
        EventManager.Instance.UnsubscribeEvent(emEventType.OnUpgradeRuneBack, OnEventUpgradeRuneBack);
        MainForm.Instance.HideLoadingPanel();
    }

    private void OnEventUpgradeRuneBack(params object[] args)
    {
        bool isSuccess = (bool)args[0];
        int runeId = (int)args[1];
        int runeNum = (int)args[2];

        Console.WriteLine($"{DateTime.Now}---升级符文：{runeId}#-{runeNum}--{isSuccess}");
        if (onUpgradeRuneCallBack != null)
            onUpgradeRuneCallBack(true);
        //if (!tcs.Task.IsCompleted)
        //{
        //    onUpgradeRuneCallBack(true);
        //}
    }
    //public void OnMainFormBrowseFrameLoad(object sender, FrameLoadEndEventArgs e)
    //{
    //    var bro = sender as ChromiumWebBrowser;
    //    string url = bro.Address;
    //    onUpgradeRuneCallBack(PageLoadHandler.ContainsUrl(url, PageLoadHandler.MaterialPage));
    //}

    public void OnGetJsRuneData()
    {
        //var runeData = data.ToObject<List<RuneModel>>();
        //RuneCompandCfg.Instance.RuneCompandData = runeData;
    }

    public async Task<JavascriptResponse> GetRuneNum(int runeId)
    {
        return await MainForm.Instance.browser.EvaluateScriptAsync($@"getRuneNum({runeId})");
    }
    public void UpgradeRune(int runeId, int count)
    {
        MainForm.Instance.browser.ExecuteScriptAsync($@"upgradeRune({runeId},{count})");
    }
}

