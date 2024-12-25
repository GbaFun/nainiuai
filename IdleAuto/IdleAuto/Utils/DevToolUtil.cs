using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Utils
{
    //F12控制台相关操作
    public class DevToolUtil
    {
        //保存cookie
        public static async Task SaveCookiesAsync(ChromiumWebBrowser browser, string fileName)
        {
            FileUtil.EnsureDirectoryExists(fileName);
            var cookieManager = browser.GetCookieManager();
            var cookies = await cookieManager.VisitAllCookiesAsync();
            using (var writer = new StreamWriter(fileName))
            {
                foreach (var cookie in cookies)
                {
                    writer.WriteLine($"{cookie.Domain}\t{cookie.Name}\t{cookie.Value}\t{cookie.Path}\t{cookie.Expires}");
                }
            }
            Console.WriteLine("Cookies saved.");
        }
        /// <summary>
        /// 载入指定路径下的所有cookie
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static async Task LoadCookiesAsync(ChromiumWebBrowser browser, string filename)
        {
            var cookieManager = browser.GetCookieManager();
            using (var reader = new StreamReader(filename))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split('\t');
                    var cookie = new CefSharp.Cookie
                    {
                        Domain = parts[0],
                        Name = parts[1],
                        Value = parts[2],
                        Path = parts[3],
                        Expires = parts[4] == "" ? DateTime.Now.AddDays(1) : DateTime.Parse(parts[4])
                    };
                    var url = "https://www.idleinfinity.cn/";
                    bool success = await cookieManager.SetCookieAsync(url, cookie);
                    if (!success)
                    {
                        Console.WriteLine($"Failed to set cookie: {cookie.Name} for domain: {cookie.Domain}");
                    }
                }
            }
            Console.WriteLine("Cookies loaded.");
        }

        public static async Task ClearCookiesAsync(ChromiumWebBrowser browser)
        {
            var cookieManager = browser.GetCookieManager();
            await cookieManager.DeleteCookiesAsync("", "");
            Console.WriteLine("Cookies cleared.");
        }

        public static async Task PrintCookiesAsync(ChromiumWebBrowser browser)
        {
            var cookieManager = browser.GetCookieManager();
            var cookies = await cookieManager.VisitAllCookiesAsync();
            foreach (var cookie in cookies)
            {
                Console.WriteLine($"{cookie.Domain}\t{cookie.Name}\t{cookie.Value}\t{cookie.Path}\t{cookie.Expires}");
            }
        }

        public static async Task SaveLocalStorageAsync(ChromiumWebBrowser browser, string filename)
        {
            FileUtil.EnsureDirectoryExists(filename);
            var script = @"
                (function() {
                    var items = {};
                    for (var i = 0; i < localStorage.length; i++) {
                        var key = localStorage.key(i);
                        items[key] = localStorage.getItem(key);
                    }
                    return JSON.stringify(items);
                })();";
            var response = await browser.EvaluateScriptAsync(script);
            if (response.Success && response.Result != null)
            {
                File.WriteAllText(filename, response.Result.ToString());
                Console.WriteLine("LocalStorage saved.");
            }
            else
            {
                Console.WriteLine("Failed to save LocalStorage.");
            }
        }

        public static async Task LoadLocalStorageAsync(ChromiumWebBrowser browser, string filename)
        {
            if (File.Exists(filename))
            {
                var json = File.ReadAllText(filename);
                var script = $@"
                    (function() {{
                        var items = {json};
                        for (var key in items) {{
                            localStorage.setItem(key, items[key]);
                        }}
                    }})();";
                var response = await browser.EvaluateScriptAsync(script);
                if (response.Success)
                {
                    Console.WriteLine("LocalStorage loaded.");
                }
                else
                {
                    Console.WriteLine("Failed to load LocalStorage.");
                }
            }
        }

        public static async Task ClearLocalStorageAsync(ChromiumWebBrowser browser)
        {
            var script = @"
                (function() {
                    localStorage.clear();
                })();";
            var response = await browser.EvaluateScriptAsync(script);
            if (response.Success)
            {
                Console.WriteLine("LocalStorage cleared.");
            }
            else
            {
                Console.WriteLine("Failed to clear LocalStorage.");
            }
        }


    }


}
