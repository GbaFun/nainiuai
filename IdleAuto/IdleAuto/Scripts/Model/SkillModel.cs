using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Model
{
    /// <summary>
    /// 技能配置表和读取人物技能公用
    /// </summary>
    public class SkillModel
    {
        /// <summary>
        /// 和爱液库保持一致
        /// </summary>
        [Column( IsPrimary = true)]
        public int Id { get; set; }

        public int Lv { get; set; }

        /// <summary>
        /// 技能名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否K了
        /// </summary>
        public bool IsK { get; set; }
    }
}
