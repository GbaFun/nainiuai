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
        Data = new Dictionary<string, List<SkillModel>>();
        LoadConfig();
    }

    public Dictionary<string, List<SkillModel>> Data;
    public void LoadConfig()
    {
        if (File.Exists(ConfigFilePath))
        {
            var json = File.ReadAllText(ConfigFilePath);
            //需要处理下把系列合并到职业下
            var dic = json.ToUpperCamelCase<Dictionary<string, List<SkillModel>>>();
            foreach (var item in dic)
            {
                if (item.Key.EndsWith("系"))
                {
                    var key = item.Key.Substring(0, 2);
                    if (Data.ContainsKey(key))
                    {
                        Data[key].AddRange(item.Value);
                    }
                    else
                    {
                        Data.Add(key, item.Value);
                    }
                }
                else
                {
                    Data.Add(item.Key, item.Value);
                }
            }
        }

    }

    public SkillModel GetIdleSkill(string jobName, string skillName)
    {
        var skills = Data[jobName];
        var s = skills.Find(p => p.Name == skillName);
        if (s == null)
        {
            throw new Exception($"未找到技能{skillName}");
        }
        return s;
    }

}

