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

public enum emEquipType
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
}

public enum emMaskType
{
    WEB_LOADING = 0,
    RES_LOADING = 1,
    AUTO_EQUIPING = 2,
    AH_SCANING = 3,
    RUNE_UPDATING = 4,
}