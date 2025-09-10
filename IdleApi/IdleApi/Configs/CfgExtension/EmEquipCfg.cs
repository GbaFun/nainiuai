using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Configs.CfgExtension
{
    public class EmEquipCfg
    {
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "EmEquipCfg.json");
        public static EmEquipCfg Instance { get; } = new EmEquipCfg();
        public Dictionary<emEquip, Equipment> Data;


        public EmEquipCfg()
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
            try
            {
                Data = json.ToUpperCamelCase<Dictionary<emEquip, Equipment>>();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Equipment GetEquipCondition(emEquip e)
        {
            return Data[e];
        }
    }
}
