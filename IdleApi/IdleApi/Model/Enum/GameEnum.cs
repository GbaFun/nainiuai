﻿using System.ComponentModel;

public enum emCategory
{
    全部 = 0,
    帽子,
    衣服,
    盾,
    手套,
    靴子,
    腰带,
    头饰,
    戒指,
    项链,
    珠宝,
    护符,
    秘境,
    斧,
    剑,
    锤,
    长矛,
    匕首,
    法杖,
    权杖,
    弓,
    十字弓,
    标枪,
    投掷武器,
    法珠,
    爪,
    游侠弓,
    游侠标枪,
    战士头盔,
    祭祀刀,
    牧师副手,
    手杖,
    死灵副手,
    骑士盾牌,
    萨满头饰,
    贤者头盔,
    拳套,
    手弩,
    死骑面罩,
    道具,
    卡片,
    宝箱,

    //混合类型
    武器,
    副手,
    头部,
}
public enum emAttrType
{
    名称 = 0,
    词缀,
    力量,
    敏捷,
    体力,
    精力,
    生命,
    法力,
    凹槽,
    增强伤害,
    物理伤害,
    魔法伤害,
    元素抗性,
    抗电,
    抗火,
    抗毒,
    抗寒,
    单项元素抗性之和,
    最大伤害,
    最小伤害,
    毒素伤害,
    毒伤,
    减毒,
    冰伤,
    减冰,
    物品掉率,
    更佳魔法装备,
    额外金币取得,
    施法速度,
    唤醒光环,
    攻击速度,
    所有技能,
    技能等级,
    职业全系技能,
    指定职业全系技能,
    职业单系技能,
    指定职业单系技能,
    伤害转换,
    召唤最大数量,
    需要力量,
    需要敏捷,
    掉落等级,
    物品等级,
    武器速度,
    自定义,
}

public enum emEquipContent
{
    全部,

    神器底子,

    神器词条,
}
public enum emOperateType
{
    大于,
    大于等于,
    小于,
    小于等于,
    等于,
    不等于,
    在范围内
}
public enum emMatchType
{
    //必须满足的条件
    必需,
    //可选条件
    可选,
    //可配置与其他条件互斥
    互斥,
    //可配置与其他条件关联，至少满足其中一项
    任一
}
public enum emMatchResult
{
    完美满足 = 0,
    仅满足必须条件,
}

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
    全部,           //全部
    破碎,           //灰色
    普通,           //白色
    魔法,           //魔法
    稀有,           //稀有
    手工,           //手工
    套装,           //套装
    传奇,           //传奇
    神器,           //神器
    圣衣,           //圣衣
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
    AUTO_INIT = 5
}


public enum emEquipStatus
{
    /// <summary>
    /// 在背包
    /// </summary>
    Package = 0,
    /// <summary>
    /// 在仓库
    /// </summary>
    Repo = 1,

    /// <summary>
    /// 已装备
    /// </summary>
    Equipped = 2,
    /// <summary>
    /// 交易中
    /// </summary>
    Trading = 3,

    /// <summary>
    /// 拍卖中
    /// </summary>
    Auction = 4,

    /// <summary>
    /// 已升级
    /// </summary>
    Upgraded = 5,





}

public enum emEquip
{
    未知 = 0,

    亡者遗产32,

    亡者遗产满roll,

    天灾2,
    塔格奥之手,
    塔格奥之鳞,
    施法手,
    轮回底子,
    骑士技能衣服,
    死骑冰霜武器,
    冰转珠宝,
    蓝项链,
    刺客毒素黄珠宝,
    物理珠宝,
    冰霜珠宝,
    死灵圣衣洗点21,
    死灵圣衣洗点25,
    太古珠装备名单,
    死灵太古珠条件,
    冥神条件

}
public enum emArtifactBase
{
    未知 = 0,
    [Description("隐密")]
    低力量隐密,
    [Description("知识")]
    知识,
    [Description("知识")]
    死骑知识,
    [Description("眼光")]
    单手眼光,
    [Description("复苏之风")]
    单手复苏,
    [Description("解毒")]
    解毒,
    [Description("解毒")]
    死骑凋零解毒,
    [Description("天灾")]
    天灾,
    [Description("灿烂")]
    死灵灿烂,
    [Description("亡者遗产")]
    亡者遗产,
    [Description("亡者遗产")]
    亡者遗产32,
    [Description("亡者遗产")]
    亡者遗产31,
    [Description("执法者")]
    执法者,
    [Description("轮回")]
    轮回法师,
    [Description("轮回")]
    轮回献祭,
    [Description("永恒")]
    双头斧永恒,
    [Description("末日")]
    双头斧末日,
    [Description("正义之手")]
    正义改造底子,
    [Description("冰冻")]
    巨神冰冻
}

/// <summary>
/// 打孔规则 EquipReform接口type参数值
/// </summary>
public enum emSlotType
{
    Direct,
    Random
}

public enum emSkillMode
{



    法师,
    献祭,
    boss,
    自动,
    法师石魔,
    全能法师,
    献祭无尸爆,
    稳固,
    爆炸

}

public enum emSuitType
{
    效率,
    MF,
    未知,
    boss,
    腐蚀,
    华莱士
}

public enum emReformType
{
    /// <summary>
    /// 施法者
    /// </summary>
    Mage,
    Direct,
    /// <summary>
    /// 魔法
    /// </summary>
    UpgradeMagical,
    /// <summary>
    /// 稀有
    /// </summary>
    UpgradeRare,
    /// <summary>
    /// 稀有19洗数值
    /// </summary>
    Rare19,
    /// <summary>
    /// 随机打孔
    /// </summary>
    SlotRandom,

    /// <summary>
    /// 套装21
    /// </summary>
    Set21,

    /// <summary>
    /// 套装25
    /// </summary>
    Set25,

    /// <summary>
    /// 传奇22
    /// </summary>
    Unique22,

}

public enum emGem
{
    骷髅,
    黄宝石,
    绿宝石,
    蓝宝石,
    红宝石,
    紫宝石,
    钻石

}

public enum emSuitName
{
    死灵召唤最终配装,
    骑士正义,
    骑士末日,
    DK双头永恒,
    骑士冰冻,
    DK剑永恒
}

public enum emSignal
{
    Continue,
    Skip

    
}