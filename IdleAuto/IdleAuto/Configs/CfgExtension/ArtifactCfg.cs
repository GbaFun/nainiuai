using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Configs.CfgExtension
{
    public class ArtifactCfg
    {
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "ArtifactCfg.json");
        public static ArtifactCfg Instance { get; } = new ArtifactCfg();
        public Dictionary<emArtifact, Equipment> Data;


        public ArtifactCfg()
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
                Data = json.ToUpperCamelCase<Dictionary<emArtifact, Equipment>>();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
