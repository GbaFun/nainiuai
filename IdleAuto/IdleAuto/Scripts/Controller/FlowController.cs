﻿using AttributeMatch;
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
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using IdleAuto.Scripts.View;
using IdleAuto.Scripts.Service;

namespace IdleAuto.Scripts.Controller
{
    /// <summary>
    /// 流程控制 写静态方法
    /// </summary>
    public class FlowController
    {
        static Equipment conCold = EmEquipCfg.Instance.Data[emEquip.冰霜珠宝];
        static Equipment conPhysic = EmEquipCfg.Instance.Data[emEquip.物理珠宝];
        static Equipment conPoison = EmEquipCfg.Instance.Data[emEquip.刺客毒素黄珠宝];
        static List<Equipment> whiteList = new List<Equipment>() { conCold, conPhysic, conPoison };
        /// <summary>
        /// 获取需要制作的神器数量
        /// </summary>
        /// <returns></returns>
        public static int GetArtifactCount()
        {
            return int.Parse(MenuInstance.SecondForm.TxtArtifactCount.Text.Trim());
        }

        /// <summary>
        /// 是否收集底子
        /// </summary>
        /// <returns></returns>
        public static bool IsCollectBase()
        {
            return bool.Parse(MenuInstance.SecondForm.ComCollectBase.SelectedValue.ToString());
        }

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
            if (win.User.AccountName.StartsWith("0"))
            {
                roleIndex = 5;
            }
            await controller.StartDailyDungeon(win.User.Roles[roleIndex], targetLv);
        }
        public static async Task StartNainiu(BroWindow win)
        {

            var controller = new CharacterController(win);

            await controller.StartNainiu(win.User.Roles[0]);
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
        public static async Task SaveRuneMap(BroWindow win)
        {
            var t = new TradeController(win);
            await Task.Delay(1000);
            await t.SaveRuneMap();
        }
        public static async Task SendRune()
        {
            await FlowController.GroupWork(3, 1, FlowController.SaveRuneMap);
            var sendDic = new Dictionary<int, int>() { { 21, 1 }, { 25, 1 } };
            // var sendDic = new Dictionary<int, int>() { { 33, 1 } };
            //var sendDic = new Dictionary<int, int>() { { 26, 1 }, { 27, 1 }, { 28, 1 }, { 29, 1 },{32,1 } };
            //  var sendDic = new Dictionary<int, int>() { { 21, 1 }, { 19, 1 } };
            //var sendDic = new Dictionary<int, int>() { { 28, 1 },{ 29, 1 } };
            foreach (var job in sendDic)
            {
                await SendRune(job.Key, job.Value);
            }


        }
        private static async Task SendRune(int rune, int count)
        {
            var specifiedAccount = RepairManager.ActiveAcc;

            //所有资源将汇集到这 为了避免二次验证经常要换号收货
            var reciver = RepairManager.RepoRole;
            var reciverUser = RepairManager.RepoAcc;
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
            await Task.Delay(1000);
            await t.AcceptAll();

            await t.TradeRune(sendDic, role.RoleName, true);

        }

        public static async Task SendEquip(List<EquipModel> eqList = null)
        {
            string[] conditions = MenuInstance.SecondForm.TxtSendEqCon.Text.Split(',');

            string numStr = MenuInstance.SecondForm.TxtSendEqNum.Text.Trim();
            var num = 1;
            if (numStr != "")
            {
                num = int.Parse(numStr);
            }
            var toSend = MenuInstance.SecondForm.TxtRoleToSend.Text.Trim();
            var toSendRole = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.RoleName == toSend).First();
            Expression<Func<EquipModel, bool>> exp = (a) => a.IsLocal == false
            && (a.EquipStatus == emEquipStatus.Repo || a.EquipStatus == emEquipStatus.Package);
            if (toSendRole != null)
            {
                exp = exp.And(p => p.AccountName != toSendRole.AccountName);
            }
            exp = exp.And(p => !(p.AccountName == "南方仓库" && p.EquipName.Contains("精华")));
            var eqName = conditions[0];
            exp = exp.And(p => p.EquipName.Contains(eqName));
            if (conditions.Length > 1)
            {
                for (int i = 1; i < conditions.Length; i++)
                {
                    var con = conditions[i];
                    if (con == "非太古")
                    {
                        exp = exp.And(p => p.IsPerfect == false);
                    }
                    else if (con == "南方")
                    {
                        exp = exp.And(p => p.AccountName != "南方仓库");
                    }
                    else
                    {
                        exp = exp.And(p => p.Content.Contains(con));
                    }

                }

            }
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(exp).Take(num).ToList();
            if (eqList != null)
            {
                list = eqList;
            }
            //var usefulList = FreeDb.Sqlite.Select<UsefulEquip>().Where(p => p.EquipName.Contains("权杖") && p.Content.Contains("+3 狂热") && p.Content.Contains("+3 审判")
            //&& (p.Quality == "base" || p.Quality == "slot") && p.EquipStatus == emEquipStatus.Repo && p.AccountName != RepairManager.RepoAcc).ToList();
            // var list = usefulList.ToObject<List<EquipModel>>();
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
                    await tradeControl.StartTrade(e, MenuInstance.SecondForm.TxtRoleToSend.Text.Trim());
                    await Task.Delay(1500);
                    e.EquipStatus = emEquipStatus.Trading;
                    DbUtil.InsertOrUpdate<EquipModel>(e);
                }
                win.Close();
            }
        }

        /// <summary>
        /// 发送一套法师套到dk
        /// </summary>
        /// <returns></returns>
        public static async Task SendMageSuitToDk()
        {
            string[] conditions = MenuInstance.SecondForm.TxtSendEqCon.Text.Split(',');
            var list = new List<EquipModel>();



            var toSend = MenuInstance.SecondForm.TxtRoleToSend.Text.Trim();
            var toSendRole = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.RoleName == toSend).First();
            Expression<Func<EquipModel, bool>> exp = (a) => a.Content.Contains("水瓶座") && a.IsLocal == false && a.EquipStatus == emEquipStatus.Repo;
            if (toSendRole != null)
            {
                exp = exp.And(p => p.AccountName != toSendRole.AccountName);
            }

            var eqList = FreeDb.Sqlite.Select<EquipModel>().Where(exp).ToList();
            var shoes = eqList.Find(p => p.EquipName == "雷网之径");
            var cloth = eqList.Find(p => p.EquipName == "元素掌御者");
            var glove = eqList.Find(p => p.EquipName == "熔岩之泉");
            var head = eqList.Find(p => p.EquipName == "冻结之晶");
            if (shoes != null)
            {
                list.Add(shoes);
            }
            if (cloth != null)
            {
                list.Add(cloth);
            }
            if (glove != null)
            {
                list.Add(glove);
            }
            if (head != null)
            {
                list.Add(head);
            }
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
                    await tradeControl.StartTrade(e, MenuInstance.SecondForm.TxtRoleToSend.Text.Trim());
                    await Task.Delay(1500);
                    e.EquipStatus = emEquipStatus.Trading;
                    DbUtil.InsertOrUpdate<EquipModel>(e);
                }
                win.Close();
            }
        }

        public static async Task SendXianji(string targetAcc, string targetRole)
        {

            var bingzhuanList = FreeDb.Sqlite.Select<EquipModel>().Where(p => !p.Content.Contains("增强伤害") && p.EquipName.Contains("冰冷转换") && p.Category == "珠宝" && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).ToList();

            if (bingzhuanList.Count == 0) throw new Exception("冰转不够");
            var zishaList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName.Contains("自杀支系") && p.EquipStatus == emEquipStatus.Repo && !p.EquipName.Contains("无形") && p.IsLocal == false).ToList();
            //var shitiList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName.Contains("尸体的哀伤") && p.EquipStatus == emEquipStatus.Repo && !p.EquipName.Contains("无形") && p.IsLocal == false).ToList();
            var xianglianList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName.Contains("阴影之环") && p.Category == "项链" && !p.EquipName.Contains("无形") && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).ToList();
            var xianglianList2 = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.Content.Contains("学徒") && p.Content.Contains("施法者") && p.Category == "项链" && p.Quality == "craft" && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).ToList();
            xianglianList = xianglianList.Concat(xianglianList2).ToList();


            var jiaohuaList = EquipUtil.QueryEquipInRepo().Where((a, b) => a.EquipName == "教化" && a.EquipStatus == emEquipStatus.Repo && a.IsLocal == false).ToList();
            var hufuList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.Content.Contains("重生最大召唤数量") && p.Category == "护符" && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).ToList();
            //var yaodaiList = FreeDb.Sqlite.Select<EquipModel>().Where(p => (p.EquipName == "浩劫复生" || p.EquipName == "邪灵之束缚") && p.Category == "腰带" && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).ToList();
            //var rings = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.Content.Contains("施法速度") && p.Category == "戒指" && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).ToList();
            var list = new List<EquipModel>();

            var localBingzhuanList = bingzhuanList.Where(p => p.AccountName == targetAcc).ToList();
            if (localBingzhuanList.Count < 1)
            {
                list.AddRange(bingzhuanList.Where(p => p.AccountName != targetAcc).Take(1 - localBingzhuanList.Count));
            }
            var localZishaList = zishaList.Where(p => p.AccountName == targetAcc && p.EquipStatus == emEquipStatus.Repo).ToList();
            if (localZishaList.Count == 0)
            {
                list.AddRange(zishaList.Take(1));
            }
            //var localRings = rings.Where(p => p.AccountName == targetAcc && p.EquipStatus == emEquipStatus.Repo).ToList();
            //if (localRings.Count < 2)
            //{
            //    list.AddRange(rings.Where(p => p.AccountName != targetAcc).Take(2 - localRings.Count));
            //}
            //var localShitiList = shitiList.Where(p => p.AccountName == targetAcc && p.EquipStatus == emEquipStatus.Repo).ToList();
            //if (localShitiList.Count == 0)
            //{
            //    list.AddRange(shitiList.Take(1));
            //}
            var localXianglianList = xianglianList.Where(p => p.AccountName == targetAcc && p.EquipStatus == emEquipStatus.Repo).ToList();
            if (localXianglianList.Count == 0)
            {

                list.AddRange(xianglianList.Take(1));

            }

            var localJiaohua = jiaohuaList.Where(p => p.AccountName == targetAcc && p.EquipStatus == emEquipStatus.Repo).ToList();
            if (localJiaohua.Count == 0)
            {
                list.AddRange(jiaohuaList.Take(1));
            }
            var localHufuList = hufuList.Where(p => p.AccountName == targetAcc && p.EquipStatus == emEquipStatus.Repo).ToList();
            if (localHufuList.Count == 0)
            {
                list.AddRange(hufuList.Take(1));
            }
            //var localYaodaiList = yaodaiList.Where(p => p.AccountName == targetAcc && p.EquipStatus == emEquipStatus.Repo).ToList();
            //if (localYaodaiList.Count == 0)
            //{
            //    list.AddRange(yaodaiList.Take(1));
            //}
            await SendEquip(list, targetRole, targetAcc);
        }

        public static async Task SendEquip(List<EquipModel> list, string targetRoleName, string targetAcc)
        {
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
                    await tradeControl.StartTrade(e, targetRoleName);
                    await Task.Delay(1500);
                    e.EquipStatus = emEquipStatus.Trading;
                    DbUtil.InsertOrUpdate<EquipModel>(e);
                }
                win.Close();
            }
            if (list.Count > 0)
            {
                var user2 = AccountCfg.Instance.GetUserModel(targetAcc);
                var win2 = await TabManager.Instance.TriggerAddBroToTap(user2);
                var t = new TradeController(win2);
                await t.AcceptAll();
                win2.Close();
            }
        }

        /// <summary>
        /// 一键拍卖
        /// </summary>
        /// <returns></returns>
        public static async Task SellEquipToAuction()
        {
            var dic = new Dictionary<string, int>();
            dic.Add("真理之心", 23);
            dic.Add("永恒之悟", 21);
            dic.Add("至善之眼", 23);
            dic.Add("血红之白热的珠宝", 30);
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "铁矿石" && p.Content.Contains("飞马座") && !p.Content.Contains("无形") && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false && p.IsPerfect == false).ToList();
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
                    var price = dic[e.EquipName];
                    await tradeControl.PutToAuction(e, price, 1);
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
            var rejectedIdList = equipList.Where(p => !(p.EquipStatus == emEquipStatus.Repo || p.EquipStatus == emEquipStatus.Package)).Select(s => s.EquipID).ToList();
            var rejectRoleList = list.Where(p => rejectedIdList.Contains(p.EquipId)).ToList().Select(s => s.DemandRoleId).Distinct();
            var rejectedList = FreeDb.Sqlite.Select<TradeModel>().Where(p => rejectRoleList.Contains(p.DemandRoleId)).ToList();
            rejectedList.ForEach(p => { p.TradeStatus = emTradeStatus.Rejected; });
            DbUtil.InsertOrUpdate<TradeModel>(rejectedList);
            //按需求人分组
            var toTradeList = list.Where(p => !rejectedIdList.Contains(p.EquipId)).GroupBy(g => new { g.DemandRoleId, g.DemandRoleName, g.DemandAccountName });
            foreach (var demandRole in toTradeList)
            {
                //对拥有账号分组一次性发来
                var ownerGroup = demandRole.GroupBy(g => new { g.OwnerAccountName });
                foreach (var targetItem in ownerGroup)
                {

                    var owner = AccountCfg.Instance.GetUserModel(targetItem.Key.OwnerAccountName);
                    var win = await TabManager.Instance.TriggerAddBroToTap(owner);
                    var t = new TradeController(win);
                    foreach (var e in targetItem)
                    {
                        await Task.Delay(1000);
                        var equip = FreeDb.Sqlite.Select<EquipModel>(new long[] { e.EquipId }).First();

                        var flag = await t.StartTrade(equip, e.DemandRoleName);
                        if (flag) { e.TradeStatus = emTradeStatus.Sent; DbUtil.InsertOrUpdate<TradeModel>(e); }

                    }
                    win.Close();
                }
                var acc = AccountCfg.Instance.Accounts.Find(p => p.AccountName == demandRole.Key.DemandAccountName);
                var user = new UserModel(acc);
                var win2 = await TabManager.Instance.TriggerAddBroToTap(user);
                var t2 = new TradeController(win2);
                var r1 = await win2.CallJs("_char.hasNotice()");
                if (r1.Result.ToObject<bool>())
                {
                    await t2.AcceptAll();
                }


                var control = new EquipController(win2);
                var role = win2.User.Roles.Find(p => p.RoleId == demandRole.Key.DemandRoleId);
                var roleIndex = win2.User.Roles.FindIndex(p => p.RoleId == role.RoleId);



                await RepairManager.Instance.AutoRepair(win2, role);




                win2.Close();

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
            var r2 = e.GetMatchEquips(user.AccountName, con32Base, null, out _);
            foreach (var base31 in r2)
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
                await t2.AcceptAll();
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
            var lunhuiList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.Lv >= 70 && p.Category == "死灵副手" &&
            (p.Quality == "slot" || p.Quality == "base") && p.Content.Contains("+3 骷髅法师") &&
            (p.Content.Contains("+3 支配骷髅")) && p.EquipStatus == emEquipStatus.Repo).ToList();
            // var lunhuiList1 = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "RasdGch" && p.Category == "死灵副手" && (p.Quality == "slot" || p.Quality == "base") && p.Content.Contains("+3 生生不息") && (p.Content.Contains("+3 重生") || p.Content.Contains("+3 献祭")) && p.Lv >= 70).ToList().GroupBy(g => new { g.RoleID, g.RoleName, g.AccountName });
            await CollectAndMakeArtifact(emArtifactBase.轮回法师, lunhuiList, RepairManager.RepoRole, RepairManager.RepoAcc);
            //发给装备了塔手的号
        }

        public static async Task MakeYongheng()
        {
            Expression<Func<EquipModel, bool>> exp = (p) => p.Lv >= 70 && p.Category == "斧" &&
         (p.Quality == "slot" || p.Quality == "base") && (p.Content.Contains("凹槽(0/5)")) && p.EquipName.Contains("双头斧") && !p.Content.Contains("+15") && p.EquipStatus == emEquipStatus.Repo;
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(exp.And(p => p.AccountName != RepairManager.RepoAcc)).ToList();
            // var lunhuiList1 = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "RasdGch" && p.Category == "死灵副手" && (p.Quality == "slot" || p.Quality == "base") && p.Content.Contains("+3 生生不息") && (p.Content.Contains("+3 重生") || p.Content.Contains("+3 献祭")) && p.Lv >= 70).ToList().GroupBy(g => new { g.RoleID, g.RoleName, g.AccountName });
            await CollectAndMakeArtifact(emArtifactBase.双头斧永恒, list, RepairManager.RepoRole, RepairManager.RepoAcc, exp.And(p => p.AccountName == RepairManager.RepoAcc), false);
        }
        public static async Task MakeMori()
        {
            int count = GetArtifactCount();
            Expression<Func<EquipModel, bool>> exp = (p) => p.Lv >= 70 && p.Category == "斧" &&
         (p.Quality == "slot" || p.Quality == "base") && (p.Content.Contains("凹槽(0/5)") || p.Content.Contains("最大凹槽：5")) && p.EquipName == "超强的双头斧" && p.Content.Contains("+15") && p.EquipStatus == emEquipStatus.Repo;
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(exp.And(p => p.AccountName != RepairManager.RepoAcc)).Take(count).ToList();

            await CollectAndMakeArtifact(emArtifactBase.双头斧末日, list, RepairManager.RepoRole, RepairManager.RepoAcc, exp.And(p => p.AccountName == RepairManager.RepoAcc), IsCollectBase());
            await RegisterMori();
        }

        public static async Task MakeFeilong()
        {
            int count = GetArtifactCount();
            Expression<Func<EquipModel, bool>> exp = (p) => p.Lv >= 70 && p.Category == "衣服" &&
         (p.Quality == "slot") && (p.Content.Contains("凹槽(0/3)")) && p.EquipName.Contains("圣堂武士") && !p.EquipName.Contains("无形") && p.EquipStatus == emEquipStatus.Repo;
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(exp.And(p => p.AccountName != RepairManager.RepoAcc)).Take(count).ToList();
            // var lunhuiList1 = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "RasdGch" && p.Category == "死灵副手" && (p.Quality == "slot" || p.Quality == "base") && p.Content.Contains("+3 生生不息") && (p.Content.Contains("+3 重生") || p.Content.Contains("+3 献祭")) && p.Lv >= 70).ToList().GroupBy(g => new { g.RoleID, g.RoleName, g.AccountName });
            await CollectAndMakeArtifact(emArtifactBase.圣堂飞龙, list, RepairManager.RepoRole, RepairManager.RepoAcc, exp.And(p => p.AccountName == RepairManager.RepoAcc), IsCollectBase());
            await RegisterFeilong();
        }

        public static async Task MakeJusticeMinxiang()
        {
            Expression<Func<EquipModel, bool>> exp = (p) => p.Lv >= 70 && p.Category == "权杖" &&
         (p.Quality == "slot") && (p.Content.Contains("凹槽(0/4)")) && p.Content.Contains("+3 冥想") && p.Content.Contains("+3 审判") && p.EquipStatus == emEquipStatus.Repo;
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(exp.And(p => p.AccountName == RepairManager.RepoAcc)).Take(20).ToList();
            // var lunhuiList1 = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName != "RasdGch" && p.Category == "死灵副手" && (p.Quality == "slot" || p.Quality == "base") && p.Content.Contains("+3 生生不息") && (p.Content.Contains("+3 重生") || p.Content.Contains("+3 献祭")) && p.Lv >= 70).ToList().GroupBy(g => new { g.RoleID, g.RoleName, g.AccountName });
            await CollectAndMakeArtifact(emArtifactBase.冥审正义, list, RepairManager.RepoRole, RepairManager.RepoAcc, exp.And(p => p.AccountName == RepairManager.RepoAcc), false);
        }

        public static async Task MakeFrost()
        {

            Expression<Func<EquipModel, bool>> exp = (p) => p.Lv >= 70 && p.Category == "十字弓" &&
         (p.Quality == "slot" || p.Quality == "base") && (p.Content.Contains("凹槽(0/4)")) && p.EquipName.Contains("巨神十字弓") && p.EquipStatus == emEquipStatus.Repo;
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(exp.And(p => p.AccountName != RepairManager.RepoAcc)).Take(19).ToList();

            //  await CollectAndMakeArtifact(emArtifactBase.巨神冰冻, list, RepairManager.RepoRole, RepairManager.RepoAcc, exp.And(p => p.AccountName == RepairManager.RepoAcc), false);
            await RegisterFrost();
        }
        /// <summary>
        /// 移动底子和制作神器
        /// </summary>
        /// <returns></returns>
        public static async Task CollectAndMakeArtifact(emArtifactBase emBase, List<EquipModel> baseList, string receiver, string receiverAcc, Expression<Func<EquipModel, bool>> exp = null, bool isCollect = true)
        {

            var eqBase = ArtifactBaseCfg.Instance.GetEquipCondition(emBase);

            var r = new AttributeMatchReport();
            baseList = baseList.Where(p => AttributeMatchUtil.Match(p, eqBase, out r)).ToList();
            var l = baseList.GroupBy(g => new { g.RoleID, g.RoleName, g.AccountName });
            foreach (var item in l)
            {
                if (!isCollect) break;
                if (item.Key.AccountName == RepairManager.RepoAcc) continue;
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
            await t2.AcceptAll();
            await Task.Delay(1500);

            var a = new ArtifactController(win2);
            var roleId = int.Parse(ConfigUtil.GetAppSetting("equipMaker"));

            exp.And(p => p.AccountName == win2.User.AccountName);
            baseList = FreeDb.Sqlite.Select<EquipModel>().Where(exp).ToList();
            int count = GetArtifactCount();
            baseList = baseList.Take(count).ToList();
            foreach (var item in baseList)
            {
                var con = ArtifactBaseCfg.Instance.GetEquipCondition(emBase);
                if (!AttributeMatchUtil.Match(item, con, out _))
                {
                    continue;
                }
                item.SetAccountInfo(win2.User);
                var eq = await a.MakeArtifact(emBase, item, roleId, eqBase, isCheckBag: false);
                if (eq == null) break;
                await Task.Delay(10000);

            }

            win2.Close();



        }

        /// <summary>
        /// 升级底子
        /// </summary>
        /// <returns></returns>
        public async static Task UpgradeBaseEq(BroWindow win)
        {
            var user = win.User;
            var eqList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "戒指" && p.Quality == "base" && p.EquipStatus == emEquipStatus.Repo && p.AccountName == user.AccountName).ToList();
            var bindedList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName == user.AccountName && p.EquipBaseName == "戒指" && p.EquipName == "全能法戒" && p.IsLocal).ToList();
            if (bindedList.Count >= 8)
            {
                return;
            }
            var r = new ReformController(win);
            var curEqList = eqList.Where(p => p.AccountName == user.AccountName).ToList();
            await r.UpgradeBaseEquip(win.User.FirstRole, curEqList);
            curEqList.ForEach(p => { p.EquipStatus = emEquipStatus.Upgraded; });
            DbUtil.InsertOrUpdate<EquipModel>(curEqList);
        }


        public async static Task ReformMageNecklace(BroWindow win)
        {
            var eqList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.Category == "项链" && p.Quality == "magical" && p.AccountName == win.User.AccountName && p.EquipStatus == emEquipStatus.Repo && p.Lv >= 50).ToList();
            var con = EmEquipCfg.Instance.GetEquipCondition(emEquip.蓝项链);
            eqList = eqList.Where(p => p.IsMatch(con)).ToList();

            await ReformEq(emReformType.Mage, eqList, win);
        }

        public async static Task ReformBaseJustice()
        {
            var eqList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.Category == "权杖" && p.Quality == "base" && p.EquipStatus == emEquipStatus.Repo && p.Lv >= 70).ToList();
            var con = ArtifactBaseCfg.Instance.GetEquipCondition(emArtifactBase.正义改造底子);
            eqList = eqList.Where(p => p.IsMatch(con)).ToList();

            var list = eqList.GroupBy(g => g.AccountName);
            foreach (var item in list)
            {
                var user = AccountCfg.Instance.GetUserModel(item.Key);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                var r = new ReformController(win);
                foreach (var eq in item)
                {
                    await r.SlotReform(eq, win.User.FirstRole.RoleId, con, emArtifactBase.正义改造底子);
                }

            }

        }


        public async static Task ReformDungeon(BroWindow win)
        {
            var eqList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.Category == "秘境" && p.Quality == "base" && p.AccountName == win.User.AccountName && p.EquipStatus == emEquipStatus.Repo).ToList();
            if (eqList.Count == 0) return;

            await ReformEq(emReformType.UpgradeMagical, eqList, win);
        }
        /// <summary>
        /// 改造装备
        /// </summary>
        /// <returns></returns>
        public async static Task ReformEq(emReformType reformType, List<EquipModel> eqList, BroWindow win)
        {

            var group = eqList.GroupBy(g => new { g.AccountName });
            foreach (var acc in group)
            {

                var user = AccountCfg.Instance.GetUserModel(acc.Key.AccountName);
                // win.GetBro().ShowDevTools();
                var r = new ReformController(win);
                var curEqList = eqList.Where(p => p.AccountName == user.AccountName).ToList();
                foreach (var e in acc)
                {
                    await r.ReformEquip(e, win.User.FirstRole.RoleId, reformType);
                }


            }
        }

        /// <summary>
        /// 初始化组队信息
        /// </summary>
        /// <returns></returns>
        public async static Task InitGroup()
        {
            foreach (var acc in AccountCfg.Instance.Accounts)
            {
                if (FreeDb.Sqlite.Select<GroupModel>().Where(p => p.AccountName == acc.AccountName).First() != null)
                {
                    continue;
                }
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

        /// <summary>
        /// 通关最后秘境
        /// </summary>
        /// <returns></returns>
        public async static Task PassFinalDungeon()
        {
            var groupList = FreeDb.Sqlite.Select<GroupModel>().ToList();
            int dungeonLv = 100;

            foreach (var acc in AccountCfg.Instance.Accounts)
            {
                if (acc.AccountName == "铁矿石") continue;
                if (acc.AccountName == "南方仓库") continue;

                var matchRoles = groupList.Where(p => p.AccountName == acc.AccountName);

                if (matchRoles.Where(p => p.DungeonPassedLv == dungeonLv).Count() > 0) continue;

                //检测是否过了100 去改造页检查腿能否进入
                var equip = FreeDb.Sqlite.Select<EquipModel>().Where(p => (p.EquipName.Contains("维特之脚") || p.EquipName.Contains("牛王战戟")) && p.Lv >= 85 && p.AccountName == acc.AccountName && p.EquipStatus == emEquipStatus.Repo).First();

                if (equip == null) continue;
                var user = AccountCfg.Instance.GetUserModel(acc.AccountName);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                var targetRole = user.Roles[0];
                var r = new ReformController(win);
                var canNainiu = await r.CheckNainiu(equip, targetRole.RoleId, emReformType.Nainiu);
                if (canNainiu)
                {
                    var g = matchRoles.Where(p => p.RoleId == targetRole.RoleId).First();
                    g.DungeonPassedLv = 100;
                    DbUtil.InsertOrUpdate(g);
                    win.Close();
                    continue;
                }
                var c = new CharacterController(win);
                await Task.Delay(1000);

                await c.StartDungeon(win.GetBro(), targetRole, targetDungeonLv: 100);
                win.Close();

            }
        }



        public async static Task PassDungeon(int testLv, int dungeonLv, int minRoleLv = -1)
        {
            await RecoverNec(dungeonLv);
            var groupList = FreeDb.Sqlite.Select<GroupModel>().ToList();
            foreach (var acc in AccountCfg.Instance.Accounts)
            {
                if (acc.AccountName == "铁矿石") continue;

                var matchRoles = groupList.Where(p => p.AccountName == acc.AccountName);
                //该账号下目标秘境全过了
                if (matchRoles.Where(p => p.DungeonPassedLv < dungeonLv).Count() == 0) continue;
                if (minRoleLv != -1)
                {
                    //传入等级下限的话 只会优先满足等级的组以死灵等级为标准

                    if (matchRoles.Where(p => p.Lv >= minRoleLv).Count() == 0) continue;
                }


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
                    var nec = groupList.Where(w => w.TeamIndex == g.TeamIndex && w.AccountName == g.AccountName && w.Job == emJob.死灵).FirstOrDefault();
                    var knight = groupList.Where(w => w.TeamIndex == g.TeamIndex && w.AccountName == g.AccountName && w.Job == emJob.骑士).FirstOrDefault();
                    if (g.DungeonPassedLv >= dungeonLv) continue;
                    bool isPass = await c.SwitchMap(role, testLv, dungeonLv, g);
                    await Task.Delay(1000);
                    //开始移动装备过来 等待他过本
                    if (!isPass)
                    {
                        await MoveArtifact(teamIndex, win, role.RoleName);

                        curTeamIndex = teamIndex;
                        await AutoEquipSpecificJob(win, teamIndex, acc.AccountName);

                        var e = new EquipController(win);
                        var necRole = win.User.Roles.Where(w => w.RoleId == nec.RoleId).FirstOrDefault();
                        var knightRole = win.User.Roles.Where(w => w.RoleId == knight.RoleId).FirstOrDefault();
                        if (role.Job == emJob.死灵)
                        {
                            await e.AutoEquips(win, necRole, emSkillMode.法师);
                            await c.AddSkillPoints(knightRole);
                            nec.SkillMode = emSkillMode.boss;
                            DbUtil.InsertOrUpdate<GroupModel>(nec);
                            await c.AddSkillPoints(necRole);
                        }


                    }
                    else
                    {
                        await RecoverNec(dungeonLv);
                    }


                }
                win.Close();
                break;
            }
        }

        private async static Task RecoverNec(int dungeonLv)
        {
            var lastNec = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.DungeonPassedLv == dungeonLv && p.SkillMode == emSkillMode.boss).First();
            if (lastNec != null)
            {
                var lastTeamIndex = lastNec.TeamIndex;
                var lastAcc = lastNec.AccountName;
                var lastUser = AccountCfg.Instance.GetUserModel(lastAcc);


                var lastTeam = FreeDb.Sqlite.Select<GroupModel>().
                    Where(p => p.TeamIndex == lastTeamIndex && p.AccountName == lastAcc).ToList();
                var isAllPass = lastTeam.Where(p => p.DungeonPassedLv == dungeonLv).Count() == 3;
                if (lastNec.SkillMode == emSkillMode.boss && isAllPass)
                {
                    var win1 = await TabManager.Instance.TriggerAddBroToTap(lastUser);
                    var c1 = new CharacterController(win1);
                    var e1 = new EquipController(win1);
                    var nec = lastTeam.Where(w => w.TeamIndex == lastTeamIndex && w.Job == emJob.死灵).FirstOrDefault();
                    var knight = lastTeam.Where(w => w.TeamIndex == lastTeamIndex && w.Job == emJob.骑士).FirstOrDefault();

                    var necRole = win1.User.Roles.Where(w => w.RoleId == nec.RoleId).FirstOrDefault();
                    var knightRole = win1.User.Roles.Where(w => w.RoleId == knight.RoleId).FirstOrDefault();
                    await e1.AutoEquips(win1, necRole, emSkillMode.自动);
                    await c1.AddSkillPoints(necRole);
                    await c1.AddSkillPoints(knightRole);
                    win1.Close();

                }
            }
        }

        public static async Task MoveArtifact(int teamIndex, BroWindow curWin, string receiver)
        {
            var clothConfig = EmEquipCfg.Instance.GetEquipCondition(emEquip.骑士技能衣服);
            var dkWeaponConfig = EmEquipCfg.Instance.GetEquipCondition(emEquip.死骑冰霜武器);
            var feilong = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipID == RepairManager.PublicFeilongId).First();
            var yongheng = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipID == RepairManager.PublicYonghengId).First();
            var curEquips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName == curWin.User.AccountName).ToList();


            var win = curWin;
            //当前队伍的目标骑士检查有没有飞龙
            var targetKnight = win.User.Roles[teamIndex * 3];
            var targetDk = win.User.Roles[(teamIndex * 3) + 2];
            var isTargetKnightHasFeilong = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.RoleID == targetKnight.RoleId && p.EquipName == "飞龙").First() != null;
            var isTargetDkHasYongheng = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.RoleID == targetKnight.RoleId && p.EquipName.Contains("永恒")).First() != null;

            BroWindow winFeiLong = null;
            BroWindow winYongheng = null;
            var userYongheng = AccountCfg.Instance.GetUserModel(yongheng.AccountName);
            var userFeilong = AccountCfg.Instance.GetUserModel(feilong.AccountName);
            if (feilong.RoleID > 0 && feilong.RoleID != targetKnight.RoleId && !isTargetKnightHasFeilong)
            {

                winFeiLong = await TabManager.Instance.TriggerAddBroToTap(userFeilong);
                var e = new EquipController(winFeiLong);
                var knightRole = winFeiLong.User.Roles.Where(p => p.RoleId == feilong.RoleID).First();

                var anyCloth = e.GetMatchEquips(feilong.AccountName, clothConfig, knightRole, out _).FirstOrDefault();

                await e.AutoAttributeSave(winFeiLong, knightRole, new List<EquipModel>() { { anyCloth } });
                //得跳转换装页面
                await winFeiLong.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(feilong.RoleID), "equip");
                await e.WearEquipAndRecord(winFeiLong, anyCloth, (int)emEquipSort.衣服, winFeiLong.User, knightRole);
                await Task.Delay(1500);




            }
            if (yongheng.RoleID > 0 && targetDk.RoleId != yongheng.RoleID && !isTargetDkHasYongheng)
            {

                winYongheng = await TabManager.Instance.TriggerAddBroToTap(userYongheng);
                var e = new EquipController(winYongheng);
                var dkRole = winYongheng.User.Roles.Where(p => p.RoleId == yongheng.RoleID).First();
                var anyWeapon = e.GetMatchEquips(yongheng.AccountName, dkWeaponConfig, dkRole, out _).FirstOrDefault();
                await e.AutoAttributeSave(winYongheng, dkRole, new List<EquipModel>() { { anyWeapon } });
                await winYongheng.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(yongheng.RoleID), "equip");
                await e.WearEquipAndRecord(winYongheng, anyWeapon, (int)emEquipSort.副手, winYongheng.User, dkRole);


            }


            if (feilong.AccountName != curWin.User.AccountName)
            {
                if (winFeiLong == null)
                {
                    winFeiLong = await TabManager.Instance.TriggerAddBroToTap(userFeilong);
                }
                var t = new TradeController(winFeiLong);
                await t.StartTrade(feilong, receiver);


            }
            if (yongheng.AccountName != curWin.User.AccountName)
            {
                if (winYongheng == null)
                {
                    winYongheng = await TabManager.Instance.TriggerAddBroToTap(userYongheng);
                }
                var t = new TradeController(winYongheng);
                await t.StartTrade(yongheng, receiver);
            }
            var receiverAcc = AccountCfg.Instance.GetUserModel(curWin.User.AccountName);
            if (feilong.AccountName != curWin.User.AccountName || yongheng.AccountName != curWin.User.AccountName)
            {
                var win2 = await TabManager.Instance.TriggerAddBroToTap(receiverAcc);
                var t2 = new TradeController(win2);
                await t2.AcceptAll();
                win2.Close();
            }
            if (winFeiLong != null)
            {
                winFeiLong.Close();
            }
            if (winYongheng != null)
            {
                winYongheng.Close();
            }

            //  win.Close();

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
                await e.AutoEquips(win, dk);
            }
            if (knightEquips.Where(p => p.EquipName.Contains("飞龙")).Count() == 0)
            {
                await e.AutoEquips(win, knight);
            }
        }

        public static async Task RepairXianji()
        {
            foreach (var user in AccountCfg.Instance.Users)
            {
                var hasTarget = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.RoleID > 0 && p.Category == "手杖" && p.AccountName == user.AccountName && p.EquipName == "亡者遗产").ToList().Count > 0;
                if (!hasTarget) continue;
                var groups = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.AccountName == user.AccountName).ToList();
                var list = groups.Where(p => p.Job == emJob.死灵 && (p.SkillMode != emSkillMode.献祭));
                if (list.Count() == 0) continue;
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                list = list.OrderBy(o => o.Lv).ToList();
                foreach (var nec in list)
                {
                    var roleEquips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.RoleID == nec.RoleId && p.Category == "手杖").ToList();
                    if (roleEquips.Where(p => p.EquipName.Contains("冥神")).Count() > 0)
                    {
                        continue;
                    }
                    var e = new EquipController(win);
                    var necRole = win.User.Roles.Find(p => p.RoleId == nec.RoleId);
                    var curEquips = await e.AutoEquips(win, necRole, emSkillMode.献祭);
                    if (necRole.GetRoleSkillMode() != emSkillMode.献祭)
                    {
                        //先执行自动换装 如果不能切换到献祭模式则尝试发送装备过来

                        await SendXianji(user.AccountName, nec.RoleName);
                        curEquips = await e.AutoEquips(win, necRole, emSkillMode.献祭);
                    }
                    // await InsertColdConversion(curEquips, win, necRole);
                    //给死灵洗点 打宝珠 切换骑士拯救光环 监控永恒速度
                    var c = new CharacterController(win);
                    await c.AddSkillPoints(necRole, curEquips);

                    groups = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.AccountName == user.AccountName).ToList();
                    var curGroup = groups.Where(p => p.TeamIndex == nec.TeamIndex).ToList();
                    curGroup.ForEach(p => { p.IsRepairSacrificeDone = true; });
                    DbUtil.InsertOrUpdate<GroupModel>(curGroup);

                }
                win.Close();



            }
        }

        public static async Task InsertColdConversion(Dictionary<emEquipSort, EquipModel> curEquips, BroWindow win, RoleModel role)
        {
            await Task.Delay(1500);
            var bro = win.GetBro();

            // 解析凹槽数量的方法（按照您提供的格式）
            int ParseSocketCount(string content)
            {
                var match = Regex.Match(content, @"凹槽\((\d+)/(\d+)\)");
                return match.Success ? int.Parse(match.Groups[2].Value) : 0;
            }

            // 获取装备
            var head = curEquips[emEquipSort.头盔];
            var cloth = curEquips[emEquipSort.衣服];
            var weapon = curEquips[emEquipSort.主手];

            // 计算各装备的孔数和已插入冰冷转换数量
            int headSocketCount = ParseSocketCount(head.Content);
            int headColdCount = RegexUtil.MatchCount(head.Content, "冰冷转换");
            int clothSocketCount = ParseSocketCount(cloth.Content);
            int clothColdCount = RegexUtil.MatchCount(cloth.Content, "冰冷转换");
            int weaponSocketCount = ParseSocketCount(weapon.Content);
            int weaponColdCount = RegexUtil.MatchCount(weapon.Content, "冰冷转换");

            // 1. 计算需要的冰冷珠宝数量
            int requiredJewels = 0;
            EquipModel targetEquip = null;

            // 武器优先处理（自杀支系）
            if (weapon.EquipName == "自杀支系" && weaponColdCount == 0)
            {
                if (weaponSocketCount == 0)
                {
                    var r = new ReformController(win);
                    await r.ReformEquip(weapon, role.RoleId, emReformType.Direct);
                    await Task.Delay(1000);
                    weaponSocketCount = 1; // 打孔后默认1个孔
                }
                targetEquip = weapon;
                requiredJewels = 1;
            }
            // 衣服处理
            else if (clothColdCount == 0 && (cloth.EquipName == "吟唱者之袍") && cloth.Content.Contains("+2 所有技能"))
            {
                if (clothSocketCount == 0)
                {
                    var r = new ReformController(win);
                    await r.ReformEquip(cloth, role.RoleId, emReformType.Direct);
                    await Task.Delay(1000);
                    clothSocketCount = 1; // 打孔后默认1个孔
                }
                targetEquip = cloth;
                requiredJewels = 1;
            }
            // 教化头盔特殊处理
            else if (head.EquipName == "教化" && headColdCount < Math.Min(2, headSocketCount))
            {
                targetEquip = head;
                requiredJewels = Math.Min(2, headSocketCount) - headColdCount;
            }

            // 如果没有需要插入的装备则返回
            if (targetEquip == null || requiredJewels == 0) return;

            // 2. 获取足够数量的冰冷珠宝
            var e = new EquipController(win);
            var jewelList = e.GetMatchEquips(win.User.AccountName, EmEquipCfg.Instance.GetEquipCondition(emEquip.冰转珠宝), role, out _);

            if (jewelList.Count < requiredJewels)
            {
                int requiredCount = requiredJewels - jewelList.Count;
                var tradeList = FreeDb.Sqlite.Select<TradeModel>()
                    .Where(p => p.EquipSortName == "珠宝" &&
                           p.EquipName.Contains("冰冷转换")).ToList();
                var tradeIdList = tradeList.Select(p => p.EquipId).ToList();
                tradeIdList.Add(-1);
                var coldList = FreeDb.Sqlite.Select<EquipModel>()
                   .Where(p => p.Category == "珠宝" &&
                    p.EquipStatus == emEquipStatus.Repo &&
                          p.EquipName.Contains("冰冷转换") &&
                          !p.Content.Contains("增强伤害") && p.AccountName != win.User.AccountName && !tradeIdList.Contains(p.EquipID))
                   .Take(requiredCount)
                   .ToList();
                // 登记需求
                var insertList = coldList.Select(s => new TradeModel()
                {
                    EquipId = s.EquipID,
                    EquipName = s.EquipName,
                    EquipSortName = "珠宝",
                    DemandAccountName = win.User.AccountName,
                    DemandRoleId = role.RoleId,
                    DemandRoleName = role.RoleName,
                    OwnerAccountName = s.AccountName,
                    TradeStatus = emTradeStatus.Register
                }).ToList();

                // DbUtil.InsertOrUpdate<TradeModel>(insertList);

            }
            if (jewelList.Count == 0) return;
            // 3. 执行镶嵌操作
            var url = IdleUrlHelper.InlayUrl(role.RoleId, targetEquip.EquipID);
            if (bro.Address != url)
            {
                await win.LoadUrlWaitJsInit(url, "inlay");
                await Task.Delay(1000);
            }

            // 对于教化头盔，使用isEnd判断是否可以继续插入
            int insertedCount = 0;
            int jewelCount = jewelList.Count;
            while (insertedCount < requiredJewels && jewelCount != 0)
            {
                var endCheck = await win.CallJs("_inlay.isEnd()");
                if (endCheck.Result.ToObject<bool>()) break;

                await win.CallJsWaitReload($"_inlay.equipInlay({jewelList[insertedCount].EquipID})", "inlay");
                await Task.Delay(1500);

                // 更新装备状态
                jewelList[insertedCount].SetAccountInfo(win.User, role);
                jewelList[insertedCount].EquipStatus = emEquipStatus.Equipped;
                targetEquip.Content += "|冰冷转换";
                DbUtil.InsertOrUpdate<EquipModel>(jewelList[insertedCount]);

                insertedCount++;
                jewelCount--;
            }

            // 递归处理其他装备
            await InsertColdConversion(curEquips, win, role);
        }

        public static void SetSkillMode(RoleModel role, UserModel user, emSkillMode matchSkillMode)
        {
            var matchGroupRole = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.RoleId == role.RoleId && p.AccountName == user.AccountName).First();
            var matchGroup = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.TeamIndex == matchGroupRole.TeamIndex && p.AccountName == user.AccountName && p.Job != emJob.死骑).ToList();
            matchGroup.ForEach(p => { p.SkillMode = matchSkillMode; });
            DbUtil.InsertOrUpdate<GroupModel>(matchGroup);
        }
        public static void SetDkSkillMode(RoleModel role, UserModel user, emSkillMode matchSkillMode)
        {
            var matchGroupRole = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.RoleId == role.RoleId && p.AccountName == user.AccountName).First();
            matchGroupRole.SkillMode = matchSkillMode;
            DbUtil.InsertOrUpdate<GroupModel>(matchGroupRole);
        }

        public static async Task AutoUpgradeGem(BroWindow window)
        {
            var r = new RuneController(window);
            await r.UpgradeGem(emGem.黄宝石, 4);

        }

        /// <summary>
        /// 更新死骑的mf装备并且存储套装 记录 锁定装备在交易和自动换装中抹除 
        /// </summary>
        /// <returns></returns>
        public static async Task UpdateMfEquip(BroWindow win)
        {
            var e = new EquipController(win);
            foreach (var role in win.User.Roles)
            {
                if (role.Job != emJob.死骑)
                {
                    continue;
                }
                var curEquips = e.GetCurEquips();
                //先保存当前装备为效率
                var effSuitId = await e.GetSuitId(emSuitType.效率, role);
                var mfSuitId = await e.GetSuitId(emSuitType.MF, role);
                if (effSuitId <= 0)
                {
                    await e.SaveEquipSuit(emSuitType.效率, role);
                }
                else
                {
                    await e.LoadSuit(emSuitType.效率, role);
                    await e.SaveEquipSuitModel(emSuitType.效率, role, effSuitId);
                }
                if (mfSuitId > 0)
                {
                    await e.LoadSuit(emSuitType.MF, role);
                }
                await e.AutoEquips(win, role, targetSuitType: emSuitType.MF);
                await e.SaveEquipSuit(emSuitType.MF, role);
                //  await e.LoadSuit(emSuitType.效率, role);
                break;
            }
        }

        public static async Task RegisterYongheng()
        {
            var toSendRole = FreeDb.Sqlite.Select<EquipModel, GroupModel>()
                .InnerJoin<GroupModel>((a, b) => a.RoleID == b.RoleId)
                .Where((a, b) => b.Job == emJob.死骑 && a.emEquipSort == emEquipSort.副手 && !a.EquipName.Contains("永恒"))
                .ToList((a, b) => b);
            var equiped = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName.Contains("永恒") && p.RoleID > 0).ToList();
            var group = equiped.GroupBy(g => g.AccountName).ToDictionary(
                     gg => gg.Key,
                     gg => gg.ToList().Count
                     );
            foreach (var account in AccountCfg.Instance.Accounts)
            {
                if (account.AccountName == "铁矿石") continue;
                if (!group.ContainsKey(account.AccountName))
                {
                    group.Add(account.AccountName, 0);
                }
            }
            //按持有数量排序
            var toSendAccount = group.OrderBy(o => o.Value).ToList();

            toSendRole = ReSortSendRole(toSendAccount, toSendRole);
            var yonghengInRepo = EquipUtil.QueryEquipInRepo().Where((a, b) => (a.EquipName == "无形永恒" || a.EquipName == "永恒") && a.RoleID == 0).ToList();
            int index = 0;
            foreach (var item in yonghengInRepo)
            {
                var demandRole = toSendRole[index];
                var t = new TradeModel()
                {
                    DemandAccountName = demandRole.AccountName,
                    DemandRoleId = demandRole.RoleId,
                    DemandRoleName = demandRole.RoleName,
                    EquipId = item.EquipID,
                    EquipName = item.EquipName,
                    EquipSortName = "副手",
                    OwnerAccountName = item.AccountName,
                    TradeStatus = emTradeStatus.Register,


                };
                DbUtil.InsertOrUpdate<TradeModel>(t);
                index++;
            }

        }

        public static async Task RegisterMori()
        {
            var toSendRole = FreeDb.Sqlite.Select<EquipModel, GroupModel>()
                .InnerJoin<GroupModel>((a, b) => a.RoleID == b.RoleId)
                .Where((a, b) => b.Job == emJob.骑士 && a.emEquipSort == emEquipSort.主手 && (a.EquipName.Contains("末日") && a.Content.Contains("+1 所有技能")) && !a.EquipName.Contains("正义之手"))
                .ToList((a, b) => b);

            var moriEquiped = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "末日" && p.RoleID > 0).ToList();
            var group = moriEquiped.GroupBy(g => g.AccountName).ToDictionary(
                     gg => gg.Key,
                     gg => gg.ToList().Count
                     );
            foreach (var account in AccountCfg.Instance.Accounts)
            {
                if (account.AccountName == "铁矿石") continue;
                if (!group.ContainsKey(account.AccountName))
                {
                    group.Add(account.AccountName, 0);
                }
            }
            //按持有数量排序
            var toSendAccount = group.OrderBy(o => o.Value).ToList();

            toSendRole = ReSortSendRole(toSendAccount, toSendRole);
            var eqInRepo = EquipUtil.QueryEquipInRepo().Where((a, b) => (a.EquipName == "末日" && a.Content.Contains("+2 所有技能")) && a.RoleID == 0).ToList();
            int index = 0;
            foreach (var item in eqInRepo)
            {
                var demandRole = toSendRole[index];
                var t = new TradeModel()
                {
                    DemandAccountName = demandRole.AccountName,
                    DemandRoleId = demandRole.RoleId,
                    DemandRoleName = demandRole.RoleName,
                    EquipId = item.EquipID,
                    EquipName = item.EquipName,
                    EquipSortName = "主手",
                    OwnerAccountName = item.AccountName,
                    TradeStatus = emTradeStatus.Register,


                };
                DbUtil.InsertOrUpdate<TradeModel>(t);
                index++;
            }

        }

        public static async Task RegisterFeilong()
        {
            var toSendRole = FreeDb.Sqlite.Select<EquipModel, GroupModel>()
                .InnerJoin<GroupModel>((a, b) => a.RoleID == b.RoleId)
                .Where((a, b) => b.Job == emJob.骑士 && a.emEquipSort == emEquipSort.衣服 && !a.EquipName.Contains("飞龙"))
                .ToList((a, b) => b);

            var justiceEquiped = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName.Contains("正义之手") && p.RoleID > 0).ToList().Select(s => s.RoleID).ToDictionary(
                key => key,
                key => key
                );



            var eqInRepo = EquipUtil.QueryEquipInRepo().Where((a, b) => (a.EquipName == "飞龙") && a.RoleID == 0).ToList();
            toSendRole = toSendRole.Where(p => !justiceEquiped.ContainsKey(p.RoleId)).ToList();
            int index = 0;
            foreach (var item in eqInRepo)
            {
                var demandRole = toSendRole[index];
                var t = new TradeModel()
                {
                    DemandAccountName = demandRole.AccountName,
                    DemandRoleId = demandRole.RoleId,
                    DemandRoleName = demandRole.RoleName,
                    EquipId = item.EquipID,
                    EquipName = item.EquipName,
                    EquipSortName = "衣服",
                    OwnerAccountName = item.AccountName,
                    TradeStatus = emTradeStatus.Register,


                };
                DbUtil.InsertOrUpdate<TradeModel>(t);
                index++;
            }

        }

        public static async Task RegisterFrost()
        {
            var toSendRole1 = FreeDb.Sqlite.Select<EquipModel, GroupModel>()
                .InnerJoin<GroupModel>((a, b) => a.RoleID == b.RoleId)
                .Where((a, b) => b.Job == emJob.骑士 && a.emEquipSort == emEquipSort.主手 && !a.EquipName.Contains("末日") && !a.EquipName.Contains("正义之手") && !a.EquipName.Contains("冰冻")
                && a.AccountName != RepairManager.RepoAcc)
                .ToList((a, b) => b);

            var toSendRole2 = FreeDb.Sqlite.Select<EquipModel, GroupModel>()
       .InnerJoin<GroupModel>((a, b) => a.RoleID == b.RoleId)
       .Where((a, b) => b.Job == emJob.骑士 && a.emEquipSort == emEquipSort.副手 && !a.EquipName.Contains("梦境")
       && a.AccountName != RepairManager.RepoAcc)
       .ToList((a, b) => b);

            var toSendRole = toSendRole1.Intersect(toSendRole2).ToList();


            var eqInRepo = EquipUtil.QueryEquipInRepo().Where((a, b) => (a.EquipName.Contains("冰冻")) && a.Quality == "artifact" && a.RoleID == 0).ToList();
            int index = 0;
            foreach (var item in eqInRepo)
            {
                var demandRole = toSendRole[index];
                var t = new TradeModel()
                {
                    DemandAccountName = demandRole.AccountName,
                    DemandRoleId = demandRole.RoleId,
                    DemandRoleName = demandRole.RoleName,
                    EquipId = item.EquipID,
                    EquipName = item.EquipName,
                    EquipSortName = "主手",
                    OwnerAccountName = item.AccountName,
                    TradeStatus = emTradeStatus.Register,


                };
                DbUtil.InsertOrUpdate<TradeModel>(t);
                index++;
            }

        }

        private static List<GroupModel> ReSortSendRole(List<KeyValuePair<string, int>> toSendAccount, List<GroupModel> toSendRole)
        {
            List<GroupModel> result = new List<GroupModel>();
            var temp = new Stack<GroupModel>();
            toSendRole = toSendRole.OrderBy(o => o.RoleId).ToList();
            int accountIndex = 0;
            int curI = 0;
            for (int i = 0; i < toSendRole.Count; i++)
            {
                curI = i;
                var cur = toSendRole[i];
                if (accountIndex == toSendAccount.Count)
                {
                    break;
                }
                if (cur.AccountName == toSendAccount[accountIndex].Key)
                {
                    result.Add(cur);
                    accountIndex++;
                }
                else
                {
                    temp.Push(cur);
                }
            }
            result.AddRange(toSendRole.Skip(curI + 1));
            result.AddRange(temp.Select(s => s));
            return result;

        }

        public static async Task RegisterColdConversion()
        {
            var toSendRole = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.RoleID > 0 && p.EquipName == "自杀支系" && !p.Content.Contains("冰冷转换")).ToList();


            var coldList = FreeDb.Sqlite.Select<EquipModel>()
               .Where(p => p.Category == "珠宝" &&
                      p.EquipName.Contains("冰冷转换")
                      && p.EquipStatus == emEquipStatus.Repo
                      && !p.Content.Contains("增强伤害"))
               .ToList();

            int index = 0;
            foreach (var item in toSendRole)
            {
                var demandRole = toSendRole[index];
                var coldJewelry = coldList.FirstOrDefault(p => p.AccountName != item.AccountName);
                if (coldJewelry == null) continue;
                var t = new TradeModel()
                {
                    DemandAccountName = demandRole.AccountName,
                    DemandRoleId = demandRole.RoleID,
                    DemandRoleName = demandRole.RoleName,
                    EquipId = coldJewelry.EquipID,
                    EquipName = coldJewelry.EquipName,
                    EquipSortName = "珠宝",
                    OwnerAccountName = coldJewelry.AccountName,
                    TradeStatus = emTradeStatus.Register,


                };
                DbUtil.InsertOrUpdate<TradeModel>(t);
                coldList.Remove(coldJewelry);
                index++;
            }

        }

        public static async Task ClearRepoPre()
        {
            //每件太古装备保留最低数量
            Dictionary<string, int> perfectDic = new Dictionary<string, int>();
            try
            {
                var equips = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipStatus == emEquipStatus.Repo && p.AccountName != "铁矿石" && !p.AccountName.StartsWith("01") && !p.AccountName.StartsWith("02") && !p.AccountName.StartsWith("南方仓库")).ToList();
                var equipInSuitList = FreeDb.Sqlite.Select<EquipSuitModel>()

               .ToList();
                var equipInSuit = new Dictionary<long, long>();
                foreach (var item in equipInSuitList)
                {
                    if (!equipInSuit.ContainsKey(item.EquipId))
                    {
                        equipInSuit.Add(item.EquipId, item.EquipId);
                    }
                }
                List<DelPreviewEquipModel> list = new List<DelPreviewEquipModel>();
                var group = equips.GroupBy(g => g.AccountName);
                foreach (var g in group)
                {
                    var config = new RetainEquipCfg();
                    var equipList = g.ToList();
                    foreach (var item in equipList)
                    {

                        if (item.emItemQuality == emItemQuality.神器 || equipInSuit.ContainsKey(item.EquipID))
                            continue;
                        if (!config.IsRetain(item))
                        {//不需要保存的装备 如果是太古则保留一定数量
                            var delPre = item.ToObject<DelPreviewEquipModel>();
                            if (item.IsPerfect)
                            {
                                if (!perfectDic.ContainsKey(item.EquipName))
                                {
                                    perfectDic.Add(item.EquipName, 1);
                                }
                                else
                                {
                                    int count = perfectDic[item.EquipName];
                                    if (count >= 2)
                                    {
                                       
                                        delPre.AccountName = item.AccountName;
                                        list.Add(delPre);
                                    }
                                    else ++perfectDic[item.EquipName];
                                }
                            }
                            else
                            {
                                delPre.AccountName = item.AccountName;
                                list.Add(delPre);
                            }
                            
                         
                        }
                        
                    }

                }
                if (list.Count > 0)
                {
                    DbUtil.InsertOrUpdate<DelPreviewEquipModel>(list);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        private static async Task ClearRepo(BroWindow win)
        {
            var e = new EquipController(win);
            await e.ClearRepo();
        }

        public static async Task ConfirmDelEquip()
        {
            var equips = FreeDb.Sqlite.Select<DelPreviewEquipModel>().ToList();
            if (equips.Count == 0) return;
            var delWarning = FreeDb.Sqlite.Select<DelView>().ToList();
            if (delWarning.Count > 0) throw new Exception("有不能N的装备");
            var group = equips.GroupBy(g => g.AccountName);
            var accountList = group.Select(s => s.Key).ToArray();

            await FlowController.GroupWork(3, 1, ClearRepo, accountList);
        }

        public static async Task ReformDungeonAndRings()
        {
            var dungeonList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.Category == "秘境" && p.EquipName != "秘境").ToList();
            var dic = dungeonList.GroupBy(g => g.AccountName).ToDictionary(
         group => group.Key,
         group => group.ToList()
     );
            var targetAcc = dic.Where(p => p.Value.Count <= 40).Select(s => s.Key).ToArray();

            // await GroupWork(3, 1, ReformDungeon, targetAcc);
            await GroupWork(3, 1, UpgradeBaseEq, RepairManager.NanfangAccounts.Concat(RepairManager.NainiuAccounts).ToArray());
        }



        public static async Task StartSanBoss(BroWindow win)
        {
            var role = win.User.FirstRole;
            win.LoadUrl($"https://www.idleinfinity.cn/Battle/Boss?id={role.RoleId}");
            await Task.Delay(5000);
        }


        public static async Task SetHunterAndPastor()
        {
            var user = AccountCfg.Instance.GetUserModel("铁矿石");
            var win1 = await TabManager.Instance.TriggerAddBroToTap(user);
            var e1 = new EquipController(win1);
            var pastor = win1.User.Roles[4];
            var hunter = win1.User.Roles[7];

            //牧师换装
            await e1.LoadSuit(emSuitType.boss, pastor);
            await e1.LoadSuit(emSuitType.boss, hunter);
            //猎手切技能
            var c1 = new CharacterController(win1);
            await c1.ExitGroup(hunter);
            await c1.ExitGroup(pastor);
            await c1.AddSkillPoints(hunter, emSkillMode.稳固);
            await c1.MakeGroup(hunter, pastor, null);
            //await c1.ExitGroup(hunter);
            //await c1.MakeGroup(knight1, null, hunter);
            //await c1.MakeGroup(ass, null, pastor);


            //await Task.Delay(2000);
            //await c1.AddSkillPoints(hunter, emSkillMode.爆炸);
        }


        public static async Task RecoverHunterAndPastor()
        {
            var user = AccountCfg.Instance.GetUserModel("铁矿石");
            var win1 = await TabManager.Instance.TriggerAddBroToTap(user);
            var e1 = new EquipController(win1);
            var pastor = win1.User.Roles[4];
            var hunter = win1.User.Roles[7];
            //游侠
            var ke = win1.User.Roles[5];
            var knight = win1.User.Roles[10];
            //牧师换装
            await e1.LoadSuit(emSuitType.华莱士, pastor);

            //猎手切技能
            var c1 = new CharacterController(win1);
            await e1.LoadSuit(emSuitType.效率, hunter);
            await c1.ExitGroup(hunter);
            await c1.AddSkillPoints(hunter, emSkillMode.爆炸);

            await c1.MakeGroup(knight, null, hunter);
            await c1.MakeGroup(ke, null, pastor);


            //await Task.Delay(2000);
            //await c1.AddSkillPoints(hunter, emSkillMode.爆炸);
        }
        public static async Task FightWorldBoss()
        {
            // 牧师换装
            // 切换技能 猎手稳固 牧师神牧k痛苦压制
            // 牧师 猎手退队 组队
            // 目标账号骑士退队 组队
            // 骑士开始roll 
            // 下一组

            var user = AccountCfg.Instance.GetUserModel("铁矿石");
            var win1 = await TabManager.Instance.TriggerAddBroToTap(user);
            var e1 = new EquipController(win1);
            var pastor = win1.User.Roles[4];
            var hunter = win1.User.Roles[7];
            var knight1 = win1.User.Roles[1];
            var ass = win1.User.Roles[0];

            //猎手切技能
            var c1 = new CharacterController(win1);
            foreach (var a in AccountCfg.Instance.Accounts)
            {
                if (a.AccountName == "铁矿石") continue;
                if (a.AccountName == "南方仓库") continue;
                var r1 = FreeDb.Sqlite.Select<BossProgress>().Where(p => p.AccountName == a.AccountName).First();
                if (r1 != null && r1.IsPassWorldBoss) continue;
                var u = AccountCfg.Instance.GetUserModel(a.AccountName);
                var win2 = await TabManager.Instance.TriggerAddBroToTap(u);
                var t = new TradeController(win2);


                var c2 = new CharacterController(win2);
                var role1 = win2.User.Roles[0];
                var role2 = win2.User.Roles[1];
                var role3 = win2.User.Roles[2];
                if (role1.Level < 88)
                {

                    win2.Close();
                    continue;

                }
                //苦工解散
                await c2.ExitGroup(role1);
                //加入骑士
                await c1.MakeGroup(hunter, pastor, role1);
                await t.AcceptAll();
                // await win2.LoadUrl($"https://www.idleinfinity.cn/Battle/WorldEvent?id={role1.RoleId}");
                // await win2.LoadUrl($"https://www.idleinfinity.cn/Battle/WorldEvent?id={role1.RoleId}");
                await WorldBoss(role1, win2);
                //踢骑士
                await c1.RemoveGroupRole(hunter, role1);
                await c2.MakeGroup(role1, role2, role3);
                win2.Close();
            }



        }

        /// <summary>
        /// 每日boss只有一次机会 不需要roll
        /// </summary>
        /// <param name="role"></param>
        /// <param name="win"></param>
        /// <returns></returns>
        private async static Task DailyBoss(RoleModel role, BroWindow win)
        {
            await win.LoadUrlWaitJsInit($"https://www.idleinfinity.cn/Battle/Boss?id={role.RoleId}", "char");
            var bossProgress = new BossProgress() { AccountName = win.User.AccountName, IsPassDailyBoss = true };
            DbUtil.InsertOrUpdate<BossProgress>(bossProgress);
            await Task.Delay(2000);
        }

        /// <summary>
        /// 每日boss只有一次机会 不需要roll
        /// </summary>
        /// <param name="role"></param>
        /// <param name="win"></param>
        /// <returns></returns>
        private async static Task WorldBoss(RoleModel role, BroWindow win)
        {

            await win.LoadUrlWaitJsInit($"https://www.idleinfinity.cn/Battle/WorldEvent?id={role.RoleId}", "char");

            var isBossDone = await win.CallJs<bool>("_char.isBossDone()");
            await Task.Delay(3000);
            if (!isBossDone)
            {
                await WorldBoss(role, win);
            }
            var bossProgress = new BossProgress() { AccountName = win.User.AccountName, IsPassWorldBoss = true };
            DbUtil.InsertOrUpdate<BossProgress>(bossProgress);
        }

        public async static Task RepairShengyi()
        {

            var shoesName = "燃魂之焰";
            var list = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.Lv >= 87 && p.Job == emJob.死灵).ToList();
            var equipList = FreeDb.Sqlite.Select<EquipModel>().Where(p => list.Select(s => s.RoleId).Contains(p.RoleID)).ToList();
            foreach (var item in list)
            {
                var toSendList = new List<EquipModel>();
                var curEquipList = equipList.Where(p => p.RoleID == item.RoleId).ToList();
                var curEquips = GetEquipMap(curEquipList);
                await ReformOrTradeEquip(toSendList, curEquips, shoesName, emEquipSort.靴子);
                await SendEquip(toSendList, item.RoleName, item.AccountName);
            }

        }

        private static async Task ReformOrTradeEquip(List<EquipModel> toTradeList, Dictionary<emEquipSort, EquipModel> curEquips, string eqName, emEquipSort eqSort, bool isReform = false)
        {
            var accName = curEquips[eqSort].AccountName;
            if (!curEquips[eqSort].EquipName.Contains(eqName))
            {
                if (!IsLocalContainsEquip(eqName, accName))
                {
                    //邮寄一个过来
                    Expression<Func<EquipModel, bool>> exp = (p) => p.EquipName == eqName && !p.IsLocal && p.RoleID == 0;
                    var e = EquipUtil.GetOne(exp);
                    if (e == null)
                    {
                        //本地改造一个
                        if (isReform)
                        {

                        }
                    }
                    else
                    {
                        toTradeList.Add(e);
                    }

                }

            }
        }



        /// <summary>
        /// 本地仓库是否包含装备
        /// </summary>
        /// <param name="eqName"></param>
        /// <returns></returns>
        public static bool IsLocalContainsEquip(string eqName, string accName)
        {
            return FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == eqName && p.AccountName == accName && p.EquipStatus == emEquipStatus.Repo).First() == null;
        }


        /// <summary>
        /// 将list转成dic
        /// </summary>
        /// <param name="eqList"></param>
        /// <returns></returns>
        public static Dictionary<emEquipSort, EquipModel> GetEquipMap(List<EquipModel> eqList)
        {
            var dic = new Dictionary<emEquipSort, EquipModel>();
            foreach (var e in eqList)
            {
                dic.Add(e.emEquipSort, e);
            }
            return dic;
        }

        /// <summary>
        /// 将11环永恒转移到35速轮回那
        /// </summary>
        /// <returns></returns>
        public static async Task SwitchYongheng()
        {
            var groupList = FreeDb.Sqlite.Select<GroupModel>().ToList();
            var targetYonghengDkList = groupList.Where(p => p.YonghengSpeed == 20).ToList();
            var lunhui35 = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName.Contains("轮回") && p.RoleID > 0).ToList();
            var targetLunhui35RoleList = new List<int>();
            var dic = lunhui35.ToDictionary(
                d => d.RoleID,
                d => AttributeMatchUtil.GetBaseAttValue(emAttrType.施法速度, d.Content).Item2);

            foreach (var r in dic)
            {
                var nec = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.RoleId == r.Key).First();
                var dk = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.AccountName == nec.AccountName && p.TeamIndex == nec.TeamIndex && p.Job == emJob.死骑).First();
                if (r.Value == 35 && dk.YonghengSpeed == 20)
                {
                    //完美搭配不要动了剔除这批永恒
                    targetYonghengDkList.Remove(dk);
                }
                else if (r.Value == 35)
                {
                    targetLunhui35RoleList.Add(r.Key);
                }
            }

            foreach (var user in AccountCfg.Instance.Users)
            {
                if (user.AccountName == "铁矿石") continue;
                BroWindow win = null;
                var lunhuiList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName == user.AccountName && p.EquipName.Contains("轮回") && p.RoleID > 0).ToList();
                if (lunhuiList.Count != 4)
                {
                    throw new Exception("轮回数量有异常");
                }
                if (lunhuiList.Select(s => s.RoleID).Intersect(targetLunhui35RoleList).Count() == 0)
                {
                    continue;
                }
                foreach (var item in lunhuiList)
                {
                    var val = AttributeMatchUtil.GetBaseAttValue(emAttrType.施法速度, item.Content).Item2;
                    var curWin = win == null ? await TabManager.Instance.TriggerAddBroToTap(user) : win;
                    win = curWin;
                    var roleId = item.RoleID;
                    var nec = curWin.User.Roles.Find(p => p.RoleId == roleId);
                    var dkGroup = nec.GetGroup().Where(p => p.Job == emJob.死骑).First();
                    var yonghengSpeed1 = dkGroup.YonghengSpeed;
                    var curDkList = curWin.User.Roles.Where(p => p.Job == emJob.死骑).ToList();
                    if (val == 35 && dkGroup.YonghengSpeed != 20)
                    {
                        //尝试交易一个20速永恒
                        var g2 = targetYonghengDkList.Where(p => !curDkList.Select(s => s.RoleId).Contains(p.RoleId)).First();
                        var targetAcc = AccountCfg.Instance.GetUserModel(g2.AccountName);
                        var win2 = await TabManager.Instance.TriggerAddBroToTap(targetAcc);
                        var dk2 = win2.User.Roles.Find(p => p.RoleId == g2.RoleId);
                        var e2 = new EquipController(win2);
                        var mfId = await e2.GetSuitId(emSuitType.MF, dk2);
                        var effId = await e2.GetSuitId(emSuitType.效率, dk2);
                        if (mfId > 0)
                        {
                            await e2.DeleteEquipSuit(emSuitType.MF, dk2);

                        }
                        if (effId > 0)
                        {
                            await e2.DeleteEquipSuit(emSuitType.效率, dk2);
                        }
                        var yongheng20 = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.RoleID == dk2.RoleId && p.EquipName.Contains("永恒") && p.Quality == "artifact" && p.EquipStatus == emEquipStatus.Equipped).First();
                        await e2.EquipOff(win2, dk2, (int)emEquipSort.副手, yongheng20);
                        var t2 = new TradeController(win2);
                        if (yongheng20 != null)
                        {
                            await t2.StartTrade(yongheng20, nec.RoleName);
                            yongheng20.SetEquipStatus(emEquipStatus.Trading);
                        }

                        var t1 = new TradeController(win);
                        await t1.AcceptAll();
                        //下掉需求dk的永恒并删除配装

                        var dk1 = win.User.Roles.Where(p => p.RoleId == dkGroup.RoleId).FirstOrDefault();
                        var yongheng = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.RoleID == dk1.RoleId && p.EquipName.Contains("永恒") && p.Quality == "artifact" && p.EquipStatus == emEquipStatus.Equipped).First();
                        var e1 = new EquipController(win);
                        var mfId1 = await e1.GetSuitId(emSuitType.MF, dk1);
                        var effId1 = await e1.GetSuitId(emSuitType.效率, dk1);
                        if (mfId1 > 0)
                        {
                            await e1.DeleteEquipSuit(emSuitType.MF, dk1);

                        }
                        if (effId1 > 0)
                        {
                            await e1.DeleteEquipSuit(emSuitType.效率, dk1);
                        }
                        await e1.EquipOff(win, dk1, (int)emEquipSort.副手);
                        await e1.AutoAttributeSave(curWin, dk1, new List<EquipModel> { yongheng20 });
                        await e1.EquipOn(win, dk1, yongheng20);
                        yongheng20.RoleID = dk1.RoleId;
                        DbUtil.InsertOrUpdate<EquipModel>(yongheng20);
                        dkGroup.YonghengSpeed = 20;
                        DbUtil.InsertOrUpdate<GroupModel>(dkGroup);
                        if (yongheng != null)
                        {
                            await t1.StartTrade(yongheng, dk2.RoleName);
                            yongheng.SetEquipStatus(emEquipStatus.Trading);
                        }

                        await t2.AcceptAll();
                        await e1.AutoAttributeSave(win2, dk2, new List<EquipModel> { yongheng });
                        await e2.EquipOn(win2, dk2, yongheng);
                        yongheng.RoleID = dk2.RoleId;
                        DbUtil.InsertOrUpdate<EquipModel>(yongheng);
                        var dkGroup2 = FreeDb.Sqlite.Select<GroupModel>().Where(p => p.RoleId == dk2.RoleId).First();
                        dkGroup2.YonghengSpeed = yonghengSpeed1;

                        DbUtil.InsertOrUpdate<GroupModel>(dkGroup2);

                        win2.Close();
                        targetYonghengDkList.Remove(g2);
                    }

                }
                win.Close();

            }
        }

        public static async Task RollJewelry()
        {
            var con = MenuInstance.SecondForm.GetSelectedEquipmentConfig();
            var eqId = long.Parse(MenuInstance.SecondForm.TxtJewelryId.Text);
            // var con = EmEquipCfg.Instance.Data[emEquip.冰霜珠宝];
            //30712677  32386191
            var testEq = new EquipModel() { Category = "珠宝", Quality = "rare", EquipID = eqId };
            var mainAcc = RepairManager.MainAcc;
            var u = AccountCfg.Instance.GetUserModel(mainAcc);
            var win = await TabManager.Instance.TriggerAddBroToTap(u);
            var r = new ReformController(win);
            await ReformLoop(testEq, win, r, con);

        }
        private static async Task ReformLoop(EquipModel eq, BroWindow win, ReformController r, Equipment con)
        {
            var url = $"https://www.idleinfinity.cn/Equipment/Reform?id={win.User.FirstRole.RoleId}&eid={eq.EquipID}";
            if (win.GetBro().Address != url)
            {
                await win.LoadUrlWaitJsInit(url, "reform");
            }
            Random random = new Random();
            int randomNumber = random.Next(5, 7); // 上限但不包含
            await Task.Delay(randomNumber);
            var c = await win.CallJs<string>("_reform.getEquipContent()");
            eq.Content = c;
            AttributeMatchReport report = new AttributeMatchReport();

            if (whiteList.Where(p => AttributeMatchUtil.Match(eq, p, out _)).Count() > 0)
            {
                Console.WriteLine("命中");
                return;

            }
            if (AttributeMatchUtil.Match(eq, con, out report))
            {
                Console.WriteLine("命中");
                return;
            }
            Console.WriteLine("命中条件数:" + report.MustMastchCount);

            await r.ReformEquip(eq, win.User.FirstRole.RoleId, emReformType.Rare19);

            await ReformLoop(eq, win, r, con);
        }

        public static async Task RuneUpgrade(BroWindow win)
        {
            var r = new RuneController(win);
            await r.AutoUpgradeRune(win, win.User);
        }
        public static void InitializeRuneCfgItems()
        {
            var Runes = RuneCompandCfg.Instance.RuneCompandData;
            DbUtil.InsertOrUpdate<RuneCompandData>(Runes);
        }

        public static async Task ReformShengyi()
        {
            var specialList = new string[] { "亡灵哀嚎", "燃魂之焰" };
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.Content.Contains("蛇夫座") && p.RoleID > 0 && p.EquipStatus == emEquipStatus.Equipped  ).ToList();
            list = list.Where(p => specialList.Contains(p.EquipName)).ToList();
            var active25 = bool.Parse(ConfigUtil.GetAppSetting("Active25"));
            var group = list.GroupBy(g => g.AccountName);
            var con21 = EmEquipCfg.Instance.GetEquipCondition(emEquip.死灵圣衣洗点21);
            var con25 = EmEquipCfg.Instance.GetEquipCondition(emEquip.死灵圣衣洗点25);
            var conTaiguEq = EmEquipCfg.Instance.GetEquipCondition(emEquip.太古珠装备名单);
            var conNecTaigu = EmEquipCfg.Instance.GetEquipCondition(emEquip.死灵太古珠条件);
            foreach (var item in group)
            {
                BroWindow win = null;
                var user = AccountCfg.Instance.GetUserModel(item.Key);
                foreach (var e in item)
                {
                    var isMatch21Rule = AttributeMatchUtil.Match(e, con21, out _);
                    //不满足满roll法师 

                    if (!isMatch21Rule)
                    {
                        //开始洗点21
                        win = await GetWin(win, user);
                        var r = new ReformController(win);
                        await ReformUntilCondition(e, win, con21, 120, 0, emReformType.Set21);

                    }
                    isMatch21Rule = AttributeMatchUtil.Match(e, con21, out _);
                    var hasTaigu = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "太古之珠" && (p.EquipStatus == emEquipStatus.Repo || p.EquipStatus == emEquipStatus.Package)).Count() > 0;
                    var isMatchTaigu = isMatch21Rule && AttributeMatchUtil.Match(e, conTaiguEq, out _) && AttributeMatchUtil.Match(e, conNecTaigu, out _);
                    //满足太古条件 且有珠子
                    if (isMatchTaigu && hasTaigu)
                    {
                        win = await GetWin(win, user);
                        //尝试插太古珠
                        var isSuccess = await InsertTaigu(win, e);
                        if (isSuccess) continue;
                    }
                    else if (isMatchTaigu)
                    {
                        //没珠子跳过 别浪费25
                        // continue;
                    }
                    isMatch21Rule = AttributeMatchUtil.Match(e, con21, out _);

                    if (active25 && isMatch21Rule && !AttributeMatchUtil.Match(e, con25, out _))
                    {
                        //开始洗点25
                        win = await GetWin(win, user);
                        var r = new ReformController(win);
                        await ReformUntilCondition(e, win, con25, 20, 0, emReformType.Set25);

                    }
                }
                if (win != null) win.Close();
            }

            async Task<BroWindow> GetWin(BroWindow win, UserModel user)
            {
                win = win == null ? await TabManager.Instance.TriggerAddBroToTap(user) : win;

                return win;
            }
        }

        /// <summary>
        /// 将符文发送给指定用户
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="toRoleName"></param>
        /// <returns></returns>
        public static async Task TradeRune(Dictionary<int, int> dic)
        {
            var win = TabManager.Instance.GetWindow();
            var role = win.User.FirstRole;
            var roleName = role.RoleName;
            var recWin = await TabManager.Instance.TriggerAddBroToTap(win.User);
            string acc = ConfigUtil.GetAppSetting("RuneAcc");
            var runeWin = await TabManager.Instance.TriggerAddBroToTap(AccountCfg.Instance.GetUserModel(acc));
            var t = new TradeController(runeWin);
            await t.TradeRune(dic, roleName, false);
            runeWin.Close();
            var t1 = new TradeController(recWin);
            await t1.AcceptAll();
            recWin.Close();

        }

        /// <summary>
        /// 改造冥神
        /// </summary>
        /// <returns></returns>
        public static async Task ReformMinshen()
        {
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName.Contains("冥神之力") && p.RoleID > 0).ToList();
            var group = list.GroupBy(g => g.AccountName);
            var con = EmEquipCfg.Instance.GetEquipCondition(emEquip.冥神条件);
            foreach (var item in group)
            {
                BroWindow win = null;
                var user = AccountCfg.Instance.GetUserModel(item.Key);
                foreach (var e in item)
                {
                    var isMatch = AttributeMatchUtil.Match(e, con, out _);
                    if (!isMatch)
                    {
                        win = win == null ? await TabManager.Instance.TriggerAddBroToTap(user) : win;
                        var r = new ReformController(win);
                        await ReformUntilCondition(e, win, con, 10, 0, emReformType.Unique22);

                    }

                }
                if (win != null) win.Close();
            }
        }

        /// <summary>
        /// 插太古珠
        /// </summary>
        /// <returns></returns>
        private static async Task<bool> InsertTaigu(BroWindow win, EquipModel eq)
        {
            var role = win.User.Roles.Find(p => p.RoleId == eq.RoleID);
            var url = IdleUrlHelper.InlayUrl(role.RoleId, eq.EquipID);
            if (win.GetBro().Address != url)
            {
                await win.LoadUrlWaitJsInit(url, "inlay");
                await Task.Delay(1000);
            }

            var i = new InlayController(win);
            var content = await i.GetEquipContent();//防止bug页面上二次确认当前属性
            if (content.Contains("★")) return true;


            var localJewel = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "太古之珠" && p.AccountName == win.User.AccountName && p.EquipStatus == emEquipStatus.Repo).First();
            var otherJewel = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "太古之珠" && p.AccountName != win.User.AccountName && (p.EquipStatus == emEquipStatus.Repo || p.EquipStatus == emEquipStatus.Package)).First();
            EquipModel target;
            target = localJewel;
            if (localJewel == null && otherJewel != null)
            {
                //交易一个珠宝过来
                var targetUser = AccountCfg.Instance.GetUserModel(otherJewel.AccountName);
                var win2 = await TabManager.Instance.TriggerAddBroToTap(targetUser);
                var t2 = new TradeController(win2);
                await t2.StartTrade(otherJewel, role.RoleName);
                var t1 = new TradeController(win);
                await t1.AcceptAll();
                target = otherJewel;
                await win.LoadUrlWaitJsInit(url, "inlay");
                await Task.Delay(1000);
                win2.Close();
            }
            if (target == null) return false;

            await win.CallJsWaitReload($"_inlay.equipInlay({target.EquipID})", "inlay");
            await Task.Delay(1500);

            // 更新装备状态
            target.EquipStatus = emEquipStatus.Equipped;
            DbUtil.InsertOrUpdate<EquipModel>(target);
            eq.Content = await i.GetEquipContent();//防止bug页面上二次确认当前属性
            DbUtil.InsertOrUpdate<EquipModel>(eq);
            return true;
        }

        private static async Task ReformUntilCondition(EquipModel eq, BroWindow win, Equipment con, int maxCount, int curCount, emReformType type)
        {
            var bro = win.GetBro();
            var url = IdleUrlHelper.ReformUrl(eq.RoleID, eq.EquipID);
            if (bro.Address != url)
            {
                await win.LoadUrlWaitJsInit(url, "reform");
            }
            var r = new ReformController(win);
            var content = await r.GetEquipContent();//防止bug页面上二次确认当前属性
            eq.Content = content;
            if (AttributeMatchUtil.Match(eq, con, out _))
            {
                await Task.Delay(1500);
                return;
            }
            if (curCount >= maxCount) return;
            await Task.Delay(5000);
            var result = await r.ReformEquip(eq, eq.RoleID, type);

            if (!result && type == emReformType.Set21)
            {
                await ReformUntilCondition(eq, win, con, maxCount, ++curCount, emReformType.Set23);
            }
            if (!result)
            {
                var reformService = new ReformService(win.User.AccountName);
                var summary = reformService.GetEquipCountToReform();
                if (emReformType.Set21 == type || emReformType.Set23 == type)
                {
                    await TradeRune(new Dictionary<int, int>() { { 21, summary[21] * 20 } });
                    await Task.Delay(1000);
                }
                else if (emReformType.Set25 == type)
                {
                    await TradeRune(new Dictionary<int, int>() { { 25, summary[25] * 4 } });
                    await Task.Delay(1000);

                }

                //var matchSignal = await win.SignalRaceCallBack(new string[] { emSignal.Continue.ToString(), emSignal.Skip.ToString() }, () =>
                //{

                //});
                //if (matchSignal == emSignal.Skip.ToString())
                //{
                //    return;
                //}
                if (!result && emReformType.Set23 == type)
                {
                    type = emReformType.Set21;
                }
                bro.Reload();
            }

            await ReformUntilCondition(eq, win, con, maxCount, ++curCount, type);
        }

        public static Dictionary<int, int> GetEquipCountToReform(string accountName, Equipment con21, Equipment con25)
        {
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName == accountName && p.RoleID > 0 && p.Content.Contains("蛇夫座")).ToList();

            var result = new Dictionary<int, int>();
            var count21 = 0;
            var count25 = 0;

            foreach (var item in list)
            {
                if (!AttributeMatchUtil.Match(item, con21, out _))
                {
                    count21++;
                }
                else
                {
                    if (!AttributeMatchUtil.Match(item, con25, out _))
                    {
                        count25++;
                    }
                }
            }
            result.Add(21, count21);
            result.Add(25, count25);
            return result;
        }

        public static async Task ThrowJordan()
        {
            int count = int.Parse(MenuInstance.SecondForm.TxtJordan.Text.Trim());
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "乔丹之石").Take(count).ToList();
            var group = list.GroupBy(g => g.AccountName).ToList();
            foreach (var item in group)
            {
                var user = AccountCfg.Instance.GetUserModel(item.Key);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                var g2 = item.GroupBy(g => g.RoleID).ToList();
                foreach (var l in g2)
                {

                    var idList = l.Select(s => s.EquipID).ToList();
                    var eids = string.Join(",", idList);
                    int roleId = l.Key == 0 ? win.User.FirstRole.RoleId : l.Key;
                    await win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(roleId), "equip");
                    await Task.Delay(1500);
                    var r = await win.CallJsWaitReload($@"equipClear({roleId},""{eids}"")", "equip");
                    if (r.Success)
                    {
                        FreeDb.Sqlite.Delete<EquipModel>().Where(p => idList.Contains(p.EquipID)).ExecuteAffrows();
                        FreeDb.Sqlite.Delete<DelPreviewEquipModel>().Where(p => idList.Contains(p.EquipID)).ExecuteAffrows();
                    }
                }

                win.Close();
            }


        }

        /// <summary>
        /// 正义发给斧永恒且有末日的队伍 冰冻发给剑永恒队伍
        /// </summary>
        /// <returns></returns>
        public static async Task SwitchFrostAndJustice()
        {

            var groupList = FreeDb.Sqlite.Select<GroupModel>().ToList();
            var justiceList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName.Contains("正义之手")).ToList();
            //检查正义队伍dk是否有末日 有的话跳过
            var filterJusticeList = new List<EquipModel>();
            foreach (var item in justiceList)
            {
                if (item.RoleID == 0)
                {
                    filterJusticeList.Add(item);
                    continue;
                }
                var jusKnight = groupList.Find(p => p.RoleId == item.RoleID);
                //正义队的dk
                var jusDk = groupList.Find(p => p.AccountName == jusKnight.AccountName && p.TeamIndex == jusKnight.TeamIndex && p.Job == emJob.死骑);
                var hasMori = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "末日" && p.RoleID == jusDk.RoleId).First() != null;
                if (!hasMori) filterJusticeList.Add(item);
            }
            var frostList = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName.Contains("冰冻") && p.Quality == "artifact").ToList();
            var shuangtouYongheng = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName.Contains("永恒") && p.Content.Contains("双头斧")).ToList();
            var toSwitchMoriList = new List<EquipModel>();
            foreach (var item in shuangtouYongheng)
            {
                var dkId = item.RoleID;
                var dk = groupList.Where(p => p.RoleId == dkId).FirstOrDefault();
                var knight = groupList.Find(p => p.AccountName == dk.AccountName && p.TeamIndex == dk.TeamIndex && p.Job == emJob.骑士);
                var mori = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "末日" && !p.Content.Contains("巨神") && p.RoleID == knight.RoleId).First();
                var hasFeilong = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "飞龙" && p.RoleID == knight.RoleId).First() != null;
                if (mori != null && !hasFeilong)
                {
                    toSwitchMoriList.Add(mori);
                }
            }

            for (int i = 0; i < filterJusticeList.Count; i++)
            {

                var justice = filterJusticeList[i];
                var mori = toSwitchMoriList[i];
                var justiceUser = AccountCfg.Instance.GetUserModel(justice.AccountName);
                var winJus = await TabManager.Instance.TriggerAddBroToTap(justiceUser);
                var moriUser = AccountCfg.Instance.GetUserModel(mori.AccountName);
                var winMori = await TabManager.Instance.TriggerAddBroToTap(moriUser);
                // 脱下正义
                var e1 = new EquipController(winJus);
                if (justice.RoleID > 0)
                {
                    var knightJus = winJus.User.Roles.Find(p => p.RoleId == justice.RoleID);
                    await e1.EquipOff(winJus, knightJus, (int)emEquipSort.主手, justice);
                    await Task.Delay(1500);
                    await e1.AutoEquips(winJus, knightJus);
                }
                var t1 = new TradeController(winJus);
                await t1.StartTrade(justice, mori.RoleName);
                var t2 = new TradeController(winMori);
                await t2.AcceptAll();
                await Task.Delay(1500);
                var e2 = new EquipController(winMori);
                var knightMori = winMori.User.Roles.Find(p => p.RoleId == mori.RoleID);
                var knightMoriGroup = knightMori.GetGroup();
                var dkMoriRoleid = knightMoriGroup.Find(p => p.Job == emJob.死骑).RoleId;
                var dkMori = winMori.User.Roles.Find(p => p.RoleId == dkMoriRoleid);
                var curEquips = await e2.AutoEquips(winMori, knightMori);
                var c2 = new CharacterController(winMori);
                await c2.AddSkillPoints(knightMori, curEquips);
                await Task.Delay(1500);
                var yongheng = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.RoleID == dkMori.RoleId && p.EquipName.Contains("永恒")).First();

                await MoveMoriToDk(mori, dkMori, winMori, yongheng);

            }



        }

        /// <summary>
        /// 将双头末日装配给dk 需要放在副手 且腰带和手套不能有攻速
        /// </summary>
        /// <returns></returns>
        public static async Task MoveMoriToDk(EquipModel mori, RoleModel dk, BroWindow win, EquipModel yongheng)
        {
            var e = new EquipController(win);

            var main = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.RoleID == dk.RoleId && p.emEquipSort == emEquipSort.主手).First();




            var mfId = await e.GetSuitId(emSuitType.MF, dk);
            if (mfId > 0)
            {
                await e.LoadSuit(emSuitType.MF, dk);
            }
            await e.EquipOff(win, dk, (int)emEquipSort.副手, yongheng);
            await e.EquipOff(win, dk, (int)emEquipSort.主手, main);

            if (mfId > 0)
            {
                await e.AutoEquips(win, dk, targetSuitType: emSuitType.MF);

                await e.SaveEquipSuit(emSuitType.MF, dk);
            }
            else
            {
                await e.AutoEquips(win, dk, targetSuitType: emSuitType.效率);
            }


        }

        public static async Task UseBox()
        {
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName.Contains("神秘宝箱") && p.EquipStatus == emEquipStatus.Repo).ToList();
            var group = list.GroupBy(g => g.AccountName);
            foreach (var item in group)
            {
                var acc = item.Key;
                var user = AccountCfg.Instance.GetUserModel(acc);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                var e = new EquipController(win);
                foreach (var eq in item)
                {

                    await e.EquipUse(eq);
                }
                win.Close();
            }
        }

        public static async Task SendJieduBase()
        {

            for (int i = 37; i < AccountCfg.Instance.Accounts.Count; i++)
            {
                var a = AccountCfg.Instance.Accounts[i];
                if (!a.AccountName.StartsWith("0")) continue;
                var user = AccountCfg.Instance.GetUserModel(a.AccountName);
                var win = await TabManager.Instance.TriggerAddBroToTap(user);
                var toSendRole = win.User.FirstRole;

                var list = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.Content.Contains("凋零光环") && p.Content.Contains("0/3") && !p.AccountName.StartsWith("0") && p.Category == "死骑面罩" && p.Quality == "slot" && p.EquipStatus == emEquipStatus.Repo).ToList();
                var group = list.GroupBy(g => g.AccountName).ToList();
                int count = 0;
                foreach (var item in group)
                {
                    if (count == 3) break;
                    var accountName = item.Key;
                    var u = AccountCfg.Instance.Accounts.Where(p => p.AccountName == accountName).First();
                    var user1 = new UserModel(u);
                    var win2 = await TabManager.Instance.TriggerAddBroToTap(user1);
                    var tradeControl = new TradeController(win2);
                    await Task.Delay(1500);
                    foreach (var e in item)
                    {
                        if (count == 3) break;
                        await tradeControl.StartTrade(e, toSendRole.RoleName);
                        await Task.Delay(1500);
                        e.EquipStatus = emEquipStatus.Trading;
                        DbUtil.InsertOrUpdate<EquipModel>(e);
                        count++;
                    }
                    win2.Close();
                }
                win.Close();

            }
        }


    }
}
