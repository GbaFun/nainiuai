﻿using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class UserModel
{
    public UserModel(Account account)
    {
        Id = account.AccountID;
        AccountName = account.AccountName;
        Username = account.Username;
        Password = account.Password;
        Key = account.ChromeVersion;
        Roles = new List<RoleModel>();
        IsLogin = false;
    }
    public UserModel()
    {

    }
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }
    /// <summary>
    /// 账户名字
    /// </summary>
    public string AccountName { get; set; }

    /// <summary>
    /// 登录账号
    /// </summary>
    public string Username { get; set; }

    public string Password { get; set; }
    public string Key { get; set; }

    public List<RoleModel> Roles = new List<RoleModel>();

    public bool IsLogin { get; private set; }
    public void SetLogin(bool isSuccess, string account, List<RoleModel> roles)
    {
        IsLogin = isSuccess;
        //AccountName = account;
        Roles.AddRange(roles);
    }

    public void UnionRoles(List<RoleModel> roles)
    {
        Roles = Roles.Union(roles).ToList();
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

 