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
using AttributeMatch;
using CefSharp.DevTools.FedCm;
using CefSharp.WinForms;
using IdleAuto.Scripts.Wrap;
using System.Security.Principal;

public class EquipController
{
    /// <summary>
    /// 转移角色背包物品到仓库
    /// </summary>
    /// <param name="broSeed">执行逻辑的浏览器页签编号</param>
    /// <param name="account">执行逻辑的账号</param>
    public async Task EquipsToRepository(BroWindow win, UserModel account, bool cleanWhenFull = false)
    {
        if (account == null || account.Roles.Count <= 0) { MessageBox.Show("账户数据错误，或者账户内没有角色，请检查！"); return; }

        P.Log("开始转移角色背包物品到仓库", emLogType.AutoEquip);
        foreach (var role in account.Roles)
        {
            await EquipsToRepository(win, account, role, cleanWhenFull);
        }
    }

    /// <summary>
    /// 转移角色背包物品到仓库
    /// </summary>
    /// <param name="broSeed">执行逻辑的浏览器页签编号</param>
    /// <param name="title">执行逻辑的浏览器页签标题</param>
    /// <param name="role">执行逻辑的角色</param>
    /// <returns></returns>
    public async Task EquipsToRepository(BroWindow win, UserModel account, RoleModel role, bool cleanWhenFull = false)
    {
        P.Log($"开始转移{role.RoleName}的背包物品到仓库", emLogType.AutoEquip);
        await Task.Delay(1000);
        P.Log($"跳转{role.RoleName}的装备详情页面", emLogType.AutoEquip);
        var response = await win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId), "equip");
        if (response.Success)
        {
            bool hasEquips = false;
            int boxCount = 0;
            P.Log($"检查{role.RoleName}的仓库物品总量", emLogType.AutoEquip);
            var response1 = await win.CallJs($@"repositoryEquipsCount()");
            if (response1.Success)
            {
                boxCount = (int)response1.Result;
                P.Log($"{role.RoleName}的仓库物品总量为{boxCount}", emLogType.AutoEquip);
                P.Log($"检查{role.RoleName}当前页背包物品总量", emLogType.AutoEquip);
                var result = await win.CallJs($@"packageHasEquips()");
                if (result.Success)
                {
                    int bagCount = (int)result.Result;
                    P.Log($"{role.RoleName}当前页背包物品总量为{bagCount}", emLogType.AutoEquip);
                    hasEquips = bagCount > 0;
                    while (hasEquips)
                    {
                        if (bagCount + boxCount > 3000)
                        {
                            P.Log($"{role.RoleName}的背包物品存储到仓库失败，仓库已满", emLogType.AutoEquip);
                            if (cleanWhenFull)
                            {
                                await ClearRepository(win, account);
                            }
                            else
                            {
                                hasEquips = false;
                                break;
                            }
                        }

                        await Task.Delay(1000);
                        P.Log($"{role.RoleName}的背包仍有物品，现将当前页所有物品存储到仓库", emLogType.AutoEquip);
                        var result2 = await win.CallJsWaitReload($@"equipStorage({role.RoleId})", "equip");
                        if (result2.Success)
                        {
                            boxCount += bagCount;
                            P.Log($"{role.RoleName}的背包物品存储到仓库完成", emLogType.AutoEquip);
                            P.Log($"检查{role.RoleName}当前页背包物品总量", emLogType.AutoEquip);
                            var result3 = await win.CallJs($@"packageHasEquips()");
                            if (result3.Success)
                            {
                                bagCount = (int)result3.Result;
                                hasEquips = bagCount > 0;
                                P.Log($"{role.RoleName}当前页背包物品总量为{bagCount}", emLogType.AutoEquip);
                            }
                            else
                            {
                                hasEquips = false;
                                P.Log($"检查{role.RoleName}当前页背包物品总量失败", emLogType.AutoEquip);
                            }
                        }
                        else
                        {
                            hasEquips = false;
                            P.Log($"{role.RoleName}的背包物品存储到仓库失败", emLogType.AutoEquip);
                        }
                    }
                }
                else
                {
                    P.Log($"检查{role.RoleName}当前页背包物品总量失败", emLogType.AutoEquip);
                }
            }
            else
            {
                P.Log($"检查{role.RoleName}的仓库物品总量失败", emLogType.AutoEquip);
            }
        }
    }

    /// <summary>
    /// 盘点仓库所有装备
    /// </summary>
    /// <param name="broSeed">执行逻辑的浏览器页签编号</param>
    /// <param name="account">执行逻辑的账号</param>
    /// <returns></returns>
    public async Task InventoryEquips(BroWindow win, UserModel account, bool ignoreTime = false)
    {
        Dictionary<long, EquipModel> repositoryEquips = new Dictionary<long, EquipModel>();
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

        P.Log($"清空当前账号{account.AccountName}在数据库的所有数据", emLogType.AutoEquip);
        FreeDb.Sqlite.Delete<EquipModel>().Where(p => p.AccountID == account.Id).ExecuteAffrows();
        P.Log("开始盘点所有装备", emLogType.AutoEquip);

        RoleModel role = account.FirstRole;
        await Task.Delay(1000);
        P.Log($"跳转仓库装备详情页面", emLogType.AutoEquip);
        var response = await win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId), "equip");
        if (response.Success)
        {
            int page = 1;
            bool jumpNextPage = false;
            #region 缓存仓库装备
            do
            {
                jumpNextPage = false;
                P.Log($"缓存仓库第{page}页装备", emLogType.AutoEquip);
                var response1 = await win.CallJs($@"getRepositoryEquips()");
                if (response1.Success)
                {
                    var equips = response1.Result.ToObject<Dictionary<long, EquipModel>>();
                    if (equips != null)
                    {
                        P.Log($"获取仓库第{page}页装备成功，装备总数{equips.Count}", emLogType.AutoEquip);
                        foreach (var item in equips)
                        {
                            EquipModel equip = item.Value;
                            equip.Category = CategoryUtil.GetCategory(equip.EquipBaseName);
                            equip.SetAccountInfo(account);
                            if (!repositoryEquips.ContainsKey(item.Key))
                                repositoryEquips.Add(item.Key, item.Value);
                        }

                        P.Log($"缓存仓库第{page}页装备完成,当前缓存装备数量:{repositoryEquips.Count}", emLogType.AutoEquip);
                        P.Log("开始跳转仓库下一页", emLogType.AutoEquip);
                        await Task.Delay(1000);
                        var response2 = await win.CallJsWithReload($@"repositoryNext()", "equip");
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
                    }
                    else
                    {
                        P.Log($"获取仓库第{page}页装备失败", emLogType.AutoEquip);
                        jumpNextPage = false;
                    }
                }
                else
                {
                    P.Log($"缓存仓库第{page}页装备失败", emLogType.AutoEquip);
                    jumpNextPage = false;
                }

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
    }

    /// <summary>
    /// 清理仓库装备
    /// </summary>
    /// <param name="broSeed">执行逻辑的浏览器页签编号</param>
    /// <param name="account">执行逻辑的账号</param>
    /// <returns></returns>
    public async Task ClearRepository(BroWindow win, UserModel account)
    {
        P.Log("开始清理仓库装备", emLogType.AutoEquip);
        RetainEquipCfg.Instance.ResetCount();

        await Task.Delay(1000);
        P.Log($"跳转仓库装备详情页面", emLogType.AutoEquip);
        var response = await win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(account.FirstRole.RoleId), "equip");
        if (response.Success)
        {
            Dictionary<long, EquipModel> toClear = new Dictionary<long, EquipModel>();

            RoleModel role = account.FirstRole;
            int page = 0;
            //先跳转到仓库最后一页，防止因为删除装备页面数据变化，导致装备重复检查
            await Task.Delay(1000);
            P.Log($"跳转仓库装备详情页面", emLogType.AutoEquip);
            var response1 = await win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId), "equip");
            if (response1.Success)
            {
                P.Log("开始缓存仓库装备", emLogType.AutoEquip);
                int boxCount = 0;
                P.Log($"检查{role.RoleName}的仓库物品总量", emLogType.AutoEquip);
                var response2 = await win.CallJs($@"repositoryEquipsCount()");
                if (response2.Success)
                {
                    boxCount = (int)response2.Result;
                    P.Log($"{role.RoleName}的仓库物品总量为{boxCount}", emLogType.AutoEquip);
                    if (boxCount > 0)
                    {
                        P.Log($"开始跳转仓库最后一页", emLogType.AutoEquip);
                        page = (int)Math.Floor((double)(boxCount - 1) / 60);
                        await Task.Delay(1000);
                        P.Log($"跳转仓库最后一页-第{page}页", emLogType.AutoEquip);
                        var response3 = await win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId, 0, page), "equip");
                        if (response3.Success)
                        {
                            P.Log($"已跳转到仓库最后一页(第{page}页)", emLogType.AutoEquip);

                            P.Log("开始检查仓库装备", emLogType.AutoEquip);
                            bool jumpNextPage = false;
                            do
                            {
                                jumpNextPage = false;
                                toClear.Clear();
                                P.Log($"检查仓库第{page}页装备", emLogType.AutoEquip);
                                var response4 = await win.CallJs($@"getRepositoryEquips()");
                                if (response4.Success)
                                {
                                    var equips = response4.Result.ToObject<Dictionary<long, EquipModel>>();
                                    if (equips != null)
                                    {
                                        P.Log($"获取仓库第{page}页装备成功，装备总数：{equips.Count}", emLogType.AutoEquip);
                                        foreach (var item in equips)
                                        {
                                            item.Value.Category = CategoryUtil.GetCategory(item.Value.EquipBaseName);
                                            if (item.Value.emItemQuality == emItemQuality.套装 || item.Value.emItemQuality == emItemQuality.传奇 || item.Value.emItemQuality == emItemQuality.神器)
                                                continue;
                                            if (!RetainEquipCfg.Instance.IsRetain(item.Value))
                                                toClear.Add(item.Key, item.Value);
                                        }

                                        string eids = string.Join(",", toClear.Keys);
                                        await Task.Delay(1000);
                                        P.Log($"开始清理仓库第{page}页装备,清理数量:{toClear.Count}", emLogType.AutoEquip);
                                        var response5 = await win.CallJsWaitReload($@"equipClear({account.FirstRole.RoleId},""{eids}"")", "equip");
                                        if (response5.Success)
                                        {
                                            P.Log($"清理仓库第{page}页装备完成,当前清理装备数量:{toClear.Count}", emLogType.AutoEquip);
                                            await Task.Delay(1000);
                                            P.Log("开始跳转仓库上一页", emLogType.AutoEquip);
                                            var response6 = await win.CallJsWithReload($@"repositoryPre()", "equip");
                                            if (response6.Success && (bool)response6.Result)
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
                                        }
                                        else
                                        {
                                            jumpNextPage = false;
                                            P.Log($"清理仓库第{page}页装备失败", emLogType.AutoEquip);
                                        }
                                    }
                                    else
                                    {
                                        jumpNextPage = false;
                                        P.Log($"获取仓库第{page}页装备失败", emLogType.AutoEquip);
                                    }
                                }
                                else
                                {
                                    jumpNextPage = false;
                                    P.Log($"检查仓库第{page}页装备失败", emLogType.AutoEquip);
                                }
                            } while (jumpNextPage);

                            P.Log("清理仓库完成！！");
                        }
                    }
                    else
                    {
                        P.Log($"{role.RoleName}的仓库物品总量为0，无需盘点", emLogType.AutoEquip);
                        return;
                    }
                }
                else
                {
                    P.Log($"检查{role.RoleName}的仓库物品总量失败", emLogType.AutoEquip);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 自动更换装备
    /// </summary>
    /// <param name="broSeed">执行逻辑的浏览器页签编号</param>
    /// <param name="account">执行逻辑的账号</param>
    /// <returns></returns>
    public async Task AutoEquips(BroWindow win, UserModel account, RoleModel role)
    {
        P.Log("开始自动修车", emLogType.AutoEquip);
        Dictionary<emEquipSort, EquipModel> towearEquips = new Dictionary<emEquipSort, EquipModel>();

        await Task.Delay(1000);
        //跳转装备详情页面
        var result = await win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId), "equip");
        if (result.Success)
        {
            P.Log($"开始获取{role.RoleName}当前穿戴的装备", emLogType.AutoEquip);
            Dictionary<emEquipSort, EquipModel> curEquips = null;
            var response = await win.CallJs($@"getCurEquips()");
            if (response.Success)
            {
                P.Log($"获取{role.RoleName}当前穿戴的装备成功", emLogType.AutoEquip);
                curEquips = response.Result.ToObject<Dictionary<emEquipSort, EquipModel>>();
                P.Log($"开始获取{role.Level}级{role.Job}配置的装备", emLogType.AutoEquip);
                var targetEquips = GetEquipConfig(role.Job, role.Level);
                if (targetEquips != null)
                {
                    P.Log("获取{role.Level}级{role.Job}配置的装备成功");
                    foreach (var suit in targetEquips.EquipSuit)
                    {
                        var suitEquips = MatchEquipSuit(account.Id, role, suit, curEquips);
                        if (suitEquips.IsSuccess)
                        {
                            P.Log($"匹配{role.Level}级{role.Job}配置的装备成功，匹配套装：{suitEquips.MatchSuitName},开始更换装备", emLogType.AutoEquip);
                            towearEquips = suitEquips.ToWearEquips;
                            break;
                        }
                    }
                }
                else
                {
                    P.Log($"获取{role.Level}级{role.Job}配置的装备失败", emLogType.AutoEquip);
                }

                if (towearEquips.Count > 0)
                {
                    bool canWear = await AutoAttributeSave(win, role, towearEquips.Values.ToList());
                    if (canWear)
                    {
                        await Task.Delay(1000);
                        var result3 = await win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId), "equip");
                        if (result3.Success)
                        {
                            for (int j = 0; j < 11; j++)
                            {
                                if (towearEquips.ContainsKey((emEquipSort)j))
                                {
                                    EquipModel equip = towearEquips[(emEquipSort)j];
                                    ReplaceEquipStruct replaceResult = await WearEquip(win, equip, j, account, role);
                                    if (replaceResult.IsSuccess)
                                    {
                                        if (replaceResult.ReplacedEquip != null)
                                        {
                                            replaceResult.ReplacedEquip.Category = CategoryUtil.GetCategory(replaceResult.ReplacedEquip.EquipBaseName);
                                            //如果有替换下来的装备，加入到仓库装备中
                                            FreeDb.Sqlite.Insert<EquipModel>(replaceResult.ReplacedEquip).ExecuteAffrows();
                                        }

                                        //从仓库中移除穿戴的装备
                                        FreeDb.Sqlite.Delete<EquipModel>().Where(p => p.EquipID == equip.EquipID).ExecuteAffrows();
                                    }
                                    P.Log($"{role.RoleName}更换装备{equip.EquipName}完成", emLogType.AutoEquip);
                                }
                            }
                            P.Log($"{role.RoleName}全部位置装备更换完成", emLogType.AutoEquip);
                        }
                    }
                    else
                    {
                        P.Log($"{role.RoleName}的属性需求不满足，跳过更换装备流程", emLogType.AutoEquip);
                    }

                    P.Log($"{role.RoleName}自动修车完成！", emLogType.AutoEquip);
                }
                else
                {
                    P.Log($"未找到更换装备，跳过更换装备流程", emLogType.AutoEquip);
                }
            }
            else
            {
                P.Log($"获取{role.RoleName}当前穿戴装备失败", emLogType.AutoEquip);
            }
        }
    }

    /// <summary>
    /// 获取匹配的装备套装
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="role"></param>
    /// <param name="equipSuit"></param>
    /// <param name="curEquips"></param>
    /// <returns></returns>
    private EquipSuitMatchStruct MatchEquipSuit(int accountId, RoleModel role, EquipSuit equipSuit, Dictionary<emEquipSort, EquipModel> curEquips)
    {
        EquipSuitMatchStruct result = new EquipSuitMatchStruct();
        result.MatchSuitName = equipSuit.SuitName;
        result.ToWearEquips = new Dictionary<emEquipSort, EquipModel>();
        result.IsSuccess = true;
        List<long> toWearEquipIds = new List<long>();
        for (int j = 0; j < 11; j++)
        {
            P.Log($"开始匹配{role.RoleName}{(emEquipSort)j}位置的装备", emLogType.AutoEquip);
            EquipModel curEquip = null;
            if (curEquips != null)
                curEquips.TryGetValue((emEquipSort)j, out curEquip);
            Equipment equipment = equipSuit.GetEquipBySort((emEquipSort)j);
            if (equipment == null)
            {
                P.Log($"{role.RoleName}{(emEquipSort)j}位置的装备没有找到配置，无需更换!");
                continue;
            }
            List<EquipModel> matchEquips = GetMatchEquipBySort(accountId, role, (emEquipSort)j, curEquip, equipment);
            for (int i = 0; i < matchEquips.Count;)
            {
                if (matchEquips[0].CanWear(role))
                {
                    break;
                }
                else
                {
                    matchEquips.RemoveAt(0);
                }
            }
            if (matchEquips.Count > 0)
            {
                if (curEquip != null && matchEquips.First().EquipID == curEquip.EquipID)
                {
                    P.Log($"当前穿戴的装备为找到的最佳装备，无需更换", emLogType.AutoEquip);
                    continue;
                }
                else
                {
                    P.Log($"找到最佳装备{matchEquips.First().EquipName}，准备更换", emLogType.AutoEquip);

                    foreach (var item in matchEquips)
                    {
                        if (!toWearEquipIds.Contains(item.EquipID))
                        {
                            result.ToWearEquips.Add((emEquipSort)j, item);
                            toWearEquipIds.Add(item.EquipID);
                            break;
                        }
                    }

                    if (!result.ToWearEquips.ContainsKey((emEquipSort)j) && equipment.Necessary)
                    {
                        result.IsSuccess = false;
                        break;
                    }
                }
            }
            else if (equipment.Necessary)
            {
                P.Log($"未找到匹配的装备，跳过该部位换装！", emLogType.AutoEquip);
                result.IsSuccess = false;
                break;
            }
        }

        return result;
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
    private async Task<ReplaceEquipStruct> WearEquip(BroWindow win, EquipModel equip, int sort, UserModel account, RoleModel role)
    {
        Dictionary<emEquipSort, EquipModel> curEquips = null;
        ReplaceEquipStruct replaceEquipStruct = new ReplaceEquipStruct();
        replaceEquipStruct.ReplacedEquip = null;
        replaceEquipStruct.IsSuccess = false;
        var response = await win.CallJs($@"getCurEquips()");
        if (response.Success)
        {
            curEquips = response.Result.ToObject<Dictionary<emEquipSort, EquipModel>>();

            {
                if (curEquips != null && curEquips.ContainsKey((emEquipSort)sort))
                {
                    P.Log($"{(emEquipSort)sort}部位当前已穿戴装备，为防止穿戴时部位冲突导致换装失败，优先卸下当前部位装备", emLogType.AutoEquip);
                    await Task.Delay(1000);
                    var response3 = await win.CallJsWaitReload($@"equipOff({role.RoleId},{sort})", "equip");
                    if (response3.Success)
                    {
                        equip.SetAccountInfo(account);
                        replaceEquipStruct.ReplacedEquip = curEquips[(emEquipSort)sort];
                    }
                }
            }

            P.Log($"{role.RoleName}现在更换{(emEquipSort)sort}位置的装备{equip.EquipName}", emLogType.AutoEquip);
            await Task.Delay(1000);
            var response2 = await win.CallJsWaitReload($@"equipOn({role.RoleId},{equip.EquipID})", "equip");
            if (response2.Success)
            {
                replaceEquipStruct.IsSuccess = true;
                return replaceEquipStruct;
            }
        }
        return replaceEquipStruct;
    }

    /// <summary>
    /// 根据需要替换的装备列表，自动加点满足装备需求
    /// </summary>
    /// <param name="win">执行逻辑的浏览器</param>
    /// <param name="role">执行逻辑的角色</param>
    /// <param name="towearEquips">要穿戴的装备列表</param>
    /// <returns>是否能满足条件</returns>
    public async Task<bool> AutoAttributeSave(BroWindow win, RoleModel role, List<EquipModel> towearEquips)
    {
        P.Log($"开始检查待穿戴装备的属性需求", emLogType.AutoEquip);
        AttrV4 requareV4 = AttrV4.Default;
        foreach (var item in towearEquips)
        {
            requareV4 = AttrV4.Max(requareV4, item.RequareAttr);
        }
        P.Log($"检查待穿戴装备的属性需求成功，需要（力-{requareV4.Str}，敏-{requareV4.Dex}，体-{requareV4.Vit}，精-{requareV4.Eng}）", emLogType.AutoEquip);

        P.Log($"开始检查角色{role.RoleName}的属性是否满足穿戴条件", emLogType.AutoEquip);
        bool canWear = false;
        await Task.Delay(1000);
        P.Log($"跳转角色{role.RoleName}详情页面", emLogType.AutoEquip);
        var result2 = await win.LoadUrlWaitJsInit(IdleUrlHelper.RoleUrl(role.RoleId), "char");
        if (result2.Success)
        {
            P.Log($"获取角色{role.RoleName}四维属性", emLogType.AutoEquip);
            var response3 = await win.CallJs($@"_char.getSimpleAttribute();");
            if (response3.Success)
            {
                var baseAttr = response3.Result.ToObject<CharBaseAttributeModel>();
                P.Log($"获取角色{role.RoleName}四维属性成功（力-{baseAttr.Str}，敏-{baseAttr.Dex}，体-{baseAttr.Vit}，精-{baseAttr.Eng}）", emLogType.AutoEquip);
                AttrV4 jobAttr = JobBaseAttributeUtil.JobBaseAttr(role.Job);
                emMeetType meetType = baseAttr.Meets(requareV4, jobAttr);
                switch (meetType)
                {
                    case emMeetType.AlreadyMeet:
                        P.Log($"{role.RoleName}的属性满足穿戴条件，开始更换装备", emLogType.AutoEquip);
                        canWear = true;
                        break;
                    case emMeetType.MeetAfterAdd:
                        P.Log($"{role.RoleName}的属性不满足穿戴条件，但是剩余属性点足够", emLogType.AutoEquip);
                        if (baseAttr.AddPoint(requareV4, jobAttr))
                        {
                            await Task.Delay(1000);
                            P.Log($"开始{role.RoleName}的属性加点", emLogType.AutoEquip);
                            var response4 = await win.CallJsWaitReload($@"_char.attributeSave({role.RoleId},{baseAttr.ToLowerCamelCase()});", "char");
                            if (response4.Success)
                            {
                                P.Log($"{role.RoleName}的属性加点完成", emLogType.AutoEquip);
                                canWear = true;
                            }
                            else
                            {

                                P.Log($"{role.RoleName}的属性加点错误", emLogType.AutoEquip);
                                canWear = false;
                            }
                        }
                        else
                        {
                            P.Log($"{role.RoleName}的属性计算错误", emLogType.AutoEquip);
                            canWear = false;
                        }
                        break;
                    case emMeetType.MeetAfterReset:
                        P.Log($"{role.RoleName}的属性不满足穿戴条件，但重置后重新加点可以满足", emLogType.AutoEquip);
                        await Task.Delay(1000);
                        P.Log($"开始{role.RoleName}的重置加点", emLogType.AutoEquip);
                        var response5 = await win.CallJsWaitReload($@"_char.attributeReset({role.RoleId});", "char");
                        if (response5.Success)
                        {
                            P.Log($"{role.RoleName}重置加点完成", emLogType.AutoEquip);
                            await Task.Delay(1000);
                            P.Log($"获取角色{role.RoleName}四维属性", emLogType.AutoEquip);
                            var response6 = await win.CallJs($@"_char.getSimpleAttribute();");
                            if (response6.Success)
                            {
                                baseAttr = response6.Result.ToObject<CharBaseAttributeModel>();
                                P.Log($"获取角色{role.RoleName}四维属性成功（力-{baseAttr.Str}，敏-{baseAttr.Dex}，体-{baseAttr.Vit}，精-{baseAttr.Eng}）", emLogType.AutoEquip);
                                if (baseAttr.AddPoint(requareV4, jobAttr))
                                {
                                    await Task.Delay(1000);
                                    var response7 = await win.CallJsWaitReload($@"_char.attributeSave({role.RoleId},{baseAttr.ToLowerCamelCase()});", "char");
                                    if (response7.Success)
                                    {
                                        P.Log($"{role.RoleName}的属性加点完成", emLogType.AutoEquip);
                                        canWear = true;
                                    }
                                    else
                                    {

                                        P.Log($"{role.RoleName}的属性加点错误", emLogType.AutoEquip);
                                        canWear = false;
                                    }
                                }
                                else
                                {
                                    P.Log($"{role.RoleName}的属性计算错误", emLogType.AutoEquip);
                                    canWear = false;
                                }
                            }
                        }
                        else
                        {
                            P.Log($"{role.RoleName}的重置加点错误", emLogType.AutoEquip);
                            canWear = false;
                        }
                        break;
                    default:
                        P.Log($"{role.RoleName}的属性不满足穿戴条件,为保证效率，所有装备不予更换", emLogType.AutoEquip);
                        canWear = false;
                        break;
                }
            }
        }
        return canWear;
    }

    /// <summary>
    /// 检查符合要求的所有装备
    /// </summary>
    /// <param name="role"></param>
    /// <param name="sort"></param>
    /// <param name="curEquip"></param>
    /// <param name="targetConfig"></param>
    /// <returns></returns>
    private List<EquipModel> GetMatchEquipBySort(int accountId, RoleModel role, emEquipSort sort, EquipModel curEquip, Equipment targetConfig)
    {
        List<EquipModel> matchEquips = new List<EquipModel>();
        Dictionary<long, EquipModel> matchEquipMap = new Dictionary<long, EquipModel>();
        Dictionary<long, AttributeMatchReport> matchReports = new Dictionary<long, AttributeMatchReport>();

        P.Log($"开始查询数据库装备", emLogType.AutoEquip);
        List<EquipModel> findEquips = new List<EquipModel>();
        var __equips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountID == accountId).ToList();
        foreach (var item in __equips)
        {
            if (AttributeMatchUtil.MatchCategory(item, targetConfig.Category) && AttributeMatchUtil.MatchQuallity(item, targetConfig.Quality))
            {
                findEquips.Add(item);
            }
        }
        P.Log($"查询数据库装备完成,共找到{findEquips.Count}个装备", emLogType.AutoEquip);
        if (curEquip != null)
        {
            P.Log($"当前部位已穿戴装备，将此装备一并加入匹配列表参与比较！");
            findEquips.Add(curEquip);
        }

        P.Log($"开始依照配置顺序比较装备，并将匹配的装备按比较权重排序！");
        foreach (var item in findEquips)
        {
            if (AttributeMatchUtil.Match(item, targetConfig, out AttributeMatchReport report))
            {
                matchReports.Add(item.EquipID, report);
                matchEquipMap.Add(item.EquipID, item);
            }
        }

        P.Log($"比较完成，共找到{matchEquipMap.Count}个符合要求的装备", emLogType.AutoEquip);
        if (matchEquipMap.Count > 0)
            matchEquips = matchEquipMap.Values.OrderByDescending(p => matchReports[p.EquipID].MatchWeight).ToList();
        if (matchEquipMap.Count > 1)
        {
            P.Log($"已找到符合要求的装备，不再匹配后续要求，直接返回查询列表", emLogType.AutoEquip);
        }

        return matchEquips;
    }
    public Dictionary<long, EquipModel> GetMatchEquips(int accountid, Equipment target, out Dictionary<long, AttributeMatchReport> reportMap)
    {
        Dictionary<long, EquipModel> matchEquipMap = new Dictionary<long, EquipModel>();
        Dictionary<long, AttributeMatchReport> matchReports = new Dictionary<long, AttributeMatchReport>();

        P.Log($"开始查询数据库装备", emLogType.AutoEquip);
        List<EquipModel> findEquips = new List<EquipModel>();
        var __equips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountID == accountid).ToList();
        try {
            foreach (var item in __equips)
            {
                if (AttributeMatchUtil.MatchCategory(item, target.Category) && AttributeMatchUtil.MatchQuallity(item, target.Quality))
                {
                    findEquips.Add(item);
                }
            }
        }
        catch(Exception e)
        {
            throw e;
        }
        
        P.Log($"查询数据库装备完成,共找到{findEquips.Count}个装备", emLogType.AutoEquip);

        P.Log($"开始依照配置顺序比较装备，并将匹配的装备按比较权重排序！");
        foreach (var item in findEquips)
        {
            if (AttributeMatchUtil.Match(item, target, out AttributeMatchReport report))
            {
                matchReports.Add(item.EquipID, report);
                matchEquipMap.Add(item.EquipID, item);
            }
        }
        reportMap = matchReports;
        return matchEquipMap;
    }

    private Equipments GetEquipConfig(emJob job, int level)
    {
        return EquipCfg.Instance.GetEquipmentByJobAndLevel(job, level);
    }

    public async Task Test(BroWindow win)
    {
    }
}

public struct ReplaceEquipStruct
{
    public bool IsSuccess;
    public EquipModel ReplacedEquip;
}
public struct EquipSuitMatchStruct
{
    public bool IsSuccess;
    public string MatchSuitName;
    public Dictionary<emEquipSort, EquipModel> ToWearEquips;
}