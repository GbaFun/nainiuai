using System;
using System.Collections.Generic;
using System.IO;
using IdleAuto.Configs.CfgExtension;
using Newtonsoft.Json;
using System.Linq;
using AttributeMatch;
using System.Text.RegularExpressions;

public class ArtifactBaseCfg
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "ArtifactBaseCfg.json");
    public static ArtifactBaseCfg Instance { get; } = new ArtifactBaseCfg();
    public Dictionary<emArtifactBase, ArtifactBaseConfig> Data;

    public Dictionary<emArtifactBase, List<SlotConfigStruct>> SlotConfig;

    public ArtifactBaseCfg()
    {
        SlotConfig = new Dictionary<emArtifactBase, List<SlotConfigStruct>>();
        LoadConfig();
        InitSlotConfig();

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
            Data = json.ToUpperCamelCase<Dictionary<emArtifactBase, ArtifactBaseConfig>>();
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    private void InitSlotConfig()
    {
        foreach (var item in this.Data)
        {
            var config = item.Value;
            if (string.IsNullOrWhiteSpace(config.SlotRule)) continue;
            List<SlotConfigStruct> list = new List<SlotConfigStruct>();
            foreach (var s in config.SlotMap)
            {
                if (s.Key == "else")
                {
                    continue;
                }

                var newConfig = config.DeepCopy();
                newConfig.Conditions.RemoveAll(p => true);
                var skillArr = config.Conditions.Where(p => p.AttributeType == emAttrType.技能等级).ToList();

                var skillLvArr = s.Key.ToCharArray();
                for (int i = 0; i < skillLvArr.Length; i++)
                {
                    //按同索引修改条件
                    var matchSkill = skillArr[i].DeepCopy();
                    matchSkill.Operate = emOperateType.大于等于;
                    matchSkill.MatchType = emMatchType.必需;
                    matchSkill.ConditionContent = skillLvArr[i].ToString();
                    newConfig.Conditions.Add(matchSkill);
                }
                var ss = new SlotConfigStruct();
                ss.Config = newConfig;
                ss.SlotType = s.Value;
                list.Add(ss);
            }
            SlotConfig.Add(item.Key, list);
        }
    }

    public ArtifactBaseConfig GetEquipCondition(emArtifactBase e)
    {
        var des = e.GetEnumDescription();
        return Data[e];
    }

    /// <summary>
    /// 底子命中的打孔类型
    /// </summary>
    /// <param name="eq"></param>
    /// <param name="artifactBase"></param>
    /// <returns></returns>
    public emSlotType MatchSlotType(EquipModel eq, emArtifactBase artifactBase)
    {
        var configs = this.SlotConfig[artifactBase];
        foreach (var item in configs)
        {
            var targetSlotCount = item.Config.TargetSlotCount;
            var regexStr = @"最大凹槽：(?<v>\d+)";
            var regex = new Regex(regexStr, RegexOptions.Multiline);
            var match = regex.Match(eq.Content);
            if (match.Success)
            {
                int slotValue = int.Parse(match.Groups["v"].Value);
                if (slotValue > targetSlotCount && eq.Category == emCategory.死骑面罩.ToString())
                {
                    return emSlotType.DkHeadRandom;
                }
                if (slotValue > targetSlotCount)
                {
                    return emSlotType.Random;
                }
            }
            var isMatch = AttributeMatchUtil.Match(eq, item.Config, out _);
            if (isMatch)
            {
                return item.SlotType;
            }
        }
        return emSlotType.Random;

    }


}

public struct SlotConfigStruct
{
    public emSlotType SlotType;
    public ArtifactBaseConfig Config;
}