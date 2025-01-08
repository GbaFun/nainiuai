using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class UserModel
{
    public UserModel(Account account)
    {
        Username = account.Username;
        Password = account.Password;
        Roles = new List<RoleModel>();
        IsLogin = false;
    }
    /// <summary>
    /// 账户名字
    /// </summary>
    public string AccountName { get; set; }

    /// <summary>
    /// 登录账号
    /// </summary>
    public string Username { get; set; }

    public string Password { get; set; }

    public List<RoleModel> Roles = new List<RoleModel>();

    public bool IsLogin { get; private set; }
    public void SetLogin(bool isSuccess, string account, string loginInfo)
    {
        IsLogin = isSuccess;
        AccountName = account;
        string[] sroles = loginInfo.Split(';');
        foreach (var item in sroles)
        {
            if (!string.IsNullOrEmpty(item))
            {
                RoleModel role = new RoleModel(item);
                Roles.Add(role);
            }
        }
    }

    public RoleModel FirstRole
    {
        get
        {
            if (Roles.Count > 0)
            {
                return Roles[0];
            }
            return null;
        }
    }
}

