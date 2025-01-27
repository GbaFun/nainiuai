using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Model
{
    /// <summary>
    /// 记录人物所在层数 秘境等状态
    /// </summary>
    public class MapModel
    {
        [Column(IsPrimary = true)]
        public int RoleId { get; set; }

        /// <summary>
        /// 是否在秘境中
        /// </summary>
        public bool IsInDungeon{get;set;}

        /// <summary>
        /// 所在地图等级
        /// </summary>
        public int MapLv { get; set; }

        /// <summary>
        /// 秘境失败次数
        /// </summary>
        public int DungeonFailCount { get; set; }
    }
}
