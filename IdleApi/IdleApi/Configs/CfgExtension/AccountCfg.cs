using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class AccountCfg
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "AccountCfg.json");
    public static AccountCfg Instance { get; } = new AccountCfg();

    public List<Account> Accounts { get; set; }

    public List<UserModel> Users { get; set; }

    public AccountCfg()
    {
        LoadConfig();
        LoadUserModel();
    }

    public UserModel GetUserModel(string name)
    {
        var acc = Accounts.Find(f => f.AccountName == name);
        return new UserModel(acc);
    }

    public void LoadConfig()
    {
        Console.WriteLine($"当前工作目录: {Environment.CurrentDirectory}");
        Console.WriteLine($"尝试访问的配置文件路径: {ConfigFilePath}");
        Console.WriteLine($"绝对路径: {Path.GetFullPath(ConfigFilePath)}");
        Console.WriteLine($"文件是否存在: {File.Exists(ConfigFilePath)}");
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

    private void LoadUserModel()
    {
        Users = new List<UserModel>();
        foreach (var acc in Accounts)
        {
            var user = GetUserModel(acc.AccountName);
            Users.Add(user);
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
    /// 起号时候的前缀
    /// </summary>
    public string Prefix { get; set; }
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
    /// <summary>
    /// 浏览器版本（登录密钥）
    /// </summary>
    [JsonProperty("chromeVersion")]
    public string ChromeVersion { get; set; }
}