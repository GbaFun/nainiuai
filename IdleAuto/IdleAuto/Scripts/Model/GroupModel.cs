using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Model
{
    public class GroupModel : IModel
    {
        [Column(IsPrimary = true)]
        public int RoleId { get; set; }

        /// <summary>
        /// 用户等级
        /// </summary>
        public int Lv { get; set; }

        public string AccountName { get; set; }

        /// <summary>
        /// 每个号四组 roleindex/3 向下取整
        /// </summary>
        public int TeamIndex { get; set; }

        public string RoleName { get; set; }


        public emJob Job { get; set; }


        public emSkillMode SkillMode { get; set; } = emSkillMode.法师;

        /// <summary>
        /// 已过秘境
        /// </summary>
        public int DungeonPassedLv { get; set; }
        /// <summary>
        /// 永恒速度
        /// </summary>
        public int YonghengSpeed { get; set; }

        /// <summary>
        /// 献祭是否修完
        /// </summary>
        public bool IsRepairSacrificeDone { get; set; } = false;
    }
}
