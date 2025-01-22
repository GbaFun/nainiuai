using CefSharp;
using IdleAuto.Db;
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

    private int m_equipBroID = 0;


    public async void SaveAllEquips()
    {
        await StartSaveEquips();
    }
    public async Task StartSaveEquips()
    {
        var v = FreeDb.Sqlite.Select<CommonModel>().Where(P => P.CommonKey == "EquipSaveTime").ToList();
        if (v.Count > 0)
        {
            var time = Convert.ToDateTime(v[0].CommonValue);
            if (DateTime.Now.Subtract(time).TotalHours < 24)
            {
                P.Log("24小时内已经保存过装备，无需再次保存", emLogType.AutoEquip);
                return;
            }
        }

        FreeDb.Sqlite.Delete<EquipModel>().Where(p => true).ExecuteAffrows();
        MainForm.Instance.ShowLoadingPanel("开始盘点所有装备", emMaskType.AUTO_EQUIPING);
        EventManager.Instance.SubscribeEvent(emEventType.OnJsInited, OnEquipJsInited);
        Dictionary<long, EquipModel> repositoryEquips = new Dictionary<long, EquipModel>();

        for (int i = 0; i < AccountController.Instance.User.Roles.Count; i++)
        {
            RoleModel role = AccountController.Instance.User.Roles[i];
            if (m_equipBroID == 0)
                m_equipBroID = await MainForm.Instance.TabManager.TriggerAddTabPage(AccountController.Instance.User.AccountName, $"https://www.idleinfinity.cn/Equipment/Query?id={role.RoleId}", "equip");
            else
                await MainForm.Instance.TabManager.TriggerLoadUrl(AccountController.Instance.User.AccountName, $"https://www.idleinfinity.cn/Equipment/Query?id={role.RoleId}", m_equipBroID, "equip");

            //等待页面跳转并加载js
            //var tcs = new TaskCompletionSource<bool>();
            //onJsInitCallBack = (result) => tcs.SetResult(result);
            //await tcs.Task;

            MainForm.Instance.SetLoadContent("开始缓存仓库装备");

            P.Log("开始缓存装备", emLogType.AutoEquip);
            int page = 1;
            bool jumpNextPage = false;
            if (i == 0)
            {
                #region 缓存仓库装备
                MainForm.Instance.SetLoadContent($"正在缓存仓库的装备");
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
                                EquipModel equip = item.Value;
                                equip.SetAccountInfo(AccountController.Instance.User);
                                if (!repositoryEquips.ContainsKey(item.Key))
                                    repositoryEquips.Add(item.Key, item.Value);
                                //goto TESTFINISH;
                            }
                        }
                    }

                    P.Log($"缓存仓库第{page}页装备完成,当前缓存装备数量:{repositoryEquips.Count}", emLogType.AutoEquip);
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

                P.Log("缓存仓库完成！！");
                #endregion
            }

            #region 缓存背包装备
            MainForm.Instance.SetLoadContent($"正在缓存背包的装备");
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
                            EquipModel equip = item.Value;
                            equip.SetAccountInfo(AccountController.Instance.User, role);
                            //CheckEquipType(equip);
                            if (!repositoryEquips.ContainsKey(item.Key))
                                repositoryEquips.Add(item.Key, item.Value);
                        }
                    }
                }

                P.Log($"缓存背包第{page}页装备完成,当前缓存装备数量:{repositoryEquips.Count}", emLogType.AutoEquip);
                MainForm.Instance.SetLoadContent($"正在缓存背包的装备，当前已缓存数量{repositoryEquips.Count}");
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
        }

        //TESTFINISH:
        FreeDb.Sqlite.Insert<EquipModel>(repositoryEquips.Values.ToList()).ExecuteAffrows();

        var time1 = new CommonModel() { CommonKey = "EquipSaveTime", CommonValue = DateTime.Now.ToString() };
        if (v.Count > 0)
            FreeDb.Sqlite.Update<CommonModel>(time1).ExecuteAffrows();
        else
            FreeDb.Sqlite.Insert<CommonModel>(time1).ExecuteAffrows();

        MainForm.Instance.TabManager.DisposePage(m_equipBroID);
        m_equipBroID = 0;
        EventManager.Instance.UnsubscribeEvent(emEventType.OnJsInited, OnEquipJsInited);
        MainForm.Instance.HideLoadingPanel(emMaskType.AUTO_EQUIPING);
    }
    public async void StartAutoEquip()
    {
        await AutoEquip();
    }
    private async Task AutoEquip()
    {
        MainForm.Instance.ShowLoadingPanel("开始自动修车", emMaskType.AUTO_EQUIPING);

        await StartSaveEquips();

        EventManager.Instance.SubscribeEvent(emEventType.OnJsInited, OnEquipJsInited);
        //角色背包装备缓存
        Dictionary<long, EquipModel> packageEquips = new Dictionary<long, EquipModel>();
        //账号仓库装备缓存
        Dictionary<long, EquipModel> repositoryEquips = new Dictionary<long, EquipModel>();

        repositoryEquips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountID == AccountController.Instance.User.Id && p.RoleID == 0).ToList().ToDictionary(p => p.EquipID, p => p);

        for (int i = 0; i < AccountController.Instance.User.Roles.Count; i++)
        {
            RoleModel role = AccountController.Instance.User.Roles[i];
            if (m_equipBroID == 0)
                m_equipBroID = await MainForm.Instance.TabManager.TriggerAddTabPage(AccountController.Instance.User.AccountName, $"https://www.idleinfinity.cn/Equipment/Query?id={role.RoleId}", "equip");
            else
                await MainForm.Instance.TabManager.TriggerLoadUrl(AccountController.Instance.User.AccountName, $"https://www.idleinfinity.cn/Equipment/Query?id={role.RoleId}", m_equipBroID, "equip");

            packageEquips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountID == AccountController.Instance.User.Id && p.RoleID == role.RoleId).ToList().ToDictionary(p => p.EquipID, p => p);

            #region 检查角色装备
            MainForm.Instance.SetLoadContent($"正在检查{role.RoleName}的装备");

            Dictionary<emEquipSort, EquipModel> curEquips = null;
            var response = await GetCurEquips();
            if (response.Success)
            {
                curEquips = response.Result.ToObject<Dictionary<emEquipSort, EquipModel>>();
                var targetEquips = EquipCfg.Instance.GetEquipmentByJobAndLevel(role.Job, role.Level);
                if (targetEquips == null)
                {
                    P.Log($"未找到{role.Level}级{role.Job}的装备配置,无法更换", emLogType.AutoEquip);
                }
                else
                {
                    //逐个检查装备部位的装备是否符合要求
                    for (int j = 0; j < 11; j++)
                    {
                        bool isSuccess = false;
                        //每个部位检查装备前增加500ms得等待时间
                        await Task.Delay(500);
                        Equipment targetEquip = targetEquips.GetEquipBySort((emEquipSort)j);
                        string targetEquipName = targetEquip.Name;
                        EquipModel equip = null;
                        if (curEquips != null && curEquips.TryGetValue((emEquipSort)j, out equip))
                        {
                            if (string.IsNullOrEmpty(targetEquipName) || equip.EquipName.Contains(targetEquipName))
                            {
                                P.Log($"{role.RoleName}的{equip.EquipBaseName}位置装备{equip.EquipName}符合要求，无需更换", emLogType.AutoEquip);
                                continue;
                            }
                        }
                        foreach (var item in packageEquips)
                        {
                            if (targetEquip.AdaptAttr(item.Value.EquipName, item.Value.Content))
                            {
                                if (equip != null && (j == (int)emEquipSort.副手 || j == (int)emEquipSort.主手 || j == (int)emEquipSort.戒指1 || j == (int)emEquipSort.戒指2))
                                {
                                    P.Log($"{(emEquipSort)j}部位当前已穿戴装备，为防止穿戴时部位冲突导致换装失败，优先卸下当前部位装备", emLogType.AutoEquip);
                                    var response3 = await EquipOff(role, j);
                                    if (response3.Success)
                                    {
                                        P.Log($"等待卸下装备消息返回", emLogType.AutoEquip);
                                        var tcs2 = new TaskCompletionSource<bool>();
                                        onJsInitCallBack = (result) => tcs2.SetResult(result);
                                        await tcs2.Task;
                                        equip.SetAccountInfo(AccountController.Instance.User);
                                        packageEquips.Add(equip.EquipID, equip);
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
                                    isSuccess = true;
                                    goto WEAR_EQUIP_FIANLLY;
                                }
                            }
                        }
                        foreach (var item in repositoryEquips)
                        {
                            if (targetEquip.AdaptAttr(item.Value.EquipName, item.Value.Content))
                            {
                                if (equip != null && (j == (int)emEquipSort.副手 || j == (int)emEquipSort.主手 || j == (int)emEquipSort.戒指1 || j == (int)emEquipSort.戒指2))
                                {
                                    P.Log($"{(emItemType)j}部位当前已穿戴装备，为防止穿戴时部位冲突导致换装失败，优先卸下当前部位装备", emLogType.AutoEquip);
                                    var response3 = await EquipOff(role, j);
                                    if (response3.Success)
                                    {
                                        P.Log($"等待卸下装备消息返回", emLogType.AutoEquip);
                                        var tcs2 = new TaskCompletionSource<bool>();
                                        onJsInitCallBack = (result) => tcs2.SetResult(result);
                                        equip.SetAccountInfo(AccountController.Instance.User);
                                        repositoryEquips.Add(equip.EquipID, equip);
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
                                    isSuccess = true;
                                    goto WEAR_EQUIP_FIANLLY;
                                }
                            }
                        }

                        isSuccess = false;
                        WEAR_EQUIP_FIANLLY:
                        if (isSuccess)
                            P.Log($"{role.RoleName}更换{targetEquipName}装备完成", emLogType.AutoEquip);
                        else
                            P.Log($"{role.RoleName}未找到{targetEquipName}装备，无法更换", emLogType.AutoEquip);
                    }
                }
                MainForm.Instance.TabManager.DisposePage(m_equipBroID);

                P.Log($"{role.RoleName}全部位置装备更换完成\n\t\n\t\n\t", emLogType.AutoEquip);
            }
            #endregion
        }
        P.Log($"全部角色装备更换完成\n\t\n\t\n\t\n\t\n\t", emLogType.AutoEquip);

        FreeDb.Sqlite.Update<EquipModel>(repositoryEquips.Values.ToList()).ExecuteAffrows();
        FreeDb.Sqlite.Update<EquipModel>(packageEquips.Values.ToList()).ExecuteAffrows();

        MainForm.Instance.TabManager.DisposePage(m_equipBroID);
        m_equipBroID = 0;

        EventManager.Instance.UnsubscribeEvent(emEventType.OnJsInited, OnEquipJsInited);
        MainForm.Instance.HideLoadingPanel(emMaskType.AUTO_EQUIPING);
    }

    public async Task<JavascriptResponse> GetCurEquips()
    {
        var bro = MainForm.Instance.TabManager.BroDic[m_equipBroID];
        return await bro.EvaluateScriptAsync($@"getCurEquips()");
    }
    public async Task<JavascriptResponse> GetPackageEquips()
    {
        var bro = MainForm.Instance.TabManager.BroDic[m_equipBroID];
        return await bro.EvaluateScriptAsync($@"getPackageEquips()");
    }
    public async Task<JavascriptResponse> GetRepositoryEquips()
    {
        var bro = MainForm.Instance.TabManager.BroDic[m_equipBroID];
        return await bro.EvaluateScriptAsync($@"getRepositoryEquips()");
    }
    public async Task<JavascriptResponse> JumpRepositoryPage()
    {
        var bro = MainForm.Instance.TabManager.BroDic[m_equipBroID];
        return await bro.EvaluateScriptAsync($@"repositoryNext()");
    }
    public async Task<JavascriptResponse> JumpPackagePage()
    {
        var bro = MainForm.Instance.TabManager.BroDic[m_equipBroID];
        return await bro.EvaluateScriptAsync($@"packageNext()");
    }
    public async Task<JavascriptResponse> EquipOn(RoleModel role, EquipModel equip)
    {
        var bro = MainForm.Instance.TabManager.BroDic[m_equipBroID];
        return await bro.EvaluateScriptAsync($@"equipOn({role.RoleId},{equip.EquipID})");
    }
    public async Task<JavascriptResponse> EquipOff(RoleModel role, int etype)
    {
        var bro = MainForm.Instance.TabManager.BroDic[m_equipBroID];
        return await bro.EvaluateScriptAsync($@"equipOff({role.RoleId},{etype})");
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

