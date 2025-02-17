using CefSharp;
using IdleAuto.Db;
using IdleAuto.Scripts.Controller;
using IdleAuto.Scripts.Model;
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

        //if (v.Count > 0)
        //{
        //    var time = Convert.ToDateTime(v[0].CommonValue);
        //    if (DateTime.Now.Subtract(time).TotalHours < 24)
        //    {
        //        P.Log("24小时内已经保存过装备，无需再次保存", emLogType.AutoEquip);
        //        return;
        //    }
        //}

        FreeDb.Sqlite.Delete<EquipModel>().Where(p => true).ExecuteAffrows();
        MainForm.Instance.ShowLoadingPanel("开始盘点所有装备", emMaskType.AUTO_EQUIPING);
        EventManager.Instance.SubscribeEvent(emEventType.OnJsInited, OnEquipJsInited);
        Dictionary<long, EquipModel> repositoryEquips = new Dictionary<long, EquipModel>();

        for (int i = 0; i < AccountController.Instance.User.Roles.Count; i++)
        {
            RoleModel role = AccountController.Instance.User.Roles[i];
            if (m_equipBroID == 0)
                m_equipBroID = await BroTabManager.Instance.TriggerAddTabPage(AccountController.Instance.User.AccountName, $"https://www.idleinfinity.cn/Equipment/Query?id={role.RoleId}", "equip");
            else
                await BroTabManager.Instance.TriggerLoadUrl(AccountController.Instance.User.AccountName, $"https://www.idleinfinity.cn/Equipment/Query?id={role.RoleId}", m_equipBroID, "equip");

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
                    var response1 = await BroTabManager.Instance.TriggerCallJs(m_equipBroID, $@"getRepositoryEquips()");
                    if (response1.Success)
                    {
                        var equips = response1.Result.ToObject<Dictionary<long, EquipModel>>();
                        if (equips != null)
                        {
                            foreach (var item in equips)
                            {
                                EquipModel equip = item.Value;
                                equip.Category = TxtUtil.GetCategory(equip.EquipBaseName);
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
                    var response2 = await BroTabManager.Instance.TriggerCallJsWithReload(m_equipBroID, $@"repositoryNext()", "equip");
                    if (response2.Success && (bool)response2.Result)
                    {

                        P.Log("仓库切页完成");
                        page++;
                        jumpNextPage = true;
                    }
                    else
                    {
                        P.Log("仓库最后一页了！", emLogType.AutoEquip);
                        jumpNextPage = false;
                    }
                    await Task.Delay(500);
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
                var response1 = await BroTabManager.Instance.TriggerCallJs(m_equipBroID, $@"getPackageEquips()");
                //GetPackageEquips();
                if (response1.Success)
                {
                    var equips = response1.Result.ToObject<Dictionary<long, EquipModel>>();
                    if (equips != null)
                    {
                        foreach (var item in equips)
                        {
                            EquipModel equip = item.Value;
                            equip.Category = TxtUtil.GetCategory(equip.EquipBaseName);
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
                var response2 = await BroTabManager.Instance.TriggerCallJsWithReload(m_equipBroID, $@"packageNext()", "equip");
                //JumpPackagePage();
                if (response2.Success)
                {
                    if ((bool)response2.Result)
                    {
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
        FreeDb.Sqlite.InsertOrUpdate<CommonModel>().SetSource(time1).ExecuteAffrows();

        BroTabManager.Instance.DisposePage(m_equipBroID);
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
        P.Log("开始自动修车", emLogType.AutoEquip);

        // await StartSaveEquips();

        EventManager.Instance.SubscribeEvent(emEventType.OnJsInited, OnEquipJsInited);
        //角色背包装备缓存
        Dictionary<long, EquipModel> packageEquips = new Dictionary<long, EquipModel>();
        //账号仓库装备缓存
        Dictionary<long, EquipModel> repositoryEquips = new Dictionary<long, EquipModel>();

        repositoryEquips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountID == AccountController.Instance.User.Id && p.RoleID == 0).ToList().ToDictionary(p => p.EquipID, p => p);

        for (int i = 0; i < AccountController.Instance.User.Roles.Count; i++)
        {
            RoleModel role = AccountController.Instance.User.Roles[i];
            Dictionary<emEquipSort, EquipModel> towearEquips = new Dictionary<emEquipSort, EquipModel>();

            //测试切图
            if (m_equipBroID == 0)
                m_equipBroID = await BroTabManager.Instance.TriggerAddTabPage(AccountController.Instance.User.AccountName, $"https://www.idleinfinity.cn/Map/Detail?id={role.RoleId}", "char");
          
                

           // await CharacterController.Instance.SwitchMap(BroTabManager.Instance.GetBro(m_equipBroID), role);
            //测试结束

            await CharacterController.Instance.AddSkillPoints(BroTabManager.Instance.GetBro(m_equipBroID), role);

            await BroTabManager.Instance.TriggerLoadUrl(AccountController.Instance.User.AccountName, $"https://www.idleinfinity.cn/Equipment/Query?id={role.RoleId}", m_equipBroID, "equip");

            packageEquips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountID == AccountController.Instance.User.Id && p.RoleID == role.RoleId).ToList().ToDictionary(p => p.EquipID, p => p);
            #region 检查角色装备
            MainForm.Instance.SetLoadContent($"正在检查{role.RoleName}的装备");
            P.Log($"正在检查{role.RoleName}的装备", emLogType.AutoEquip);



            Dictionary<emEquipSort, EquipModel> curEquips = null;
            var response = await BroTabManager.Instance.TriggerCallJs(m_equipBroID, $@"getCurEquips()");
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
                        if (targetEquip == null) continue;
                        //string targetEquipName = targetEquip.Name;
                        EquipModel equip = null;
                        if (curEquips != null && curEquips.TryGetValue((emEquipSort)j, out equip))
                        {
                            equip.Category = TxtUtil.GetCategory(equip.EquipBaseName);
                            if (targetEquip.AdaptAttr(equip))
                            {
                                P.Log($"{role.RoleName}的{(emEquipSort)j}位置装备{equip.EquipName}符合要求，无需更换", emLogType.AutoEquip);
                                continue;
                            }
                        }
                        foreach (var item in packageEquips)
                        {
                            if (towearEquips.ContainsValue(item.Value))
                            {
                                continue;
                            }
                            if (!item.Value.CanWear(role)) continue;
                            if (targetEquip.AdaptAttr(item.Value))
                            {
                                towearEquips.Add((emEquipSort)j, item.Value);
                                isSuccess = true;
                                goto WEAR_EQUIP_FIANLLY;
                            }
                        }
                        foreach (var item in repositoryEquips)
                        {
                            if (towearEquips.ContainsValue(item.Value))
                            {
                                continue;
                            }

                            if (!item.Value.CanWear(role)) continue;
                            if (targetEquip.AdaptAttr(item.Value))
                            {
                                towearEquips.Add((emEquipSort)j, item.Value);
                                isSuccess = true;
                                goto WEAR_EQUIP_FIANLLY;
                            }
                        }

                        isSuccess = false;
                        WEAR_EQUIP_FIANLLY:
                        if (isSuccess)
                            P.Log($"{role.RoleName}查找{targetEquip.SimpleName}装备完成", emLogType.AutoEquip);
                        else
                            P.Log($"{role.RoleName}查找{targetEquip.SimpleName}装备失败", emLogType.AutoEquip);
                    }
                }

                AttrV4 requareV4 = AttrV4.Default;
                foreach (var item in towearEquips)
                {
                    requareV4 = AttrV4.Max(requareV4, item.Value.RequareAttr);
                }
                bool canWear = false;

                await BroTabManager.Instance.TriggerLoadUrl(AccountController.Instance.User.AccountName, $"https://www.idleinfinity.cn/Character/Detail?id={role.RoleId}", m_equipBroID, "char");
                var response3 = await BroTabManager.Instance.TriggerCallJs(m_equipBroID, $@"_char.getSimpleAttribute();");
                if (response3.Success)
                {
                    var baseAttr = response3.Result.ToObject<CharBaseAttributeModel>();
                    emMeetType meetType = baseAttr.Meets(requareV4);
                    switch (meetType)
                    {
                        case emMeetType.AlreadyMeet:
                            P.Log($"{role.RoleName}的属性满足穿戴条件，开始更换装备", emLogType.AutoEquip);
                            canWear = true;
                            break;
                        case emMeetType.MeetAfterAdd:
                            P.Log($"{role.RoleName}的属性不满足穿戴条件，但是剩余属性点足够", emLogType.AutoEquip);
                            if (baseAttr.AddPoint(requareV4))
                            {
                                var response4 = await BroTabManager.Instance.TriggerCallJsWithReload(m_equipBroID, $@"_char.attributeSave({baseAttr.ToLowerCamelCase()});", "char");
                                if (response4.Success)
                                {
                                    P.Log($"{role.RoleName}的属性加点完成", emLogType.AutoEquip);
                                    canWear = true;
                                }
                            }
                            break;
                        case emMeetType.MeetAfterReset:
                            P.Log($"{role.RoleName}的属性不满足穿戴条件，但重置后重新加点可以满足", emLogType.AutoEquip);
                            var response5 = await BroTabManager.Instance.TriggerCallJsWithReload(m_equipBroID, $@"_char.attributeReset();", "char");
                            if (response5.Success)
                            {
                                P.Log($"{role.RoleName}重置加点完成", emLogType.AutoEquip);
                                var response6 = await BroTabManager.Instance.TriggerCallJs(m_equipBroID, $@"_char.getSimpleAttribute();");
                                if (response6.Success)
                                {
                                    baseAttr = response6.Result.ToObject<CharBaseAttributeModel>();
                                    if (baseAttr.AddPoint(requareV4))
                                    {
                                        var response7 = await BroTabManager.Instance.TriggerCallJsWithReload(m_equipBroID, $@"_char.attributeSave({baseAttr.ToLowerCamelCase()});", "char");
                                        if (response7.Success)
                                        {
                                            P.Log($"{role.RoleName}的属性加点完成", emLogType.AutoEquip);
                                            canWear = true;
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            P.Log($"{role.RoleName}的属性不满足穿戴条件,为保证效率，所有装备不予更换", emLogType.AutoEquip);
                            break;
                    }

                    ///满足装备需求属性后仍有属性点剩余，自动加到体力值
                    if (baseAttr.Point > 0)
                    {
                        baseAttr.VitAdd += baseAttr.Point;
                        baseAttr.Point = 0;
                        var response8 = await BroTabManager.Instance.TriggerCallJsWithReload(m_equipBroID, $@"_char.attributeSave({baseAttr.ToLowerCamelCase()});", "char");
                        if (response8.Success)
                        {
                            P.Log($"{role.RoleName}的属性加点完成", emLogType.AutoEquip);
                        }
                    }

                    await BroTabManager.Instance.TriggerLoadUrl(AccountController.Instance.User.AccountName, $"https://www.idleinfinity.cn/Equipment/Query?id={role.RoleId}", m_equipBroID, "char");

                    if (canWear)
                    {
                        for (int j = 0; j < 11; j++)
                        {
                            if (towearEquips.ContainsKey((emEquipSort)j))
                            {
                                EquipModel equip = towearEquips[(emEquipSort)j];
                                ReplaceEquipStruct replaceResult = await WearEquip(equip, j, role);
                                if (replaceResult.IsSuccess)
                                {
                                    if (replaceResult.ReplacedEquip != null)
                                    {
                                        //如果有替换下来的装备，加入到仓库装备中
                                        repositoryEquips.Add(replaceResult.ReplacedEquip.EquipID, replaceResult.ReplacedEquip);
                                    }
                                    if (packageEquips.ContainsKey(equip.EquipID))
                                    {
                                        //如果背包中有该装备，从背包中移除
                                        packageEquips.Remove(equip.EquipID);
                                    }
                                    else if (repositoryEquips.ContainsKey(equip.EquipID))
                                    {
                                        //如果仓库中有该装备，从仓库中移除
                                        repositoryEquips.Remove(equip.EquipID);
                                    }
                                }
                                P.Log($"{role.RoleName}更换装备{equip.EquipName}完成", emLogType.AutoEquip);
                            }
                        }
                        P.Log($"{role.RoleName}全部位置装备更换完成", emLogType.AutoEquip);
                    }

                    P.Log($"{role.RoleName}自动修车完成！", emLogType.AutoEquip);
                }
            }
            #endregion
        }
        P.Log($"全部角色装备更换完成\n\t\n\t\n\t\n\t\n\t", emLogType.AutoEquip);
        BroTabManager.Instance.DisposePage(m_equipBroID);
        m_equipBroID = 0;

        FreeDb.Sqlite.Update<EquipModel>(repositoryEquips.Values.ToList()).ExecuteAffrows();
        FreeDb.Sqlite.Update<EquipModel>(packageEquips.Values.ToList()).ExecuteAffrows();

        EventManager.Instance.UnsubscribeEvent(emEventType.OnJsInited, OnEquipJsInited);
        MainForm.Instance.HideLoadingPanel(emMaskType.AUTO_EQUIPING);
    }

    private async Task<ReplaceEquipStruct> WearEquip(EquipModel equip, int sort, RoleModel role)
    {
        Dictionary<emEquipSort, EquipModel> curEquips = null;
        ReplaceEquipStruct replaceEquipStruct = new ReplaceEquipStruct();
        replaceEquipStruct.ReplacedEquip = null;
        replaceEquipStruct.IsSuccess = false;
        var response = await BroTabManager.Instance.TriggerCallJs(m_equipBroID, $@"getCurEquips()");
        if (response.Success)
        {
            curEquips = response.Result.ToObject<Dictionary<emEquipSort, EquipModel>>();


            if (equip != null && (sort == (int)emEquipSort.副手 || sort == (int)emEquipSort.主手 || sort == (int)emEquipSort.戒指1 || sort == (int)emEquipSort.戒指2))
            {
                if (curEquips != null && curEquips.ContainsKey((emEquipSort)sort))
                {
                    P.Log($"{(emEquipSort)sort}部位当前已穿戴装备，为防止穿戴时部位冲突导致换装失败，优先卸下当前部位装备", emLogType.AutoEquip);
                    var response3 = await BroTabManager.Instance.TriggerCallJsWithReload(m_equipBroID, $@"equipOff({role.RoleId},{sort})", "equip");
                    if (response3.Success)
                    {
                        equip.SetAccountInfo(AccountController.Instance.User);
                        replaceEquipStruct.ReplacedEquip = curEquips[(emEquipSort)sort];
                    }
                }
            }

            P.Log($"{role.RoleName}现在更换{(emEquipSort)sort}位置的装备{equip.EquipName}", emLogType.AutoEquip);

            var response2 = await BroTabManager.Instance.TriggerCallJsWithReload(m_equipBroID, $@"equipOn({role.RoleId},{equip.EquipID})", "equip");
            if (response2.Success)
            {
                replaceEquipStruct.IsSuccess = true;
                return replaceEquipStruct;
            }
        }
        return replaceEquipStruct;
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

public struct ReplaceEquipStruct
{
    public bool IsSuccess;
    public EquipModel ReplacedEquip;
}