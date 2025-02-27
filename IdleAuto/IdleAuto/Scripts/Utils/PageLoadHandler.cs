using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/// <summary>
/// 页面载入过程中涉及的处理逻辑
/// </summary>
public class PageLoadHandler
{
    public const string LoginPage = "Login";
    public const string HomePage = "Home/Index";
    public const string RolePage = "Character";
    public const string MaterialPage = "Equipment/Material";
    public const string AhPage = "Auction/Query";
    public const string EquipPage = "Equipment/Query";
    public const string CharCreate = "Character/Create";
    public const string CharGroup = "Character/Group";
    public const string CharDetail = "Character/Detail";
    public const string MapPage = "Map/Detail";
    public const string InDungeon = "InDungeon";
    public const string MapDungeon = "Map/Dungeon";
    public const string GuaJi = "Battle/Guaji";

    #region 载入js
    public static async Task LoadJsByUrl(ChromiumWebBrowser browser)
    {
        var url = browser.Address;
        //全局js
        await LoadGlobalJs(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "IdleUtils.js"), browser);
        await LoadGlobalJs(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "IdleTools.js"), browser);
        if (!ContainsUrl(url, LoginPage))
        {
            await LoadGlobalJs(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "char.js"), browser);
        }
        if (ContainsUrl(url, LoginPage))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "login.js");
            //var voPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "versionOverride.js");
            await LoadGlobalJs(jsPath, browser);
            // await LoadGlobalJs(voPath, browser);
        }
        else if (ContainsUrl(url, HomePage))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "login.js");
            await LoadGlobalJs(jsPath, browser);
            var initPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "init.js");
            await LoadGlobalJs(initPath, browser);
        }
        else if (ContainsUrl(url, AhPage))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "ah.js");
            await LoadGlobalJs(jsPath, browser);
        }
        else if (ContainsUrl(url, MaterialPage))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "rune.js");
            await LoadGlobalJs(jsPath, browser);
        }
        else if (ContainsUrl(url, EquipPage))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "equip.js");
            await LoadGlobalJs(jsPath, browser);
            //var jsPath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "equipattr.js");
            //await LoadJs(jsPath2, browser);
        }
        else if (ContainsUrl(url, CharCreate))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "init.js");
            await LoadGlobalJs(jsPath, browser);
        }
        else if (ContainsUrl(url, CharGroup))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "init.js");
            await LoadGlobalJs(jsPath, browser);
        }
        else if (ContainsUrl(url, MapPage) || ContainsUrl(url, InDungeon) || ContainsUrl(url, MapDungeon))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "map.js");
            await LoadGlobalJs(jsPath, browser);
        }
        else if (ContainsUrl(url, GuaJi))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "guaji.js");
            await LoadGlobalJs(jsPath, browser);
        }

    }


    private static async Task LoadJs(string path, ChromiumWebBrowser bro)
    {
        // 在主框架中执行自定义脚本
        // 获取WinForms程序目录下的JavaScript文件路径

        string scriptContent = File.ReadAllText(path);

        // 在主框架中执行自定义脚本
        string script = $@"
                    (function() {{
                        var script = document.createElement('script');
                        script.type = 'text/javascript';
                        script.text = {scriptContent};
                        document.head.appendChild(script);
                    }})();
                ";

        await Task.Run(() =>
        {
            bro.ExecuteScriptAsync(script);
        });
    }
    private static async Task LoadGlobalJs(string path, ChromiumWebBrowser bro)
    {
        // 在主框架中执行自定义脚本
        // 获取WinForms程序目录下的JavaScript文件路径

        string scriptContent = File.ReadAllText(path);

        // 在主框架中执行自定义脚本
        string script = $@"
                    {scriptContent}
                ";

        await Task.Run(() =>
        {
            bro.ExecuteScriptAsync(script);
        });
    }


    #endregion

    #region 载入替换cookie


    public static async void SaveCookieAndCache(ChromiumWebBrowser bro, string name, bool isDirectUpdate = false)
    {
        if (bro.CanExecuteJavascriptInMainFrame)
        {
            string stroagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", name + ".json");
            string cookiePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", name + ".txt");
            var createTime = File.GetCreationTime(cookiePath);
            var size = File.ReadAllBytes(cookiePath).Length;
            TimeSpan val = DateTime.Now - createTime;
            if (size == 0 || val.TotalMinutes >= 10 || isDirectUpdate)
            {
                await DevToolUtil.SaveCookiesAsync(bro, cookiePath);
                await DevToolUtil.SaveLocalStorageAsync(bro, stroagePath);
            }
        }
    }

    public static async void DeleteCookie(ChromiumWebBrowser bro, string name)
    {
        string stroagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", name + ".json");
        string cookiePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", name + ".txt");

        if (File.Exists(cookiePath))
        {
            await DevToolUtil.ClearCookiesAsync(bro);
            File.Delete(cookiePath);
        }
    }

    public static async Task LoadCookieAndCache(ChromiumWebBrowser bro, string name, string url = "")
    {

        string stroagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", name + ".json");
        string cookiePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", name + ".txt");

        if (!File.Exists(cookiePath))
        {
            FileStream fs = File.Create(cookiePath);
            fs.Close();
        }
        else if (!ValidCookie(cookiePath))
        {
            File.Delete(cookiePath);
            FileStream fs = File.Create(cookiePath);
            fs.Close();
        }

        await DevToolUtil.ClearCookiesAsync(bro);
        await DevToolUtil.LoadCookiesAsync(bro, cookiePath);
        if (url == "")
        {
            bro.LoadUrl("https://www.idleinfinity.cn/Home/Index");
        }
        else
        {
            bro.LoadUrl(url);
        }

        if (!File.Exists(stroagePath))
        {
            FileStream fs = File.Create(stroagePath);
            fs.Close();
        }
        await DevToolUtil.ClearLocalStorageAsync(bro);
        await DevToolUtil.LoadLocalStorageAsync(bro, stroagePath);
    }
    /// <summary>
    /// 检查cookie是否在有效期
    /// </summary>
    /// <param name="cookiePath"></param>
    /// <returns></returns>
    public static bool ValidCookie(string cookiePath)
    {
        if (!File.Exists(cookiePath))
        {
            return false;
        }
        var cookieList = new List<CefSharp.Cookie>();
        DateTime createdTime = File.GetCreationTime(cookiePath);
        using (var reader = new StreamReader(cookiePath))
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
                cookieList.Add(cookie);

            }
        }
        if (cookieList.Count == 0) return false;
        var idleCookie = cookieList.Where(p => p.Name == "idleinfinity.cookies").FirstOrDefault();
        if (idleCookie == null) return false;
        if (DateTime.Now.AddMinutes(-60) >= idleCookie.Expires) return false;
        return true;
    }
    #endregion

    /// <summary>
    /// 检测url是否有指定key
    /// </summary>
    /// <param name="url"></param>
    /// <param name="keyPage"></param>
    /// <returns></returns>
    public static Boolean ContainsUrl(string url, string keyPage)
    {
        return url.IndexOf(keyPage) > -1;
    }
}
