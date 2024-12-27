using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 页面载入过程中涉及的处理逻辑
/// </summary>
public class PageLoadHandler
{
    public const string LoginPage = "Login";
    public const string HomePage = "Home/Index";
    public static string AhPage = "Auction/Query";

    #region 载入js
    public static void LoadJsByUrl(ChromiumWebBrowser browser)
    {
        var url = browser.Address;
        if (ContainsUrl(url, LoginPage))
        {
            //var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "login.js");
            //LoadJs(jsPath, browser);
        }
        if (ContainsUrl(url, HomePage))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "login.js");
            LoadJs(jsPath, browser);
        }
        if (ContainsUrl(url, AhPage))
        {
            var jsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "ah.js");
            LoadJs(jsPath, browser);
        }

    }


    private static void LoadJs(string path, ChromiumWebBrowser bro)
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

        bro.ExecuteScriptAsync(script);

    }

    #endregion

    #region 载入替换cookie


    public static async void SaveCookieAndCache(ChromiumWebBrowser bro, bool isDirectUpdate = false)
    {
        string stroagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", AccountController.User.Username + ".json");
        string cookiePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", AccountController.User.Username + ".txt");
        var createTime = File.GetCreationTime(cookiePath);
        TimeSpan val = DateTime.Now - createTime;
        if (val.Minutes >= 10 || isDirectUpdate)
        {
            await DevToolUtil.SaveCookiesAsync(bro, cookiePath);
            await DevToolUtil.SaveLocalStorageAsync(bro, stroagePath);
        }

    }

    public static async void LoadCookieAndCache(ChromiumWebBrowser bro)
    {

        string stroagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", AccountController.User.Username + ".json");
        string cookiePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookie", AccountController.User.Username + ".txt");

        if (File.Exists(cookiePath))
        {
            await DevToolUtil.ClearCookiesAsync(bro);
            await DevToolUtil.LoadCookiesAsync(bro, cookiePath);
            bro.LoadUrl("https://www.idleinfinity.cn/Home/Index");
        }

        if (File.Exists(stroagePath))
        {

            await DevToolUtil.ClearLocalStorageAsync(bro);
            await DevToolUtil.LoadLocalStorageAsync(bro, stroagePath);
        }

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
