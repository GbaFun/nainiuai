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
using IdleAuto.Scripts.Utils;
using FreeSql.Internal;
using System.Linq.Expressions;

public class EquipController : BaseController
{
    public static readonly object _obj = new object();
    public EquipController(BroWindow bro) : base(bro)
    {

    }
    /// <summary>
    /// 转移角色背包物品到仓库
    /// </summary>
    /// <param name="broSeed">执行逻辑的浏览器页签编号</param>
    /// <param name="account">执行逻辑的账号</param>
    public async Task EquipsToRepository(BroWindow win, UserModel account, bool cleanWhenFull = false)
    {
        if (account == null || account.Roles.Count <= 0) { MessageBox.Show("账户数据错误，或者账户内没有角色，请检查！"); return; }
        var r = await win.CallJs("_char.hasNotice()");
        if (r.Result.ToObject<bool>())
        {
            var t = new TradeController(win);
            await t.AcceptAll(win.User);
        }
        await Task.Delay(2000);
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
        await UpdateCurEquips(win, role);
        bool hasEquips = false;
        int boxCount = 0;
        P.Log($"检查{role.RoleName}的仓库物品总量", emLogType.AutoEquip);
        var response1 = await win.CallJs($@"repositoryEquipsCount()");
        if (response1.Success)
        {
            boxCount = (int)response1.Result;
            P.Log($"{role.RoleName}的仓库物品总量为{boxCount}", emLogType.AutoEquip);
            P.Log($"检查{role.RoleName}当前页背包物品总量", emLogType.AutoEquip);
            int maxNum = int.Parse(ConfigUtil.GetAppSetting("BoxMaxNum"));
            var result = await win.CallJs($@"packageHasEquips()");
            if (result.Success)
            {
                int bagCount = (int)result.Result;
                P.Log($"{role.RoleName}当前页背包物品总量为{bagCount}", emLogType.AutoEquip);
                hasEquips = bagCount > 0;
                while (hasEquips)
                {
                    if (bagCount + boxCount >= 2800)
                    {
                        P.Log($"{role.RoleName}的背包物品存储到仓库失败，仓库已满", emLogType.AutoEquip);
                        if (cleanWhenFull)
                        {
                            await ClearRepository(win);
                        }
                        else
                        {
                            hasEquips = false;
                            break;
                        }
                    }

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

                P.Log($"重新检查{role.RoleName}的仓库物品总量", emLogType.AutoEquip);
                var response2 = await win.CallJs($@"repositoryEquipsCount()");
                if (response2.Success)
                {
                    boxCount = (int)response1.Result;
                    int retainNum = int.Parse(ConfigUtil.GetAppSetting("BoxRetainNum"));
                    if (boxCount >= 3000)
                    {
                        //await ClearRepository(win, account);
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
                            // equip.Category = CategoryUtil.GetCategory(equip.EquipBaseName);
                            equip.SetAccountInfo(account);
                            if (!repositoryEquips.ContainsKey(item.Key))
                            {
                                item.Value.EquipStatus = emEquipStatus.Repo;
                                repositoryEquips.Add(item.Key, item.Value);
                            }

                        }

                        P.Log($"缓存仓库第{page}页装备完成,当前缓存装备数量:{repositoryEquips.Count}", emLogType.AutoEquip);
                        P.Log("开始跳转仓库下一页", emLogType.AutoEquip);
                        var response2 = await win.CallJsWaitReload($@"repositoryNext()", "equip");
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
    public async Task ClearRepository(BroWindow win)
    {
        var config = new RetainEquipCfg();
        var account = win.User;
        var equipInSuitList = FreeDb.Sqlite.Select<EquipSuitModel>()
        .Where(p => p.AccountName == account.AccountName)
        .ToList();
        var equipInSuit = new Dictionary<long, long>();
        foreach (var item in equipInSuitList)
        {
            if (!equipInSuit.ContainsKey(item.EquipId))
            {
                equipInSuit.Add(item.EquipId, item.EquipId);
            }
        }
        P.Log("开始清理仓库装备", emLogType.AutoEquip);


        await Task.Delay(1500);
        P.Log($"跳转仓库装备详情页面", emLogType.AutoEquip);
        var response = await win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(account.FirstRole.RoleId), "equip");
        if (response.Success)
        {
            Dictionary<long, EquipModel> toClear = new Dictionary<long, EquipModel>();

            RoleModel role = account.FirstRole;
            int retainNum = int.Parse(ConfigUtil.GetAppSetting("BoxRetainNum"));
            int page = 0;
            //先跳转到仓库最后一页，防止因为删除装备页面数据变化，导致装备重复检查
            await Task.Delay(1500);
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
                    if (boxCount > 0 && boxCount >= retainNum)
                    {
                        P.Log($"开始跳转仓库最后一页", emLogType.AutoEquip);
                        page = (int)Math.Floor((double)(boxCount - 1) / 60);
                        await Task.Delay(1500);
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

                                            // item.Value.Category = CategoryUtil.GetCategory(item.Value.EquipBaseName);
                                            if (item.Value.emItemQuality == emItemQuality.神器 || equipInSuit.ContainsKey(item.Value.EquipID))
                                                continue;
                                            if (!config.IsRetain(item.Value))
                                            {
                                                // toClear.Add(item.Key, item.Value);
                                                var delPre = item.Value.ToObject<DelPreviewEquipModel>();
                                                delPre.SetAccountInfo(win.User);
                                                DbUtil.InsertOrUpdate<DelPreviewEquipModel>(delPre);
                                            }

                                        }

                                        string eids = string.Join(",", toClear.Keys);
                                        await Task.Delay(1500);
                                        if (toClear.Count != 0 && boxCount >= retainNum)
                                        {
                                            P.Log($"开始清理仓库第{page}页装备,清理数量:{toClear.Count}", emLogType.AutoEquip);
                                            // P.Log(ConsoleEquips(toClear.Values.ToList()), emLogType.Debug);
                                            await win.CallJsWaitReload($@"equipClear({account.FirstRole.RoleId},""{eids}"")", "equip");
                                            boxCount -= toClear.Count;
                                            P.Log($"清理仓库第{page}页装备完成,当前清理装备数量:{toClear.Count}", emLogType.AutoEquip);
                                            await Task.Delay(1000);
                                        }

                                        P.Log("开始跳转仓库上一页", emLogType.AutoEquip);
                                        var response6 = await win.CallJsWaitReload($@"repositoryPre()", "equip");
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
                        P.Log($"{role.RoleName}的仓库物品总量低于设置的保留数量（{retainNum}），无需盘点", emLogType.AutoEquip);
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

    public async Task ClearRepo()
    {
        var preList = FreeDb.Sqlite.Select<DelPreviewEquipModel>().Where(p => p.AccountName == _win.User.AccountName).ToList();
        var idList = preList.Select(s => s.EquipID);
        var eids = string.Join(",", idList);
        await _win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(_win.User.FirstRole.RoleId), "equip");
        await Task.Delay(1500);
        var r = await _win.CallJsWaitReload($@"equipClear({_win.User.FirstRole.RoleId},""{eids}"")", "equip");
        if (r.Success)
        {
            FreeDb.Sqlite.Delete<EquipModel>().Where(p => idList.Contains(p.EquipID)).ExecuteAffrows();
            FreeDb.Sqlite.Delete<DelPreviewEquipModel>().Where(p => idList.Contains(p.EquipID)).ExecuteAffrows();
        }


    }

    private void WrapEquip(EquipModel eq, UserModel user, RoleModel role, emEquipStatus status)
    {
        eq.EquipStatus = status;
        // eq.Category = CategoryUtil.GetCategory(eq.EquipBaseName);
        eq.SetAccountInfo(user, role);
    }

    private string ConsoleEquips(List<EquipModel> list)
    {
        StringBuilder sb = new StringBuilder();
        list.ForEach(p =>
        {
            sb.Append(p.Content);
            sb.Append("*****************************************************");
            sb.Append("\r\n");

        });
        return sb.ToString();
    }

    public async Task SaveEquipSuit(emSuitType suitType, RoleModel role)
    {
        await Task.Delay(1500);
        //跳转装备详情页面
        var url = IdleUrlHelper.EquipUrl(role.RoleId);
        if (_win.GetBro().Address.IndexOf(url)>-1) await _win.LoadUrlWaitJsInit(url, "equip");
        var r = await _win.CallJs($"getEquipSuitId(\"{suitType.ToString()}\")");
        var suitId = await GetSuitId(suitType, role);
        var curEquips = await GetCurEquips();
        if (curEquips[emEquipSort.副手].EquipID == RepairManager.PublicYonghengId)
        {
            return;
        }
        var isReplace = suitId > 0 ? true : false;

        var dic = new Dictionary<string, object>();
        dic.Add("cfname", suitType.ToString());
        if (isReplace)
        {
            dic.Add("isReplace", true);
            dic.Add("cfid", suitId);
        }

        await _win.CallJsWaitReload($"saveEquipSuit({dic.ToLowerCamelCase()})", "equip");
        await Task.Delay(1500);
        var r1 = await _win.CallJs($"getEquipSuitId(\"{suitType.ToString()}\")");
        suitId = r1.Result.ToObject<int>();
        if (suitId > 0)
        {
            await SaveEquipSuitModel(suitType, role, suitId);
        }
    }

    public async Task SaveEquipSuitModel(emSuitType suitType, RoleModel role, int suitId)
    {
        //更新当前用户的对应配装表
        var curEquips = await GetCurEquips();
        FreeDb.Sqlite.Delete<EquipSuitModel>().Where(p => p.SuitType == suitType && p.RoleId == role.RoleId).ExecuteAffrows();
        var data = curEquips.Values.Select(s => new EquipSuitModel()
        {
            SuitId = suitId,
            AccountName = _win.User.AccountName,
            EquipId = s.EquipID,
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            SuitType = suitType
        }).ToList();
        DbUtil.InsertOrUpdate<EquipSuitModel>(data);
    }

    public async Task LoadSuit(emSuitType suitType, RoleModel role)
    {
        var hasSuit = FreeDb.Sqlite.Select<EquipSuitModel>().Where(p => p.RoleId == role.RoleId && p.SuitType == suitType).First() != null;
        var dkSuits = new emSuitType[] { emSuitType.MF, emSuitType.效率 };
        if (!hasSuit && dkSuits.Contains(suitType)) return;
        await Task.Delay(1500);
        //跳转装备详情页面
        var url = IdleUrlHelper.EquipUrl(role.RoleId);
        if (_win.GetBro().Address != url) await _win.LoadUrlWaitJsInit(url, "equip");
        var r = await _win.CallJs($"getEquipSuitId(\"{suitType.ToString()}\")");
        var suitId = await GetSuitId(suitType, role);
        var curEquips = await GetCurEquips();
        var toSetInRepoList = curEquips.Values.ToList();
        toSetInRepoList.ForEach(p => { p.SetAccountInfo(_win.User); p.EquipStatus = emEquipStatus.Repo; });
        var isReplace = suitId > 0 ? true : false;
        var dic = new Dictionary<string, object>();
        dic.Add("cfid", suitId);
        await _win.CallJsWaitReload($"loadSuit({dic.ToLowerCamelCase()})", "equip");
        var curEquips2 = await GetCurEquips();
        var toSetEquippedList = curEquips2.Values.ToList();
        toSetEquippedList.ForEach(p => { p.SetAccountInfo(_win.User, role); p.EquipStatus = emEquipStatus.Equipped; });
        if (dkSuits.Contains(suitType))
        {
            DbUtil.InsertOrUpdate<EquipModel>(toSetInRepoList);
            DbUtil.InsertOrUpdate<EquipModel>(toSetEquippedList);
        }


    }


    public async Task<int> GetSuitId(emSuitType suitType, RoleModel role)
    {
        //跳转装备详情页面
        var url = IdleUrlHelper.EquipUrl(role.RoleId);
        if (_win.GetBro().Address != url) await _win.LoadUrlWaitJsInit(url, "equip");
        var r = await _win.CallJs($"getEquipSuitId(\"{suitType.ToString()}\")");
        var suitId = r.Result.ToObject<int>();
        return suitId;
    }



    /// <summary>
    /// 获取当前页穿戴的装备
    /// </summary>
    /// <returns></returns>
    public async Task<Dictionary<emEquipSort, EquipModel>> GetCurEquips(RoleModel role = null)
    {
        if (role != null)
        {
            var result = await _win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId), "equip");
        }
        var response = await _win.CallJs($@"getCurEquips()");
        var curEquips = response.Result.ToObject<Dictionary<emEquipSort, EquipModel>>();
        return curEquips;
    }

    /// <summary>
    /// 自动更换装备
    /// </summary>
    /// <param name="broSeed">执行逻辑的浏览器页签编号</param>
    /// <param name="account">执行逻辑的账号</param>
    /// <returns></returns>
    public async Task<Dictionary<emEquipSort, EquipModel>> AutoEquips(BroWindow win, RoleModel role, emSkillMode targetSkillMode = emSkillMode.自动, emSuitType targetSuitType = emSuitType.效率)
    {
        var account = win.User;
        P.Log("开始自动修车", emLogType.AutoEquip);
        Dictionary<emEquipSort, EquipModel> towearEquips = new Dictionary<emEquipSort, EquipModel>();
        Dictionary<emEquipSort, List<ArtifactMakeStruct>> toMakeEquips = new Dictionary<emEquipSort, List<ArtifactMakeStruct>>();

        await Task.Delay(1500);
        var curSkillMode = targetSkillMode;
        //跳转装备详情页面
        var result = await win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId), "equip");
        P.Log($"开始获取{role.RoleName}当前穿戴的装备", emLogType.AutoEquip);
        Dictionary<emEquipSort, EquipModel> curEquips = null;
        var response = await win.CallJs($@"getCurEquips()");
        curEquips = response.Result.ToObject<Dictionary<emEquipSort, EquipModel>>();
        var curEquipSuitType = EquipUtil.GetEquipSuitType(curEquips, role);
        if (targetSuitType == emSuitType.效率 && role.Job == emJob.死骑)
        {
            if (curEquipSuitType == emSuitType.MF)
            {
                await LoadSuit(emSuitType.效率, role);
                response = await win.CallJs($@"getCurEquips()");
                curEquips = response.Result.ToObject<Dictionary<emEquipSort, EquipModel>>();
            }
        }
        P.Log($"开始获取{role.Level}级{role.Job}配置的装备", emLogType.AutoEquip);
        var targetEquips = GetEquipConfig(role.Job, role.Level, targetSuitType);
        int idealFcr = 0;
        var tradeSuitMap = new List<Dictionary<emEquipSort, TradeModel>>();
        if (targetEquips != null)
        {
            if (role.Job == emJob.死灵 && targetSkillMode != emSkillMode.自动)
            {
                targetEquips.EquipSuit.RemoveAll(p => p.SkillMode != targetSkillMode);
            }

            P.Log("获取{role.Level}级{role.Job}配置的装备成功");
            for (int i = 0; i < targetEquips.EquipSuit.Count; i++)
            {
                var suit = targetEquips.EquipSuit[i];

                tradeSuitMap.Add(new Dictionary<emEquipSort, TradeModel>());
                var suitEquips = MatchEquipSuit(account.Id, role, suit, curEquips, tradeSuitMap[i]);
                idealFcr = suit.IdealFcr;
                //调整戒指等级如果不能满足速度要求则不匹配 因为把死灵戒指调整成非必要 要额外计算速度是否满足理想速度 如果戒指非必要都没命中则没必要调整戒指
                if (idealFcr != 0 && role.Job == emJob.死灵 && suitEquips.IsSuccess && targetSkillMode == emSkillMode.自动)
                {
                    suitEquips.IsSuccess = AdjustRings(role, curEquips, suitEquips.ToWearEquips, suit);
                }

                if (suitEquips.IsSuccess)
                {

                    if (suitEquips.MatchSuitName == emSkillMode.献祭.ToString())
                    {
                        curSkillMode = emSkillMode.献祭;

                    }
                    if (role.Job == emJob.死灵)
                    {
                        FlowController.SetSkillMode(role, account, suitEquips.MatchSkillModel);
                    }

                    P.Log($"匹配{role.Level}级{role.Job}配置的装备成功，匹配套装：{suitEquips.MatchSuitName},开始更换装备", emLogType.AutoEquip);
                    towearEquips = suitEquips.ToWearEquips;
                    toMakeEquips = suitEquips.ToMakeEquips;

                    break;
                }

            }
        }
        else
        {
            throw new Exception("未配置该等级配置");
        }
        if (tradeSuitMap.Count > 0)
        {
            foreach (var toTradeSuit in tradeSuitMap)
            {
                if (toTradeSuit.Count == 0 || toTradeSuit.Values.Contains(null))
                {
                    continue;
                }
                EquipTradeQueue.Enqueue(toTradeSuit.Values.ToList());
                break;
            }
        }

        if (toMakeEquips.Count > 0)
        {

            var artifactControl = new ArtifactController(win);
            foreach (var m in toMakeEquips)
            {
                bool isSuccessMake = false;
                foreach (var toMake in m.Value)
                {
                    if (isSuccessMake) break;
                    if (curEquips.ContainsKey(m.Key) && curEquips[m.Key].EquipName == toMake.ArtifactBase.GetEnumDescription())
                    {
                        //当前穿戴装备就是最佳装备 跳过制作这个部位 此时towear应该是curEquip
                        break;
                    }
                    EquipModel artifactEquip = await artifactControl.MakeArtifact(toMake.ArtifactBase, toMake.EquipBase, role.RoleId, toMake.Config);
                    if (artifactEquip != null)
                    {
                        isSuccessMake = true;
                        if (towearEquips.ContainsKey(m.Key))
                        {
                            towearEquips[m.Key] = artifactEquip;

                        }
                        else towearEquips.Add(m.Key, artifactEquip);
                    }
                    await Task.Delay(1500);
                }
            }

        }
        var r1 = await win.CallJs($@"getCurEquips()");

        var curEquip = r1.Result.ToObject<Dictionary<emEquipSort, EquipModel>>();

        if (towearEquips.Count > 0)
        {
            List<EquipModel> equipModels = MergeEquips(towearEquips, curEquips);
            bool canWear = await AutoAttributeSave(win, role, equipModels);
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
                            await WearEquipAndRecord(win, equip, j, account, role);
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


        r1 = await win.CallJs($@"getCurEquips()");
        await UpdateCurEquips(win, role);
        curEquip = r1.Result.ToObject<Dictionary<emEquipSort, EquipModel>>();
        if (role.Job == emJob.死灵 && role.GetRoleSkillMode() == emSkillMode.献祭)
        {

            await FlowController.InsertColdConversion(curEquip, win, role);
            CalNecFcrSpeed(win.User, role, curEquip, role.GetRoleSkillMode());
        }
        if (role.Job == emJob.死骑)
        {
            RecordDkYongheng(role, curEquip);
        }
        if (targetEquips.SuitType == emSuitType.效率 && role.Job == emJob.死骑 && _win.User.Roles.FindIndex(p => p.RoleId == role.RoleId) == 2)
        {
            await SaveEquipSuit(emSuitType.效率, role);
        }
        if (curEquipSuitType == emSuitType.MF)
        {
         //   await LoadSuit(emSuitType.MF, role);
        }
        return curEquip;
    }

    private bool AdjustRings(RoleModel role, Dictionary<emEquipSort, EquipModel> curEquips,
     Dictionary<emEquipSort, EquipModel> toWearEquips, EquipSuit suit)
    {
        // 1. 计算基础施法速度（不含戒指）
        var baseFcr = 5M;
        var idealFcr = suit.IdealFcr;

        var group = role.GetGroup();
        var dk = group.FirstOrDefault(p => p.Job == emJob.死骑);
        if (dk != null) baseFcr += dk.YonghengSpeed;

        var curFcr = baseFcr;

        // 2. 更新非戒指装备
        foreach (var to in toWearEquips.Where(p => p.Key != emEquipSort.戒指1 && p.Key != emEquipSort.戒指2))
        {
            curEquips[to.Key] = to.Value;
        }

        // 3. 计算非戒指装备提供的FCR
        foreach (var cur in curEquips.Where(p => p.Key != emEquipSort.戒指1 && p.Key != emEquipSort.戒指2))
        {
            var s = AttributeMatchUtil.GetBaseAttValue(emAttrType.施法速度, cur.Value?.Content).Item2;
            curFcr += s;
        }


        var allRings = EquipUtil.QueryEquipInRepo().Where((a, b) => a.AccountName == _win.User.AccountName && a.RoleID == 0 && a.Category == "戒指" && a.Content.Contains("所有技能") && !a.EquipName.Contains("乔丹")).ToList();

        if (curEquips.ContainsKey(emEquipSort.戒指1)) allRings.Add(curEquips[emEquipSort.戒指1]);
        if (curEquips.ContainsKey(emEquipSort.戒指2)) allRings.Add(curEquips[emEquipSort.戒指2]);

        if (toWearEquips.ContainsKey(emEquipSort.戒指1)) allRings.Add(toWearEquips[emEquipSort.戒指1]);
        if (toWearEquips.ContainsKey(emEquipSort.戒指2)) allRings.Add(toWearEquips[emEquipSort.戒指2]);
        allRings = allRings.Where(p => p.CanWear(role) && p != null).Distinct().ToList();
        // 5. 找出所有可能的戒指组合（最多2个戒指）
        var allCombinations = GenerateAllRingCombinations(allRings);

        // 6. 评估并选择最佳组合
        var (bestCombination, finalFcr) = SelectBestRingCombination(allCombinations, curFcr, idealFcr);
        var currentRing1 = curEquips[emEquipSort.戒指1];
        var currentRing2 = curEquips[emEquipSort.戒指2];

        // 7. 处理待穿戴戒指
        ProcessToWearEquips(toWearEquips, bestCombination, currentRing1, currentRing2);

        // 8. 返回是否满足FCR要求
        return finalFcr >= idealFcr;
    }

    private List<List<EquipModel>> GenerateAllRingCombinations(List<EquipModel> allRings)
    {
        var combinations = new List<List<EquipModel>>();



        // 双戒指组合
        for (int i = 0; i < allRings.Count; i++)
        {
            for (int j = i + 1; j < allRings.Count; j++)
            {
                combinations.Add(new List<EquipModel> { allRings[i], allRings[j] });
            }
        }

        return combinations;
    }

    //private (List<EquipModel> bestCombo, decimal finalFcr) SelectBestRingCombination(
    //    List<List<EquipModel>> allCombinations, decimal curFcr, int idealFcr)
    //{
    //    List<EquipModel> bestCombo = new List<EquipModel>();
    //    //施法速度
    //    decimal bestFcr = curFcr;
    //    //最佳的全能数量
    //    int bestQuanNengCount = 0;
    //    //最佳的技能戒指数量
    //    int bestSkillCount = 0;
    //    //最佳的绑定戒指数量
    //    int bestBindingCount = 0;
    //    //是否满足fcr最低要求
    //    bool hasMetFcrRequirement = false;

    //    foreach (var combo in allCombinations)
    //    {
    //        var (comboFcr, quanNengCount, skillCount, bindingCount) = EvaluateRingCombination(combo, curFcr);

    //        bool meetsFcr = comboFcr >= idealFcr;
    //        bool isBetter = false;

    //        if (!hasMetFcrRequirement)
    //        {
    //            if (meetsFcr)
    //            {
    //                hasMetFcrRequirement = true;
    //                isBetter = true;
    //            }
    //            else
    //            {
    //                isBetter = comboFcr > bestFcr ||
    //                          (comboFcr == bestFcr && quanNengCount > bestQuanNengCount) ||
    //                          (comboFcr == bestFcr && quanNengCount == bestQuanNengCount && skillCount > bestSkillCount) ||
    //                          (comboFcr == bestFcr && quanNengCount == bestQuanNengCount && skillCount > bestSkillCount) && bindingCount > bestBindingCount;
    //            }
    //        }
    //        else
    //        {
    //            isBetter = meetsFcr &&
    //                      (quanNengCount > bestQuanNengCount ||
    //                       (quanNengCount == bestQuanNengCount && skillCount > bestSkillCount) ||
    //                         (quanNengCount == bestQuanNengCount && skillCount > bestSkillCount) && bindingCount > bestBindingCount);
    //        }

    //        if (isBetter)
    //        {
    //            bestCombo = combo;
    //            bestFcr = comboFcr;
    //            bestQuanNengCount = quanNengCount;
    //            bestSkillCount = skillCount;
    //            bestBindingCount = bindingCount;
    //        }
    //    }

    //    return (bestCombo, bestFcr);
    //}

    private (List<EquipModel> bestCombo, decimal finalFcr) SelectBestRingCombination(
    List<List<EquipModel>> allCombinations, decimal curFcr, int idealFcr)
    {
        // 先评估所有组合
        var evaluatedCombos = allCombinations
            .Select(combo =>
            {
                var eval = EvaluateRingCombination(combo, curFcr);
                return (combo, eval.comboFcr, eval.quanNengCount, eval.skillCount, eval.bindingCount, eval.weight);
            })
            .ToList();

        // 找出所有满足FCR最低要求的组合
        var validCombos = evaluatedCombos
            .Where(x => x.comboFcr >= idealFcr)
            .ToList();

        if (validCombos.Any())
        {
            // 在满足FCR的组合中，按优先级选择最优
            var best = validCombos
                .OrderByDescending(x => x.quanNengCount)  // 1. 最多全能
                .ThenByDescending(x => x.skillCount)      // 2. 其次最多技能
                .ThenByDescending(x => x.weight)            // 3.权重
                .ThenByDescending(x => x.bindingCount)    // 4. 最后最多绑定
                .First();

            return (best.combo, best.comboFcr);
        }
        else
        {
            // 没有满足FCR的组合时，选择最接近的组合（但不会达到idealFcr）
            var closest = evaluatedCombos
                .OrderByDescending(x => x.comboFcr)       // 最接近FCR要求
                .ThenByDescending(x => x.quanNengCount)   // 然后最多全能
                .ThenByDescending(x => x.skillCount)      // 然后最多技能
                  .ThenByDescending(x => x.weight)            // 3.权重
                .ThenByDescending(x => x.bindingCount)    // 4. 最后最多绑定
                .First();

            return (closest.combo, closest.comboFcr);
        }
    }

    /// <summary>
    /// 评估戒指组合
    /// </summary>
    /// <param name="combo"></param>
    /// <param name="baseFcr"></param>
    /// <returns>返回一个元组 weight为权重</returns>
    private (decimal comboFcr, int quanNengCount, int skillCount, int bindingCount, decimal weight) EvaluateRingCombination(
        List<EquipModel> combo, decimal baseFcr)
    {
        decimal comboFcr = baseFcr;
        int quanNengCount = 0;
        int skillCount = 0;
        int bindingCount = 0;
        decimal weight = 0;

        foreach (var ring in combo)
        {
            if (ring.Content.Contains("施法速度"))
            {
                comboFcr += AttributeMatchUtil.GetBaseAttValue(emAttrType.施法速度, ring.Content).Item2;
            }

            if (ring.EquipName == "全能法戒")
            {
                quanNengCount++;
                skillCount++;
            }
            else if (ring.Content.Contains("所有技能"))
            {
                skillCount++;
            }
            if (ring.Content.Contains("已绑定"))
            {
                bindingCount++;
            }
            weight += CalRingWeight(ring);
        }

        return (comboFcr, quanNengCount, skillCount, bindingCount, weight);
    }

    private decimal CalRingWeight(EquipModel ring)
    {
        var name = ring.EquipName;
        switch (name)
        {
            case "全能法戒":
                return 1000;
            case "希望之翼":
                return 500;
            case "天空之戒":
                return 300;
            case "萤火虫":
                return 250;
            case "布尔凯索之戒":
                return 210;
            case "部落徽记":
                return 200;
        }
        return 1;
    }

    private void ProcessToWearEquips(Dictionary<emEquipSort, EquipModel> toWearEquips,
        List<EquipModel> bestCombination, EquipModel currentRing1, EquipModel currentRing2)
    {
        // 清空现有的待穿戴戒指
        toWearEquips.Remove(emEquipSort.戒指1);
        toWearEquips.Remove(emEquipSort.戒指2);

        if (bestCombination.Count == 0)
            return;

        var index1 = bestCombination.FindIndex(f => f.EquipID == currentRing1.EquipID);

        if (index1 > -1)
        {
            currentRing1 = bestCombination[index1];
            bestCombination.RemoveAt(index1);
        }
        var index2 = bestCombination.FindIndex(f => f.EquipID == currentRing2.EquipID);
        if (index2 > -1)
        {
            currentRing2 = bestCombination[index2];
            bestCombination.RemoveAt(index2);
        }
        if (index1 == -1 && bestCombination.Count > 0)
        {
            toWearEquips[emEquipSort.戒指1] = bestCombination[0];
            bestCombination.RemoveAt(0);
        }
        if (index2 == -1 && bestCombination.Count > 0)
        {
            toWearEquips[emEquipSort.戒指2] = bestCombination[0];
        }
        if (toWearEquips[emEquipSort.戒指1].EquipName == currentRing1.EquipName)
        {
            toWearEquips.Remove(emEquipSort.戒指1);
        }
        if (toWearEquips[emEquipSort.戒指2].EquipName == currentRing2.EquipName)
        {
            toWearEquips.Remove(emEquipSort.戒指2);
        }


    }

    /// <summary>
    /// 
    /// </summary>
    public void CalNecFcrSpeed(UserModel user, RoleModel role, Dictionary<emEquipSort, EquipModel> curEquip, emSkillMode skillMode)
    {
        //侏儒自带
        var basicSpeed = 5M;
        var yonghengSpeed = role.GetGroup().Where(p => p.Job == emJob.死骑).FirstOrDefault().YonghengSpeed;
        basicSpeed += yonghengSpeed;
        foreach (var item in curEquip)
        {
            var speed = AttributeMatchUtil.GetBaseAttValue(emAttrType.施法速度, item.Value.Content).Item2;
            basicSpeed += speed;
        }
        if (skillMode == emSkillMode.献祭 && basicSpeed < 180)
        {
            P.Log($"{user.AccountName} {role.RoleName}施法速度不够", emLogType.FcrLog);
        }
        else if (skillMode == emSkillMode.法师)
        {
            if (curEquip[emEquipSort.主手].EquipName == "自杀支系")
            {
                P.Log($"{user.AccountName} {role.RoleName}献祭BUG", emLogType.FcrLog);
            }
        }
    }

    public void RecordDkYongheng(RoleModel role, Dictionary<emEquipSort, EquipModel> curEquip)
    {
        var eq = curEquip[emEquipSort.副手];
        if (eq.EquipName.Contains("永恒"))
        {
            var baseVal = 15;
            var val = AttributeMatchUtil.GetBaseAttValue(emAttrType.唤醒光环, eq.Content).Item2;
            var g = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.RoleId == role.RoleId).First();
            g.YonghengSpeed = int.Parse((baseVal + (val - 6)).ToString());
            DbUtil.InsertOrUpdate<GroupModel>(g);

        }
        else
        {
            var g = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.RoleId == role.RoleId).First();
            g.YonghengSpeed = 0;
            DbUtil.InsertOrUpdate<GroupModel>(g);
        }


    }

    /// <summary>
    /// 穿戴装备
    /// </summary>
    /// <returns></returns>
    public async Task WearEquipAndRecord(BroWindow win, EquipModel equip, int sort, UserModel account, RoleModel role)
    {
        ReplaceEquipStruct replaceResult = await WearEquip(win, equip, sort, account, role);
        if (replaceResult.IsSuccess)
        {
            if (replaceResult.ReplacedEquip != null)
            {
                // replaceResult.ReplacedEquip.Category = CategoryUtil.GetCategory(replaceResult.ReplacedEquip.EquipBaseName);
                replaceResult.ReplacedEquip.EquipStatus = emEquipStatus.Repo;
                //如果有替换下来的装备，加入到仓库装备中
                FreeDb.Sqlite.InsertOrUpdate<EquipModel>().SetSource(replaceResult.ReplacedEquip).ExecuteAffrows();
            }

            //改变穿戴中的状态
            WrapEquip(equip, account, role, emEquipStatus.Equipped);
            FreeDb.Sqlite.InsertOrUpdate<EquipModel>().SetSource(equip).ExecuteAffrows();
        }
    }


    /// <summary>
    /// 更新当前装备
    /// </summary>
    private async Task UpdateCurEquips(BroWindow win, RoleModel role)
    {
        var response = await win.CallJs($@"getCurEquips()");
        var curEquips = response.Result.ToObject<Dictionary<emEquipSort, EquipModel>>();
        var list = curEquips.Select(p => p.Value).ToList();
        list.ForEach(p => WrapEquip(p, win.User, role, emEquipStatus.Equipped));
        DbUtil.InsertOrUpdate<EquipModel>(list);
        await Task.Delay(1000);
    }

    /// <summary>
    /// 获取匹配的装备套装
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="role"></param>
    /// <param name="equipSuit"></param>
    /// <param name="curEquips"></param>
    /// <returns></returns>
    private EquipSuitMatchStruct MatchEquipSuit(int accountId, RoleModel role, EquipSuit equipSuit, Dictionary<emEquipSort, EquipModel> curEquips, Dictionary<emEquipSort, TradeModel> tradeResult)
    {
        //只查找仓库中的装备
        var registeredEquips = FreeDb.Sqlite.Select<TradeModel>().Where(p => p.TradeStatus == emTradeStatus.Register).ToList();

        var _equipsSelf = EquipUtil.QueryEquipInRepo(role.RoleId).Where((a, b) => a.AccountID == accountId && a.EquipStatus == emEquipStatus.Repo).ToList();
        var _equipsOthers = EquipUtil.QueryEquipInRepo().Where((a, b) => a.AccountID != accountId && a.EquipStatus == emEquipStatus.Repo && a.IsLocal == false).ToList();
        if (registeredEquips != null)
        {
            _equipsOthers = _equipsOthers.Where(p => !registeredEquips.Select(s => s.EquipId).Contains(p.EquipID)).ToList();
        }
        var dicOthers = GetEquipDicWithCategoaryQuality(_equipsOthers);
        var dicSelf = GetEquipDicWithCategoaryQuality(_equipsSelf);
        EquipSuitMatchStruct result = new EquipSuitMatchStruct();
        result.MatchSuitName = equipSuit.SuitName;
        result.ToWearEquips = new Dictionary<emEquipSort, EquipModel>();
        result.ToMakeEquips = new Dictionary<emEquipSort, List<ArtifactMakeStruct>>();
        result.IsSuccess = true;
        List<long> toWearEquipIds = new List<long>();
        for (int j = 0; j < 11; j++)
        {
            if (j == 4)
            {
                P.Log("调试腰带");
            }
            P.Log($"开始匹配{role.RoleName}{(emEquipSort)j}位置的装备", emLogType.AutoEquip);
            EquipModel curEquip = null;
            if (curEquips != null)
                curEquips.TryGetValue((emEquipSort)j, out curEquip);
            SuitInfo equipment = equipSuit.GetEquipBySort((emEquipSort)j);
            if (equipment == null)
            {
                P.Log($"{role.RoleName}{(emEquipSort)j}位置的装备没有找到配置，无需更换!");
                continue;
            }
            AutoEquipMatchDto dto = new AutoEquipMatchDto()
            {
                AccountId = accountId,
                Role = role,
                EmEquipSort = (emEquipSort)j,
                CurEquip = curEquip,
                Equipment = equipment,
                Result = result,
                DbEquipsSelf = _equipsSelf,
                DbEquipOthers = _equipsOthers,
                DbEquipDicOthers = dicOthers,
                DbEquipDicSelf = dicSelf,

            };
            List<EquipModel> matchEquips = GetMatchEquip(dto, tradeResult);

            EquipModel bestEq = matchEquips.Count > 0 ? matchEquips[0] : null;
            if (equipment.IsNecessery && bestEq == null)
            {
                result.IsNecessaryEquipMatch = false;
                result.IsSuccess = false;
            }
            if (bestEq == null)
            {
                P.Log("未找到现成装备");
                continue;
            }
            else
            {
                var aa = _equipsSelf.Remove(bestEq);
            }

            if (curEquip != null && bestEq.EquipID == curEquip.EquipID)
            {
                P.Log($"当前穿戴的装备为找到的最佳装备，无需更换", emLogType.AutoEquip);
                continue;
            }
            else
            {
                P.Log($"找到最佳装备{bestEq.EquipName}，准备更换", emLogType.AutoEquip);

                result.ToWearEquips.Add((emEquipSort)j, bestEq);
                continue;

            }
        }
        result.MatchSkillModel = equipSuit.SkillMode;

        return result;
    }





    /// <summary>
    /// 将装备分类整理用于在匹配阶段大量减小运算量
    /// </summary>
    /// <param name="equips"></param>
    /// <returns></returns>
    private Dictionary<string, List<EquipModel>> GetEquipDicWithCategoaryQuality(List<EquipModel> equips)
    {
        Dictionary<string, List<EquipModel>> d = new Dictionary<string, List<EquipModel>>();
        foreach (var e in equips)
        {
            var key = e.Category + e.emItemQuality.ToString();
            var key1 = "全部" + e.emItemQuality.ToString();
            var key2 = e.Category + "全部";
            SaveEquipToDic(d, key, e);
            SaveEquipToDic(d, key1, e);
            SaveEquipToDic(d, key2, e);
        }
        return d;
    }

    private void SaveEquipToDic(Dictionary<string, List<EquipModel>> dic, string key, EquipModel e)
    {
        if (!dic.ContainsKey(key))
        {
            dic.Add(key, new List<EquipModel>());

        }
        dic[key].Add(e);
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
        var sortEm = (emEquipSort)sort;
        var response = await win.CallJs($@"getCurEquips()");
        if (response.Success)
        {
            curEquips = response.Result.ToObject<Dictionary<emEquipSort, EquipModel>>();

            {
                if (curEquips != null &&
                    curEquips.ContainsKey((emEquipSort)sort) &&
                    ((role.Job == emJob.死骑 && sortEm == emEquipSort.副手) || sortEm == emEquipSort.戒指2))
                {
                    P.Log($"{(emEquipSort)sort}部位当前已穿戴装备，为防止穿戴时部位冲突导致换装失败，优先卸下当前部位装备", emLogType.AutoEquip);
                    var response3 = await win.CallJsWaitReload($@"equipOff({role.RoleId},{sort})", "equip");
                    if (response3.Success)
                    {

                        equip.SetAccountInfo(account);
                        replaceEquipStruct.ReplacedEquip = curEquips[(emEquipSort)sort];
                        replaceEquipStruct.ReplacedEquip.SetAccountInfo(account);
                    }
                }
            }


            P.Log($"{role.RoleName}现在更换{(emEquipSort)sort}位置的装备{equip.EquipName}", emLogType.AutoEquip);
            var response2 = await win.CallJsWaitReload($@"equipOn({role.RoleId},{equip.EquipID})", "equip");
            if (response2.Success)
            {
                equip.SetAccountInfo(account);
                if (curEquips.ContainsKey((emEquipSort)sort))
                {
                    replaceEquipStruct.ReplacedEquip = curEquips[(emEquipSort)sort];
                    replaceEquipStruct.ReplacedEquip.SetAccountInfo(account);
                    replaceEquipStruct.IsSuccess = true;
                }
                else
                {
                    replaceEquipStruct.IsSuccess = false;
                }

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
    /// suitinfo name改为可以配置逗号分隔的数组
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="role"></param>
    /// <param name="sort"></param>
    /// <param name="curEquip"></param>
    /// <param name="targetConfig"></param>
    /// <returns></returns>
    public List<EquipModel> GetMatchEquip(AutoEquipMatchDto dto, Dictionary<emEquipSort, TradeModel> tradeResult)
    {
        List<EquipModel> r = new List<EquipModel>();
        for (int i = 0; i < dto.Equipment.EquipNameArr.Count; i++)
        {
            var eqName = dto.Equipment.EquipNameArr[i];
            var equipConfig = dto.Equipment.GetEquipment(eqName);
            if (equipConfig == null)
            {
                throw new Exception($"{eqName}配置为空");
            }
            var equip = GetMatchEquipBySort(dto, equipConfig, dto.DbEquipsSelf);


            if (equip != null)
            {//排序较高的配置找到了现成装备 只需要找到一件最高排序的
                r.Add(equip);
                break;
            }
            //暂时不给献祭队永恒
            //var isXianji = (dto.Role.Job == emJob.死骑) && (dto.Role.GetRoleSkillMode() == emSkillMode.献祭);
            if (equip == null && equipConfig.IsTrade)
            {

                //整套装备能凑齐才乞讨交易 然后index较大的不会被后续乞讨覆盖 可以直接查库跳过index更大的交易请求
                var filteredList = GetEquipInDic(dto.DbEquipDicOthers, equipConfig);
                var demandEquip = GetMatchEquipBySort(dto, equipConfig, filteredList);


                //没有现成装备 且没有已经登记的需求装备则加入
                if (demandEquip != null)
                {
                    if (!tradeResult.ContainsKey(dto.EmEquipSort))
                    {
                        tradeResult.Add(dto.EmEquipSort, GetTradeMode(dto, demandEquip, eqName));
                    }
                    else if( tradeResult[dto.EmEquipSort] == null)
                    {
                        tradeResult[dto.EmEquipSort]= GetTradeMode(dto, demandEquip, eqName);
                    }
                }
                else if (dto.Equipment.IsNecessery)
                {//这个部位标记为null则表明这套装备找不齐 稍后会跳过
                    if (!tradeResult.ContainsKey(dto.EmEquipSort))
                    {
                        tradeResult.Add(dto.EmEquipSort, null);
                    }
                }
            }
        }

        return r;
    }

    public TradeModel GetTradeMode(AutoEquipMatchDto dto, EquipModel demandEquip, string eqName)
    {

        var eq = new TradeModel
        {
            EquipName = eqName,
            EquipSortName = dto.EmEquipSort.ToString(),
            EquipId = demandEquip.EquipID,
            DemandRoleId = dto.Role.RoleId,
            DemandRoleName = dto.Role.RoleName,
            OwnerAccountName = demandEquip.AccountName,
            DemandAccountName = _win.User.AccountName,
            TradeStatus = emTradeStatus.Register,


        };

        return eq;
    }

    private List<EquipModel> GetEquipInDic(Dictionary<string, List<EquipModel>> dic, Equipment config)
    {
        var keyList = config.CategoryQualityKeyList;
        var list = new List<EquipModel>();
        foreach (var item in dic)
        {
            if (config.CategoryQualityKeyList.Contains(item.Key))
            {
                list.AddRange(item.Value);
            }
        }
        return list;
    }


    /// <summary>
    /// 找到权重最高的那件能穿的装备
    /// </summary>
    /// <param name="role"></param>
    /// <param name="sort"></param>
    /// <param name="curEquip"></param>
    /// <param name="targetConfig"></param>
    /// <returns></returns>
    private EquipModel GetMatchEquipBySort(AutoEquipMatchDto dto, Equipment targetConfig, List<EquipModel> dbEquips)
    {

        List<EquipModel> matchEquips = new List<EquipModel>();
        Dictionary<long, EquipModel> matchEquipMap = new Dictionary<long, EquipModel>();
        Dictionary<long, AttributeMatchReport> matchReports = new Dictionary<long, AttributeMatchReport>();

        P.Log($"开始查询数据库装备", emLogType.AutoEquip);
        List<EquipModel> findEquips = new List<EquipModel>();
        var __equips = dbEquips;
        //这边有性能问题 应该先把装备按category和quality分类 手套&&稀有 手套&&全部
        foreach (var item in __equips)
        {
            if (AttributeMatchUtil.MatchCategory(item, targetConfig.Category) && AttributeMatchUtil.MatchQuallity(item, targetConfig.Quality) && item.CanWear(dto.Role))
            {
                findEquips.Add(item);
            }
        }
        P.Log($"查询数据库装备完成,共找到{findEquips.Count}个装备", emLogType.AutoEquip);
        if (dto.CurEquip != null)
        {
            //由于中断导致的数据异常
            var errorData = findEquips.Where(p => p.EquipID == dto.CurEquip.EquipID);
            if (errorData.Count() > 0)
            {
                var errEquip = errorData.First();
                findEquips.Remove(errEquip);
                var t = FreeDb.Sqlite.Delete<EquipModel>(new EquipModel() { EquipID = errEquip.EquipID }).ExecuteAffrows();
            }
            P.Log($"当前部位已穿戴装备，将此装备一并加入匹配列表参与比较！");
            findEquips.Add(dto.CurEquip);

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
        {
            matchEquips = matchEquipMap.Values.OrderByDescending(p => matchReports[p.EquipID].MatchWeight).ToList();
            //如果当前穿戴的装备的匹配权重和找到的最高权重相当，则不更换装备（将当前装备的放到匹配列表的最前面）
            if (dto.CurEquip != null && matchEquips[0].EquipID != dto.CurEquip.EquipID)
            {
                if (matchReports.TryGetValue(dto.CurEquip.EquipID, out var curEquipReport))
                {
                    if (matchReports[matchEquips[0].EquipID].MatchWeight <= curEquipReport.MatchWeight)
                    {
                        matchEquips.Remove(dto.CurEquip);
                        matchEquips.Insert(0, dto.CurEquip);
                    }
                }
            }
        }
        if (matchEquipMap.Count > 1)
        {
            P.Log($"已找到符合要求的装备，不再匹配后续要求，直接返回查询列表", emLogType.AutoEquip);
        }
        var bestEq = matchEquips.FirstOrDefault(p => p.CanWear(dto.Role));
        var config = targetConfig.Conditions[0];
        if (config.ArtifactBase != emArtifactBase.未知 && bestEq == null)
        {

            if (!dto.Result.ToMakeEquips.ContainsKey(dto.EmEquipSort))
            {
                dto.Result.ToMakeEquips.Add(dto.EmEquipSort, new List<ArtifactMakeStruct>());
            }

            var condition = ArtifactBaseCfg.Instance.GetEquipCondition(config.ArtifactBase);


            //找底子
            var baseEqList = GetMatchEquips(dto.AccountId, condition, dto.Role, out _).ToList();
            if (baseEqList.Count != 0)
            {    //为了满足孔位大于目标可以运用随机打孔公式 所以可能会匹配出来孔位大于目标孔位的装备需要额外筛选下
                var slotList = baseEqList.Where(p => p.emItemQuality == emItemQuality.破碎).ToList();
                var baseList = baseEqList.Where(p => p.emItemQuality == emItemQuality.普通).ToList();
                var slotConfig = condition.DeepCopy();
                slotConfig.Conditions.Where(p => p.AttributeType == emAttrType.凹槽).First().Operate = emOperateType.等于;
                var slotMatchList = slotList.Where(p => AttributeMatchUtil.Match(p, slotConfig, out _));
                var concatList = slotMatchList.Concat(baseList);
                if (concatList.Count() != 0)
                {
                    var baseEq = concatList.FirstOrDefault();
                    dto.Result.ToMakeEquips[dto.EmEquipSort].Add(new ArtifactMakeStruct() { ArtifactBase = config.ArtifactBase, Config = ArtifactBaseCfg.Instance.GetEquipCondition(config.ArtifactBase), EquipBase = baseEq, Seq = config.Seq });
                }

            }


        }
        return bestEq;

    }
    public List<EquipModel> GetMatchEquips(int accountid, Equipment target, RoleModel role, out Dictionary<long, AttributeMatchReport> reportMap)
    {
        List<EquipModel> matchEquips = new List<EquipModel>();
        Dictionary<long, AttributeMatchReport> matchReports = new Dictionary<long, AttributeMatchReport>();

        P.Log($"开始查询数据库装备", emLogType.AutoEquip);
        List<EquipModel> findEquips = new List<EquipModel>();
        var __equips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountID == accountid && p.RoleID == 0).ToList();
        try
        {
            foreach (var item in __equips)
            {
                if (AttributeMatchUtil.MatchCategory(item, target.Category) && AttributeMatchUtil.MatchQuallity(item, target.Quality))
                {
                    if (role != null && !item.CanWear(role)) continue;
                    findEquips.Add(item);
                }
            }
        }
        catch (Exception e)
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
                matchEquips.Add(item);
            }
        }
        reportMap = matchReports;
        if (reportMap.Count > 0)
        {
            var best = reportMap.OrderByDescending(o => o.Value.MatchWeight).First();
            var bestEq = matchEquips.Find(p => p.EquipID == best.Key);
            matchEquips.Remove(bestEq);
            matchEquips.Insert(0, bestEq);
        }

        return matchEquips;
    }

    private EquipSuits GetEquipConfig(emJob job, int level, emSuitType suitType)
    {
        return SuitCfg.Instance.GetEquipmentByJobAndLevel(job, level, suitType);
    }

    public List<EquipModel> MergeEquips(Dictionary<emEquipSort, EquipModel> toWear, Dictionary<emEquipSort, EquipModel> curWear)
    {
        List<EquipModel> equips = new List<EquipModel>();
        for (int i = 0; i < 11; i++)
        {
            if (toWear.TryGetValue((emEquipSort)i, out EquipModel equipModel))
            {
                equips.Add(equipModel);
            }
            else if (curWear.TryGetValue((emEquipSort)i, out EquipModel curEquipModel))
            {
                equips.Add(curEquipModel);
            }
        }

        return equips;
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
public struct ArtifactMakeStruct
{
    public emArtifactBase ArtifactBase;
    public EquipModel EquipBase;
    public ArtifactBaseConfig Config;
    /// <summary>
    /// 制作装备的seq
    /// </summary>
    public int Seq;
}
public struct EquipSuitMatchStruct
{
    public bool IsSuccess;
    public string MatchSuitName;
    public emSkillMode MatchSkillModel;
    public Dictionary<emEquipSort, EquipModel> ToWearEquips;
    //需要制作的神器
    public Dictionary<emEquipSort, List<ArtifactMakeStruct>> ToMakeEquips;



    /// <summary>
    /// 是否必要装备都满足匹配
    /// </summary>
    public bool IsNecessaryEquipMatch;

}





/// <summary>
/// 匹配时用到的业务参数
/// </summary>
public class AutoEquipMatchDto
{
    public AutoEquipMatchDto()
    {

    }
    public int AccountId { get; set; }
    public RoleModel Role { get; set; }

    public emEquipSort EmEquipSort { get; set; }

    public EquipModel CurEquip { get; set; }

    public SuitInfo Equipment { get; set; }

    public EquipSuitMatchStruct Result { get; set; }



    /// <summary>
    /// 自己的装备
    /// </summary>
    public List<EquipModel> DbEquipsSelf { get; set; }
    /// <summary>
    /// 别人的装备
    /// </summary>
    public List<EquipModel> DbEquipOthers { get; set; }

    /// <summary>
    /// 本号仓库的装备分类
    /// </summary>
    public Dictionary<string, List<EquipModel>> DbEquipDicSelf { get; set; }

    public Dictionary<string, List<EquipModel>> DbEquipDicOthers
    {
        get; set;
    }
}


