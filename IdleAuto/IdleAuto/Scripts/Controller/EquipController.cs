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
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

public class EquipController
{
    /// <summary>
    /// 转移角色背包物品到仓库
    /// </summary>
    /// <param name="broSeed">执行逻辑的浏览器页签编号</param>
    /// <param name="account">执行逻辑的账号</param>
    public async static Task EquipsToRepository(int broSeed, UserModel account)
    {
        if (broSeed <= 0) { MessageBox.Show("浏览器页签编号有问题，请检查是否正确打开页签！"); return; }
        if (account == null || account.Roles.Count <= 0) { MessageBox.Show("账户数据错误，或者账户内没有角色，请检查！"); return; }

        P.Log("开始转移角色背包物品到仓库", emLogType.AutoEquip);
        foreach (var role in account.Roles)
        {
            //跳转装备详情页面
            await BroTabManager.Instance.TriggerLoadUrl(account.AccountName, IdleUrlHelper.EquipUrl(role.RoleId), broSeed, "equip");

            P.Log($"开始转移{role.RoleName}的背包物品到仓库", emLogType.AutoEquip);

            bool hasEquips = false;
            var result = await BroTabManager.Instance.TriggerCallJs(broSeed, $@"packageHasEquips()");
            if (result.Success)
            {
                hasEquips = (bool)result.Result;
                while (hasEquips)
                {
                    P.Log($"{role.RoleName}的背包仍有物品，现将当前页所有物品存储到仓库", emLogType.AutoEquip);
                    await Task.Delay(1000);
                    var result2 = await BroTabManager.Instance.TriggerCallJsWithReload(broSeed, $@"equipStorage({role.RoleId})", "equip");
                    if (result2.Success)
                    {
                        P.Log($"{role.RoleName}的背包物品存储到仓库完成", emLogType.AutoEquip);
                        var result3 = await BroTabManager.Instance.TriggerCallJs(broSeed, $@"packageHasEquips()");
                        if (result3.Success)
                            hasEquips = (bool)result3.Result;
                        else
                            hasEquips = false;
                    }
                }
            }

            await Task.Delay(1000);
        }
    }

    /// <summary>
    /// 盘点仓库所以装备
    /// </summary>
    /// <param name="broSeed">执行逻辑的浏览器页签编号</param>
    /// <param name="account">执行逻辑的账号</param>
    /// <returns></returns>
    public async static Task InventoryEquips(int broSeed, UserModel account, bool ignoreTime = false)
    {
        if (broSeed <= 0) { MessageBox.Show("浏览器页签编号有问题，请检查是否正确打开页签！"); return; }
        if (account == null || account.Roles.Count <= 0) { MessageBox.Show("账户数据错误，或者账户内没有角色，请检查！"); return; }

        Dictionary<long, EquipModel> repositoryEquips = new Dictionary<long, EquipModel>();

        //跳转装备详情页面
        await BroTabManager.Instance.TriggerLoadUrl(account.AccountName, IdleUrlHelper.EquipUrl(account.FirstRole.RoleId), broSeed, "equip");

        //检查是否需要保存装备
        //如果忽略检查保存时间，直接进行盘点
        if (!ignoreTime)
        {
            var v = FreeDb.Sqlite.Select<CommonModel>().Where(P => P.CommonKey == "EquipSaveTime").ToList();

            if (v.Count > 0)
            {
                var time = Convert.ToDateTime(v[0].CommonValue);
                if (DateTime.Now.Subtract(time).TotalHours < 8)
                {
                    P.Log("8小时内已经保存过装备，无需再次保存", emLogType.AutoEquip);
                    return;
                }
            }
        }

        //清空当前账号的仓库装备数据库
        FreeDb.Sqlite.Delete<EquipModel>().Where(p => p.AccountID == account.Id).ExecuteAffrows();
        P.Log("开始盘点所有装备", emLogType.AutoEquip);

        RoleModel role = account.FirstRole;
        await BroTabManager.Instance.TriggerLoadUrl(account.AccountName, IdleUrlHelper.EquipUrl(role.RoleId), broSeed, "equip");

        P.Log("开始缓存仓库装备", emLogType.AutoEquip);
        int page = 1;
        bool jumpNextPage = false;
        #region 缓存仓库装备
        do
        {
            jumpNextPage = false;
            P.Log($"缓存仓库第{page}页装备", emLogType.AutoEquip);
            var response1 = await BroTabManager.Instance.TriggerCallJs(broSeed, $@"getRepositoryEquips()");
            if (response1.Success)
            {
                var equips = response1.Result.ToObject<Dictionary<long, EquipModel>>();
                if (equips != null)
                {
                    foreach (var item in equips)
                    {
                        EquipModel equip = item.Value;
                        equip.Category = TxtUtil.GetCategory(equip.EquipBaseName);
                        equip.SetAccountInfo(account);
                        if (!repositoryEquips.ContainsKey(item.Key))
                            repositoryEquips.Add(item.Key, item.Value);
                        //goto TESTFINISH;
                    }
                }
            }

            P.Log($"缓存仓库第{page}页装备完成,当前缓存装备数量:{repositoryEquips.Count}", emLogType.AutoEquip);
            P.Log("开始跳转仓库下一页", emLogType.AutoEquip);
            var response2 = await BroTabManager.Instance.TriggerCallJsWithReload(broSeed, $@"repositoryNext()", "equip");
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
            await Task.Delay(1500);
        } while (jumpNextPage);

        P.Log("缓存仓库完成！！");
        #endregion


        P.Log("写入仓库装备到数据库", emLogType.AutoEquip);
        FreeDb.Sqlite.InsertOrUpdate<EquipModel>().SetSource(repositoryEquips.Values.ToList()).ExecuteAffrows();

        P.Log("写入更新仓库时间到数据库", emLogType.AutoEquip);
        var time1 = new CommonModel() { CommonKey = "EquipSaveTime", CommonValue = DateTime.Now.ToString() };
        FreeDb.Sqlite.InsertOrUpdate<CommonModel>().SetSource(time1).ExecuteAffrows();

        P.Log("盘点所有装备完成", emLogType.AutoEquip);
    }

    public async static Task ClearRepository(int broSeed, UserModel account)
    {
        if (broSeed <= 0) { MessageBox.Show("浏览器页签编号有问题，请检查是否正确打开页签！"); return; }
        if (account == null || account.Roles.Count <= 0) { MessageBox.Show("账户数据错误，或者账户内没有角色，请检查！"); return; }

        P.Log("开始清理仓库装备", emLogType.AutoEquip);
        RetainEquipCfg.Instance.ResetCount();

        //跳转装备详情页面
        await BroTabManager.Instance.TriggerLoadUrl(account.AccountName, IdleUrlHelper.EquipUrl(account.FirstRole.RoleId), broSeed, "equip");

        Dictionary<long, EquipModel> toClear = new Dictionary<long, EquipModel>();

        //先跳转到仓库最后一页，防止因为删除装备页面数据变化，导致装备重复检查
        P.Log("先跳转到仓库最后一页", emLogType.AutoEquip);
        bool jumpNextPage = false;
        int page = 1;
        do
        {
            jumpNextPage = false;
            P.Log("开始跳转仓库下一页", emLogType.AutoEquip);
            var response2 = await BroTabManager.Instance.TriggerCallJsWithReload(broSeed, $@"repositoryNext()", "equip");
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
            await Task.Delay(1000);
        } while (jumpNextPage);

        P.Log($"已跳转到仓库最后一页(第{page}页)", emLogType.AutoEquip);

        P.Log("开始检查仓库装备", emLogType.AutoEquip);
        jumpNextPage = false;
        do
        {
            jumpNextPage = false;
            toClear.Clear();
            P.Log($"检查仓库第{page}页装备", emLogType.AutoEquip);
            var response1 = await BroTabManager.Instance.TriggerCallJs(broSeed, $@"getRepositoryEquips()");
            if (response1.Success)
            {
                var equips = response1.Result.ToObject<Dictionary<long, EquipModel>>();
                if (equips != null)
                {
                    foreach (var item in equips)
                    {
                        item.Value.Category = TxtUtil.GetCategory(item.Value.EquipBaseName);
                        P.Log($"正在检查装备{item.Value}({item.Value.Quality}-{item.Value.Category})-{item.Key}");
                        if (item.Value.emItemQuality == emItemQuality.SET || item.Value.emItemQuality == emItemQuality.UNIQUE || item.Value.emItemQuality == emItemQuality.ARTIFACT)
                            continue;
                        if (!RetainEquipCfg.Instance.IsRetain(item.Value))
                            toClear.Add(item.Key, item.Value);
                    }

                    string eids = string.Join(",", toClear.Keys);
                    await Task.Delay(1000);
                    var response2 = await BroTabManager.Instance.TriggerCallJsWithReload(broSeed, $@"equipClear({account.FirstRole.RoleId},""{eids}"")", "equip");
                    if (response2.Success)
                    {
                        P.Log($"清理仓库第{page}页装备完成,当前清理装备数量:{toClear.Count}", emLogType.AutoEquip);
                    }
                }
            }

            P.Log("开始跳转仓库上一页", emLogType.AutoEquip);
            await Task.Delay(1000);
            var response3 = await BroTabManager.Instance.TriggerCallJsWithReload(broSeed, $@"repositoryPre()", "equip");
            if (response3.Success && (bool)response3.Result)
            {

                P.Log("仓库切页完成");
                page--;
                jumpNextPage = true;
            }
            else
            {
                P.Log("仓库第一页了！", emLogType.AutoEquip);
                jumpNextPage = false;
            }
            await Task.Delay(1500);
        } while (jumpNextPage);

        P.Log("清理仓库完成！！");
    }

    /// <summary>
    /// 自动更换装备
    /// </summary>
    /// <param name="broSeed">执行逻辑的浏览器页签编号</param>
    /// <param name="account">执行逻辑的账号</param>
    /// <returns></returns>
    public async static Task AutoEquips(int broSeed, UserModel account, RoleModel role)
    {
        if (broSeed <= 0) { MessageBox.Show("浏览器页签编号有问题，请检查是否正确打开页签！"); return; }
        if (role == null) { MessageBox.Show("角色数据错误，请检查！"); return; }

        MainForm.Instance.ShowLoadingPanel("开始自动修车", emMaskType.AUTO_EQUIPING);
        P.Log("开始自动修车", emLogType.AutoEquip);

        Dictionary<emEquipSort, EquipModel> towearEquips = new Dictionary<emEquipSort, EquipModel>();

        //跳转装备详情页面
        await BroTabManager.Instance.TriggerLoadUrl(account.AccountName, IdleUrlHelper.EquipUrl(role.RoleId), broSeed, "equip");

        #region 检查角色装备
        P.Log($"正在检查{role.RoleName}的装备", emLogType.AutoEquip);
        Dictionary<emEquipSort, EquipModel> curEquips = null;
        var response = await BroTabManager.Instance.TriggerCallJs(broSeed, $@"getCurEquips()");
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
                    await Task.Delay(1500);
                    Equipment targetEquip = targetEquips.GetEquipBySort((emEquipSort)j);
                    if (targetEquip == null)
                    {
                        P.Log($"{role.RoleName}的{(emEquipSort)j}位置装备配置不存在，无需更换", emLogType.AutoEquip);
                        continue;
                    }
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
                    P.Log($"开始查询{role.RoleName}的{(emEquipSort)j}位置装备", emLogType.AutoEquip);
                    Dictionary<long, EquipModel> equips = new Dictionary<long, EquipModel>();
                    if (!string.IsNullOrEmpty(targetEquip.Name))
                    {
                        equips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountID == account.Id && p.EquipName.Contains(targetEquip.Name)).ToList().ToDictionary(p => p.EquipID, p => p);
                    }
                    else if (!string.IsNullOrEmpty(targetEquip.Category))
                    {
                        equips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountID == account.Id && p.Category == targetEquip.Category).ToList().ToDictionary(p => p.EquipID, p => p);
                    }


                    foreach (var item in equips)
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

            await BroTabManager.Instance.TriggerLoadUrl(account.AccountName, IdleUrlHelper.RoleUrl(role.RoleId), broSeed, "char");
            var response3 = await BroTabManager.Instance.TriggerCallJs(broSeed, $@"_char.getSimpleAttribute();");
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
                            var response4 = await BroTabManager.Instance.TriggerCallJsWithReload(broSeed, $@"_char.attributeSave({baseAttr.ToLowerCamelCase()});", "char");
                            if (response4.Success)
                            {
                                P.Log($"{role.RoleName}的属性加点完成", emLogType.AutoEquip);
                                canWear = true;
                            }
                        }
                        break;
                    case emMeetType.MeetAfterReset:
                        P.Log($"{role.RoleName}的属性不满足穿戴条件，但重置后重新加点可以满足", emLogType.AutoEquip);
                        var response5 = await BroTabManager.Instance.TriggerCallJsWithReload(broSeed, $@"_char.attributeReset();", "char");
                        if (response5.Success)
                        {
                            P.Log($"{role.RoleName}重置加点完成", emLogType.AutoEquip);
                            var response6 = await BroTabManager.Instance.TriggerCallJs(broSeed, $@"_char.getSimpleAttribute();");
                            if (response6.Success)
                            {
                                baseAttr = response6.Result.ToObject<CharBaseAttributeModel>();
                                if (baseAttr.AddPoint(requareV4))
                                {
                                    var response7 = await BroTabManager.Instance.TriggerCallJsWithReload(broSeed, $@"_char.attributeSave({baseAttr.ToLowerCamelCase()});", "char");
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

                if (canWear)
                {
                    await BroTabManager.Instance.TriggerLoadUrl(account.AccountName, IdleUrlHelper.EquipUrl(role.RoleId), broSeed, "equip");

                    for (int j = 0; j < 11; j++)
                    {
                        if (towearEquips.ContainsKey((emEquipSort)j))
                        {
                            EquipModel equip = towearEquips[(emEquipSort)j];
                            ReplaceEquipStruct replaceResult = await WearEquip(broSeed, equip, j, account, role);
                            if (replaceResult.IsSuccess)
                            {
                                if (replaceResult.ReplacedEquip != null)
                                {
                                    //如果有替换下来的装备，加入到仓库装备中
                                    FreeDb.Sqlite.Insert<EquipModel>(replaceResult.ReplacedEquip).ExecuteAffrows();
                                }

                                //从仓库中移除穿戴的装备
                                FreeDb.Sqlite.Delete<EquipModel>(equip).ExecuteAffrows();
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
        P.Log($"{role.RoleName}的装备更换完成\n\t\n\t", emLogType.AutoEquip);
    }

    /// <summary>
    /// 穿戴装备
    /// </summary>
    /// <param name="broSeed">执行逻辑的浏览器页签编号</param>
    /// <param name="equip">要穿戴的装备</param>
    /// <param name="sort">要穿戴的位置</param>
    /// <param name="account">所属账号</param>
    /// <param name="role">执行逻辑的角色</param>
    /// <returns></returns>
    private async static Task<ReplaceEquipStruct> WearEquip(int broSeed, EquipModel equip, int sort, UserModel account, RoleModel role)
    {
        Dictionary<emEquipSort, EquipModel> curEquips = null;
        ReplaceEquipStruct replaceEquipStruct = new ReplaceEquipStruct();
        replaceEquipStruct.ReplacedEquip = null;
        replaceEquipStruct.IsSuccess = false;
        var response = await BroTabManager.Instance.TriggerCallJs(broSeed, $@"getCurEquips()");
        if (response.Success)
        {
            curEquips = response.Result.ToObject<Dictionary<emEquipSort, EquipModel>>();


            //if (equip != null && (sort == (int)emEquipSort.副手 || sort == (int)emEquipSort.主手 || sort == (int)emEquipSort.戒指1 || sort == (int)emEquipSort.戒指2))
            {
                if (curEquips != null && curEquips.ContainsKey((emEquipSort)sort))
                {
                    P.Log($"{(emEquipSort)sort}部位当前已穿戴装备，为防止穿戴时部位冲突导致换装失败，优先卸下当前部位装备", emLogType.AutoEquip);
                    var response3 = await BroTabManager.Instance.TriggerCallJsWithReload(broSeed, $@"equipOff({role.RoleId},{sort})", "equip");
                    if (response3.Success)
                    {
                        equip.SetAccountInfo(account);
                        replaceEquipStruct.ReplacedEquip = curEquips[(emEquipSort)sort];
                    }
                }
            }


            P.Log($"{role.RoleName}现在更换{(emEquipSort)sort}位置的装备{equip.EquipName}", emLogType.AutoEquip);

            var response2 = await BroTabManager.Instance.TriggerCallJsWithReload(broSeed, $@"equipOn({role.RoleId},{equip.EquipID})", "equip");
            if (response2.Success)
            {
                replaceEquipStruct.IsSuccess = true;
                return replaceEquipStruct;
            }
        }
        return replaceEquipStruct;
    }
}

public struct ReplaceEquipStruct
{
    public bool IsSuccess;
    public EquipModel ReplacedEquip;
}