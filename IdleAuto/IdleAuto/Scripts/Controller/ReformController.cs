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

        public async Task<bool> SlotReform(EquipModel equip, int roleId, ArtifactBaseConfig eq, emArtifactBase emBase)
        {

            await Task.Delay(1500);
            var aa = await _win.LoadUrlWaitJsInit($" https://www.idleinfinity.cn/Equipment/Reform?id={roleId}&eid={equip.EquipID}", "reform");
            await Task.Delay(1500);
            var d = new Dictionary<string, object>();
            var type = ArtifactBaseCfg.Instance.MatchSlotType(equip, emBase);
            d.Add("type", type);
            //改造完会跳转到装备栏界面
            var materialResult = await _win.CallJs("_reform.isMeterialEnough()");
            if (materialResult.Success)
            {
                var r = materialResult.Result.ToObject<Dictionary<string, bool>>();
                if (type == emSlotType.Random)
                {
                    if (!r["canRandom"]) return false;
                }
                else if (type == emSlotType.Direct)
                {
                    if (!r["canDirect"]) return false;
                }
            }
            var a = await _win.CallJsWaitReload($"_reform.reform({d.ToLowerCamelCase()})", "reform");
            await Task.Delay(1500);
            await _win.LoadUrlWaitJsInit($"https://www.idleinfinity.cn/Equipment/Inlay?id={roleId}&eid={equip.EquipID} ", "inlay");
       
            var dd = await _win.CallJs("_inlay.getSlotCount();");
            //更新下content
            var count = dd.Result.ToObject<int>();
            var e = await _win.CallJs("_inlay.getEquipContent()");
            var content = e.Result.ToObject<string>();
            equip.Quality = emItemQuality.破碎.ToString();
            equip.Content = content;
            FreeDb.Sqlite.InsertOrUpdate<EquipModel>().SetSource(equip).ExecuteAffrows();
            if (count == eq.TargetSlotCount)
            {
                return true;
            }
            else return false;
        }

    }
}
