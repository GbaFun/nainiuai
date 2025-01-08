using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RoleModel
{
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    //public int Level;
    //public string Job;
    public RoleModel(string info)
    {
        string[] sinfo = info.Split(',');
        RoleId = int.Parse(sinfo[0]);
        RoleName = sinfo[1];
    }
}

