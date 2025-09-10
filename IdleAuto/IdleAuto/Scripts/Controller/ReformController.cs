using AttributeMatch;
using CefSharp;
using IdleAuto.Configs.CfgExtension;
using IdleAuto.Db;
using IdleAuto.Scripts.Service;
using IdleAuto.Scripts.Utils;
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



        public async Task<bool> ReformEquip(EquipModel equip, int roleId, emReformType reformType)
        {
            var summaryType = new emReformType[] { emReformType.Set21, emReformType.Set23, emReformType.Set25 };
            await Task.Delay(1500);
            var url = $"https://www.idleinfinity.cn/Equipment/Reform?id={roleId}&eid={equip.EquipID}";
            if (_win.GetBro().Address != url)
            {
                await _win.LoadUrlWaitJsInit(url, "reform");
            }
            await Task.Delay(1500);
      
            var d = new Dictionary<string, object>();

            d.Add("type", reformType);
            //改造完会跳转到装备栏界面
            var materialResult = await _win.CallJs("_reform.isMeterialEnough()");
            if (materialResult.Success)
            {
                var r = materialResult.Result.ToObject<Dictionary<string, bool>>();
                if (reformType == emReformType.Mage)
                {
                    if (!r["canMage"]) return false;

                }
                else if (reformType == emReformType.Set21)
                {
                    if (!r["canSet21"]) return false;
                }
                else if (reformType == emReformType.Set23)
                {
                    if (!r["canSet23"]) return false;
                }
                else if (reformType == emReformType.Set25)
                {
                    if (!r["canSet25"]) return false;
                }
                else if (reformType == emReformType.Unique22)
                {
                    if (!r["canUnique22"]) return false;
                }

            }
            var a = await _win.CallJsWaitReload($"_reform.reform({d.ToLowerCamelCase()})", "reform");
            await UpdateContent(equip, reformType);
            return true;
        }

        private async Task UpdateContent(EquipModel equip, emReformType reformType)
        {
            //打孔会直接跳到装备页不能更新装备内容
            var updateTypeList = new List<emReformType>() { emReformType.Mage, emReformType.UpgradeMagical, emReformType.UpgradeRare, emReformType.Set21, emReformType.Set25, emReformType.Rare19, emReformType.Unique22 };
            if (!updateTypeList.Contains(reformType)) return;
            var c = await _win.CallJs<string>("_reform.getEquipContent()");
            var content = c;
            var quality = equip.Quality;
            switch (reformType)
            {
                case emReformType.UpgradeRare:
                    quality = "rare";
                    break;
                case emReformType.UpgradeMagical:
                    quality = "magical";
                    break;
                case emReformType.Mage:
                    quality = "craft";
                    break;
            }
            equip.Quality = quality;
            equip.Content = content;
            FreeDb.Sqlite.InsertOrUpdate<EquipModel>().SetSource(equip).ExecuteAffrows();
        }

        public async Task<string> GetEquipContent()
        {
            var c = await _win.CallJs<string>("_reform.getEquipContent()");
            return c;
        }



        public async Task RemoveRune(EquipModel equip, int roleId)
        {
            await Task.Delay(1500);
            var aa = await _win.LoadUrlWaitJsInit($" https://www.idleinfinity.cn/Equipment/Reform?id={roleId}&eid={equip.EquipID}", "reform");
            var d = new Dictionary<string, object>();
            var a = await _win.CallJsWaitReload($"_reform.removeRune()", "equip");
            equip.Quality = "slot";
            DbUtil.InsertOrUpdate<EquipModel>(equip);
            await Task.Delay(1000);
            //改造完会跳转到装备栏界面
        }

        /// <summary>
        /// 升级背包
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="eqList"></param>
        /// <returns></returns>
        public async Task UpgradeBaseEquip(RoleModel role, List<EquipModel> eqList)
        {
            //https://www.idleinfinity.cn/Equipment/EquipUpgradeBoxAll
            //eidsbox: 拼接
            var response = await _win.LoadUrlWaitJsInit(IdleUrlHelper.EquipUrl(role.RoleId), "equip");

            var edis = string.Join(",", eqList.Select(p => p.EquipID));
            var data = new Dictionary<string, object> { { "eidsbox", edis } };
            var r = await _win.CallJsWaitReload($"upgradeAllInRepo({data.ToLowerCamelCase()})", "equip");

        }


    }
}
