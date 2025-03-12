using IdleAuto.Scripts.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Controller
{
    /// <summary>
    /// 流程控制 写静态方法
    /// </summary>
    public class FlowController
    {

        public static async Task GroupWork(int size, Func<BroWindow, Task> act)
        {
            if (size <= 0)
                throw new ArgumentException("Group size must be greater than 0.", nameof(size));

            if (act == null)
                throw new ArgumentNullException(nameof(act));


            // 获取账户列表
            var accounts = AccountCfg.Instance.Accounts;

            // 每次处理3个账户
            int groupSize = size;

            for (int i = 1; i < accounts.Count; i += groupSize)
            {    // 创建一个任务列表，用于存储当前组的任务
                var tasks = new List<Task>();
                // 获取当前组的账户
                var group = accounts.Skip(i).Take(groupSize).ToList();
                foreach (var account in group)
                {
                    var user = new UserModel(account);
                    var window = await TabManager.Instance.TriggerAddBroToTap(user);
                    tasks.Add(act(window));
                }
                // 等待当前组的所有任务完成
                await Task.WhenAll(tasks);
            }


        }

        public static async Task StartMapSwitch()
        {

            var user = AccountController.Instance.User;
            var window = await TabManager.Instance.TriggerAddBroToTap(user);
            var controller = new CharacterController(window);
            await controller.StartSwitchMap(window.GetBro(), window.User);
            window.Close();

        }

        public static async Task StartMapSwitch(BroWindow window)
        {
            var controller = new CharacterController(window);
            await controller.StartSwitchMap(window.GetBro(), window.User);
            window.Close();
        }

        /// <summary>
        /// 加点
        /// </summary>
        /// <returns></returns>
        public static async Task StartAddSkill()
        {
            for (int i = 1; i < AccountCfg.Instance.Accounts.Count; i++)
            {
                var account = AccountCfg.Instance.Accounts[i];
                if (account.AccountName == "铁矿石") continue;
                var user = new UserModel(account);
                var window = await TabManager.Instance.TriggerAddBroToTap(user);
                var controller = new CharacterController(window);
                await controller.StartAddSkill(window.GetBro(), window.User);
                window.Close();
            }


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
        public static async Task SyncFilter()
        {
            for (int i = 2; i < AccountCfg.Instance.Accounts.Count; i++)
            {
                var account = AccountCfg.Instance.Accounts[i];
                if (account.AccountName == "铁矿石") continue;
                var user = new UserModel(account);
                var window = await TabManager.Instance.TriggerAddBroToTap(user);
                var control = new CharacterController(window);
                await control.StartSyncFilter(window.GetBro(), window.User);
                window.Close();
            }
        }

        public static async Task StartEfficencyMonitor()
        {
            for (int i = 0; i < AccountCfg.Instance.Accounts.Count; i++)
            {
                var account = AccountCfg.Instance.Accounts[i];
                var user = new UserModel(account);
                var window = await TabManager.Instance.TriggerAddBroToTap(user);
                var control = new EfficiencyController(window);
                await control.StartMonitor(user);
                window.Close();
            }
        }

        public static async Task MakeArtifact()
        {
            var account = AccountCfg.Instance.Accounts[19];
            var user = new UserModel(account);
            var window = await TabManager.Instance.TriggerAddBroToTap(user);
            var control = new ArtifactController(window);
            var condition = ArtifactBaseCfg.Instance.GetEquipCondition(emArtifactBase.低力量隐密);
            var eqControll = new EquipController();
            var baseEq = eqControll.GetMatchEquips(account.AccountID, condition, out _).ToList().FirstOrDefault();
            if (baseEq.Value != null)
            {
                long equipId = await control.MakeArtifact(emArtifactBase.低力量隐密, baseEq.Value, user.Roles[1].RoleId);
            }


        }

        public static async Task MakeArtifactTest()
        {
            for (int i = 11; i < AccountCfg.Instance.Accounts.Count; i++)
            {
                var account = AccountCfg.Instance.Accounts[i];
                if (account.AccountName == "铁矿石" || account.AccountName == "阿绿5") continue;
                var user = new UserModel(account);

                //await RepairManager.Instance.ClearEquips(user);
               // await RepairManager.Instance.UpdateEquips(user);
                var window = await TabManager.Instance.TriggerAddBroToTap(user);
                var control = new ArtifactController(window);
                var condition = ArtifactBaseCfg.Instance.GetEquipCondition(emArtifactBase.低力量隐密);
                var eqControll = new EquipController();
                for (int j = 1; j < user.Roles.Count; j += 3)
                {
                    var role = user.Roles[j];
                    var baseEq = eqControll.GetMatchEquips(account.AccountID, condition, out _).ToList().FirstOrDefault();
                    if (baseEq.Value != null)
                    {
                        long equipId = await control.MakeArtifact(emArtifactBase.低力量隐密, baseEq.Value, role.RoleId);
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

    }
}
