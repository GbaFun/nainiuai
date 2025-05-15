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
    public class SkillModel: IModel
    {
        /// <summary>
        /// 和爱液库保持一致
        /// </summary>
        [Column(IsPrimary = true)]
        public int Id { get; set; }

        /// <summary>
        /// 已加技能点数
        /// </summary>
        public int Lv { get; set; }

        /// <summary>
        /// 实际技能总额
        /// </summary>
        public int LvSum { get; set; }

        /// <summary>
        /// 技能名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 等级需求
        /// </summary>
        public int LevelRequirement { get; set; }

        /// <summary>
        /// 当前人物等级能点的最高点数
        /// </summary>
        public int CurLvMaxPoint(int roleLv)
        {
            var val = roleLv - LevelRequirement + 1;
            return val >= 20 ? 20 : val;

        }

        /// <summary>
        /// 是否K了
        /// </summary>
        public bool IsK { get; set; }


    }
}
