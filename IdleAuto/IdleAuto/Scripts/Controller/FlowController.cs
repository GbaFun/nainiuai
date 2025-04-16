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

        public static async Task MakeArtifact()
        {
            var account = AccountCfg.Instance.Accounts[19];
            var user = new UserModel(account);
            var window = await TabManager.Instance.TriggerAddBroToTap(user);
            var control = new ArtifactController(window);
            var condition = ArtifactBaseCfg.Instance.GetEquipCondition(emArtifactBase.低力量隐密);
            var eqControll = new EquipController(window);
            var baseEq = eqControll.GetMatchEquips(account.AccountID, condition, out _).ToList().FirstOrDefault();
            if (baseEq.Value != null)
            {
                var equip = await control.MakeArtifact(emArtifactBase.低力量隐密, baseEq.Value, user.Roles[1].RoleId, condition);
            }


        }

        public static async Task MakeArtifactTest()
        {
            for (int i = 1; i < AccountCfg.Instance.Accounts.Count; i++)
            {
                var account = AccountCfg.Instance.Accounts[i];
                if (account.AccountName == "铁矿石") continue;
                var user = new UserModel(account);

                //await RepairManager.Instance.ClearEquips(user);
                // await RepairManager.Instance.UpdateEquips(user);
                var window = await TabManager.Instance.TriggerAddBroToTap(user);
                var control = new ArtifactController(window);
                var condition = ArtifactBaseCfg.Instance.GetEquipCondition(emArtifactBase.天灾);
                var eqControll = new EquipController(window);
                for (int j = 2; j < user.Roles.Count; j += 3)
                {
                    var role = user.Roles[j];
                    var baseEq = eqControll.GetMatchEquips(account.AccountID, condition, out _).ToList().FirstOrDefault();
                    if (baseEq.Value != null)
                    {
                        var equip = await control.MakeArtifact(emArtifactBase.天灾, baseEq.Value, role.RoleId, condition);
                        long equipId = equip.EquipID;
                        await Task.Delay(2000);
                        await eqControll.AutoAttributeSave(window, role, new List<EquipModel> { baseEq.Value });
                        await Task.Delay(2000);
                        var result3 = await window.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId), "equip");
                        await Task.Delay(2000);
                        await window.CallJsWaitReload($@"equipOn({role.RoleId},{equipId})", "equip");
                    }
                }

                window.Close();
                //给每个死灵穿上隐密
                //先清包
                //盘库
                //做神器
                //穿上
            }
        }

        public static async Task SendRune()
        {
            var specifiedAccount = RepairManager.NainiuAccounts;
            //所有资源将汇集到这 为了避免二次验证经常要换号收货
            var reciver = "奶牛苦工24";
            var reciverUser = "RasdGch";
            var sendDic = new Dictionary<int, int>() { { 26, 1 } };
            var userList = AccountCfg.Instance.Accounts.Where(p => p.AccountName != reciverUser && specifiedAccount.Contains(p.AccountName)).ToList();
            BroWindow curWin, nextWin = null;
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

                curWin = nextWin != null ? nextWin : await TabManager.Instance.TriggerAddBroToTap(user);

                if (i != userList.Count - 1)
                {
                    nextUser = new UserModel(userList[i + 1]);


                    var r = await CheckNextAccountRune(nextUser, sendDic);
                    if (r == null) { curWin.Close(); continue; };
                    nextWin = r;
                    await SendRuneToNext(curWin, nextUser.AccountName, sendDic);
                    curWin.Close();
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
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.EquipName == "彩虹刻面" && p.Content.Contains("火焰") && p.EquipStatus == emEquipStatus.Repo && p.IsLocal == false).ToList();
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
            var repairList = FreeDb.Sqlite.Select<TradeModel>().Where(p => p.TradeStatus == emTradeStatus.Sent).ToList().GroupBy(g => g.DemandRoleId);
            foreach (var item in repairList)
            {
                var roleId = item.Key;
                var accName = item.Max(m => m.DemandAccountName);
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
                    var role = win.User.Roles.Find(p => p.RoleId == roleId);
                   await RepairManager.Instance.AutoEquip(win,control, user, role);
                }
                win.Close();
            }
        }


    }
}
