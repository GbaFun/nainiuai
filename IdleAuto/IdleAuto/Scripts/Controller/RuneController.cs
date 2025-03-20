using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FreeSql;
using FreeSql.Sqlite;
using IdleAuto.Scripts.Wrap;
using IdleAuto.Db;

public class RuneController
{
    private EventSystem EventSystem;
    public RuneController()
    {
        EventSystem = new EventSystem();
    }

    private delegate void OnUpgradeRuneBack(bool result);
    private OnUpgradeRuneBack onUpgradeRuneCallBack;
    private delegate void OnJsInitCallBack(bool result);
    private OnJsInitCallBack onJsInitCallBack;

    public async Task AutoUpgradeRune(BroWindow win, UserModel account)
    {
        long start = DateTime.Now.Ticks;
        P.Log("开始一键升级符文", emLogType.RuneUpgrate);

        await Task.Delay(1000);
        await win.LoadUrlWaitJsInit(IdleUrlHelper.MaterialUrl(account.FirstRole.RoleId), "rune");

        EventSystem.SubscribeEvent(emEventType.OnUpgradeRuneBack, OnEventUpgradeRuneBack);
        EventSystem.SubscribeEvent(emEventType.OnJsInited, OnRuneJsInited);
        var runeDb = FreeDb.Sqlite.Select<RuneCompandData>().ToList();
        runeDb.Sort((a, b) => a.ID.CompareTo(b.ID));
        foreach (var item in runeDb)
        {
            P.Log($"开始检查{item.ID}#符文", emLogType.RuneUpgrate);
            long duration = (DateTime.Now.Ticks - start) / 10000;
            //如果配置保留数量为-1，则不处理
            if (item.CompandNum == -1)
            {
                P.Log($"{item.ID}#符文配置保留数量为无限，无需升级", emLogType.RuneUpgrate);
                continue;
            }
            var response = await win.CallJs($@"getRuneNum({item.ID})");
            if (response.Success)
            {
                int curNum = (int)response.Result;
                if (curNum >= item.CompandNum)
                {
                    int count = curNum - item.CompandNum;
                    count = count - count % 2;
                    if (count < 2)
                    {
                        P.Log($"{item.ID}#符文空余数量不足2，无需升级", emLogType.RuneUpgrate);
                        continue;
                    }
                    await Task.Delay(1000);
                    P.Log($"开始升级{item.ID}#符文，升级数量{count}", emLogType.RuneUpgrate);
                    var response2 = await win.CallJsWaitReload($@"upgradeRune({item.ID},{count})", "rune");
                    if (response2.Success == false)
                    {
                        MessageBox.Show($"自动升级符文失败，详情请查看log文件({Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", emLogType.RuneUpgrate.ToString())})");
                        break;
                    }
                }
            }
        }

        P.Log($"全部符文升级完成\n\t\n\t\n\t\n\t\n\t", emLogType.RuneUpgrate);
        EventSystem.UnsubscribeEvent(emEventType.OnUpgradeRuneBack, OnEventUpgradeRuneBack);
        EventSystem.UnsubscribeEvent(emEventType.OnJsInited, OnRuneJsInited);
    }

    private void OnEventUpgradeRuneBack(params object[] args)
    {
        bool isSuccess = (bool)args[0];
        int runeId = (int)args[1];
        int runeNum = (int)args[2];

        if (!isSuccess)
        {
            string msg = args[3] as string;
            P.Log($"{runeId}#符文升级失败，升级数量{runeNum}，失败原因{msg}", emLogType.RuneUpgrate);
        }
        else
        {
            P.Log($"{runeId}#符文升级成功，升级数量{runeNum}", emLogType.RuneUpgrate);
        }

        onUpgradeRuneCallBack?.Invoke(true);
        onUpgradeRuneCallBack = null;
    }

    private void OnRuneJsInited(params object[] args)
    {
        string jsName = args[0] as string;
        if (jsName == "rune")
        {
            onJsInitCallBack?.Invoke(true);
            onJsInitCallBack = null;
        }
    }
}

