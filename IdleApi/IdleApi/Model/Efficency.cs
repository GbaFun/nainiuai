using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Model
{
    public class Efficency : IModel
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int Roleid { get; set; }

        public string UserName { get; set; }

        public string RoleName { get; set; }

        /// <summary>
        /// 人物等级
        /// </summary>
        public int Lv { get; set; }

        /// <summary>
        /// 回合数
        /// </summary>
        public decimal Round { get; set; }

        /// <summary>
        /// 效率
        /// </summary>
        public decimal EffVal { get; set; }

        public string MapLv { get; set; }

        /// <summary>
        /// 胜率
        /// </summary>
        public string WinningRate { get; set; }
    }
}
