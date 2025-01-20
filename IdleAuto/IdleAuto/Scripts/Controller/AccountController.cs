using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AccountController
{
    private static AccountController instance;
    public static AccountController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AccountController();
            }
            return instance;
        }
    }


    public AccountController()
    {
        User = null;
        EventManager.Instance.SubscribeEvent(emEventType.OnLoginSuccess, OnLoginSuccess);
    }

    //当前登录账号
    public UserModel User;


    private void OnLoginSuccess(params object[] args)
    {
        if (args.Length == 3)
        {
            bool isSuccess = (bool)args[0];
            string account = args[1] as string;
            List<RoleModel> roles = args[2].ToObject<List<RoleModel>>();
            if (!User.IsLogin)
            {
                User.SetLogin(isSuccess, account, roles);
                EventManager.Instance.InvokeEvent(emEventType.OnAccountDirty, null);
            }
            else
            {
                User.UnionRoles(roles);
            }
        }
    }
}
