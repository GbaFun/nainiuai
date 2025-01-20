using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


   public class ConfigUtil
    {    /// <summary>
         /// 获取 App.config 文件中的 appSettings 配置项的值
         /// </summary>
         /// <param name="key">配置项的键</param>
         /// <returns>配置项的值</returns>
        public static string GetAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

    }

