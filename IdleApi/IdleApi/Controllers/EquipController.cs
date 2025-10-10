using IdleApi.Model;
using IdleApi.Scripts;
using IdleApi.Service;
using IdleApi.Service.Interface;
using IdleApi.Utils;
using IdleAuto.Db;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static FreeSql.Internal.GlobalFilter;

namespace IdleApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class EquipController
    {



        /// <summary>
        /// 保存所有装备
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task SaveAll()
        {

            await FlowScript.SaveAllEquip();

        }
        [HttpGet]
        public async Task TestCollect(string name)
        {
            var taskList = new List<Task>();
            foreach (var item in AccountCfg.Instance.Accounts)
            {
                try
                {
                    if (item.AccountName != name) continue;

                    try
                    {
                        taskList.Add(TaskExecutor.ExecuteWithConcurrencyControl(item.AccountName, async () =>
                        {
                            Console.WriteLine($"{item.AccountName}正在收菜");


                            var bro = await BroWinManager.GetWin(item.AccountName);
                            var e = new EquipService(bro);
                            await e.SaveAll();

                        }));
                    }
                    catch (Exception e)
                    {
                        ErrorLog error = new ErrorLog() { AccountName = item.AccountName, Msg = "收菜失败" };
                        DbUtil.InsertOrUpdate(error);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }

            }


        }
    }
}
