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
        public async Task<long> MakeArtifact(emArtifactBase art, EquipModel baseEq, int roleid)
        {
            _win.GetBro().ShowDevTools();
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
                var rows = FreeDb.Sqlite.InsertOrUpdate<EquipModel>().SetSource(baseEq).ExecuteAffrows();
                return baseEq.EquipID;
            }
        
            var name = art.GetEnumDescription();//神器名字
            var data = new Dictionary<string, object>();
            data.Add("name", name);
            await _win.CallJsWaitReload($"_inlay.makeArtifact({data.ToLowerCamelCase()})", "inlay");
      
            
            if (!isEnd)
            {
                await MakeArtifact(art, baseEq, roleid);
            }
            return baseEq.EquipID;
        }
    }
}
