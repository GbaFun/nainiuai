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

        /// <summary>
        /// 主属性类型
        /// </summary>
        public emAttrType AttType { get; set; }

        /// <summary>
        /// 骷髅法速度
        /// </summary>
        public int SkeletonMageFcr { get; set; }

        /// <summary>
        /// 当前穿戴套装名称 维护在suitconfig
        /// </summary>
        public emSuitName SuitName { get; set; }

        public override bool Equals(object obj)
        {
            var t = obj as GroupModel;
            if (t == null) return false;
            else return t.RoleId == this.RoleId;
        }
        public override int GetHashCode()
        {
            return this.RoleId;
        }
    }
}
