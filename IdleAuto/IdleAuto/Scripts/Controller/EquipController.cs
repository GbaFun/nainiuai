using CefSharp;
using System;
using System.Collections.Generic;
using System.Data;
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
        MainForm.Instance.ShowLoadingPanel("开始自动修车", emMaskType.AUTO_EQUIPING);
        EventManager.Instance.SubscribeEvent(emEventType.OnJsInited, OnEquipJsInited);
        //角色背包装备缓存
        Dictionary<long, EquipModel> packageEquips = new Dictionary<long, EquipModel>();
        //账号仓库装备缓存
        Dictionary<long, EquipModel> repositoryEquips = new Dictionary<long, EquipModel>();
        bool isInitRepository = false;

        //int i = 0;
        for (int i = 0; i < AccountController.Instance.User.Roles.Count; i++)
        {
            RoleModel role = AccountController.Instance.User.Roles[i];
            MainForm.Instance.browser.Load($"https://www.idleinfinity.cn/Equipment/Query?id={role.RoleId}");

            //等待页面跳转并加载js
            var tcs = new TaskCompletionSource<bool>();
            onJsInitCallBack = (result) => tcs.SetResult(result);
            await tcs.Task;

            P.Log("开始缓存装备", emLogType.AutoEquip);
            int page = 1;
            bool jumpNextPage = false;
            #region 缓存仓库装备
            MainForm.Instance.SetLoadContent($"正在缓存仓库的装备");
            if (!isInitRepository)
            {
                do
                {
                    jumpNextPage = false;
                    P.Log($"缓存仓库第{page}页装备", emLogType.AutoEquip);
                    var response1 = await GetRepositoryEquips();
                    if (response1.Success)
                    {
                        var equips = response1.Result.ToObject<Dictionary<long, EquipModel>>();
                        if (equips != null)
                        {
                            foreach (var item in equips)
                            {
                                if (!repositoryEquips.ContainsKey(item.Key))
                                    repositoryEquips.Add(item.Key, item.Value);
                            }
                        }
                    }

                    P.Log($"缓存仓库第{page}页装备完成,当前缓存仓库装备数量:{repositoryEquips.Count}", emLogType.AutoEquip);
                    MainForm.Instance.SetLoadContent($"正在缓存仓库的装备，当前已缓存数量{repositoryEquips.Count}");
                    P.Log("开始跳转仓库下一页", emLogType.AutoEquip);
                    var response2 = await JumpRepositoryPage();
                    if (response2.Success)
                    {
                        if ((bool)response2.Result)
                        {
                            P.Log("等待仓库切页完成", emLogType.AutoEquip);
                            var tcs2 = new TaskCompletionSource<bool>();
                            onJsInitCallBack = (result) => tcs2.SetResult(result);
                            await tcs2.Task;
                            P.Log("仓库切页完成");
                            page++;
                            jumpNextPage = true;
                            await Task.Delay(500);
                        }
                        else
                        {
                            P.Log("仓库最后一页了！", emLogType.AutoEquip);
                            jumpNextPage = false;
                        }
                    }
                } while (jumpNextPage);
                isInitRepository = true;
            }
            P.Log("缓存仓库完成！！");
            #endregion
            #region 缓存背包装备
            MainForm.Instance.SetLoadContent($"正在缓存背包的装备");
            packageEquips.Clear();
            page = 1;
            jumpNextPage = false;
            do
            {
                jumpNextPage = false;
                P.Log($"缓存背包第{page}页装备", emLogType.AutoEquip);
                var response1 = await GetPackageEquips();
                if (response1.Success)
                {
                    var equips = response1.Result.ToObject<Dictionary<long, EquipModel>>();
                    if (equips != null)
                    {
                        foreach (var item in equips)
                        {
                            if (!packageEquips.ContainsKey(item.Key))
                                packageEquips.Add(item.Key, item.Value);
                        }
                    }
                }

                P.Log($"缓存背包第{page}页装备完成,当前缓存背包装备数量:{packageEquips.Count}", emLogType.AutoEquip);
                MainForm.Instance.SetLoadContent($"正在缓存背包的装备，当前已缓存数量{packageEquips.Count}");
                P.Log("开始跳转背包下一页", emLogType.AutoEquip);
                var response2 = await JumpPackagePage();
                if (response2.Success)
                {
                    if ((bool)response2.Result)
                    {
                        P.Log("等待背包切页完成", emLogType.AutoEquip);
                        var tcs2 = new TaskCompletionSource<bool>();
                        onJsInitCallBack = (result) => tcs2.SetResult(result);
                        await tcs2.Task;
                        P.Log("背包切页完成", emLogType.AutoEquip);
                        page++;
                        jumpNextPage = true;
                        await Task.Delay(500);
                    }
                    else
                    {
                        P.Log("背包最后一页了！", emLogType.AutoEquip);
                        jumpNextPage = false;
                    }
                }
            } while (jumpNextPage);
            P.Log("缓存背包完成！！", emLogType.AutoEquip);

            #endregion

            #region 检查角色装备
            MainForm.Instance.SetLoadContent($"正在检查{role.RoleName}的装备");

            Dictionary<emEquipType, EquipModel> curEquips = null;
            var response = await GetCurEquips();
            if (response.Success)
            {
                curEquips = response.Result.ToObject<Dictionary<emEquipType, EquipModel>>();
                var targetEquip = EquipCfg.Instance.GetEquipmentByJobAndLevel(role.Job, role.Level);
                if (targetEquip == null)
                {
                    P.Log($"未找到{role.Level}级{role.Job}的装备配置,无法更换", emLogType.AutoEquip);
                }
                else
                {
                    //逐个检查装备部位的装备是否符合要求
                    for (int j = 0; j < 11; j++)
                    {
                        //每个部位检查装备前增加500ms得等待时间
                        await Task.Delay(500);
                        string targetEquipName = targetEquip.GetEquipByType((emEquipType)j);
                        if (curEquips.TryGetValue((emEquipType)j, out EquipModel equip))
                        {
                            if (string.IsNullOrEmpty(targetEquipName) || equip.equipName.Contains(targetEquipName))
                            {
                                P.Log($"{role.RoleName}的{equip.etypeName}位置装备{equip.equipName}符合要求，无需更换", emLogType.AutoEquip);
                                continue;
                            }
                        }
                        foreach (var item in packageEquips)
                        {
                            if (item.Value.equipName.Contains(targetEquipName))
                            {
                                if (j == (int)emEquipType.副手 || j == (int)emEquipType.戒指1 || j == (int)emEquipType.戒指2)
                                {
                                    P.Log($"{(emEquipType)j}部位当前已穿戴装备，为防止穿戴时部位冲突导致换装失败，优先卸下当前部位装备", emLogType.AutoEquip);
                                    var response3 = await EquipOff(role, j);
                                    if (response3.Success)
                                    {
                                        P.Log($"等待卸下装备消息返回", emLogType.AutoEquip);
                                        var tcs2 = new TaskCompletionSource<bool>();
                                        onJsInitCallBack = (result) => tcs2.SetResult(result);
                                        await tcs2.Task;
                                    }
                                }

                                P.Log($"找到{role.Level}级{role.RoleName}的符合条件的装备{targetEquipName}，现在更换", emLogType.AutoEquip);
                                var response2 = await EquipOn(role, item.Value);
                                if (response2.Success)
                                {
                                    packageEquips.Remove(item.Key);
                                    P.Log($"等待更换装备消息返回", emLogType.AutoEquip);
                                    var tcs2 = new TaskCompletionSource<bool>();
                                    onJsInitCallBack = (result) => tcs2.SetResult(result);
                                    await tcs2.Task;
                                    goto WEAR_EQUIP_SUCCESS;
                                }
                            }
                        }
                        foreach (var item in repositoryEquips)
                        {
                            if (item.Value.equipName.Contains(targetEquipName))
                            {
                                if (j == (int)emEquipType.副手 || j == (int)emEquipType.戒指1 || j == (int)emEquipType.戒指2)
                                {
                                    P.Log($"{(emEquipType)j}部位当前已穿戴装备，为防止穿戴时部位冲突导致换装失败，优先卸下当前部位装备", emLogType.AutoEquip);
                                    var response3 = await EquipOff(role, j);
                                    if (response3.Success)
                                    {
                                        P.Log($"等待卸下装备消息返回", emLogType.AutoEquip);
                                        var tcs2 = new TaskCompletionSource<bool>();
                                        onJsInitCallBack = (result) => tcs2.SetResult(result);
                                        await tcs2.Task;
                                    }
                                }

                                P.Log($"找到{role.Level}级{role.RoleName}的符合条件的装备{targetEquipName}，现在更换", emLogType.AutoEquip);
                                var response2 = await EquipOn(role, item.Value);
                                if (response2.Success)
                                {
                                    repositoryEquips.Remove(item.Key);
                                    P.Log($"等待更换装备消息返回", emLogType.AutoEquip);
                                    var tcs2 = new TaskCompletionSource<bool>();
                                    onJsInitCallBack = (result) => tcs2.SetResult(result);
                                    await tcs2.Task;
                                    goto WEAR_EQUIP_SUCCESS;
                                }
                            }
                        }

                        WEAR_EQUIP_SUCCESS:
                        P.Log($"{role.RoleName}更换{targetEquipName}装备完成", emLogType.AutoEquip);
                    }
                }
                P.Log($"{role.RoleName}全部位置装备更换完成\n\t\n\t\n\t", emLogType.AutoEquip);
                MainForm.Instance.browser.Load("https://www.idleinfinity.cn/Home/Index");
            }
            #endregion
        }
        P.Log($"全部角色装备更换完成\n\t\n\t\n\t\n\t\n\t", emLogType.AutoEquip);
        EventManager.Instance.UnsubscribeEvent(emEventType.OnJsInited, OnEquipJsInited);
        MainForm.Instance.HideLoadingPanel(emMaskType.AUTO_EQUIPING);
    }

    public async Task<JavascriptResponse> GetCurEquips()
    {
        return await MainForm.Instance.browser.EvaluateScriptAsync($@"getCurEquips()");
    }
    public async Task<JavascriptResponse> GetPackageEquips()
    {
        return await MainForm.Instance.browser.EvaluateScriptAsync($@"getPackageEquips()");
    }
    public async Task<JavascriptResponse> GetRepositoryEquips()
    {
        return await MainForm.Instance.browser.EvaluateScriptAsync($@"getRepositoryEquips()");
    }
    public async Task<JavascriptResponse> JumpRepositoryPage()
    {
        return await MainForm.Instance.browser.EvaluateScriptAsync($@"repositoryNext()");
    }
    public async Task<JavascriptResponse> JumpPackagePage()
    {
        return await MainForm.Instance.browser.EvaluateScriptAsync($@"packageNext()");
    }
    public async Task<JavascriptResponse> EquipOn(RoleModel role, EquipModel equip)
    {
        return await MainForm.Instance.browser.EvaluateScriptAsync($@"equipOn({role.RoleId},{equip.eid})");
    }
    public async Task<JavascriptResponse> EquipOff(RoleModel role, int etype)
    {
        return await MainForm.Instance.browser.EvaluateScriptAsync($@"equipOff({role.RoleId},{etype})");
    }

    private void OnEquipJsInited(params object[] args)
    {
        string jsName = args[0] as string;
        if (jsName == "equip")
        {
            onJsInitCallBack?.Invoke(true);
            onJsInitCallBack = null;
        }
    }
}

