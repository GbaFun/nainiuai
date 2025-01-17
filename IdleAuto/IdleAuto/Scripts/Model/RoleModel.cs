using FreeSql.DataAnnotations;
using IdleAuto.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RoleModel
{
    [Column( IsPrimary = true)]
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public string RoleInfo { get; set; }
    [Navigate(nameof(CharAttributeModel.RoleId))]
    public List<CharAttributeModel> Attribute { get; set; }

    public int Level
    {
        get
        {
            var s = RoleInfo.Split(' ');
            return int.Parse(s[0].Replace("Lv", ""));
        }
    }
    public emJob Job
    {
        get
        {
            var s = RoleInfo.Split(' ');
            var job = s[1].Substring(2, 2);
            return (emJob)Enum.Parse(typeof(emJob), job);
        }
    }
    public emRace Race
    {
        get
        {
            var s = RoleInfo.Split(' ');
            var race = s[1].Substring(0, 2);
            return (emRace)Enum.Parse(typeof(emRace), race);
        }
    }
}