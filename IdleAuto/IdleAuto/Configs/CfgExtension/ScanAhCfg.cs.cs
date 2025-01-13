using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Configs.CfgExtension
{
    public class ScanAhCfg
    {
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "ScanAhCfg.json");
        public static ScanAhCfg Instance { get; } = new ScanAhCfg();

        public List<DemandEquip> data { get; set; }

        public ScanAhCfg()
        {
            LoadConfig();
        }

        public void LoadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                data = JsonConvert.DeserializeObject<List<DemandEquip>>(json);
            }
            else
            {
                data = new List<DemandEquip>();
            }
        }
    }
    public class DemandEquip
    {
        /// <summary>
        /// 品质
        /// </summary>
        public string quality { get; set; }

        /// <summary>
        /// 部位
        /// </summary>
        public string part { get; set; }
        /// <summary>
        /// 底子
        /// </summary>
        public string eqbase { get; set; }

        /// <summary>
        /// 装备名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 最低装等
        /// </summary>
        public int minLv { get; set; }

        /// <summary>
        /// 包含词缀
        /// </summary>
        public List<string> content { get; set; }

        public List<RegexMatch> regexList { get; set; }
    }
    public class RegexMatch
    {
        /// <summary>
        /// 匹配类型 compareNum:数值比较
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 数值
        /// </summary>
        public string val { get; set; }

        /// <summary>
        /// 操作 >= <= ==
        /// </summary>
        public string op { get; set; }

        /// <summary>
        /// 请维护数值前后两个关键字 逗号拼接 触发,凤凰击
        /// </summary>
        public string keywords { get; set; }
    }
}
