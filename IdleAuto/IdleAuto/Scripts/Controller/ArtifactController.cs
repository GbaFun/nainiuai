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
    public class ArtifactController : BaseController
    {
        public ArtifactController(BroWindow win) : base(win)
        {

        }
        /// <summary>
        /// 做一件神器
        /// </summary>
        /// <param name="baseEq">ArtifactBaseCfg.Instance.GetEquip(e);通过这个接口查询到可用的底子传进来开始制作</param>
        /// <param name="art">神器枚举</param>
        /// <returns></returns>
        public async Task<EquipModel> MakeArtifact(emArtifactBase art, EquipModel baseEq, int roleid, ArtifactBaseConfig config, bool isSecondCheck = false,bool isCheckBag=true)
        {
            if (baseEq == null) return null;
            var existedEq = isCheckBag ? await CheckBagArtifact(art.GetEnumDescription(), config, roleid) : null;
            if (existedEq != null) return existedEq;
            if (baseEq.emItemQuality == emItemQuality.普通)
            {//需要打孔
                var reformControl = new ReformController(_win);
                var targetSlotCount = int.Parse(config.Conditions.Where(p => p.AttributeType == emAttrType.凹槽).FirstOrDefault().ConditionContent);

                var isSuccess = await reformControl.SlotReform(baseEq, roleid, config,art);
                if (!isSuccess) return null;
            }
          

            //_win.GetBro().ShowDevTools();
            await Task.Delay(1000);
            if (_win.GetBro().Address != IdleUrlHelper.InlayUrl(roleid, baseEq.EquipID))
            {
                await _win.LoadUrlWaitJsInit(IdleUrlHelper.InlayUrl(roleid, baseEq.EquipID), "inlay");
            }
            var r = await _win.CallJs("_inlay.isEnd()");
            var isEnd = r.Result.ToObject<bool>();
            if (isEnd)
            {
                baseEq.Quality = "artifact";
                baseEq.EquipName = art.GetEnumDescription();
                var content = await _win.CallJs("_inlay.getEquipContent()");
                baseEq.Content = content.Result.ToObject<string>();
                //把content也覆盖
                var rows = FreeDb.Sqlite.Update<EquipModel>().SetSource(baseEq).ExecuteAffrows();
                return baseEq;
            }

            var name = art.GetEnumDescription();//神器名字
            var isRuneEnough = !isSecondCheck ? await IsRuneEnouth(name) : true;

            if (!isRuneEnough)
            {
                //不准自动合符文
                if (!config.isUpdateRune) return null;
                var updateRuneMap = await GetUpdateRuneMap(name);
                var isSuccess = await UpdateRune(updateRuneMap);
                //没合成成功 则结束神器制作
                if (!isSuccess) return null;
                if (_win.GetBro().Address != IdleUrlHelper.InlayUrl(roleid, baseEq.EquipID))
                {
                    await Task.Delay(1500);
                    await _win.LoadUrlWaitJsInit(IdleUrlHelper.InlayUrl(roleid, baseEq.EquipID), "inlay");
               
                }
            }
            var data = new Dictionary<string, object>();
            data.Add("name", name);
            var makeResult = await _win.CallJsWaitReload($"_inlay.makeArtifact({data.ToLowerCamelCase()})", "inlay");

            var isEnough = makeResult.Result.ToObject<int>();
            if (isEnough == -1)
            {
                return null;
            }

            if (!isEnd)
            {
                await MakeArtifact(art, baseEq, roleid, config, true,false);
            }
            return baseEq;
        }

        /// <summary>
        /// 检查背包是否有现成的神器 防止异常中断做多了
        /// </summary>
        /// <param name="art"></param>
        /// <param name="baseEq"></param>
        /// <param name="roleid"></param>
        /// <returns></returns>
        public async Task<EquipModel> CheckBagArtifact(string eqName, Equipment config, int roleid)
        {
            var eqControll = new EquipController(_win);
            //做成神器凹槽匹配不到了
            var copyConfig = config.DeepCopy<Equipment>();
            copyConfig.Conditions.RemoveAll(p => p.AttributeType == emAttrType.凹槽);
            //跳转神器页
            var result = await _win.LoadUrlWaitJsInit($"https://www.idleinfinity.cn/Equipment/Query?id={roleid}&pt2=5", "equip");
            await Task.Delay(1500);
            var categoryArr = copyConfig.Category.Split('|');
            for (int i = 0; i < categoryArr.Length; i++)
            {
                var category = categoryArr[i];
                var aa = await _win.CallJsWaitReload($"jumpToCategory('{category}')", "equip");
                bool hasNextPage = true;
                while (hasNextPage)
                {
                    var r = await _win.CallJs("getRepositoryEquips()");
                    var eqMap = r.Result.ToObject<Dictionary<long, EquipModel>>();
                    if (eqMap == null) break;
                    foreach (var item in eqMap.Values)
                    {
                        if (AttributeMatchUtil.Match(item, copyConfig, out _) && eqName == item.EquipName)
                        {
                            return item;
                        }
                    }
                    //有下一页继续
                    var response2 = await _win.CallJsWaitReload($@"repositoryNext()", "equip");
                    if (response2.Success && (bool)response2.Result)
                    {
                        P.Log("");

                    }
                    else
                    {
                        P.Log("仓库最后一页了！", emLogType.AutoEquip);
                        hasNextPage = false;
                    }
                }
            }


            return null;

        }

        /// <summary>
        /// 获取符文是否够用 不够将进行升级
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<Dictionary<int, int>> GetUpdateRuneMap(string name)
        {
            var data = new Dictionary<string, object>();
            data.Add("name", name);
            var r = await _win.CallJs($"_inlay.getRuneMap({data.ToLowerCamelCase()})");
            var map = r.Result.ToObject<Dictionary<int, int>>();
            return map;
        }

        public async Task<bool> IsRuneEnouth(string name)
        {
            var data = new Dictionary<string, object>();
            data.Add("name", name);
            var r = await _win.CallJs($"_inlay.isRuneEnough({data.ToLowerCamelCase()})");
            var rr = r.Result.ToObject<bool>();
            return rr;
        }

        /// <summary>
        /// 升级符文
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public async Task<bool> UpdateRune(Dictionary<int, int> map)
        {
            var runeConrol = new RuneController(_win);
            await Task.Delay(1000);
            var isSuccess = await runeConrol.UpgradeRune(_win, _win.User, map);
            return isSuccess;
        }
    }
}
