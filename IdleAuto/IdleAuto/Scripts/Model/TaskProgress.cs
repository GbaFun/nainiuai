using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


public enum emTaskType
{
    [Description("切图")]
    MapSwitch,
    [Description("修车")]
    AutoEquip,
    [Description("发送符文")]
    RuneTrade,
    [Description("精修死灵")]
    RepairNec,
    [Description("刷天灾")]
    RollTianzai

}

/// <summary>
/// 记录任务执行到哪
/// </summary>
public class TaskProgress
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public int Id { get; set; }

    private string _typeName;
    /// <summary>
    /// 任务种类
    /// </summary>
    public string TypeName
    {
        get => _typeName;
        private set => _typeName = value;
    }

    private emTaskType _type;
    public emTaskType Type
    {
        get => _type;
        set
        {
            _type = value;
            TypeName = value.GetEnumDescription();
        }
    }

    public string UserName { get; set; }

    public int Roleid { get; set; }

    /// <summary>
    /// 是否结束了
    /// </summary>
    public bool IsEnd { get; set; }


}
