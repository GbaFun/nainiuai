using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EquipController
{
    private static EquipController instance;
    public static EquipController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new EquipController();
            }
            return instance;
        }
    }

    private delegate void OnJsInitCallBack(bool result);
    private OnJsInitCallBack onJsInitCallBack;
    public void StartAutoEquip()
    {
        AutoEquip();
    }
    private async void AutoEquip()
    {
        MainForm.Instance.ShowLoadingPanel("开始自动修车");
        EventManager.Instance.SubscribeEvent(emEventType.OnJsInited, OnEquipJsInited);

        int i = 0;
        //for (int i = 0; i < AccountController.Instance.User.Roles.Count; i++)
        {
            RoleModel role = AccountController.Instance.User.Roles[i];
            MainForm.Instance.browser.Load($"https://www.idleinfinity.cn/Equipment/Query?id={role.RoleId}");

            var tcs = new TaskCompletionSource<bool>();
            onJsInitCallBack = (result) => tcs.SetResult(result);
            await tcs.Task;

            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "开始检查装备");
            MainForm.Instance.ShowLoadingPanel($"正在检查{role.RoleName}的装备");

            var response = await GetCurEquips();
            Dictionary<emEquipType, EquipModel> curEquips = null;
            if (response.Success)
            {
                curEquips = response.Result.ToObject<Dictionary<emEquipType, EquipModel>>();
                var targetEquip = EquipCfg.Instance.GetEquipmentByJobAndLevel(role.Job, role.Level);
                if (targetEquip == null)
                {
                    Console.WriteLine($"未找到{role.Level}级{role.Job}的装备配置");
                    return;
                }
                foreach (var item in curEquips)
                {
                    string curEquipName = item.Value.equipName;
                    string targetEquipName = targetEquip.GetEquipByType(item.Value.etype);
                    if (string.IsNullOrEmpty(targetEquipName))
                    {
                        Console.WriteLine($"未找到{role.Level}级{role.Job}的{item.Value.etype}装备配置");
                        continue;
                    }
                    Console.WriteLine($"当前装备{item.Key}：{curEquipName}，目标装备：{targetEquipName}");
                    if (string.IsNullOrEmpty(curEquipName) || !curEquipName.Contains(targetEquipName))
                    {
                        //Console.WriteLine($"开始更换{item.Key}装备");
                        //do
                        //{
                        //    检查当前背包页是否有目标装备
                        //    if (有)
                        //    {
                        //        更换装备
                        //        等待装备更换完成
                        //        goto EQUIPSUCCESS;
                        //    }
                        //} while (跳转下一页是否成功)
                        //Console.WriteLine($"未在背包找到匹配装备{targetEquipName}，无法更换");

                        //do
                        //{
                        //    检查当前仓库页是否有目标装备
                        //    if (有)
                        //    {
                        //        更换装备
                        //        等待装备更换完成
                        //        goto EQUIPSUCCESS;
                        //    }
                        //} while (跳转下一页是否成功)
                        //Console.WriteLine($"未在仓库找到匹配装备{targetEquipName}，无法更换");

                        //EQUIPSUCCESS:
                        Console.WriteLine($"{item.Key}位置更换{targetEquipName}装备完成");
                    }
                }
            }
            EventManager.Instance.UnsubscribeEvent(emEventType.OnJsInited, OnEquipJsInited);
        }
    }

    public async Task<JavascriptResponse> GetCurEquips()
    {
        return await MainForm.Instance.browser.EvaluateScriptAsync($@"getCurEquips()");
    }

    private void OnEquipJsInited(params object[] args)
    {
        string jsName = args[0] as string;
        if (jsName == "equip")
        {
            onJsInitCallBack.Invoke(true);
        }
    }
}

