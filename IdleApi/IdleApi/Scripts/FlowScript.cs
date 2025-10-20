using IdleApi.Model;
using IdleApi.Service;
using IdleApi.Utils;
using IdleApi.Wrap;
using IdleAuto.Db;

namespace IdleApi.Scripts
{
    public class FlowScript
    {
        //刷新cookie
        public static async Task VisitHomePage()
        {
            foreach (var item in AccountCfg.Instance.Accounts)
            {
                try
                {
                    await Task.Delay(2000);
                    var bro = new BroWindow(item.AccountName);
                    await bro.LoadUrlAsync(IdleUrlHelper.HomeUrl());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }

            }
            await Task.CompletedTask;
        }

        public static async Task SaveAllEquip(bool isIncludeBuding=true)
        {
            List<Task> taskList = new List<Task>();
            if (isIncludeBuding)
            {
                var rows = FreeDb.Sqlite.Delete<EquipModel>().Where(p => 1 == 1).ExecuteAffrows();
            }
            else
            {
                var rows = FreeDb.Sqlite.Delete<EquipModel>().Where(p => !p.AccountName.StartsWith("0")).ExecuteAffrows();
            }
            foreach (var item in AccountCfg.Instance.Accounts)
            {
                try
                {
                    if (item.AccountName == "铁矿石") continue;
                    if (isIncludeBuding == false&&item.AccountName.StartsWith("0"))
                    {
                        break;
                    }
                   
                        taskList.Add(TaskExecutor.ExecuteWithConcurrencyControl(item.AccountName, async () =>
                         {
                             Console.WriteLine($"{item.AccountName}正在执行任务");


                             var bro = await BroWinManager.GetWin(item.AccountName);
                             var e = new EquipService(bro);
                             await e.SaveAll();

                         }));
             

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }

            }
            await Task.WhenAll(taskList);
        }
    }
}
