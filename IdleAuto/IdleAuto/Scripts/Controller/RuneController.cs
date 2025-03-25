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
    }

    public async Task<bool> UpgradeRune(BroWindow win, UserModel account, Dictionary<int, int> runeMap)
    {
        bool isSuccess = true;
        P.Log("开始升级符文", emLogType.RuneUpgrate);
        await Task.Delay(1000);
        P.Log("开始跳转材料页面", emLogType.RuneUpgrate);
        await win.LoadUrlWaitJsInit(IdleUrlHelper.MaterialUrl(account.FirstRole.RoleId), "rune");

        P.Log("开始整理升级所需符文", emLogType.RuneUpgrate);
        List<int> runeIds = runeMap.Keys.ToList();
        int maxRune = runeIds.Max();
        bool runeEnough = true;
        Dictionary<int, int> toUpgrade = new Dictionary<int, int>();
        for (int i = maxRune; i > 0; i--)
        {
            int runeId = i;
            int needNum = 0;
            if (runeMap.TryGetValue(runeId, out int num1))
                needNum += num1;
            if (toUpgrade.TryGetValue(runeId, out int num2))
                needNum += num2;
            if (needNum <= 0) continue;
            var response = await win.CallJs($@"getRuneNum({runeId})");
            if (response.Success)
            {
                int curNum = (int)response.Result;
                if (curNum < needNum)
                {
                    if (runeId > 1)
                    {
                        int needUpNum = (needNum - curNum) * 2;
                        toUpgrade.Add(runeId - 1, needUpNum);
                    }
                    else
                    {
                        P.Log($"符文数量不足，无法升级", emLogType.RuneUpgrate);
                        runeEnough = false;
                        break;
                    }
                }
            }
        }

        P.Log("开始升级符文", emLogType.RuneUpgrate);
        if (runeEnough)
        {
            List<int> toUpIds = toUpgrade.Keys.ToList();
            toUpIds.Sort((a, b) =>
            {
                return a.CompareTo(b);
            });
            for (int i = 0; i < toUpIds.Count; i++)
            {
                var response = await win.CallJs($@"getRuneNum({toUpIds[i]})");
                if (response.Success)
                {
                    int curNum = (int)response.Result;
                    if (curNum >= toUpgrade[toUpIds[i]])
                    {
                        await Task.Delay(1000);
                        var response2 = await win.CallJsWaitReload($@"upgradeRune({toUpIds[i]},{toUpgrade[toUpIds[i]]})", "rune");
                        if (response2.Success == true)
                        {
                            P.Log($"升级符文{toUpIds[i]}（{toUpgrade[toUpIds[i]]}个）成功", emLogType.RuneUpgrate);
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        MessageBox.Show($"符文{toUpIds[i]}数量不足，无法升级");
                        break;
                    }
                }
            }
        }
        return isSuccess;
    }
}

