using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Model
{
    public class GroupModel
    {
        [Column(IsPrimary = true)]
        public int RoleId { get; set; }

        public string AccountName { get; set; }

        /// <summary>
        /// 每个号四组 roleindex/3 向下取整
        /// </summary>
        public int TeamIndex { get; set; }

        public string RoleName { get; set; }



        public emJob Job { get; set; }

     
        public emSkillMode SkillMode { get; set; } = emSkillMode.默认;
    }
}
