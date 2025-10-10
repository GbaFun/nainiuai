using IdleApi.Scripts;
using IdleApi.Service;
using IdleApi.Service.Interface;
using IdleApi.Wrap;
using IdleAuto.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using static FreeSql.Internal.GlobalFilter;

namespace IdleApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class BaseApi : ControllerBase
    {
       
        [HttpGet]

        public async Task<string> RefreshCookie()
        {
            await FlowScript.VisitHomePage();
            return "结束";
        }

        [HttpGet]
        public async Task Test()
        {
            var bro = new BroWindow("铁矿石");
            await bro.LoadUrlAsync(IdleUrlHelper.HomeUrl());
            var form = new Dictionary<string, string> { { "cid", "" } };
            await bro.SubmitFormAsync("/Home/SwitchStyle", form);
        }

        [HttpGet]
        public async Task TestEq()
        {
            var bro = new BroWindow("铁矿石");
            // 2. 使用自定义窗口

            EquipService ser = new EquipService(bro);
            await ser.SaveEquipInRepo();
            var str = await bro.LoadUrlAsync("/Equipment/Query?id=116");
            var parser = new EquipParser(str);
            var currentEquips = parser.GetCurEquips();
            var packageEquips = parser.GetPackageEquips();
            var repositoryEquips = parser.GetRepositoryEquips();
            int packageCount = parser.PackageEquipsCount();
            int repositoryCount = parser.RepositoryEquipsCount();
        }

        [HttpGet]
        public async Task TestHome()
        {

            var win =await BroWinManager.GetWin("RasdGch");
            EquipService ser = new EquipService(win);
            await ser.SaveEquipInRepo();

        }
        [HttpGet]
        public async Task CompareDb()
        {

            var dataAuto = FreeDb.SqliteAuto.Select<EquipModel>().Where(p => p.AccountName == "RasdGch" && p.EquipStatus == emEquipStatus.Repo).ToList();
            var dataApi= FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName == "RasdGch" && p.EquipStatus == emEquipStatus.Repo).ToList();
            var list=dataAuto.Except(dataApi);

        }
    }
}
