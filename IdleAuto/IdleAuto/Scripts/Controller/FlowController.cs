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





        public static async Task SendRune()
        {

            var sendDic = new Dictionary<int, int>() { { 27, 1 }, { 28, 1 }, { 26, 1 } };
            foreach (var job in sendDic)
            {
                var dic = new Dictionary<int, int>() { { job.Key, job.Value } };
                await SendRune(dic);
            }


        }
        private static async Task SendRune(Dictionary<int, int> dic)
        {
            var specifiedAccount = RepairManager.NainiuAccounts.Concat(RepairManager.NanfangAccounts);

            //所有资源将汇集到这 为了避免二次验证经常要换号收货
            var reciver = "奶牛苦工72";
            var reciverUser = "绿宝石";
            var sendDic = dic;

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
                    var r1 = await CheckNextAccountRune(user, sendDic);
                    if (r1 == null) continue;
                    curWin = r1;
                }
                if (i != userList.Count - 1)
                {
                    nextUser = new UserModel(userList[i + 1]);
                    //寻找给谁发 没符文的账号直接跳过
                    var r2 = await CheckNextAccountRune(nextUser, sendDic);
                    if (r2 == null) { continue; };

                    await SendRuneToNext(curWin, nextUser.AccountName, sendDic);
                    curWin.Close();
                    curWin = r2;//将当前窗口指向被发送方
                    unfinishTask.IsEnd = true;
                }
                else
                {
                    var reciverAccount = AccountCfg.Instance.Accounts.Where(p => p.AccountName == reciverUser).First();
                    nextUser = new UserModel(reciverAccount);
                    await SendRuneToNext(curWin, nextUser.AccountName, sendDic);
                    unfinishTask.IsEnd = true;
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
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "铁矿石" && p.EquipName == "彩虹刻面" && p.Content.Contains("火焰") && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).ToList();
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
                    await tradeControl.StartTrade(e, "奶牛");
                    await Task.Delay(1500);
                    e.EquipStatus = emEquipStatus.Trading;
                    DbUtil.InsertOrUpdate<EquipModel>(e);
                }
            }
        }

        /// <summary>
        /// 一键拍卖
        /// </summary>
        /// <returns></returns>
        public static async Task SellEquipToAuction()
        {
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "铁矿石" && p.EquipName == "击头者" && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).ToList();
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
                    await tradeControl.PutToAuction(e, 15, 1);
                    await Task.Delay(1500);
                    e.EquipStatus = emEquipStatus.Auction;
                    DbUtil.InsertOrUpdate<EquipModel>(e);
                }
                win.Close();
            }
        }
        public static async Task DealDemandEquip()
        {
            var list = FreeDb.Sqlite.Select<TradeModel>().Where(p => p.TradeStatus == emTradeStatus.Register).ToList().GroupBy(g => g.OwnerAccountName);
            foreach (var item in list)
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
        private static async Task RollOrEquipWangzhe(BroWindow win)
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
            var r2 = e.GetMatchEquips(user.Id, con31Base, out _);
            foreach (var base31 in r2.Values)
            {


                await a.MakeArtifact(emArtifactBase.亡者遗产31, base31, user.FirstRole.RoleId, con31Base, isCheckBag: false);
                if (!AttributeMatchUtil.Match(base31, conPerfect, out _))
                {
                    await ReMakeArtifact(base31, emArtifactBase.亡者遗产31, conPerfect, con31Base, win, user.FirstRole);
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
            var lunhuiList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "轮回").ToList().GroupBy(g => new { g.RoleID, g.RoleName, g.AccountName });
            var tageaoList1 = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "塔格奥之鳞" && p.EquipStatus == emEquipStatus.Repo).ToList();
            var tageaoList2 = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "塔格奥之手" && p.EquipStatus == emEquipStatus.Repo).ToList();
            tageaoList1 = tageaoList1.Where(p => AttributeMatchUtil.Match(p, EmEquipCfg.Instance.GetEquipCondition(emEquip.塔格奥之鳞), out _)).ToList();

            foreach (var item in lunhuiList)
            {
                //身上或者仓库没有 发一件过来

                var local = FreeDb.Sqlite.Select<EquipModel>().Where(p => (p.EquipName == "塔格奥之鳞" && p.RoleID == item.Key.RoleID) || (p.EquipName == "塔格奥之鳞" && p.AccountName == item.Key.AccountName)).ToList();
                var hasTa1 = local.Find(p => AttributeMatchUtil.Match(p, EmEquipCfg.Instance.GetEquipCondition(emEquip.塔格奥之鳞), out _)) != null;
                var hasTa2 = FreeDb.Sqlite.Select<EquipModel>().Where(p => (p.EquipName == "塔格奥之手" && p.RoleID == item.Key.RoleID) || (p.EquipName == "塔格奥之手" && p.AccountName == item.Key.AccountName)).First() != null;
                if (!hasTa1 && tageaoList1.Count > 0)
                {
                    var userName = tageaoList1[0].AccountName;

                    var user = AccountCfg.Instance.GetUserModel(userName);
                    var win = await TabManager.Instance.TriggerAddBroToTap(user);
                    var t = new TradeController(win);
                    await t.StartTrade(tageaoList1[0], item.Key.RoleName);
                    tageaoList1.RemoveAt(0);
                    win.Close();
                }
                if (!hasTa2 && tageaoList2.Count > 0)
                {
                    var userName = tageaoList2[0].AccountName;

                    var user = AccountCfg.Instance.GetUserModel(userName);
                    var win = await TabManager.Instance.TriggerAddBroToTap(user);
                    var t = new TradeController(win);
                    await t.StartTrade(tageaoList2[0], item.Key.RoleName);
                    tageaoList2.RemoveAt(0);
                    win.Close();
                }
                var receiver = AccountCfg.Instance.GetUserModel(item.Key.AccountName);
                var win2 = await TabManager.Instance.TriggerAddBroToTap(receiver);
                var t2 = new TradeController(win2);
                await t2.AcceptAll(win2.User);
                win2.Close();
            }




        }



    }
}
