using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Model
{
    public enum SkillTypeEnum
    {
        [Description("主动")]
        Active,
        [Description("被动")]
        Passive,
        [Description("光环")]
        Aura
    }
    public class SkillModel
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        /// <summary>
        /// 技能类型
        /// </summary>
        public SkillTypeEnum Type { get; set; }

        public int Lv { get; set; }

        public string SkillName { get; set; }
    }
}
