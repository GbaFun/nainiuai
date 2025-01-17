using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class AccountCfg
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "AccountCfg.json");
    public static AccountCfg Instance { get; } = new AccountCfg();

    public List<Account> Accounts { get; set; }

    public AccountCfg()
    {
        LoadConfig();
    }

    public void LoadConfig()
    {
        if (File.Exists(ConfigFilePath))
        {
            var json = File.ReadAllText(ConfigFilePath);
            Accounts = JsonConvert.DeserializeObject<List<Account>>(json);
        }
        else
        {
            Accounts = new List<Account>();
        }
    }
}

public class Account
{
    /// <summary>
    /// 账户登录名
    /// </summary>
    [JsonProperty("username")]
    public string Username { get; set; }
    /// <summary>
    /// 登录密码
    /// </summary>
    [JsonProperty("password")]
    public string Password { get; set; }
    /// <summary>
    /// 账号名字
    /// </summary>
    [JsonProperty("account")]
    public string AccountName { get; set; }
    /// <summary>
    /// 账号ID
    /// </summary>
    [JsonProperty("accountId")]
    public int AccountID { get; set; }
}