using AttributeMatch;
using CefSharp;
using IdleAuto.Db;
using IdleAuto.Scripts.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace IdleAuto.Scripts.Controller
{
    public class ReformController : BaseController
    {
        public ReformController(BroWindow win) : base(win)
        {

        }
        //post type 54 随机 50 20打孔
        //跳转 https://www.idleinfinity.cn/Equipment/Reform?id=392&eid=10076189 
        //按改造条件选择改造 https://www.idleinfinity.cn/Equipment/EquipReform
        //https://www.idleinfinity.cn/Equipment/Inlay?id=392&eid=8833783 去看一下几孔 失败返回false

        public async Task<bool> SlotReform(long eid, int roleId, Equipment eq)
        {
            
            await Task.Delay(1500);
            await _win.LoadUrlWaitJsInit($" https://www.idleinfinity.cn/Equipment/Reform?id={roleId}&eid={eid}", "reform");

            if (2 == 2)
            {
                return false;
            }
            else return true;
        }

    }
}
