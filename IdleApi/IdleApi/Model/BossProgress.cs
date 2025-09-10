using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Model
{
    public class BossProgress : IModel
    {
        [Column(IsPrimary = true)]
        public string AccountName { get; set; }

        public bool IsPassDailyBoss { get; set; }

        public bool IsPassWorldBoss { get; set; }

        

    }
}
