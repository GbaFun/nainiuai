using IdleAuto.Db;
using IdleAuto.Scripts.Model;
using IdleAuto.Scripts.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Controller
{
    public class EfficiencyController : BaseController
    {
        public EfficiencyController(BroWindow win) : base(win)
        {

        }
        /// <summary>
        /// 监控效率 存表
        /// </summary>
        /// <returns></returns>
        public async Task StartMonitor(UserModel user)
        {
            await Task.Delay(3000);
            await _win.LoadUrlWaitJsInit("https://www.idleinfinity.cn/Battle/Guaji", "guaji");
            var data = await _win.CallJs($"_guaji.getData()");
            var arr = data.Result.ToObject<List<Efficency>>();
            arr.ForEach(p =>
            {
                p.UserName = user.AccountName;
            });
            var r = FreeDb.Sqlite.InsertOrUpdate<Efficency>().SetSource(arr).ExecuteAffrows();
            Console.WriteLine(arr);
        }
    }
}
