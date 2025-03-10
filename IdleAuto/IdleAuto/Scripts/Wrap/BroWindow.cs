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


        public UserModel User;

        private string _proxy;

        private int _seed;


        private delegate void OnJsInitCallBack(string jsName);
        private OnJsInitCallBack onJsInitCallBack;

        /// <summary>
        /// 等待特定信号的委托
        /// </summary>
        /// <param name="signal"></param>
        protected delegate void OnSignalCallBack(string signal);
        protected OnSignalCallBack onSignalCallBack;

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
            EventMa.SubscribeEvent(emEventType.OnSignal, OnSignalCallback);
            EventMa.SubscribeEvent(emEventType.OnLoginSuccess, SetInstanceUser);

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

        public EventSystem GetEventMa()
        {
            return EventMa;
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
                if ( jsName == result) { jsTask.SetResult(true); onJsInitCallBack = null; }
            };

            var res = await _bro.LoadUrlAsync(url);
            if (res.Success)
            {
                await jsTask.Task;

                return res;
            }
            else { 
                return res;
            }
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
            P.Log("Start Reload With Js CallFunc Finished!");
            _bro.Reload();

            await jsTask.Task;
            return response;
        }
        public async Task<JavascriptResponse> CallJsWaitReload(string jsFunc, string jsName)
        {
            var jsTask = new TaskCompletionSource<bool>();
            onJsInitCallBack = (result) =>
            {
                if (jsName == string.Empty || jsName == result) { jsTask.SetResult(true); onJsInitCallBack = null; }
            };
            var response = await _bro.EvaluateScriptAsync(jsFunc);

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

        public void OnSignalCallback(params object[] args)
        {
            string t = args[0] as string;
            onSignalCallBack?.Invoke(t);
        }

        /// <summary>
        /// 执行一个js或者跳转页面 等待js使用特定信号回调
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="js"></param>
        /// <param name="urlToJump"></param>
        /// <returns></returns>
        public async Task SignalCallback(string signal, Action act)
        {

            var tcs2 = new TaskCompletionSource<bool>();
            if (onSignalCallBack != null)
            {
                throw new Exception("重复添加信号事件方法");
            }
            onSignalCallBack = (result) =>
            {
                if (result == signal)
                {
                    tcs2.SetResult(true);
                    onSignalCallBack = null;
                }
            };
            act.Invoke();
            await tcs2.Task;
            await Task.Delay(1000);
        }

        /// <summary>
        /// 一个方法多种结果 只需等待其中一种结果的情况用这个 比如切图可能会异常可能会直接切过去
        /// </summary>
        /// <param name="signals"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        public async Task SignalRaceCallBack(string[] signals, Action act)
        {
            var tcs2 = new TaskCompletionSource<bool>();
            if (onSignalCallBack != null)
            {
                throw new Exception("重复添加信号事件方法");
            }
            onSignalCallBack = (result) =>
            {
                if (signals.Contains(result))
                {
                    tcs2.SetResult(true);
                    onSignalCallBack = null;
                }
            };
            act.Invoke();
            await tcs2.Task;

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
            P.Log($"On {name} FrameLoadStart");
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
