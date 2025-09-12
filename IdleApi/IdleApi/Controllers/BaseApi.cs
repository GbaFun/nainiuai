using IdleApi.Scripts;
using IdleApi.Wrap;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            await bro.LoadUrlAsync("/Equipment/Query?id=116");
        }
    }
}
