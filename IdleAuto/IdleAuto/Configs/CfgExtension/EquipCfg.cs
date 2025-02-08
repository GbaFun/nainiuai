using System;
using System.Collections.Generic;
using System.IO;
using IdleAuto.Configs.CfgExtension;
using Newtonsoft.Json;
using System.Linq;


public class LevelRange
{
    public int Min { get; set; }
    public int Max { get; set; }

    public bool AdaptLevel(int level)
    {
        return level >= Min && level <= Max;
    }
}
public class Equipment : Item
{
    public emItemType emEquipType { get; set; }
}
public class Item
{
    public string Name { get; set; }
    public string Category { get; set; }
    public List<string> Content { get; set; } = new List<string>();
    public List<RegexMatch> RegexList { get; set; } = new List<RegexMatch>();

    public bool AdaptAttr(string name, string attr)
    {
        if (!name.Contains(Name)) return false;
        if (!Content.All(p => attr.Contains(p))) return false;
        if (RegexList != null && !RegexUtil.Match(attr, RegexList)) return false;

        return true;
    }
    public bool AdaptAttr(EquipModel equip)
    {
        ///如果没有配置名字和装备类型则不匹配
        if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Category)) return false;
        if (!string.IsNullOrEmpty(Name) && !equip.EquipName.Contains(Name)) return false;
        if (!string.IsNullOrEmpty(Category) && !equip.Category.Equals(Category)) return false;
        if (!Content.All(p => equip.Content.Contains(p))) return false;
        if (RegexList != null && !RegexUtil.Match(equip.Content, RegexList)) return false;

        return true;
    }

    public string SimpleName
    {
        get
        {
            if (string.IsNullOrEmpty(Name)) return Category;
            if (string.IsNullOrEmpty(Category)) return Name;
            return $"{Name}({Category})";
        }
    }
}

public class Equipments
{
    public string JobName;
    public emJob Job => (emJob)Enum.Parse(typeof(emJob), JobName);
    public LevelRange Lv { get; set; }
    public Equipment 主手 { get; set; }
    public Equipment 副手 { get; set; }
    public Equipment 头盔 { get; set; }
    public Equipment 护符 { get; set; }
    public Equipment 项链 { get; set; }
    public Equipment 戒指1 { get; set; }
    public Equipment 戒指2 { get; set; }
    public Equipment 衣服 { get; set; }
    public Equipment 腰带 { get; set; }
    public Equipment 手套 { get; set; }
    public Equipment 靴子 { get; set; }

    public Equipment GetEquipBySort(emEquipSort type)
    {
        switch (type)
        {
            case emEquipSort.主手:
                return 主手;
            case emEquipSort.副手:
                return 副手;
            case emEquipSort.头盔:
                return 头盔;
            case emEquipSort.护符:
                return 护符;
            case emEquipSort.项链:
                return 项链;
            case emEquipSort.戒指1:
                return 戒指1;
            case emEquipSort.戒指2:
                return 戒指2;
            case emEquipSort.衣服:
                return 衣服;
            case emEquipSort.腰带:
                return 腰带;
            case emEquipSort.手套:
                return 手套;
            case emEquipSort.靴子:
                return 靴子;
            default:
                return null;
        }
    }
}

public class EquipCfg
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "EquipCfg.json");
    public static EquipCfg Instance { get; } = new EquipCfg();
    public List<Equipments> EquipList { get; set; }
    private Dictionary<emJob, List<Equipments>> _equipMap;

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
        EquipList = json.ToUpperCamelCase<List<Equipments>>();
    }

    public Equipments GetEquipmentByJobAndLevel(emJob job, int level)
    {
        if (_equipMap == null)
        {
            _equipMap = new Dictionary<emJob, List<Equipments>>();
            foreach (var equipment in EquipList)
            {
                if (!_equipMap.ContainsKey(equipment.Job))
                {
                    _equipMap.Add(equipment.Job, new List<Equipments>());
                }
                _equipMap[equipment.Job].Add(equipment);
            }
        }
        if (_equipMap.TryGetValue(job, out var equipmentList))
        {
            return equipmentList.Find(e => e.Lv.AdaptLevel(level));
        }

        return null;
    }
}