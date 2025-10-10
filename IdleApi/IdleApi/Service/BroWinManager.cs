using IdleApi.Scripts;
using IdleApi.Utils;
using IdleApi.Wrap;

namespace IdleApi.Service
{
    public class BroWinManager
    {
        public static async Task<BroWindow> GetWin(string userName)
        {
            var BroWin = new BroWindow(userName);
           var str= await BroWin.LoadUrlAsync(IdleUrlHelper.Home);
            var p = new RoleParser(str);
            var list=await p.GetRoleInfoAsync();
            BroWin.User.SetLogin(true, list);
            DbUtil.InsertOrUpdate(list);
            return BroWin;

        }
    }
}
