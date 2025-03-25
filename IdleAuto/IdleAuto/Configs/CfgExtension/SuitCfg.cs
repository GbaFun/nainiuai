using System;
using System.Collections.Generic;
using System.IO;
using IdleAuto.Configs.CfgExtension;
using Newtonsoft.Json;
using System.Linq;
using AttributeMatch;


public class LevelRange
{
    public int Min { get; set; }
    public int Max { get; set; }

    public bool AdaptLevel(int level)
    {
        return level >= Min && level <= Max;
    }
}

public class Equipment
{
    public emItemType emEquipType { get; set; }
    public string Category { get; set; }
    public string Quality { get; set; }

    /// <summary>
    /// 是否自动合符文
    /// </summary>
    public bool isUpdateRune { get; set; } = false;
    public List<AttributeCondition> Conditions { get; set; }

    public bool AdaptAttr(EquipModel equip, out AttributeMatchReport report)
    {
        return AttributeMatchUtil.Match(equip, this, out report);
    }

    public string SimpleName
    {
        get
        {
            return Category;
        }
    }
}

public class EquipSuits
{
    public string JobName;
    public emJob Job => (emJob)Enum.Parse(typeof(emJob), JobName);
    public LevelRange Lv { get; set; }
    public List<EquipSuit> EquipSuit { get; set; }
}
public class EquipSuit
{
    public string SuitName { get; set; }
    public SuitInfo 主手 { get; set; }
    public SuitInfo 副手 { get; set; }
    public SuitInfo 头盔 { get; set; }
    public SuitInfo 护符 { get; set; }
    public SuitInfo 项链 { get; set; }
    public SuitInfo 戒指1 { get; set; }
    public SuitInfo 戒指2 { get; set; }
    public SuitInfo 衣服 { get; set; }
    public SuitInfo 腰带 { get; set; }
    public SuitInfo 手套 { get; set; }
    public SuitInfo 靴子 { get; set; }

    public SuitInfo GetEquipBySort(emEquipSort type)
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
public class SuitInfo
{
    [JsonProperty("name")]
    public string EquipName { get; set; }
    public bool IsNecessery { get; set; }
    public Equipment Equipment
    {
        get
        {
            return EquipCfg.Instance.Get(EquipName);
        }
    }
}

public class SuitCfg
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "SuitCfg.json");
    public static SuitCfg Instance { get; } = new SuitCfg();
    public List<EquipSuits> SuitList { get; set; }
    private Dictionary<emJob, List<EquipSuits>> _equipMap;

    public SuitCfg()
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
        SuitList = json.ToUpperCamelCase<List<EquipSuits>>();
    }

    public EquipSuits GetEquipmentByJobAndLevel(emJob job, int level)
    {
        if (_equipMap == null)
        {
            _equipMap = new Dictionary<emJob, List<EquipSuits>>();
            foreach (var equipment in SuitList)
            {
                if (!_equipMap.ContainsKey(equipment.Job))
                {
                    _equipMap.Add(equipment.Job, new List<EquipSuits>());
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