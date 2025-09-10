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

    public bool IsTrade { get; set; } = false;


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

    private List<string> _categoryQualityKeyList;
    /// <summary>
    /// 返回一个由category+quality的Key list 用于减少匹配装备那边过多数据造成的性能浪费
    /// </summary>
    public List<string> CategoryQualityKeyList
    {

        get
        {
            if (_categoryQualityKeyList == null)
            {
                var cArr = Category.Split('|').ToList();
                var qArr = Quality.Split('|').ToList();

                _categoryQualityKeyList = cArr.SelectMany(a => qArr.Select(b => a + b)).ToList();
            }
            return _categoryQualityKeyList;

        }
    }
}



public class EquipSuits
{
    public string JobName;
    public emJob Job => (emJob)Enum.Parse(typeof(emJob), JobName);

    public emSuitType SuitType { get; set; } = emSuitType.效率;
    public LevelRange Lv { get; set; }
    public List<EquipSuit> EquipSuit { get; set; }
}
public class EquipSuit
{
    public string SuitName { get; set; }

    /// <summary>
    /// 触发特殊修车 控制死灵速度和法师速度 等等... 其他再议
    /// </summary>
    public emSkillMode SkillMode { get; set; }

    public int IdealFcr { get; set; }



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

    public List<string> EquipNameArr
    {
        get
        {
            if (string.IsNullOrEmpty(EquipName)) return new List<string>() { };
            else return EquipName.Split(',').ToList();
        }
    }
    public Equipment GetEquipment(string name)
    {
        try
        {
            return EquipCfg.Instance.Get(name);
        }
        catch (Exception e)
        {
            throw new Exception(name + e.StackTrace);
        }

    }
}

public class SuitCfg
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "SuitCfg.json");
    public static SuitCfg Instance { get; } = new SuitCfg();
    public List<EquipSuits> SuitList { get; set; }


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

    public EquipSuits GetEquipmentByJobAndLevel(emJob job, int level, emSuitType suitType = emSuitType.效率)
    {
        try
        {
            Dictionary<emJob, List<EquipSuits>> _equipMap = new Dictionary<emJob, List<EquipSuits>>() ;
           
                _equipMap = new Dictionary<emJob, List<EquipSuits>>();
                foreach (var equipment in SuitList.Where(p => p.SuitType == suitType))
                {
                    if (!_equipMap.ContainsKey(equipment.Job))
                    {
                        _equipMap.Add(equipment.Job, new List<EquipSuits>());
                    }
                    _equipMap[equipment.Job].Add(equipment);
                }
            
            if (_equipMap.TryGetValue(job, out var equipmentList))
            {

                return equipmentList.Find(e => e.Lv.AdaptLevel(level));
            }
        }
        catch (Exception e)
        {
            throw e;
        }


        return null;
    }
}