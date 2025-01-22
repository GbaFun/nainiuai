using IdleAuto.Scripts.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


   public class IdleSkillCfg
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "IdleSkillJson.json");
    public static IdleSkillCfg Instance { get; } = new IdleSkillCfg();

    public IdleSkillCfg()
    {
     
        LoadConfig();
    }

    private Dictionary<emJob, List<SkillModel>> _equipMap;
    public void LoadConfig()
    {
        if (File.Exists(ConfigFilePath))
        {
            var json = File.ReadAllText(ConfigFilePath);
         
        }
        else
        {
           
        }
    }

}

