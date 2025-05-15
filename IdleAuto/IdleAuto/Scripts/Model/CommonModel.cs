using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
using IdleAuto.Scripts.Model;

public class CommonModel : IModel
{
    [Column(IsPrimary = true)]
    public string CommonKey { get; set; }
    public string CommonValue { get; set; }
}
