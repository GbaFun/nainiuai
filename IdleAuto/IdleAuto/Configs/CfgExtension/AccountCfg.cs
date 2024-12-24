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
    public string Username { get; set; }
    public string Password { get; set; }
}