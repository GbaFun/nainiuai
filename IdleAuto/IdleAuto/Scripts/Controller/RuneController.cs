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

    public async Task UpgradeRune(BroWindow win, UserModel account, Dictionary<int, int> runeMap)
    {
        P.Log("开始升级符文", emLogType.RuneUpgrate);
        await Task.Delay(1000);
        P.Log("开始跳转材料页面", emLogType.RuneUpgrate);
        await win.LoadUrlWaitJsInit(IdleUrlHelper.MaterialUrl(account.FirstRole.RoleId), "rune");

        P.Log("开始整理所需符文", emLogType.RuneUpgrate);
        List<int> runeIds = runeMap.Keys.ToList();
        runeIds.Sort((a, b) =>
        {
            return b.CompareTo(a);
        });

        Dictionary<int, int> toUpgrade = new Dictionary<int, int>();
        for (int i = 0; i < runeIds.Count; i++)
        {
            int runeId = runeIds[i];
            int needNum = runeMap[runeId];
            if (toUpgrade.TryGetValue(runeId, out int num))
                needNum += num;
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
                }
            }
        }

        P.Log("开始升级符文", emLogType.RuneUpgrate);
        //await Task.Delay(1000);
        //await win.CallJsWaitReload($@"upgradeRune({runeId},{runeNum})", "rune");
    }

    //Account item = this.AccountCombo.SelectedItem as Account;
    //AccountController.Instance.User = new UserModel(item);
    //var user = AccountController.Instance.User;
    //var window = await TabManager.Instance.TriggerAddBroToTap(user);
    //RuneController controller = new RuneController();
    //await controller.UpgradeRune(window, user, new Dictionary<int, int>()
    //{
    //    { 10, 10 },
    //    { 11, 10 },
    //    { 12, 10 },
    //});

    //public async Task<bool> CheckRuneNum(BroWindow win, int runeId, int needNum, out Dictionary<int, int> toUpgrade)
    //{
    //    toUpgrade = new Dictionary<int, int>();
    //    var response = await win.CallJs($@"getRuneNum({runeId})");
    //    if (response.Success)
    //    {
    //        int curNum = (int)response.Result;
    //        if (curNum >= needNum)
    //        {
    //            P.Log($"符文{runeId}数量足够，需要{needNum}个，当前{curNum}个", emLogType.RuneUpgrate);
    //            toUpgrade.Add(runeId, needNum - curNum);
    //            return true;
    //        }
    //        else
    //        {
    //            int needUpNum = (needNum - curNum) * 2;
    //            if (runeId - 1 >= 1)
    //            {
    //                var response2 = await win.CallJs($@"getRuneNum({runeId - 1})");
    //                if (response2.Success)
    //                {
    //                    int curNum2 = (int)response2.Result;
    //                    if (curNum2 >= needUpNum)
    //                    {
    //                        P.Log($"符文{runeId}数量不足，需要{needNum}个，当前{curNum}个", emLogType.RuneUpgrate);
    //                        toUpgrade.Add(runeId - 1, needNum - curNum2);
    //                        return true;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
}

