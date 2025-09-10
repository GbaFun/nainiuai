using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Model
{

    public class RuneMapModel: IModel
    {
        [Column(IsPrimary = true)]

        public string Key { get; set; }
        public string AccountName { get; set; }
        public int RuneName { get; set; }
        public int Count { get; set; }
    }
}
