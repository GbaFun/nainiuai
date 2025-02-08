using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum emJob
{
    None = 0,
    骑士 = 1,
    法师 = 2,
    战士 = 3,
    游侠 = 4,
    牧师 = 5,
    刺客 = 6,
    萨满 = 7,
    死灵 = 8,
    贤者 = 9,
    武僧 = 10,
    猎手 = 11,
    死骑 = 12,
    勇者 = 13,
}

public enum emRace
{
    None = 0,
    人类 = 1,
    精灵 = 2,
    兽人 = 3,
    侏儒 = 4,
    地精 = 5,
    亡灵 = 6,
    恶魔 = 7,
    矮人 = 8,
}

public enum emEquipSort
{
    头盔 = 0,
    衣服 = 1,
    手套 = 2,
    靴子 = 3,
    腰带 = 4,
    主手 = 5,
    副手 = 6,
    戒指1 = 7,
    戒指2 = 8,
    项链 = 9,
    护符 = 10,
    未穿戴 = 999,
}

public enum emItemType
{
    帽子 = 0,
    衣服 = 1,
    手套 = 2,
    靴子 = 3,
    腰带 = 4,
    主手 = 5,
    副手 = 6,
    戒指 = 7,
    项链 = 8,
    护符 = 9,
    秘境 = 10,
    道具 = 11,
    珠宝 = 12,
    未知 = 999,
}

public enum emItemQuality
{
    PHYSICAL,       //全部
    BASE,           //普通
    MAGICAL,        //魔法
    RARE,           //稀有
    CRAFT,          //手工
    SET,            //套装
    UNIQUE,         //传奇
    ARTIFACT,       //神器
    HOLY,           //圣衣
}

public enum emMaskType
{
    WEB_LOADING = 0,
    RES_LOADING = 1,
    AUTO_EQUIPING = 2,
    AH_SCANING = 3,
    RUNE_UPDATING = 4,
    /// <summary>
    /// 起号组队工会
    /// </summary>
    AUTO_INIT=5
}