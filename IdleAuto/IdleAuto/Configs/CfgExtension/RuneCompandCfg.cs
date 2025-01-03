using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class RuneCompandCfg
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "RuneCompandCfg.json");
    public static RuneCompandCfg Instance { get; } = new RuneCompandCfg();

    public List<RuneCompandData> RuneCompandData { get; set; }

    public RuneCompandCfg()
    {
        LoadConfig();
    }

    public void LoadConfig()
    {
        if (File.Exists(ConfigFilePath))
        {
            var json = File.ReadAllText(ConfigFilePath);
            RuneCompandData = JsonConvert.DeserializeObject<List<RuneCompandData>>(json);
        }
        else
        {
            RuneCompandData = new List<RuneCompandData>();
        }
    }

    public void SetDirty()
    {
        RuneCompandData = null;
        LoadConfig();
    }
}

public class RuneCompandData
{
    public int ID { get; set; }
    public int CompandNum { get; set; }

}
