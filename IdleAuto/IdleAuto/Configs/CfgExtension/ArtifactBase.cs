using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ArtifactBaseConfig : Equipment
{
    public ArtifactBaseConfig() : base()
    {
        SlotMap = new Dictionary<string, emSlotType>();
    }
    /// <summary>
    /// 是否自动合符文
    /// </summary>
    public bool isUpdateRune { get; set; } = false;

    public string SlotRule { get; set; }

    /// <summary>
    /// 目标孔数
    /// </summary>
    public int TargetSlotCount { get; set; }

    /// <summary>
    /// 技能规则对应 打孔类型
    /// </summary>
    public Dictionary<string, emSlotType> SlotMap;

    public void ReadSlotRule()
    {
        if (string.IsNullOrWhiteSpace(SlotRule)) return;
        string[] ruleArr = SlotRule.Split(',');
        for (int i = 0; i < ruleArr.Length; i++)
        {
            var s = ruleArr[i];
            var rule = s.Split(':');
            var skill = rule[0];
            var slotTypeStr = rule[1];
            emSlotType slotType;
            Enum.TryParse<emSlotType>(slotTypeStr, out slotType);
            SlotMap.Add(skill, slotType);
        }

    }

}




