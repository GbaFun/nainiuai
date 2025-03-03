using System;
using System.Collections.Generic;
using System.IO;
using IdleAuto.Configs.CfgExtension;
using Newtonsoft.Json;
using System.Linq;
using AttributeMatch;

public class ArtifactBaseCfg
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "ArtifactBaseCfg.json");
    public static ArtifactBaseCfg Instance { get; } = new ArtifactBaseCfg();
    public Dictionary<emArtifactBase, Equipment> Data;

    public ArtifactBaseCfg()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        if (!File.Exists(ConfigFilePath))
        {
            throw new FileNotFoundException($"Config file not found: {ConfigFilePath}");
        }

        var json = File.ReadAllText(ConfigFilePath);
        try
        {
            Data = json.ToUpperCamelCase<Dictionary<emArtifactBase, Equipment>>();
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    public Equipment GetEquipCondition(emArtifactBase e)
    {
        var des=e.GetEnumDescription();
        return Data[e];
    }

    
}