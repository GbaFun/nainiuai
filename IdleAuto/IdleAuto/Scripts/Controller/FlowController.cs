using AttributeMatch;
using IdleAuto.Configs.CfgExtension;
using IdleAuto.Db;
using IdleAuto.Scripts.Model;
using IdleAuto.Scripts.Utils;
using IdleAuto.Scripts.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CefSharp.DevTools.FedCm;
using CefSharp;

namespace IdleAuto.Scripts.Controller
{
    /// <summary>
    /// 流程控制 写静态方法
    /// </summary>
    public class FlowController
    {

        /// <summary>
        /// 全账号并行任务
        /// </summary>
        /// <param name="size">并行数量</param>
        /// <param name="skip">跳过几个</param>
        /// <param name="act">将window作为入参</param>
        /// <returns></returns>
        public static async Task GroupWork(int size, int skip, Func<BroWindow, Task> act, string[] specifiedAccounts = null)
        {
            var accounts = AccountCfg.Instance.Accounts.Skip(skip);
            var semaphore = new SemaphoreSlim(size);
            var tasks = new List<Task>();

            foreach (var account in accounts)
            {
                if (specifiedAccounts != null)
                {
                    if (!specifiedAccounts.Contains(account.AccountName)) continue;
                }
                // 等待信号量
                await semaphore.WaitAsync();
                Console.WriteLine($"Semaphore acquired: {semaphore.CurrentCount}");
                var user = new UserModel(account);
                var window = await TabManager.Instance.TriggerAddBroToTap(user);
                // 启动任务
                var task = Task.Run(async () =>
                {
                    try
                    {

                        await act(window); // 可能抛出异常
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Task failed: {ex.Message}");
                    }
                    finally
                    {
                        // 确保信号量被释放
                        semaphore.Release();
                        window.Close();
                        Console.WriteLine($"Semaphore released: {semaphore.CurrentCount}");
                    }
                });

                tasks.Add(task);
            }

            // 等待所有任务完成
            await Task.WhenAll(tasks);
            Console.WriteLine("All tasks completed.");


        }

        public static async Task StartMapSwitch()
        {

            var user = AccountController.Instance.User;
            var window = await TabManager.Instance.TriggerAddBroToTap(user);
            var controller = new CharacterController(window);
            await controller.StartSwitchMap();
            window.Close();

        }

        public static async Task StartDailyDungeon(BroWindow win)
        {
            var roleIndex = int.Parse(ConfigUtil.GetAppSetting("DungeonRoleIndex"));
            var controller = new CharacterController(win);
            var targetLv = int.Parse(ConfigUtil.GetAppSetting("DungeonLv"));
            if (RepairManager.NanfangAccounts.Contains(win.User.AccountName))
            {
                targetLv = int.Parse(ConfigUtil.GetAppSetting("DungeonNanfangLv"));
            }
            await controller.StartDungeon(win.GetBro(), win.User.Roles[roleIndex], true, targetLv);
        }

        public static async Task StartMapSwitch(BroWindow window)
        {
            var controller = new CharacterController(window);
            await controller.StartSwitchMap();
        }

        /// <summary>
        /// 加点
        /// </summary>
        /// <returns></returns>
        public static async Task StartAddSkill(BroWindow win)
        {
            var controller = new CharacterController(win);
            await controller.StartAddSkill(win.GetBro(), win.User);

        }
        /// <summary>
        /// 刷新cookie
        /// </summary>
        /// <returns></returns>
        public static async Task RefreshCookie()
        {
            BroTabManager.Instance.ClearBrowsers();
            foreach (var account in AccountCfg.Instance.Accounts)
            {
                AccountController.Instance.User = new UserModel(account);
                var window = await TabManager.Instance.TriggerAddBroToTap(AccountController.Instance.User);
                await Task.Delay(2000);
                window.Close();
            }

        }

        /// <summary>
        /// 同步过滤
        /// </summary>
        /// <returns></returns>
        public static async Task SyncFilter(BroWindow win)
        {
            var control = new CharacterController(win);
            await control.StartSyncFilter(win.GetBro(), win.User);


        }

        public static async Task StartEfficencyMonitor(BroWindow window)
        {
            var control = new EfficiencyController(window);
            await control.StartMonitor(window);

        }

        public static async Task StartInit()
        {
            var user = AccountController.Instance.User;
            var win = await TabManager.Instance.TriggerAddBroToTap(user);
            var charControl = new CharacterController(win);
            await charControl.StartInit();
        }




        /// <summary>
        /// 保存符文表
        /// </summary>
        /// <returns></returns>
        public static async Task SaveRuneMap()
        {
            foreach (var acc in AccountCfg.Instance.Accounts)
            {
                if (acc.AccountName == "铁矿石") continue;
                var user = AccountCfg.Instance.GetUserModel(acc.AccountName);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                var t = new TradeController(win);
                await Task.Delay(1000);
                await t.SaveRuneMap();
                win.Close();
            }
        }
        public static async Task SendRune()
        {
            await SaveRuneMap();
            var sendDic = new Dictionary<int, int>() { { 26, 1 }, { 27, 1 }, { 28, 1 }, { 29, 1 }, { 30, 1 } };
            foreach (var job in sendDic)
            {
                await SendRune(job.Key, job.Value);
            }


        }
        private static async Task SendRune(int rune, int count)
        {
            var specifiedAccount = RepairManager.NainiuAccounts.Concat(RepairManager.NanfangAccounts);

            //所有资源将汇集到这 为了避免二次验证经常要换号收货
            var reciver = "奶牛苦工25";
            var reciverUser = "RasdGch";
            var userList = AccountCfg.Instance.Accounts.Where(p => p.AccountName != reciverUser && specifiedAccount.Contains(p.AccountName)).ToList();
            BroWindow curWin = null, nextWin = null;
            var unfinishTask = FreeDb.Sqlite.Select<TaskProgress>().Where(p => p.Type == emTaskType.RuneTrade && !p.IsEnd).First();
            int continueIndex = 1;
            if (unfinishTask == null)
            {
                unfinishTask = new TaskProgress() { IsEnd = false, UserName = "", Type = emTaskType.RuneTrade };
            }
            else
            {
                continueIndex = userList.FindIndex(p => p.AccountName == unfinishTask.UserName);
            }
            for (int i = continueIndex; i < userList.Count; i++)
            {
                UserModel nextUser = null;
                var user = new UserModel(userList[i]);
                //寻找发送起点 有符文的账号为起点
                if (curWin == null)
                {
                    var targetRune = FreeDb.Sqlite.Select<RuneMapModel>().Where(p => p.AccountName == user.AccountName && p.RuneName == rune).First();
                    if (targetRune.Count == 0) continue;
                    curWin = await TabManager.Instance.TriggerAddBroToTap(user);
                }
                if (i != userList.Count - 1)
                {
                    nextUser = new UserModel(userList[i + 1]);
                    //寻找给谁发 没符文的账号直接跳过
                    var targetRune = FreeDb.Sqlite.Select<RuneMapModel>().Where(p => p.AccountName == nextUser.AccountName && p.RuneName == rune).First();
                    if (targetRune.Count == 0) continue;
                    await SendRuneToNext(curWin, nextUser.AccountName, new Dictionary<int, int> { { rune, 1 } });
                    curWin.Close();
                    curWin = await TabManager.Instance.TriggerAddBroToTap(nextUser);//将当前窗口指向被发送方
                    unfinishTask.IsEnd = true;
                }
                else
                {
                    var reciverAccount = AccountCfg.Instance.Accounts.Where(p => p.AccountName == reciverUser).First();
                    nextUser = new UserModel(reciverAccount);
                    await SendRuneToNext(curWin, nextUser.AccountName, new Dictionary<int, int> { { rune, 1 } });
                    unfinishTask.IsEnd = true;
                    curWin.Close();
                }
                unfinishTask.UserName = nextUser.AccountName;
                DbUtil.InsertOrUpdate<TaskProgress>(unfinishTask);
            }
            unfinishTask.IsEnd = true;
            DbUtil.InsertOrUpdate<TaskProgress>(unfinishTask);

        }
        /// <summary>
        /// 如果下一个号没有这个符文，则没必要发送过去节约san值
        /// </summary>
        /// <param name="nextUser"></param>
        /// <param name="sendDic"></param>
        /// <returns></returns>
        private static async Task<BroWindow> CheckNextAccountRune(UserModel nextUser, Dictionary<int, int> sendDic)
        {
            var win = await TabManager.Instance.TriggerAddBroToTap(nextUser);
            await Task.Delay(1500);
            var t = new TradeController(win);
            var isZero = await t.CheckRuneIsZero(sendDic);
            if (isZero) win.Close();
            return isZero ? null : win;
        }

        private static async Task SendRuneToNext(BroWindow curWin, string reciverUser, Dictionary<int, int> sendDic)
        {
            var role = FreeDb.Sqlite.Select<CharAttributeModel>().Where(p => p.AccountName == reciverUser).First();
            var w = curWin;
            var t = new TradeController(w);
            await Task.Delay(2000);
            await t.AcceptAll(curWin.User);
            await Task.Delay(2000);

            await t.TradeRune(sendDic, role.RoleName, true);

        }

        public static async Task SendEquip()
        {
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "铁矿石" && (p.EquipName.Contains("冰冷转换") || (p.EquipName == "自杀之系") || p.EquipName == "尸体的哀伤") && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).ToList();
            var group = list.GroupBy(g => g.AccountName).ToList();
            foreach (var item in group)
            {
                var accountName = item.Key;
                var u = AccountCfg.Instance.Accounts.Where(p => p.AccountName == accountName).First();
                var user = new UserModel(u);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                var tradeControl = new TradeController(win);
                await Task.Delay(1500);
                foreach (var e in item)
                {
                    await tradeControl.StartTrade(e, "奶牛苦工24");
                    await Task.Delay(1500);
                    e.EquipStatus = emEquipStatus.Trading;
                    DbUtil.InsertOrUpdate<EquipModel>(e);
                }
            }
        }

        public static async Task SendXianji()
        {
            var bingzhuanList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "RasdGch" && p.EquipName.Contains("冰冷转换") && p.Category == "珠宝" && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).Take(4).ToList();
            var zishaList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "RasdGch" && p.EquipName.Contains("自杀支系") && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).Take(1).ToList();
            var shitiList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "RasdGch" && p.EquipName.Contains("尸体的哀伤") && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).Take(1).ToList();
            var xianglianList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "RasdGch" && p.EquipName.Contains("阴影之环") && p.Category == "项链" && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).Take(1).ToList();
            var list = bingzhuanList.Concat(zishaList).Concat(shitiList).Concat(xianglianList);
            var group = list.GroupBy(g => g.AccountName).ToList();
            foreach (var item in group)
            {
                var accountName = item.Key;
                var u = AccountCfg.Instance.Accounts.Where(p => p.AccountName == accountName).First();
                var user = new UserModel(u);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                var tradeControl = new TradeController(win);
                await Task.Delay(1500);
                foreach (var e in item)
                {
                    await tradeControl.StartTrade(e, "奶牛苦工24");
                    await Task.Delay(1500);
                    e.EquipStatus = emEquipStatus.Trading;
                    DbUtil.InsertOrUpdate<EquipModel>(e);
                }
                win.Close();
            }
        }

        /// <summary>
        /// 一键拍卖
        /// </summary>
        /// <returns></returns>
        public static async Task SellEquipToAuction()
        {
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "铁矿石" && p.EquipName == "知识" && p.Category != "死骑面罩" && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).ToList();
            var group = list.GroupBy(g => g.AccountName).ToList();
            foreach (var item in group)
            {
                var accountName = item.Key;
                var u = AccountCfg.Instance.Accounts.Where(p => p.AccountName == accountName).First();
                var user = new UserModel(u);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                var tradeControl = new TradeController(win);
                await Task.Delay(1500);
                foreach (var e in item)
                {
                    await tradeControl.PutToAuction(e, 7, 1);
                    await Task.Delay(1500);
                    e.EquipStatus = emEquipStatus.Auction;
                    DbUtil.InsertOrUpdate<EquipModel>(e);
                }
                win.Close();
            }
        }
        public static async Task DealDemandEquip()
        {
            var list = FreeDb.Sqlite.Select<TradeModel>().Where(p => p.TradeStatus == emTradeStatus.Register).ToList();
            var eqIds = list.Select(s => s.EquipId);
            var equipList = FreeDb.Sqlite.Select<EquipModel>().Where(p => eqIds.Contains(p.EquipID)).ToList();
            var rejectedIdList = equipList.Where(p => p.EquipStatus == emEquipStatus.Equipped).Select(s => s.EquipID).ToList();
            var rejectedList = list.Where(p => rejectedIdList.Contains(p.EquipId)).ToList();
            rejectedList.ForEach(p => p.TradeStatus = emTradeStatus.Rejected);
            DbUtil.InsertOrUpdate<TradeModel>(rejectedList);
            var toTradeList = list.Where(p => !rejectedIdList.Contains(p.EquipId)).GroupBy(g => g.OwnerAccountName);
            foreach (var item in toTradeList)
            {
                var accName = item.Key;
                var acc = AccountCfg.Instance.Accounts.Find(p => p.AccountName == accName);

                var win = await TabManager.Instance.TriggerAddBroToTap(new UserModel(acc));
                var t = new TradeController(win);
                foreach (var e in item)
                {
                    await Task.Delay(2000);
                    var equip = FreeDb.Sqlite.Select<EquipModel>(new long[] { e.EquipId }).First();
                    if (equip.EquipStatus == emEquipStatus.Repo)
                    {
                        var flag = await t.StartTrade(equip, e.DemandRoleName);
                        if (flag) { e.TradeStatus = emTradeStatus.Sent; DbUtil.InsertOrUpdate<TradeModel>(e); }
                    }
                    else
                    {
                        //可能在修车过程中自己用掉了
                        e.TradeStatus = emTradeStatus.Rejected; DbUtil.InsertOrUpdate<TradeModel>(e);
                    }
                }
                win.Close();
            }

            //开始单号修车
            var repairList = FreeDb.Sqlite.Select<TradeModel>().Where(p => p.TradeStatus == emTradeStatus.Sent).ToList().GroupBy(g => g.DemandAccountName);
            foreach (var item in repairList)
            {

                var accName = item.Key;
                var acc = AccountCfg.Instance.Accounts.Find(p => p.AccountName == accName);
                var user = new UserModel(acc);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                var t = new TradeController(win);
                var r1 = await win.CallJs("_char.hasNotice()");
                if (r1.Result.ToObject<bool>())
                {
                    await t.AcceptAll(win.User);
                }
                foreach (var r in item)
                {

                    var control = new EquipController(win);
                    var role = win.User.Roles.Find(p => p.RoleId == r.DemandRoleId);
                    await RepairManager.Instance.AutoEquip(win, control, user, role);
                    if (role.Job == emJob.死灵) await RepairManager.Instance.AddSkillPoint(win, role);
                }
                win.Close();
            }
        }

        public static async Task RollArtifact()
        {
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.Category == "手杖" &&
            (p.Quality == "slot" || p.Quality == "base" || p.Quality == "artifact") && p.Content.Contains("+3 骷髅法师") && p.Content.Contains("支配骷髅")).ToList().GroupBy(g => g.AccountName).ToList();
            var con32Base = ArtifactBaseCfg.Instance.Data[emArtifactBase.亡者遗产32];
            var conPerfect = EmEquipCfg.Instance.Data[emEquip.亡者遗产满roll];
            var con32 = EmEquipCfg.Instance.GetEquipCondition(emEquip.亡者遗产32);
            foreach (var item in list)
            {
                var accName = item.Key;
                var acc = AccountCfg.Instance.Accounts.Find(p => p.AccountName == accName);

                var win = await TabManager.Instance.TriggerAddBroToTap(new UserModel(acc));
                var a = new ArtifactController(win);
                foreach (var e in item)
                {

                    if ((e.emItemQuality == emItemQuality.普通 || e.emItemQuality == emItemQuality.破碎) && AttributeMatchUtil.Match(e, con32Base, out _))
                    {

                        await a.MakeArtifact(emArtifactBase.亡者遗产32, e, win.User.FirstRole.RoleId, con32Base, isCheckBag: false);
                        if (!AttributeMatchUtil.Match(e, conPerfect, out _))
                        {
                            await ReMakeArtifact(e, emArtifactBase.亡者遗产32, conPerfect, con32Base, win, win.User.FirstRole);
                        }

                    }
                    else
                    {
                        if (!AttributeMatchUtil.Match(e, conPerfect, out _))
                        {
                            await ReMakeArtifact(e, emArtifactBase.亡者遗产32, conPerfect, con32Base, win, win.User.FirstRole);
                        }
                    }

                }
                win.Close();
            }
        }


        private static async Task ReMakeArtifact(EquipModel e, emArtifactBase artifactBase, Equipment stopConfig, ArtifactBaseConfig artifactConfig, BroWindow win, RoleModel role)
        {
            var a = new ArtifactController(win);
            var r = new ReformController(win);
            await r.RemoveRune(e, e.RoleID == 0 ? role.RoleId : e.RoleID);
            await Task.Delay(1500);
            await a.MakeArtifact(artifactBase, e, role.RoleId, artifactConfig, isCheckBag: false);
            if (!AttributeMatchUtil.Match(e, stopConfig, out _))
            {
                await ReMakeArtifact(e, artifactBase, stopConfig, artifactConfig, win, role);
            }
        }


        public static async Task RepairNec()
        {
            await ContinueJob(emTaskType.RepairNec, RollOrEquipWangzhe);
        }

        public static async Task ContinueJob(emTaskType taskType, Func<BroWindow, Task> act, List<string> specialAccounts = null)
        {
            var t = FreeDb.Sqlite.Select<TaskProgress>().Where(p => !p.IsEnd && p.Type == taskType).First();
            if (t == null)
            {
                t = new TaskProgress() { Type = taskType, IsEnd = false };
            }
            var accs = AccountCfg.Instance.Accounts;
            var index = accs.FindIndex(p => p.AccountName == t.UserName);
            index = index == -1 ? 0 : index;
            for (int i = index; i < accs.Count; i++)
            {
                var acc = accs[i];
                if (acc.AccountName == "铁矿石") continue;
                if (specialAccounts != null)
                {
                    if (!specialAccounts.Contains(acc.AccountName)) continue;
                }
                var user = new UserModel(acc);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                await Task.Delay(1500);
                t.UserName = acc.AccountName;
                DbUtil.InsertOrUpdate<TaskProgress>(t);
                await act(win);

                win.Close();
            }
            t.IsEnd = true;
            DbUtil.InsertOrUpdate<TaskProgress>(t);

        }

        /// <summary>
        /// 选更好的亡者穿上
        /// </summary>
        /// <returns></returns>
        public static async Task RollOrEquipWangzhe(BroWindow win)
        {
            var user = win.User;
            var con32Base = ArtifactBaseCfg.Instance.GetEquipCondition(emArtifactBase.亡者遗产32);
            var con31Base = ArtifactBaseCfg.Instance.GetEquipCondition(emArtifactBase.亡者遗产31);
            var con32 = EmEquipCfg.Instance.GetEquipCondition(emEquip.亡者遗产32);
            var conPerfect = EmEquipCfg.Instance.Data[emEquip.亡者遗产满roll];
            var e = new EquipController(win);
            var a = new ArtifactController(win);
            var c = new CharacterController(win);

            //将背包中的31底子做完
            var r2 = e.GetMatchEquips(user.Id, con32Base, out _);
            foreach (var base31 in r2.Values)
            {


                await a.MakeArtifact(emArtifactBase.亡者遗产32, base31, user.FirstRole.RoleId, con32Base, isCheckBag: false);
                if (!AttributeMatchUtil.Match(base31, conPerfect, out _))
                {
                    await ReMakeArtifact(base31, emArtifactBase.亡者遗产32, conPerfect, con32Base, win, user.FirstRole);
                }


            }


            //foreach (var role in user.Roles)
            //{

            //    if (role.Job != emJob.死灵) continue;

            //    //跳转装备详情页面
            //    var result = await win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId), "equip");
            //    await Task.Delay(1500);
            //    var response = await win.CallJs($@"getCurEquips()");
            //    var curEquips = response.Result.ToObject<Dictionary<emEquipSort, EquipModel>>();
            //    //穿戴亡者
            //    var curWagezhe = curEquips[emEquipSort.主手];
            //    //主手亡者是否32且完美 然后下一个号
            //    if (AttributeMatchUtil.Match(curWagezhe, con32, out _))
            //    {
            //        if (AttributeMatchUtil.Match(curWagezhe, conPerfect, out _))
            //        {

            //        }
            //        else
            //        {
            //            await ReMakeArtifact(curWagezhe, emArtifactBase.亡者遗产32, conPerfect, con32Base, win, role);

            //        }
            //        await Task.Delay(1500);
            //        await e.AutoEquips(win, win.User, role);
            //        await c.AddSkillPoints(win.GetBro(), role);
            //        continue;
            //    }
            //    //现成32亡者
            //    var r1 = e.GetMatchEquips(user.Id, con32, out _);
            //    if (r1.Count > 0)
            //    {
            //        //现成的32遗产 con32这个配置有问题 决定新增一个神器配置文件
            //        var any32 = r1.Values.Where(p => p.EquipStatus == emEquipStatus.Repo).FirstOrDefault();
            //        if (any32 != null)
            //        {
            //            if (!AttributeMatchUtil.Match(curWagezhe, conPerfect, out _))
            //            {
            //                await ReMakeArtifact(curWagezhe, emArtifactBase.亡者遗产32, conPerfect, con32Base, win, role);
            //            }
            //            await Task.Delay(1500);
            //            await e.AutoEquips(win, win.User, role);
            //            await c.AddSkillPoints(win.GetBro(), role);
            //            continue;
            //        }


            //    }  //没有现成的32 尝试做一把32并roll




            //}
        }

        /// <summary>
        /// 选更好的亡者穿上
        /// </summary>
        /// <returns></returns>
        public static async Task RollTianzai(BroWindow win)
        {
            var e = new EquipController(win);
            var a = new ArtifactController(win);
            var c = new CharacterController(win);
            var con2 = EmEquipCfg.Instance.GetEquipCondition(emEquip.天灾2);
            var conBase = ArtifactBaseCfg.Instance.GetEquipCondition(emArtifactBase.天灾);
            var user = win.User;
            foreach (var role in user.Roles)
            {
                if (role.Job != emJob.死骑) continue;
                //跳转装备详情页面
                var result = await win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId), "equip");
                await Task.Delay(1500);
                var response = await win.CallJs($@"getCurEquips()");
                var curEquips = response.Result.ToObject<Dictionary<emEquipSort, EquipModel>>();
                //穿戴天灾
                var curSort = curEquips[emEquipSort.衣服];
                //是否完美
                if (!AttributeMatchUtil.Match(curSort, con2, out _))
                {
                    await ReMakeArtifact(curSort, emArtifactBase.天灾, con2, conBase, win, role);


                }
            }

        }

        //public static async  Task Reform()

        /// <summary>
        /// 移动塔格奥到有轮回且没成套的死灵 配一套塔格奥
        /// </summary>
        /// <returns></returns>
        public static async Task MoveTaGeAo()
        {

            //按有轮回的号分组
            var lunhuiList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "轮回").ToList().GroupBy(g => new { g.AccountName });
            var tageaoList1 = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "塔格奥之鳞" && p.EquipStatus == emEquipStatus.Repo).ToList();
            var tageaoList2 = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "塔格奥之手" && p.EquipStatus == emEquipStatus.Repo).ToList();
            var bingJingList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "冰晶" && p.EquipStatus == emEquipStatus.Repo).ToList();
            var jiaohuaList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "教化" && p.EquipStatus == emEquipStatus.Repo).ToList();
            var head = bingJingList.Concat(jiaohuaList).ToList();
            tageaoList1 = tageaoList1.Where(p => AttributeMatchUtil.Match(p, EmEquipCfg.Instance.GetEquipCondition(emEquip.塔格奥之鳞), out _)).ToList();

            foreach (var item in lunhuiList)
            {
                //身上或者仓库没有 发一件过来
                var receiver = item.Max(m => m.RoleName);
                var local = FreeDb.Sqlite.Select<EquipModel>().Where(p => (p.EquipName == "塔格奥之鳞" && p.AccountName == item.Key.AccountName)).ToList();
                //塔衣数量
                var localCount1 = local.Where(p => AttributeMatchUtil.Match(p, EmEquipCfg.Instance.GetEquipCondition(emEquip.塔格奥之鳞), out _)).Count();
                var localCount2 = FreeDb.Sqlite.Select<EquipModel>().Where(p => (p.EquipName == "塔格奥之手" && p.AccountName == item.Key.AccountName)).Count();
                var localCount3 = FreeDb.Sqlite.Select<EquipModel>().Where(p => (p.EquipName == "冰晶" && p.AccountName == item.Key.AccountName)).Count();
                var localCount4 = FreeDb.Sqlite.Select<EquipModel>().Where(p => (p.EquipName == "教化" && p.AccountName == item.Key.AccountName)).Count();

                if (item.Count() > localCount1 && tageaoList1.Count > 0)
                {
                    await SendEquipByLunhuiCount(tageaoList1, receiver, item.Key.AccountName);
                }
                if (item.Count() > localCount2 && tageaoList2.Count > 0)
                {
                    await SendEquipByLunhuiCount(tageaoList2, receiver, item.Key.AccountName);
                }
                if (item.Count() > (localCount3 + localCount4) && head.Count > 0)
                {
                    await SendEquipByLunhuiCount(head, receiver, item.Key.AccountName);
                }
                var receiverAccName = AccountCfg.Instance.GetUserModel(item.Key.AccountName);
                var win2 = await TabManager.Instance.TriggerAddBroToTap(receiverAccName);
                var t2 = new TradeController(win2);
                await t2.AcceptAll(win2.User);
                win2.Close();
            }




        }

        private static async Task SendEquipByLunhuiCount(List<EquipModel> eqList, string receiver, string accName)
        {
            var anyEq = eqList.Where(p => p.AccountName != accName).FirstOrDefault();
            if (anyEq == null) return;


            var user = AccountCfg.Instance.GetUserModel(anyEq.AccountName);
            var win = await TabManager.Instance.TriggerAddBroToTap(user);
            var t = new TradeController(win);
            await t.StartTrade(anyEq, receiver);
            eqList.Remove(anyEq);
            win.Close();
        }


        public static async Task MakeLunhui()
        {

            //按有轮回的号分组
            var lunhuiList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "RasdGch" && p.Category == "死灵副手" &&
            (p.Quality == "slot" || p.Quality == "base") && p.Content.Contains("+3 骷髅法师") &&
            (p.Content.Contains("+3 生生不息"))).ToList();
            // var lunhuiList1 = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "RasdGch" && p.Category == "死灵副手" && (p.Quality == "slot" || p.Quality == "base") && p.Content.Contains("+3 生生不息") && (p.Content.Contains("+3 重生") || p.Content.Contains("+3 献祭")) && p.Lv >= 70).ToList().GroupBy(g => new { g.RoleID, g.RoleName, g.AccountName });
            await CollectAndMakeArtifact(emArtifactBase.轮回法师, lunhuiList, "奶牛苦工24", "RasdGch");
            //发给装备了塔手的号
        }
        /// <summary>
        /// 移动底子和制作神器
        /// </summary>
        /// <returns></returns>
        public static async Task CollectAndMakeArtifact(emArtifactBase emBase, List<EquipModel> baseList, string receiver, string receiverAcc)
        {

            var eqBase = ArtifactBaseCfg.Instance.GetEquipCondition(emBase);

            var r = new AttributeMatchReport();
            baseList = baseList.Where(p => AttributeMatchUtil.Match(p, eqBase, out r)).ToList();
            var l = baseList.GroupBy(g => new { g.RoleID, g.RoleName, g.AccountName });
            foreach (var item in l)
            {
                var userName = item.Key.AccountName;
                var user = AccountCfg.Instance.GetUserModel(userName);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                var t = new TradeController(win);
                foreach (var e in item)
                {
                    await t.StartTrade(e, receiver);
                    await Task.Delay(1500);
                }
                win.Close();

            }

            var receiverUserModel = AccountCfg.Instance.GetUserModel(receiverAcc);
            var win2 = await TabManager.Instance.TriggerAddBroToTap(receiverUserModel);
            var t2 = new TradeController(win2);
            await t2.AcceptAll(win2.User);
            await Task.Delay(1500);

            var a = new ArtifactController(win2);
            var roleId = win2.User.FirstRole.RoleId;

            foreach (var item in baseList)
            {
                item.SetAccountInfo(win2.User);
                await a.MakeArtifact(emBase, item, roleId, eqBase, isCheckBag: false);
                await Task.Delay(1000);

            }





        }

        /// <summary>
        /// 升级底子
        /// </summary>
        /// <returns></returns>
        public async static Task UpgradeBaseEq()
        {
            var eqList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "戒指" && p.Quality == "base" && p.EquipStatus == emEquipStatus.Repo).ToList();
            var group = eqList.GroupBy(g => new { g.AccountName });
            foreach (var acc in group)
            {
                var bindedList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName == acc.Key.AccountName && p.EquipBaseName == "戒指" && p.Content.Contains("所有技能") && p.Content.Contains("绑定")).ToList();
                if (bindedList.Count >= 24) continue;
                var user = AccountCfg.Instance.GetUserModel(acc.Key.AccountName);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                win.GetBro().ShowDevTools();
                var r = new ReformController(win);
                var curEqList = eqList.Where(p => p.AccountName == user.AccountName).ToList();
                await r.UpgradeBaseEquip(win.User.FirstRole, curEqList);
                curEqList.ForEach(p => { p.EquipStatus = emEquipStatus.Upgraded; });
                DbUtil.InsertOrUpdate<EquipModel>(curEqList);
                win.Close();

            }
        }

        public async static Task InitGroup()
        {
            foreach (var acc in AccountCfg.Instance.Accounts)
            {

                var user = AccountCfg.Instance.GetUserModel(acc.AccountName);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                for (int index = 0; index < win.User.Roles.Count; index++)
                {
                    var teamIndex = int.Parse(Math.Floor(index / 3.0).ToString());
                    var role = win.User.Roles[index];
                    var g = new GroupModel() { AccountName = acc.AccountName, Job = role.Job, RoleId = role.RoleId, RoleName = role.RoleName, TeamIndex = teamIndex };
                    DbUtil.InsertOrUpdate<GroupModel>(g);

                }
                win.Close();
            }

        }

        public async static Task PassDungeon(int testLv, int dungeonLv)
        {
            var groupList = FreeDb.Sqlite.Select<GroupModel>().ToList();
            foreach (var acc in AccountCfg.Instance.Accounts)
            {
                if (acc.AccountName == "铁矿石") continue;
                if (RepairManager.NanfangAccounts.Contains(acc.AccountName)) break;
                var matchRoles = groupList.Where(p => p.AccountName == acc.AccountName);
                //该账号下目标秘境全过了
                if (matchRoles.Where(p => p.DungeonPassedLv < dungeonLv).Count() == 0) continue;
                var user = AccountCfg.Instance.GetUserModel(acc.AccountName);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                var c = new CharacterController(win);
                await Task.Delay(1000);
                int curTeamIndex = -1;
                for (int index = 0; index < win.User.Roles.Count; index++)
                {
                    var teamIndex = int.Parse(Math.Floor(index / 3.0).ToString());
                    if (curTeamIndex != -1 && teamIndex != curTeamIndex)
                    {
                        //不是当前分组 暂停
                        break;
                    }
                    var role = win.User.Roles[index];
                    var g = groupList.Where(p => p.RoleId == role.RoleId).FirstOrDefault();
                    if (g.DungeonPassedLv >= dungeonLv) continue;
                    bool isPass = await c.SwitchMap(role, testLv, dungeonLv, g);
                    await Task.Delay(1000);
                    //开始移动装备过来 等待他过本
                    if (!isPass)
                    {
                        await MoveArtifact(role.RoleName, acc.AccountName);

                        curTeamIndex = teamIndex;
                        await AutoEquipSpecificJob(win, teamIndex, acc.AccountName);

                    }

                }
                win.Close();
                break;
            }
        }

        public static async Task MoveArtifact(string roleName, string accName)
        {
            var clothConfig = EmEquipCfg.Instance.GetEquipCondition(emEquip.骑士技能衣服);
            var feilong = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipID == RepairManager.PublicFeilongId).First();
            var yongheng = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipID == RepairManager.PublicYonghengId).First();
            var curEquips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName == accName).ToList();
            if (feilong.RoleID > 0)
            {
                var user = AccountCfg.Instance.GetUserModel(feilong.AccountName);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                //脱
                var e = new EquipController(win);
                var role = win.User.Roles.Where(p => p.RoleId == feilong.RoleID).First();
                var anyCloth = e.GetMatchEquips(feilong.AccountID, clothConfig, out _).FirstOrDefault();
                //得跳转换装页面
                await e.WearEquip(win, anyCloth.Value, (int)emEquipSort.衣服, win.User, role);
            }
            if (feilong.AccountName != accName)
            {
                var user = AccountCfg.Instance.GetUserModel(feilong.AccountName);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                var t = new TradeController(win);
                await t.StartTrade(feilong, roleName);
                await t.StartTrade(yongheng, roleName);
                win.Close();
                var receiver = AccountCfg.Instance.GetUserModel(accName);
                var win2 = await TabManager.Instance.TriggerAddBroToTap(receiver);
                var t2 = new TradeController(win2);
                await t2.AcceptAll(win2.User);
                win2.Close();
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curRole">当前角色</param>
        /// <param name="groupIndex">分组index</param>
        /// <returns></returns>
        private static async Task AutoEquipSpecificJob(BroWindow win, int teamIndex, string accName)
        {
            var groupList = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.AccountName == accName && p.TeamIndex == teamIndex).ToList();
            var knightId = groupList.Where(p => p.Job == emJob.骑士).FirstOrDefault().RoleId;
            var dkId = groupList.Where(p => p.Job == emJob.死骑).FirstOrDefault().RoleId;
            var knight = win.User.Roles.Where(p => p.RoleId == knightId).FirstOrDefault();
            var dk = win.User.Roles.Where(p => p.RoleId == dkId).FirstOrDefault();
            var dkEquips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.RoleID == dk.RoleId).ToList();
            var knightEquips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.RoleID == knight.RoleId).ToList();
            var e = new EquipController(win);
            if (dkEquips.Where(p => p.EquipName.Contains("永恒")).Count() == 0)
            {
                await e.AutoEquips(win, win.User, dk);
            }
            if (knightEquips.Where(p => p.EquipName.Contains("飞龙")).Count() == 0)
            {
                await e.AutoEquips(win, win.User, knight);
            }
        }




    }
}
