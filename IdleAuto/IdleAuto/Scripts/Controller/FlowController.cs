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
            int seed = await TabManager.Instance.TriggerAddBroToTap(user.AccountName, "https://www.idleinfinity.cn/Home/Index", "char");
            var window = TabManager.Instance.GetWindow(seed);
            await window.CharController.StartSwitchMap(window.GetBro(), user);
        }
    }
}
