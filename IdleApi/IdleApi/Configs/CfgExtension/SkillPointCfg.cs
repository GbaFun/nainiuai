using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



public class SkillPoint
{
    public string JobName;

    public emSkillMode SkillMode { get; set; }
    public emJob Job => (emJob)Enum.Parse(typeof(emJob), JobName);
    public LevelRange Lv { get; set; }

    /// <summary>
    /// 保留技能 前置技能 key 技能名称 value 为保留几级 
    /// </summary>
    public Dictionary<string, int> RemainSkill { get; set; }

    /// <summary>
    /// 优先技能按数组顺序点满 前面的会优先点满
    /// </summary>
    public List<string> PrioritySkill { get; set; }

    /// <summary>
    /// 需要选上的技能组
    /// </summary>
    public List<string> GroupSkill { get; set; }
    public int KeySkillId { get; set; }
}

public class SkillPointCfg
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "SkillPoint.json");
    public static SkillPointCfg Instance { get; } = new SkillPointCfg();
    public List<SkillPoint> Data { get; set; }

    public SkillPointCfg()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        if (!File.Exists(ConfigFilePath))
        {
            throw new FileNotFoundException($"Config file not found: {ConfigFilePath}");
        }

        var json = File.ReadAllText(ConfigFilePath);
        Data = json.ToUpperCamelCase<List<SkillPoint>>();
    }

    public SkillPoint GetSkillPoint(emJob job, int level, emSkillMode mode = emSkillMode.法师)
    {
        //理论就一个命中的
        if (job == emJob.死骑)
        {
            mode = emSkillMode.法师;
        }
        var config = Data.Where(p => p.Job == job && p.Lv.AdaptLevel(level) && p.SkillMode == mode).FirstOrDefault();
        if (config == null) throw new Exception($"没有配置{job.ToString()},{level}级的配置");
        return config.DeepCopy();
    }
}
