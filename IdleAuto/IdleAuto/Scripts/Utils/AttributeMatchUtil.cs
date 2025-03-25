using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace AttributeMatch
{
    public class AttributeMatchUtil
    {
        /// <summary>
        /// 匹配装备品质
        /// </summary>
        /// <param name="_equip">要匹配的装备对象</param>
        /// <param name="_quality">要匹配的品质,可以是多项，用|分割</param>
        /// <returns>是否满足品质需求</returns>
        public static bool MatchQuallity(EquipModel _equip, string _quality)
        {
            string[] squality = _quality.Split('|');
            foreach (var __quality in squality)
            {
                emItemQuality quality = __quality.ToEnumQuality();
                switch (quality)
                {
                    case emItemQuality.全部:
                        return true;
                    default:
                        if (_equip.emItemQuality == quality)
                        {
                            return true;
                        }
                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// 匹配装备类型
        /// </summary>
        /// <param name="_equip">要匹配的装备对象</param>
        /// <param name="_category">要匹配的类型,可以是多项，用|分割</param>
        /// <returns>是否满足类型需求</returns>
        public static bool MatchCategory(EquipModel _equip, string _category)
        {
            string[] scategory = _category.Split('|');
            emCategory equipCategory = (emCategory)Enum.Parse(typeof(emCategory), CategoryUtil.GetCategory(_equip.EquipBaseName));
            foreach (var __category in scategory)
            {
                emCategory conditionCategory = (emCategory)Enum.Parse(typeof(emCategory), __category);

                bool ismatch;
                switch (conditionCategory)
                {
                    case emCategory.全部:
                        ismatch = true;
                        break;
                    case emCategory.武器:
                        ismatch = equipCategory == emCategory.斧
                               || equipCategory == emCategory.剑
                               || equipCategory == emCategory.锤
                               || equipCategory == emCategory.长矛
                               || equipCategory == emCategory.匕首
                               || equipCategory == emCategory.法杖
                               || equipCategory == emCategory.权杖
                               || equipCategory == emCategory.弓
                               || equipCategory == emCategory.十字弓
                               || equipCategory == emCategory.标枪
                               || equipCategory == emCategory.投掷武器
                               || equipCategory == emCategory.法珠
                               || equipCategory == emCategory.爪
                               || equipCategory == emCategory.游侠弓
                               || equipCategory == emCategory.游侠标枪
                               || equipCategory == emCategory.祭祀刀
                               || equipCategory == emCategory.手杖
                               || equipCategory == emCategory.拳套
                               || equipCategory == emCategory.手弩;
                        break;
                    case emCategory.副手:
                        ismatch = equipCategory == emCategory.盾
                               || equipCategory == emCategory.骑士盾牌
                               || equipCategory == emCategory.牧师副手
                               || equipCategory == emCategory.死灵副手;
                        break;
                    case emCategory.头部:
                        ismatch = equipCategory == emCategory.帽子
                               || equipCategory == emCategory.头饰
                               || equipCategory == emCategory.战士头盔
                               || equipCategory == emCategory.萨满头饰
                               || equipCategory == emCategory.贤者头盔
                               || equipCategory == emCategory.死骑面罩;
                        break;
                    default:
                        ismatch = equipCategory == conditionCategory;
                        break;
                }
                if (ismatch)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 匹配装备属性
        /// </summary>
        /// <param name="_equip">要匹配的装备对象</param>
        /// <param name="target">要匹配的条件配置</param>
        /// <param name="_report">匹配结果报告</param>
        /// <returns>是否满足最低匹配需求</returns>
        public static bool Match(EquipModel _equip, Equipment target, out AttributeMatchReport _report)
        {
            try
            {
                AttributeMatch match = new AttributeMatch(target.Conditions);
                match.Match(_equip, out _report);
                return match.IsMatch;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 匹配单条装备属性
        /// </summary>
        /// <param name="_equip">要匹配的装备对象</param>
        /// <param name="_condition">要匹配的属性条件</param>
        /// <returns>匹配结果报告</returns>
        internal static AttributeMatchResult MatchOne(EquipModel _equip, AttributeCondition _condition)
        {
            AttributeMatchResult result = new AttributeMatchResult()
            {
                IsMatch = false,
                Condition = _condition,
                MatchWeight = 0
            };
            int weight = 0;
            int seq = _condition.Seq <= 0 ? 1 : _condition.Seq;
            switch (_condition.AttributeType)
            {
                case emAttrType.名称:
                    result.IsMatch = MatchName(_equip, _condition, out weight);
                    result.MatchWeight = weight * seq;
                    break;
                case emAttrType.词缀:
                    result.IsMatch = MatchAffix(_equip, _condition, out weight);
                    result.MatchWeight = weight * seq;
                    break;
                case emAttrType.凹槽:
                    result.IsMatch = MatchSlot(_equip, _condition, out weight);
                    result.MatchWeight = weight * seq;
                    break;
                case emAttrType.力量:
                case emAttrType.敏捷:
                case emAttrType.体力:
                case emAttrType.精力:
                case emAttrType.生命:
                case emAttrType.法力:
                case emAttrType.增强伤害:
                case emAttrType.物理伤害:
                case emAttrType.魔法伤害:
                case emAttrType.元素抗性:
                case emAttrType.抗电:
                case emAttrType.抗火:
                case emAttrType.抗毒:
                case emAttrType.抗寒:
                case emAttrType.最大伤害:
                case emAttrType.最小伤害:
                case emAttrType.物品掉率:
                case emAttrType.施法速度:
                case emAttrType.攻击速度:
                case emAttrType.更佳魔法装备:
                case emAttrType.额外金币取得:
                case emAttrType.所有技能:
                case emAttrType.伤害转换:
                case emAttrType.需要力量:
                case emAttrType.需要敏捷:
                case emAttrType.掉落等级:
                    result.IsMatch = MatchBaseAttr(_equip, _condition, out weight);
                    if (_condition.AttributeType == emAttrType.更佳魔法装备 || _condition.AttributeType == emAttrType.额外金币取得)
                    {
                        weight = (int)Math.Floor(weight / 10.0f);
                    }
                    result.MatchWeight = weight * seq;
                    break;
                case emAttrType.单项元素抗性之和:
                    result.IsMatch = MatchResistance(_equip, _condition, out weight);
                    result.MatchWeight = weight * seq;
                    break;
                case emAttrType.技能等级:
                case emAttrType.职业全系技能:
                case emAttrType.职业单系技能:
                case emAttrType.召唤最大数量:
                case emAttrType.指定职业单系技能:
                    result.IsMatch = MatchSkill(_equip, _condition, out weight);
                    result.MatchWeight = weight * seq;
                    break;
                case emAttrType.毒素伤害:
                    result.IsMatch = MatchPoisonAttr(_equip, _condition, out weight);
                    result.MatchWeight = weight * seq;
                    break;
                case emAttrType.物品等级:
                    result.IsMatch = MatchLevel(_equip, _condition, out weight);
                    result.MatchWeight = weight * seq;
                    break;
                case emAttrType.武器速度:
                    result.IsMatch = MatchWeaponSpeed(_equip, _condition, out weight);
                    result.MatchWeight = weight * seq;
                    break;
                case emAttrType.自定义:
                    if (_condition.Operate == emOperateType.不等于)
                        result.IsMatch = !_equip.Content.Contains(_condition.ConditionContent);
                    else
                        result.IsMatch = _equip.Content.Contains(_condition.ConditionContent);
                    weight = result.IsMatch ? 1 : 0;
                    result.MatchWeight = weight * seq;
                    break;
            }

            return result;
        }

        /// <summary>
        /// 匹配装备名称
        /// </summary>
        /// <param name="_equip">要匹配的装备对象</param>
        /// <param name="_condition">要匹配的属性条件</param>
        /// <returns></returns>
        private static bool MatchName(EquipModel _equip, AttributeCondition _condition, out int weight)
        {
            bool ismatch = OperateValue(_equip.EquipName, _condition.ConditionContent, _condition.Operate, out weight);
            return ismatch;
        }

        /// <summary>
        /// 匹配装备是否包含词缀
        /// </summary>
        /// <param name="_equip">要匹配的装备对象</param>
        /// <param name="_condition">要匹配的属性条件</param>
        /// <returns></returns>
        private static bool MatchAffix(EquipModel _equip, AttributeCondition _condition, out int weight)
        {
            Regex regex = new Regex($@"\[(前缀|后缀)\] {_condition}\(\d+\)", RegexOptions.Multiline);
            var match = regex.Match(_equip.Content);
            bool ismatch = match.Success;
            if (match.Success)
            {
                switch (_condition.Operate)
                {
                    case emOperateType.不等于:
                        ismatch = !ismatch;
                        break;
                }
            }
            weight = ismatch ? 1 : 0;
            return ismatch;
        }

        /// <summary>
        /// 匹配装备基础属性
        /// </summary>
        /// <param name="_equip">要匹配的装备对象</param>
        /// <param name="_condition">要匹配的属性条件</param>
        /// <returns></returns>
        private static bool MatchBaseAttr(EquipModel _equip, AttributeCondition _condition, out int weight)
        {
            bool ismatch = false;
            string regexAttr = "";
            weight = 0;
            switch (_condition.AttributeType)
            {
                case emAttrType.力量:
                    regexAttr = $@"\+(?<v>\d+) 力量";
                    break;
                case emAttrType.敏捷:
                    regexAttr = $@"\+(?<v>\d+) 敏捷";
                    break;
                case emAttrType.体力:
                    regexAttr = $@"\+(?<v>\d+) 体力";
                    break;
                case emAttrType.精力:
                    regexAttr = $@"\+(?<v>\d+) 精力";
                    break;
                case emAttrType.生命:
                    regexAttr = $@"\+(?<v>\d+) 生命";
                    break;
                case emAttrType.法力:
                    regexAttr = $@"\+(?<v>\d+) 法力";
                    break;
                case emAttrType.增强伤害:
                    regexAttr = $@"\+(?<v>\d+)\% 增强伤害";
                    break;
                case emAttrType.物理伤害:
                    regexAttr = $@"\+(?<v>\d+)\% 物理伤害";
                    break;
                case emAttrType.魔法伤害:
                    regexAttr = $@"\+(?<v>\d+)\% 魔法伤害";
                    break;
                case emAttrType.元素抗性:
                    regexAttr = $@"元素抗性 \+(?<v>\d+)\%";
                    break;
                case emAttrType.抗电:
                    regexAttr = $@"抗闪电 \+(?<v>\d+)\%";
                    break;
                case emAttrType.抗火:
                    regexAttr = $@"抗火 \+(?<v>\d+)\%";
                    break;
                case emAttrType.抗毒:
                    regexAttr = $@"抗毒 \+(?<v>\d+)\%";
                    break;
                case emAttrType.抗寒:
                    regexAttr = $@"抗寒 \+(?<v>\d+)\%";
                    break;
                case emAttrType.最大伤害:
                    regexAttr = $@"\+(?<v>\d+) 最大伤害";
                    break;
                case emAttrType.最小伤害:
                    regexAttr = $@"\+(?<v>\d+) 最小伤害";
                    break;
                case emAttrType.物品掉率:
                    regexAttr = $@"物品掉率：\+(?<v>\d+)%";
                    break;
                case emAttrType.更佳魔法装备:
                    regexAttr = $@"\+(?<v>\d+)% 更佳的机会取得魔法装备";
                    break;
                case emAttrType.额外金币取得:
                    regexAttr = $@"\+(?<v>\d+)% 额外金币从怪物身上取得";
                    break;
                case emAttrType.施法速度:
                    regexAttr = $@"施法速度提升 (?<v>\d+)%";
                    break;
                case emAttrType.攻击速度:
                    regexAttr = $@"攻击速度提升 (?<v>\d+)%";
                    break;
                case emAttrType.所有技能:
                    regexAttr = $@"\+(?<v>\d+) 所有技能";
                    break;
                case emAttrType.伤害转换:
                    regexAttr = $@"\+(?<v>\d+) [\u4e00-\u9fa5]+转换";
                    break;
                case emAttrType.需要力量:
                    regexAttr = $@"需要力量：\n(?<v>\d+)";
                    break;
                case emAttrType.需要敏捷:
                    regexAttr = $@"需要敏捷：\n(?<v>\d+)";
                    break;
                case emAttrType.掉落等级:
                    regexAttr = $@"掉落等级：(?<v>\d+)";
                    break;
            }
            int attrValue = 0;
            Regex regex = new Regex(regexAttr, RegexOptions.Multiline);
            var match = regex.Match(_equip.Content);
            if (match.Success)
            {
                attrValue = int.Parse(match.Groups["v"].Value);
                ismatch = OperateValue(attrValue, _condition.ConditionContent, _condition.Operate, out weight);
            }
            else
            {
                ismatch = OperateValue(attrValue, _condition.ConditionContent, _condition.Operate, out weight);
            }

            return ismatch;
        }

        /// <summary>
        /// 匹配装备等级
        /// </summary>
        /// <param name="_equip">要匹配的装备对象</param>
        /// <param name="_condition">要匹配的属性条件</param>
        /// <returns></returns>
        private static bool MatchLevel(EquipModel _equip, AttributeCondition _condition, out int weight)
        {
            bool ismatch = false;
            weight = 0;
            string regexAttr = $@"{_equip.EquipName}\((?<v1>\d+)\)";
            Regex regex = new Regex(regexAttr, RegexOptions.Multiline);
            var match = regex.Match(_equip.Content);
            if (match.Success)
            {
                int lv = int.Parse(match.Groups["v1"].Value);
                ismatch = OperateValue(lv, _condition.ConditionContent, _condition.Operate, out weight);
            }

            return ismatch;
        }
        /// <summary>
        /// 匹配装备毒素属性
        /// </summary>
        /// <param name="_equip">要匹配的装备对象</param>
        /// <param name="_condition">要匹配的属性条件</param>
        /// <returns></returns>
        private static bool MatchPoisonAttr(EquipModel _equip, AttributeCondition _condition, out int weight)
        {
            //_condition.ConditionContent = "240" --+40 毒素伤害，持续6次
            bool ismatch = false;
            weight = 0;
            string regexAttr = $@"\+(?<v1>\d+) 毒素伤害，持续(?<v2>\d+)次";
            int attrValue = 0;
            Regex regex = new Regex(regexAttr, RegexOptions.Multiline);
            var match = regex.Match(_equip.Content);
            if (match.Success)
            {
                int v1 = int.Parse(match.Groups["v1"].Value);
                int v2 = int.Parse(match.Groups["v2"].Value);
                attrValue = v1 * v2;
                ismatch = OperateValue(attrValue, _condition.ConditionContent, _condition.Operate, out weight);
            }

            return ismatch;
        }

        /// <summary>
        /// 匹配装备凹槽
        /// </summary>
        /// <param name="_equip">要匹配的装备对象</param>
        /// <param name="_condition">要匹配的属性条件</param>
        /// <returns></returns>
        private static bool MatchSlot(EquipModel _equip, AttributeCondition _condition, out int weight)
        {
            bool ismatch = false;
            string regexStr;
            Regex regex;
            Match match;
            int slotValue = 0;
            weight = 0;
            emOperateType emOperateType = _condition.Operate;
            switch (_equip.emItemQuality)
            {
                case emItemQuality.普通:
                    regexStr = @"最大凹槽：(?<v>\d+)";
                    regex = new Regex(regexStr, RegexOptions.Multiline);
                    match = regex.Match(_equip.Content);
                    if (match.Success)
                    {
                        slotValue = int.Parse(match.Groups["v"].Value);
                        emOperateType = emOperateType.大于等于;
                        ismatch = OperateValue(slotValue, _condition.ConditionContent, emOperateType, out _);
                    }
                    break;
                case emItemQuality.破碎:
                    regexStr = @"凹槽\(0\/(?<v>\d+)\)";
                    regex = new Regex(regexStr, RegexOptions.Multiline);
                    match = regex.Match(_equip.Content);
                    if (match.Success)
                    {
                        slotValue = int.Parse(match.Groups["v"].Value);
                        ismatch = OperateValue(slotValue, _condition.ConditionContent, emOperateType, out _);
                    }
                    break;
            }
            weight = ismatch ? 1 : 0;
            return ismatch;
        }

        /// <summary>
        /// 匹配装备抗性属性
        /// </summary>
        /// <param name="_equip">要匹配的装备对象</param>
        /// <param name="_condition">要匹配的属性条件</param>
        /// <returns></returns>
        private static bool MatchResistance(EquipModel _equip, AttributeCondition _condition, out int weight)
        {
            bool ismatch;
            int resistanceMerge = 0;
            Regex regex1 = new Regex($@"抗闪电 \+(?<v>\d+)\%", RegexOptions.Multiline);
            Regex regex2 = new Regex($@"抗火 \+(?<v>\d+)\%", RegexOptions.Multiline);
            Regex regex3 = new Regex($@"抗毒 \+(?<v>\d+)\%", RegexOptions.Multiline);
            Regex regex4 = new Regex($@"抗寒 \+(?<v>\d+)\%", RegexOptions.Multiline);
            var match = regex1.Match(_equip.Content);
            if (match.Success)
            {
                resistanceMerge += int.Parse(match.Groups["v"].Value);
            }
            match = regex2.Match(_equip.Content);
            if (match.Success)
            {
                resistanceMerge += int.Parse(match.Groups["v"].Value);
            }
            match = regex3.Match(_equip.Content);
            if (match.Success)
            {
                resistanceMerge += int.Parse(match.Groups["v"].Value);
            }
            match = regex4.Match(_equip.Content);
            if (match.Success)
            {
                resistanceMerge += int.Parse(match.Groups["v"].Value);
            }
            ismatch = OperateValue(resistanceMerge, _condition.ConditionContent, _condition.Operate, out weight);
            return ismatch;
        }

        /// <summary>
        /// 匹配装备技能属性
        /// </summary>
        /// <param name="_equip">要匹配的装备对象</param>
        /// <param name="_condition">要匹配的属性条件</param>
        /// <returns></returns>
        private static bool MatchSkill(EquipModel _equip, AttributeCondition _condition, out int weight)
        {
            bool ismatch = false;
            weight = 0;
            string regexAttr = "";
            string[] scondition = _condition.ConditionContent.Split(',');
            if (_equip.Category == emCategory.项链.ToString())
            {
                var test = _equip;
            }
            switch (_condition.AttributeType)
            {
                //_condition.ConditionContent = "火球,3"
                case emAttrType.技能等级:
                    regexAttr = $@"\+(?<v>\d+) {scondition[0]}";
                    break;
                //_condition.ConditionContent = "法师,3"
                case emAttrType.指定职业全系技能:
                    regexAttr = $@"\+(?<v>\d+) {scondition[0]}技能";
                    break;
                //_condition.ConditionContent = "法师,元素,3"
                case emAttrType.指定职业单系技能:
                    regexAttr = $@"\+(?<v>\d+) {scondition[0]}{scondition[1]}技能";
                    break;
                //_condition.ConditionContent = "3"
                case emAttrType.职业全系技能:
                    regexAttr = $@"\+(?<v>\d+) [骑士|法师|战士|游侠|牧师|刺客|萨满|死灵|贤者|武僧|猎手|死骑]技能";
                    break;
                //_condition.ConditionContent = "3"
                case emAttrType.职业单系技能:
                    regexAttr = $@"\+(?<v>\d+) [骑士光环|骑士惩戒|法师元素|法师冥想|战士作战|战士防御|游侠远程|游侠辅助|牧师神圣|牧师暗影|刺客格斗|刺客刺杀|萨满增强|萨满元素|死灵白骨|死灵召唤|贤者变形|贤者自然|武僧武学|武僧真言|猎手陷阱|猎手生存|死骑冰霜|死骑鲜血]技能";
                    break;
                //_condition.ConditionContent = "骷髅法师,1" ||",1"(所有召唤物)
                case emAttrType.召唤最大数量:
                    regexAttr = $@"\+(?<v>\d+) {scondition[0]}最大召唤数量";
                    break;
            }
            int attrValue = 0;
            Regex regex = new Regex(regexAttr, RegexOptions.Multiline);
            var match = regex.Match(_equip.Content);
            if (match.Success)
            {
                attrValue = int.Parse(match.Groups["v"].Value);
                ismatch = OperateValue(attrValue, scondition[scondition.Length - 1], _condition.Operate, out weight);
            }
            return ismatch;
        }

        private static bool MatchWeaponSpeed(EquipModel _equip, AttributeCondition _condition, out int weight)
        {
            bool ismatch = false;
            weight = 0;
            string regexAttr1 = $@"[斧|剑|锤|长矛|匕首|法杖|权杖|弓|十字弓|标枪|投掷武器|法珠|爪|游侠弓|游侠标枪|祭祀刀|手杖|拳套|手弩]速度：(?<v>\d+)";
            string regexAttr2 = $@"[斧|剑|锤|长矛|匕首|法杖|权杖|弓|十字弓|标枪|投掷武器|法珠|爪|游侠弓|游侠标枪|祭祀刀|手杖|拳套|手弩]速度：-(?<v>\d+)";

            int attrValue = 0;
            Regex regex = new Regex(regexAttr1, RegexOptions.Multiline);
            var match = regex.Match(_equip.Content);
            if (match.Success)
            {
                attrValue = int.Parse(match.Groups["v"].Value);
                ismatch = OperateValue(attrValue, _condition.ConditionContent, _condition.Operate, out weight);
            }
            else
            {
                Regex regex2 = new Regex(regexAttr2, RegexOptions.Multiline);
                var match2 = regex2.Match(_equip.Content);
                if (match2.Success)
                {
                    attrValue = int.Parse(match2.Groups["v"].Value) * -1;
                    ismatch = OperateValue(attrValue, _condition.ConditionContent, _condition.Operate, out weight);
                }
                else
                {
                    ismatch = OperateValue(0, _condition.ConditionContent, _condition.Operate, out weight);
                }
            }

            return ismatch;
        }
        /// <summary>
        /// 操作需要比较的值
        /// </summary>
        /// <param name="_value">要操作的原始值</param>
        /// <param name="_condition">要操作的条件值</param>
        /// <param name="_operate">操作类型</param>
        /// <returns></returns>
        private static bool OperateValue(string _value, string _condition, emOperateType _operate, out int weight)
        {
            bool ismatch = false;
            weight = 0;
            switch (_operate)
            {
                case emOperateType.等于:
                    ismatch = _value.Equals(_condition);
                    break;
                case emOperateType.不等于:
                    ismatch = !_value.Equals(_condition);
                    break;
                case emOperateType.在范围内:
                    ismatch = _value.Contains(_condition);
                    break;
            }
            weight = ismatch ? 1 : 0;
            return ismatch;
        }

        /// <summary>
        /// 操作需要比较的值
        /// </summary>
        /// <param name="_value">要操作的原始值</param>
        /// <param name="_condition">要操作的条件值</param>
        /// <param name="_operate">操作类型</param>
        /// <returns></returns>
        private static bool OperateValue(int _value, string _condition, emOperateType _operate, out int weight)
        {
            bool ismatch = false;
            weight = 0;
            int[] condition = Array.ConvertAll(_condition.Split('-'), int.Parse);
            switch (_operate)
            {
                case emOperateType.大于:
                    ismatch = _value > condition[0];
                    if (ismatch)
                        weight = _value - condition[0] + 1;
                    break;
                case emOperateType.大于等于:
                    ismatch = _value >= condition[0];
                    if (ismatch)
                        weight = _value - condition[0] + 1;
                    break;
                case emOperateType.小于:
                    ismatch = _value < condition[0];
                    if (ismatch)
                        weight = -_value + condition[0] + 1;
                    break;
                case emOperateType.小于等于:
                    ismatch = _value <= condition[0];
                    if (ismatch)
                        weight = -_value + condition[0] + 1;
                    break;
                case emOperateType.等于:
                    ismatch = _value == condition[0];
                    if (ismatch)
                        weight = 1;
                    break;
                case emOperateType.不等于:
                    ismatch = _value != condition[0];
                    if (ismatch)
                        weight = 1;
                    break;
                case emOperateType.在范围内:
                    ismatch = _value >= condition[0] && _value <= condition[1];
                    if (ismatch)
                        weight = 1;
                    break;
            }

            return ismatch;
        }
    }
    /// <summary>
    /// 属性匹配条件(单条，可配置)
    /// </summary>
    public class AttributeCondition
    {
        public emMatchType MatchType;
        public string MatchGroupName;
        public emOperateType Operate;
        public emAttrType AttributeType;
        public emArtifactBase ArtifactBase;
        /// <summary>
        /// 条件匹配权重
        /// </summary>
        public int Seq;
        public string ConditionContent;

    }

    /// <summary>
    /// 互斥属性条件组
    /// </summary>
    public class MutexCondition
    {
        public string MutexName;
        public List<AttributeCondition> Conditions;
    }

    /// <summary>
    /// 任一属性条件组
    /// </summary>
    public class AnyoneCondition
    {
        public string AnyoneName;
        public List<AttributeCondition> Conditions;
    }

    /// <summary>
    /// 属性匹配类
    /// 汇总配置的属性匹配条件
    /// 实现匹配方法
    /// 生成匹配报告
    /// </summary>
    public class AttributeMatch
    {
        public bool IsMatch => Report.IsMatch;
        readonly List<AttributeCondition> MustConditions = new List<AttributeCondition>();
        readonly List<AttributeCondition> NomustConditions = new List<AttributeCondition>();
        readonly List<MutexCondition> MutexConditions = new List<MutexCondition>();
        readonly List<AnyoneCondition> AnyoneConditions = new List<AnyoneCondition>();
        AttributeMatchReport Report;

        public AttributeMatch(List<AttributeCondition> attributeConditions)
        {
            MustConditions = attributeConditions.FindAll(p => p.MatchType == emMatchType.必需);
            NomustConditions = attributeConditions.FindAll(p => p.MatchType == emMatchType.可选);

            List<AttributeCondition> mutexConditions = attributeConditions.FindAll(p => p.MatchType == emMatchType.互斥);
            List<AttributeCondition> anyoneConditions = attributeConditions.FindAll(p => p.MatchType == emMatchType.任一);
            Dictionary<string, List<AttributeCondition>> __mutexConditions = new Dictionary<string, List<AttributeCondition>>();
            foreach (var condition in mutexConditions)
            {
                if (!__mutexConditions.ContainsKey(condition.MatchGroupName))
                {
                    __mutexConditions[condition.MatchGroupName] = new List<AttributeCondition>();
                }
                __mutexConditions[condition.MatchGroupName].Add(condition);
            }
            foreach (var item in __mutexConditions)
            {
                MutexConditions.Add(new MutexCondition()
                {
                    MutexName = item.Key,
                    Conditions = item.Value
                });
            }
            Dictionary<string, List<AttributeCondition>> __anyoneConditions = new Dictionary<string, List<AttributeCondition>>();
            foreach (var condition in anyoneConditions)
            {
                if (!__anyoneConditions.ContainsKey(condition.MatchGroupName))
                {
                    __anyoneConditions[condition.MatchGroupName] = new List<AttributeCondition>();
                }
                __anyoneConditions[condition.MatchGroupName].Add(condition);
            }
            foreach (var item in __anyoneConditions)
            {
                AnyoneConditions.Add(new AnyoneCondition()
                {
                    AnyoneName = item.Key,
                    Conditions = item.Value
                });
            }
        }

        public bool Match(EquipModel _equip, out AttributeMatchReport report)
        {
            Report = new AttributeMatchReport()
            {
                MustResults = new List<AttributeMatchResult>(),
                MutexResults = new List<MutexMatchResult>(),
                AnyoneResults = new List<AnyoneMatchResult>(),
                NomustResults = new List<AttributeMatchResult>()
            };
            foreach (var condition in MustConditions)
            {
                Report.MustResults.Add(AttributeMatchUtil.MatchOne(_equip, condition));
            }
            foreach (var condition in NomustConditions)
            {
                Report.NomustResults.Add(AttributeMatchUtil.MatchOne(_equip, condition));
            }
            foreach (var mutexCondition in MutexConditions)
            {
                MutexMatchResult result = new MutexMatchResult()
                {
                    MutexName = mutexCondition.MutexName,
                    Conditions = mutexCondition.Conditions,
                    MatchResults = new List<AttributeMatchResult>()
                };
                foreach (var condition in mutexCondition.Conditions)
                {
                    result.MatchResults.Add(AttributeMatchUtil.MatchOne(_equip, condition));
                }
                Report.MutexResults.Add(result);
            }
            foreach (var anyoneCondition in AnyoneConditions)
            {
                AnyoneMatchResult result = new AnyoneMatchResult()
                {
                    AnyoneName = anyoneCondition.AnyoneName,
                    Conditions = anyoneCondition.Conditions,
                    MatchResults = new List<AttributeMatchResult>()
                };
                foreach (var condition in anyoneCondition.Conditions)
                {
                    result.MatchResults.Add(AttributeMatchUtil.MatchOne(_equip, condition));
                }
                Report.AnyoneResults.Add(result);
            }

            report = Report;
            return Report.IsMatch;
        }
    }

    /// <summary>
    /// 属性匹配报告
    /// </summary>
    public struct AttributeMatchReport
    {
        public bool IsMatch
        {
            get
            {
                if (MustResults.Any(p => !p.IsMatch)) return false;
                if (MutexResults.Any(p => !p.IsMatch)) return false;
                if (AnyoneResults.Any(p => !p.IsMatch)) return false;

                return true;
            }
        }
        public List<AttributeMatchResult> MustResults;
        public List<MutexMatchResult> MutexResults;
        public List<AnyoneMatchResult> AnyoneResults;
        public List<AttributeMatchResult> NomustResults;

        public int MustMastchCount
        {
            get
            {
                return MustResults.Count(p => p.IsMatch);
            }
        }
        public int NomustMatchCount
        {
            get
            {
                return NomustResults.Count(p => p.IsMatch);
            }
        }
        public int MutexMatchCount
        {
            get
            {
                return MutexResults.Count(p => p.IsMatch);
            }
        }
        public int AnyoneMatchCount
        {
            get
            {
                return AnyoneResults.Count(p => p.IsMatch);
            }
        }

        /// <summary>
        /// 匹配程度，用于排序最适合条件的装备
        /// </summary>
        public int MatchWeight
        {
            get
            {
                int mustWeight = MustResults.Sum(p => p.MatchWeight);
                int nomustWeight = NomustResults.Sum(p => p.MatchWeight);
                int anyWeight = AnyoneResults.Sum(p => p.MatchResults.Sum(q => q.MatchWeight));
                int mutexWeight = AnyoneResults.Sum(p => p.MatchResults.Sum(q => q.MatchWeight));

                return mustWeight * 1000000 + (anyWeight + mutexWeight) * 1000 + nomustWeight;
            }
        }
    }

    /// <summary>
    /// 单条属性匹配结果
    /// </summary>
    public struct AttributeMatchResult
    {
        public bool IsMatch { get; internal set; }
        public AttributeCondition Condition;
        public int MatchWeight;
    }

    /// <summary>
    /// 互斥属性组匹配结果
    /// </summary>
    public struct MutexMatchResult
    {
        public bool IsMatch
        {
            get
            {
                return MatchResults.Count(p => p.IsMatch) == 1;
            }
        }
        public string MutexName;
        public List<AttributeCondition> Conditions;
        public List<AttributeMatchResult> MatchResults;
    }

    /// <summary>
    /// 任一属性组匹配结果
    /// </summary>
    public struct AnyoneMatchResult
    {
        public bool IsMatch
        {
            get
            {
                return MatchResults.Count(p => p.IsMatch) > 0;
            }
        }
        public string AnyoneName;
        public List<AttributeCondition> Conditions;
        public List<AttributeMatchResult> MatchResults;
    }

    /// <summary>
    /// 物品类型工具类，根据物品属性获取物品类型
    /// </summary>
    public class CategoryUtil
    {
        private const string filePath = "Document/装备底子分类表.txt";
        private static Dictionary<string, string> ReadCategories(string filePath)
        {
            var categories = new Dictionary<string, string>();
            string currentCategory = null;

            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (!line.Contains("\t"))
                {
                    currentCategory = line.Trim();
                }
                else if (currentCategory != null)
                {
                    var parts = line.Split('\t');
                    if (parts.Length > 1)
                    {
                        if (parts[0].Contains("#")) continue;
                        string itemName = parts[1].Trim();
                        if (!categories.ContainsKey(itemName))
                        {
                            categories[itemName] = currentCategory;
                        }
                    }
                }
            }

            return categories;
        }

        public static Dictionary<string, string> Categories { get; } = ReadCategories(filePath);

        public static string GetCategory(string itemName)
        {
            if (Categories.TryGetValue(itemName, out var category))
                return category;
            else
                return itemName;
        }
    }
    /// <summary>
    /// 职业基础数据类
    /// </summary>
    public class JobBaseAttributeUtil
    {
        private const string filePath = "Document/职业基础属性表.txt";
        private static Dictionary<emJob, AttrV4> JobBaseAttributes;
        private static void LoadJobBaseAttributes()
        {
            JobBaseAttributes = new Dictionary<emJob, AttrV4>();
            var lines = File.ReadAllLines(filePath);
            for (int i = 2; i < lines.Length; i++)
            {
                var parts = lines[i].Split('\t');
                if (parts.Length < 7) continue;

                if (Enum.TryParse(parts[0], out emJob job))
                {
                    var attr = new AttrV4(
                        int.Parse(parts[3]), // 力量
                        int.Parse(parts[4]), // 敏捷
                        int.Parse(parts[5]), // 体力
                        int.Parse(parts[6])  // 精力
                    );
                    JobBaseAttributes[job] = attr;
                }
            }
        }

        public static AttrV4 JobBaseAttr(emJob job)
        {
            if (JobBaseAttributes == null || JobBaseAttributes.Count == 0)
            {
                LoadJobBaseAttributes();
            }
            if (JobBaseAttributes.TryGetValue(job, out var attr))
                return attr;
            else
                throw new ArgumentException($"Invalid job type: {job}");
        }
    }

    /// <summary>
    /// 物品品质工具类，根据配置的品质字段获取物品品质枚举
    /// </summary>
    public static class MatchHelper
    {
        public static emItemQuality ToEnumQuality(this string squality)
        {
            switch (squality)
            {
                case "physical":
                case "全部":
                    return emItemQuality.全部;
                case "slot":
                case "破碎":
                    return emItemQuality.破碎;
                case "base":
                case "普通":
                    return emItemQuality.普通;
                case "magical":
                case "魔法":
                    return emItemQuality.魔法;
                case "rare":
                case "稀有":
                    return emItemQuality.稀有;
                case "craft":
                case "手工":
                    return emItemQuality.手工;
                case "set":
                case "套装":
                    return emItemQuality.套装;
                case "unique":
                case "传奇":
                    return emItemQuality.传奇;
                case "artifact":
                case "神器":
                    return emItemQuality.神器;
                case "holy":
                case "圣衣":
                    return emItemQuality.圣衣;
                default:
                    return emItemQuality.全部;
            }
        }

        public static int MatchWeight(this Dictionary<int, AttributeMatchReport> reportMap)
        {
            int weight = 0;
            foreach (var report in reportMap)
            {
                if (report.Value.IsMatch)
                {
                    weight += 1 << (31 - report.Key);
                    weight += report.Value.MatchWeight;
                }
            }
            return weight;
        }
    }
}