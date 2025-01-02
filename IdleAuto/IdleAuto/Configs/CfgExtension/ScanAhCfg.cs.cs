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
        /// 包含词缀
        /// </summary>
        public List<string> content { get; set; }
    }
}
