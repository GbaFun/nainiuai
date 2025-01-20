using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AccountController
{
    private static object lockObject = new object();
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
        EventManager.Instance.SubscribeEvent(emEventType.OnCharLoaded, OnCharLoaded);
    }

    //当前登录账号
    public UserModel User;

    public RoleModel CurRole;

    public int CurRoleIndex;


    private void OnLoginSuccess(params object[] args)
    {
        lock (lockObject)
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

    private void OnCharLoaded(params object[] args)
    {
        lock (lockObject)
        {
            var cid = int.Parse(args[0].ToString());
            this.CurRoleIndex = User.Roles.FindIndex(p => p.RoleId == cid);
            this.CurRole = User.Roles[CurRoleIndex];
        }

    }
}
