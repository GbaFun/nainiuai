using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


public class EquipModel
{
    /// <summary>
    /// 装备唯一ID
    /// </summary>
    [JsonProperty("eid")]
    public long EquipID { get; set; }
    /// <summary>
    /// 装备栏位,非装备类物品忽略
    /// </summary>
    [JsonProperty("esort")]
    public emEquipSort emEquipSort { get; set; }
    /// <summary>
    /// 物品品质
    /// </summary>
    [JsonProperty("quality")]
    public string Quality { get; set; }
    [JsonIgnore]
    public emItemQuality emItemQuality
    {
        get
        {
            return (emItemQuality)Enum.Parse(typeof(emItemQuality), Quality.ToUpper());
        }
    }

    /// <summary>
    /// 网页上装备所有属性的文本内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 物品类型
    /// </summary>
    public emItemType emEquipType { get; set; }

    /// <summary>
    /// 装备基础类型名
    /// </summary>
    public string EquipBaseName { get; set; }

    /// <summary>
    /// 装备名
    /// </summary>
    public string EquipName { get; set; }

    /// <summary>
    /// 是否是太古
    /// </summary>
    public bool IsPerfect { get; set; }

    /// <summary>
    /// 是否是绑定
    /// </summary>
    public bool IsLocal { get; set; }

    /// <summary>
    /// 装备所属账户ID
    /// </summary>
    [JsonIgnore]
    public int AccountID { get; private set; }
    [JsonIgnore]
    public string AccountName { get; private set; }
    /// <summary>
    /// 装备所属角色ID,可以为空
    /// </summary>
    [JsonIgnore]
    public int RoleID { get; private set; }
    [JsonIgnore]
    public string RoleName { get; private set; }

    public void SetAccountInfo(UserModel account, RoleModel role = null)
    {
        AccountID = account.Id;
        AccountName = account.AccountName;
        if (role != null)
        {
            RoleID = role.RoleId;
            RoleName = role.RoleName;
        }
    }
}

