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
        [Column( IsPrimary = true)]
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

        public emMeetType Meets(AttrV4 requare, AttrV4 baseAttr)
        {
            int absoluteStr, absoluteDex, absoluteVit, absoluteEng;
            absoluteStr = baseAttr.Str + StrAdd;
            absoluteDex = baseAttr.Dex + DexAdd;
            absoluteVit = baseAttr.Vit + VitAdd;
            absoluteEng = baseAttr.Eng + EngAdd;

            if (absoluteStr >= requare.Str && absoluteDex >= requare.Dex && absoluteVit >= requare.Vit && absoluteEng >= requare.Eng)
                return emMeetType.AlreadyMeet;
            int v1 = 0;
            if (absoluteStr < requare.Str)
            {
                v1 += requare.Str - absoluteStr;
            }
            if (absoluteDex < requare.Dex)
            {
                v1 += requare.Dex - absoluteDex;
            }
            if (absoluteVit < requare.Vit)
            {
                v1 += requare.Vit - absoluteVit;
            }
            if (absoluteEng < requare.Eng)
            {
                v1 += requare.Eng - absoluteEng;
            }
            if (v1 <= Point)
                return emMeetType.MeetAfterAdd;

            int v2 = baseAttr.Str - requare.Str + baseAttr.Dex - requare.Dex + baseAttr.Vit - requare.Vit + baseAttr.Eng - requare.Eng;
            int totalAdd = StrAdd + DexAdd + VitAdd + EngAdd + Point;
            if (v2 + totalAdd >= 0)
            {
                return emMeetType.MeetAfterReset;
            }
            return emMeetType.CanNotMeet;
        }

        public bool AddPoint(AttrV4 requare, AttrV4 baseAttr)
        {
            int absoluteStr, absoluteDex, absoluteVit, absoluteEng;
            absoluteStr = baseAttr.Str + StrAdd;
            absoluteDex = baseAttr.Dex + DexAdd;
            absoluteVit = baseAttr.Vit + VitAdd;
            absoluteEng = baseAttr.Eng + EngAdd;

            int csa = 0, cda = 0, cva = 0, cea = 0;
            if (absoluteStr < requare.Str)
                csa = requare.Str - absoluteStr;
            if (absoluteDex < requare.Dex)
                cda = requare.Dex - absoluteDex;
            if (absoluteVit < requare.Vit)
                cva = requare.Vit - absoluteVit;
            if (absoluteEng < requare.Eng)
                cea = requare.Eng - absoluteEng;
            if (Point >= csa + cda + cva + cea)
            {
                StrAdd += csa;
                DexAdd += cda;
                VitAdd += cva;
                EngAdd += cea;
                Point -= csa + cda + cva + cea;
                VitAdd += Point;
                return true;
            }

            return false;
        }
    }
}
