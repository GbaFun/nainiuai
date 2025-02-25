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
            int seed = await TabManager.Instance.TriggerAddBroToTap(user);
            var window = TabManager.Instance.GetWindow(seed);
            await window.CharController.StartSwitchMap(window.GetBro(), window.User);
        }

        /// <summary>
        /// 加点
        /// </summary>
        /// <returns></returns>
        public static async Task StartAddSkill()
        {
            foreach (var account in AccountCfg.Instance.Accounts)
            {
                var user = new UserModel(account);
                int seed = await TabManager.Instance.TriggerAddBroToTap(user);
                var window = TabManager.Instance.GetWindow(seed);
                await window.CharController.StartAddSkill(window.GetBro(), window.User);
                TabManager.Instance.DisposePage(seed);
            }


        }

        public static async Task RefreshCookie()
        {
            BroTabManager.Instance.ClearBrowsers();
            foreach (var account in AccountCfg.Instance.Accounts)
            {
                AccountController.Instance.User = new UserModel(account);
                await MainForm.Instance.TabManager.TriggerAddTabPage(account.AccountName, "https://www.idleinfinity.cn/Home/Index");
                await Task.Delay(2000);
            }

        }
    }
}
