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
        public async Task<EquipModel> MakeArtifact(emArtifactBase art, EquipModel baseEq, int roleid,Equipment config)
        {
           var existedEq= await CheckBagArtifact(art.GetEnumDescription(), config, roleid);
            if (existedEq != null) return existedEq;
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
                var rows = FreeDb.Sqlite.Update<EquipModel>().SetSource(baseEq).ExecuteAffrows();
                return baseEq;
            }

            var name = art.GetEnumDescription();//神器名字
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
                await MakeArtifact(art, baseEq, roleid,config);
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
            var eqControll = new EquipController();
            //跳转神器页
            var result = await _win.LoadUrlWaitJsInit($"https://www.idleinfinity.cn/Equipment/Query?id={roleid}&pt2=5", "equip");
            await Task.Delay(1500);
            await _win.CallJsWaitReload($"jumpToCategory({config.Category})", "equip");
            bool hasNextPage = true;
            await Task.Delay(1500);
            while (hasNextPage)
            {
                var r = await _win.CallJs("getRepositoryEquips");
                var eqMap = r.Result.ToObject<Dictionary<long, EquipModel>>();
                foreach (var item in eqMap.Values)
                {
                    if (AttributeMatchUtil.Match(item, config, out _)&& eqName ==item.EquipName)
                    {
                        return item;
                    }
                }
                //有下一页继续
                var response2 = await _win.CallJsWithReload($@"repositoryNext()", "equip");
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

            return null;

        }
    }
}
