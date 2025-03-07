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
        public static async Task StartMapSwitch()
        {
            var user = AccountController.Instance.User;
            var window = await TabManager.Instance.TriggerAddBroToTap(user);
            await window.CharController.StartSwitchMap(window.GetBro(), window.User);
            window.Close();
        }

        /// <summary>
        /// 加点
        /// </summary>
        /// <returns></returns>
        public static async Task StartAddSkill()
        {
            for (int i = 0; i < AccountCfg.Instance.Accounts.Count; i++)
            {
                var account = AccountCfg.Instance.Accounts[i];
                if (account.AccountName == "铁矿石") continue;
                var user = new UserModel(account);
                var window = await TabManager.Instance.TriggerAddBroToTap(user);
                await window.CharController.StartAddSkill(window.GetBro(), window.User);
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
                await window.CharController.StartSyncFilter(window.GetBro(), window.User);
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
            for (int i = 6; i < AccountCfg.Instance.Accounts.Count; i++)
            {
                var account = AccountCfg.Instance.Accounts[i];
                if (account.AccountName == "铁矿石" || account.AccountName == "阿绿5") continue;
                var user = new UserModel(account);

                //await RepairManager.Instance.ClearEquips(user);
                //await RepairManager.Instance.UpdateEquips(user);
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
