using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Model
{
    public enum emMeetType
    {
        [Description("当前已满足")]
        AlreadyMeet,
        [Description("加点后满足")]
        MeetAfterAdd,
        [Description("重置后满足")]
        MeetAfterReset,
        [Description("无法满足")]
        CanNotMeet
    }
    public enum CharSpeedType
    {
        [Description("施法速度")]
        Fcr,
        [Description("攻击速度")]
        @As,


    }
    public class CharAttributeModel : IModel
    {


        public string AccountName { get; set; }
        [Column(IsPrimary = true)]
        [Navigate(nameof(RoleModel.RoleId))]
        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public int Lv { get; set; }

        public string JobName { get; set; }

        [Description("力量")]
        public int Str { get; set; }

        [Description("敏捷")]
        public int Dex { get; set; }

        [Description("体力")]
        public int Vit { get; set; }

        [Description("精力")]
        public int Eng { get; set; }

        [Description("准确率")]
        public int Att { get; set; }

        [Description("攻击速度")]
        public int As { get; set; }

        [Description("法速")]
        public int Fcr { get; set; }

        [Description("速度生效类型")]
        public CharSpeedType SpeedType { get; set; }

        [Description("压碎伤害")]
        public int CrushDamage { get; set; }

        [Description("撕开")]
        public int OpenWound { get; set; }

        [Description("减防")]
        public int ReduceDef { get; set; }

        [Description("是否无视防御")]
        public bool IsIgnoreDef { get; set; }
        [Description("骷髅法施法速度")]
        public int SkeletonMageFcr { get; set; }
    }
    public class CharBaseAttributeModel
    {
        [Description("剩余点数")]
        public int Point { get; set; }
        [Description("力量")]
        public int Str { get; set; }
        [Description("力量加点")]
        public int StrAdd { get; set; }
        [Description("敏捷")]
        public int Dex { get; set; }
        [Description("敏捷加点")]
        public int DexAdd { get; set; }
        [Description("体力")]
        public int Vit { get; set; }
        [Description("体力加点")]
        public int VitAdd { get; set; }
        [Description("精力")]
        public int Eng { get; set; }
        [Description("精力加点")]
        public int EngAdd { get; set; }

        public emMeetType Meets(AttrV4 require, AttrV4 baseAttr, emAttrType attType = emAttrType.体力)
        {
            int absoluteStr, absoluteDex, absoluteVit, absoluteEng;
            absoluteStr = baseAttr.Str + StrAdd;
            absoluteDex = baseAttr.Dex + DexAdd;
            absoluteVit = baseAttr.Vit + VitAdd;
            absoluteEng = baseAttr.Eng + EngAdd;


            if (absoluteStr >= require.Str && absoluteDex >= require.Dex && absoluteVit >= require.Vit && absoluteEng >= require.Eng&&Point==0)
            {
                //精力优先触发重置
                if (attType == emAttrType.精力 && absoluteVit >= absoluteEng)
                {
                    return emMeetType.MeetAfterReset;
                }
                if (attType == emAttrType.体力 && absoluteVit <= absoluteEng)
                {
                    return emMeetType.MeetAfterReset;
                }
                return emMeetType.AlreadyMeet;
            }
            int v1 = 0;
            if (absoluteStr < require.Str)
            {
                v1 += require.Str - absoluteStr;
            }
            if (absoluteDex < require.Dex)
            {
                v1 += require.Dex - absoluteDex;
            }
            if (absoluteVit < require.Vit)
            {
                v1 += require.Vit - absoluteVit;
            }
            if (absoluteEng < require.Eng)
            {
                v1 += require.Eng - absoluteEng;
            }
            if (v1 <= Point)
                return emMeetType.MeetAfterAdd;

            int v2 = baseAttr.Str - require.Str + baseAttr.Dex - require.Dex + baseAttr.Vit - require.Vit + baseAttr.Eng - require.Eng;
            int totalAdd = StrAdd + DexAdd + VitAdd + EngAdd + Point;
            if (v2 + totalAdd >= 0)
            {
                return emMeetType.MeetAfterReset;
            }
            return emMeetType.CanNotMeet;
        }

        public bool AddPoint(AttrV4 require, AttrV4 baseAttr, emAttrType attType = emAttrType.体力)
        {
            int absoluteStr, absoluteDex, absoluteVit, absoluteEng;
            absoluteStr = baseAttr.Str + StrAdd;
            absoluteDex = baseAttr.Dex + DexAdd;
            absoluteVit = baseAttr.Vit + VitAdd;
            absoluteEng = baseAttr.Eng + EngAdd;

            int csa = 0, cda = 0, cva = 0, cea = 0;
            if (absoluteStr < require.Str)
                csa = require.Str - absoluteStr;
            if (absoluteDex < require.Dex)
                cda = require.Dex - absoluteDex;
            if (absoluteVit < require.Vit)
                cva = require.Vit - absoluteVit;
            if (absoluteEng < require.Eng)
                cea = require.Eng - absoluteEng;
            if (Point >= csa + cda + cva + cea)
            {
                StrAdd += csa;
                DexAdd += cda;
                VitAdd += cva;
                EngAdd += cea;
                Point -= csa + cda + cva + cea;
                if (attType == emAttrType.体力)
                {
                    VitAdd += Point;
                }
                else if (attType == emAttrType.精力)
                {
                    EngAdd += Point;
                }
                return true;
            }

            return false;
        }
    }
}
