using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EquipCfg
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "EquipCfg.json");
    public static EquipCfg Instance { get; } = new EquipCfg();
    private Dictionary<string, Equipment> _equipMap;

    public EquipCfg()
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
        var equipList = json.ToUpperCamelCase<List<EquipInfo>>();

        _equipMap = new Dictionary<string, Equipment>();
        foreach (var equip in equipList)
        {
            _equipMap.Add(equip.Name, equip.Equipment);
        }
    }

    public Equipment Get(string name)
    {
        if (Instance._equipMap != null)
        {
            if (_equipMap.TryGetValue(name, out Equipment equip)) return equip;
        }

        return null;
    }
}

public class EquipInfo
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Equipment Equipment { get; set; }
}
