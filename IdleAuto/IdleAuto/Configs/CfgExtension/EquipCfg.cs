using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;


public class LevelRange
{
    public int Min { get; set; }
    public int Max { get; set; }

    public bool AdaptLevel(int level)
    {
        return level >= Min && level <= Max;
    }
}

public class Equipments
{
    public string JobName;
    public emJob Job => (emJob)Enum.Parse(typeof(emJob), JobName);
    public LevelRange Lv { get; set; }
    public string 主手 { get; set; }
    public string 副手 { get; set; }
    public string 头盔 { get; set; }
    public string 护符 { get; set; }
    public string 戒指1 { get; set; }
    public string 戒指2 { get; set; }
    public string 衣服 { get; set; }
    public string 腰带 { get; set; }
    public string 手套 { get; set; }
    public string 靴子 { get; set; }

    public string GetEquipByType(emEquipType type)
    {
        switch (type)
        {
            case emEquipType.主手:
                return 主手;
            case emEquipType.副手:
                return 副手;
            case emEquipType.头盔:
                return 头盔;
            case emEquipType.护符:
                return 护符;
            case emEquipType.戒指1:
                return 戒指1;
            case emEquipType.戒指2:
                return 戒指2;
            case emEquipType.衣服:
                return 衣服;
            case emEquipType.腰带:
                return 腰带;
            case emEquipType.手套:
                return 手套;
            case emEquipType.靴子:
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
    public Dictionary<emJob, List<Equipments>> _equipMap;

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
        EquipList = JsonConvert.DeserializeObject<List<Equipments>>(json);
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

//class Program
//{
//    static void Main(string[] args)
//    {
//        var configManager = new EquipConfigManager("IdleAuto/Configs/EquipCfg.json");

//        var necromancerEquipments = configManager.GetEquipmentByClassAndLevel("死灵", 15);
//        foreach (var equipment in necromancerEquipments)
//        {
//            Console.WriteLine($"主手: {equipment.主手}, 副手: {equipment.副手}");
//        }

//        var monkEquipments = configManager.GetEquipmentByClassAndLevel("武僧", 40);
//        foreach (var equipment in monkEquipments)
//        {
//            Console.WriteLine($"主手: {equipment.主手}, 副手: {equipment.副手}");
//        }
//    }
//}