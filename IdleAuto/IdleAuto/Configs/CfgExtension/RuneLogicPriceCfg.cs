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
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "RuneLogicPrice.json");
        public static RuneLogicPriceCfg Instance { get; } = new RuneLogicPriceCfg();

        public Dictionary<int,decimal> data { get; set; }

        public RuneLogicPriceCfg()
        {
            LoadConfig();
        }

        public void LoadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                data = JsonConvert.DeserializeObject<Dictionary<int,decimal>>(json);
            }
            else
            {
                data = new Dictionary<int, decimal>();
            }
        }
    }
   

}
