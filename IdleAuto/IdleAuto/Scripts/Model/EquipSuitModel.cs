using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Model
{
    public class EquipSuitModel : IModel
    {    /// <summary>
         /// 配装ID
         /// </summary>

        [Column(IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }
        public int SuitId { get; set; }

        /// <summary>
        /// 装备id 一个装备可以存在于多个套装中
        /// </summary>
        public long EquipId { get; set; }

        public string AccountName { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public emSuitType SuitType { get; set; }

        public string SuitName
        {
            get
            {
                return this.SuitType.ToString();
            }
            set { SuitName = value; }
        }
    }
}
