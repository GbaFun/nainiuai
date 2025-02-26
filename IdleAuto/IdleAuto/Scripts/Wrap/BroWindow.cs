using CefSharp;
using CefSharp.WinForms;
using IdleAuto.Scripts.Controller;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdleAuto.Scripts.Wrap
{
    public class BroWindow
    {
        private ChromiumWebBrowser _bro;

        private Bridge _bridge;

        private EventSystem EventMa;

        public BaseController BaseController;

        public CharacterController CharController;

        public UserModel User;

        private string _proxy;

        private int _seed;

        private ConcurrentDictionary<string, bool> SignalDic;

        private delegate void OnJsInitCallBack(string jsName);
        private OnJsInitCallBack onJsInitCallBack;

        private void SetSeed(int seed)
        {
            this._seed = seed;
        }

        public BroWindow(int seed, UserModel user, string url, string proxy = "")
        {
            SetSeed(seed);
            User = user;
            this.EventMa = new EventSystem();
            _bridge = new Bridge(seed, EventMa);
            this.BaseController = new BaseController();
            this.CharController = new CharacterController();
            SignalDic = new ConcurrentDictionary<string, bool>();
            EventMa.SubscribeEvent(emEventType.OnSignal, CharController.OnSignalCallback);
            EventMa.SubscribeEvent(emEventType.OnLoginSuccess, SetInstanceUser);
            EventMa.SubscribeEvent(emEventType.OnDungeonRequired, CharController.OnDungeonRequired);
            EventMa.SubscribeEvent(emEventType.OnJsInited, OnJsInited);

            this._bro = InitializeChromium(User.AccountName, url, proxy);
        }
        public void Close()
        {
            EventMa.Dispose();
            TabManager.Instance.DisposePage(_seed);
        }

        public ChromiumWebBrowser GetBro()
        {
            return _bro;
        }
        public void SubscribeEvent(emEventType eventType, Action<object[]> callback)
        {
            EventMa.SubscribeEvent(eventType, callback);
        }
        public void UnsubscribeEvent(emEventType eventType, Action<object[]> callback)
        {
            EventMa.UnsubscribeEvent(eventType, callback);
        }

        public async Task<LoadUrlAsyncResponse> LoadUrl(string url)
        {
            return await _bro.LoadUrlAsync(url);
        }
        public async Task<LoadUrlAsyncResponse> LoadUrlWaitJsInit(string url, string jsName, int outTime = 5000)
        {
            var jsTask = new TaskCompletionSource<bool>();
            onJsInitCallBack = (result) =>
            {
                if (jsName == string.Empty || jsName == result) { jsTask.SetResult(true); onJsInitCallBack = null; }
            };

            var res = await _bro.LoadUrlAsync(url);
            if (res.Success)
            {
                await jsTask.Task;

                return res;
            }
            else { return res; }
        }
        public async Task<JavascriptResponse> CallJs(string jsFunc)
        {
            return await _bro.EvaluateScriptAsync(jsFunc);
        }

        public async Task<JavascriptResponse> CallJsWithReload(string jsFunc, string jsName)
        {
            var jsTask = new TaskCompletionSource<bool>();
            onJsInitCallBack = (result) =>
            {
                if (jsName == string.Empty || jsName == result) { jsTask.SetResult(true); onJsInitCallBack = null; }
            };
            var response = await _bro.EvaluateScriptAsync(jsFunc);

            await Task.Delay(1000);
            _bro.Reload();

            await jsTask.Task;
            return response;
        }

        private void SetInstanceUser(params object[] args)
        {
            if (args.Length == 3)
            {
                bool isSuccess = (bool)args[0];
                string account = args[1] as string;
                List<RoleModel> roles = args[2].ToObject<List<RoleModel>>();
                User.Roles = roles;
            }
        }

        private void OnJsInited(params object[] args)
        {
            string jsName = args[0] as string;
            P.Log($"OnJsInited:{jsName}");
            onJsInitCallBack?.Invoke(jsName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">浏览器的流水号</param>
        /// <returns></returns>
        private ChromiumWebBrowser InitializeChromium(string name, string url, string proxy = "")
        {
            // 创建 CefRequestContextSettings 并指定缓存路径
            var requestContextSettings = new RequestContextSettings
            {
                CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"cache/{name}")
            };
            // 创建 CefRequestContext
            var requestContext = new RequestContext(requestContextSettings);


            // 创建 ChromiumWebBrowser 实例，并指定 RequestContext
            var browser = new ChromiumWebBrowser(url, requestContext) { Dock = DockStyle.Fill };

            if (!string.IsNullOrWhiteSpace(_proxy))
            {
                browser.IsBrowserInitializedChanged += OnIsBrowserInitializedChanged;

            }
            // 绑定对象
            browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            browser.JavascriptObjectRepository.Register("Bridge", _bridge, isAsync: true, options: BindingOptions.DefaultBinder);
            browser.KeyboardHandler = new CEFKeyBoardHandler();
            // 等待页面加载完成后执行脚本
            browser.FrameLoadEnd += (sender, e) => OnFrameLoadEnd(sender, e, name, url);
            browser.FrameLoadStart += (sender, e) => OnFrameLoadStart(sender, e, name, url);
            browser.IsBrowserInitializedChanged += (sender, e) =>
            {
                if (browser.IsBrowserInitialized)
                {
                    P.Log($"Start Load {name} CookieAndCache");
                    PageLoadHandler.LoadCookieAndCache(browser, name, url);
                }
            };



            return browser;
        }
        private void OnIsBrowserInitializedChanged(object sender, EventArgs e)
        {
            var bro = sender as ChromiumWebBrowser;
            // 在浏览器初始化完成后设置代理
            SetProxy(bro);

        }
        public void SetProxy(ChromiumWebBrowser browser)
        {
            var proxySettings = new Dictionary<string, object>
        {
            {"mode", "fixed_servers"},
            {"server", _proxy}
        };
            Cef.UIThreadTaskFactory.StartNew(() =>
            {
                browser.GetBrowser().GetHost().RequestContext.SetPreference("proxy", proxySettings, out string s);
                // 重新设置RequestHandler以更新代理账号密码
                browser.RequestHandler = new MyRequestHandler("d2011731996", "v84dygqx");

            });

        }
        private void RemoveProxy(ChromiumWebBrowser browser)
        {
            // 在 CEF UI 线程上移除代理
            Cef.UIThreadTaskFactory.StartNew(() =>
            {
                var proxyOptions = new Dictionary<string, object>
                {
                    ["mode"] = "direct"
                };
                browser.GetBrowser().GetHost().RequestContext.SetPreference("proxy", proxyOptions, out string error);
            });
        }

        private void OnFrameLoadStart(object sender, FrameLoadStartEventArgs e, string name, string jumpToUrl)
        {
            P.Log($"On {name} FrameLoadStart");

            var bro = sender as ChromiumWebBrowser;
            EventMa.InvokeEvent(emEventType.OnBrowserFrameLoadStart, bro.Address);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="name">账户名称 南宫</param>
        /// <param name="jumpTo">需要条状的地址 在载入cookie之后跳转</param>
        private void OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e, string name, string jumpToUrl)
        {
            var bro = sender as ChromiumWebBrowser;
            string url = bro.Address;

            Task.Run(async () =>
            {
                await PageLoadHandler.LoadJsByUrl(bro);
            });

            if (!PageLoadHandler.ContainsUrl(url, PageLoadHandler.LoginPage))
            {
                P.Log($"Start Save {name} CookieAndCache");
                PageLoadHandler.SaveCookieAndCache(bro, name);//暂时移除多于的保存cookie
                // RemoveProxy(bro);
            }

            EventMa.InvokeEvent(emEventType.OnBrowserFrameLoadEnd, bro.Address);
        }
    }
    public class MyRequestHandler : CefSharp.Handler.RequestHandler
    {
        private string _proxyUserName;
        private string _proxyUserPwd;

        public MyRequestHandler(string proxyUser, string proxyPwd)
        {
            _proxyUserName = proxyUser;
            _proxyUserPwd = proxyPwd;
        }

        protected override bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            if (isProxy)
            {
                callback.Continue(_proxyUserName, _proxyUserPwd);
                return true;
            }
            return base.GetAuthCredentials(chromiumWebBrowser, browser, originUrl, isProxy, host, port, realm, scheme, callback);
        }
    }

}
