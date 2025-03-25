using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ArtifactBase : Equipment
{
    /// <summary>
    /// 是否自动合符文
    /// </summary>
    public bool isUpdateRune { get; set; } = false;

    public string SlotRule { get; set; }

    public void ReadSlotRule()
    {
        string[] ruleArr = SlotRule.Split(',');

    }
}




