using IdleApi.Wrap;

namespace IdleApi.Scripts
{
    public class FlowScript
    {
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
    }
}
