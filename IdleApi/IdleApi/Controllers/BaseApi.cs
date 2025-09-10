using IdleApi.Scripts;
using IdleApi.Wrap;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdleApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BaseApi : ControllerBase
    {
        [HttpGet]
        public async Task<string> Test()
        {
            await FlowScript.VisitHomePage();
            return "结束";
        }
    }
}
