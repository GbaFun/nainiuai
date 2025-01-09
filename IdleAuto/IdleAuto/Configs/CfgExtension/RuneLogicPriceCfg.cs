using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Configs.CfgExtension
{
    public class RuneLogicPriceCfg
    {
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "ScanAhCfg.json");
        public static RuneLogicPriceCfg Instance { get; } = new RuneLogicPriceCfg();

        public List<DemandEquip> data { get; set; }

        public RuneLogicPriceCfg()
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
    public class RuneLogicPrice
    {
        /// <summary>
        /// 品质
        /// </summary>
        public int rune { get; set; }

        /// <summary>
        /// 部位
        /// </summary>
        public decimal price { get; set; }

    }

}
