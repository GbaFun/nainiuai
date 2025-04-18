using CefSharp;
using CefSharp.Handler;
using CefSharp.WinForms;
using IdleAuto.Scripts.Controller;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public RoleModel CurRole;

        public int CurRoleIndex;

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

        public BroWindow(int seed, UserModel user, string url, bool isProxy = false)
        {
            SetSeed(seed);
            User = user;
            this.EventMa = new EventSystem();
            _bridge = new Bridge(seed, EventMa);
            EventMa.SubscribeEvent(emEventType.OnSignal, OnSignalCallback);
            EventMa.SubscribeEvent(emEventType.OnLoginSuccess, SetInstanceUser);
            EventMa.SubscribeEvent(emEventType.OnCharLoaded, OnCharLoaded);
            EventMa.SubscribeEvent(emEventType.OnJsInited, OnJsInited);
            EventMa.SubscribeEvent(emEventType.OnPostFailed, OnPostFailed);
            if (isProxy)
            {
                SetProxy();
            }
            this._bro = InitializeChromium(User.AccountName, url);
        }
        private void OnCharLoaded(params object[] args)
        {

            var cid = int.Parse(args[0].ToString());
            this.CurRoleIndex = User.Roles.FindIndex(p => p.RoleId == cid);
            if (CurRoleIndex == -1) return;
            this.CurRole = User.Roles[CurRoleIndex];


        }

        private void SetProxy()
        {

            string proxyUrl = "https://dps.kdlapi.com/api/getdps/?secret_id=o9f4g89vo5ys4mb1kad9&signature=cpq7jedsa5f9ioof4j70nyp55auq3dpp&num=1&pt=1&format=json&sep=1";
            string s = HttpUtil.Get(proxyUrl);
            var o = JsonConvert.DeserializeObject<JObject>(s);
            var code = o.Value<int>("code");
            if (code != 0)
            {
                throw new Exception("快代理提取失败");
            }
            var data = o.Value<JToken>("data");
            var proxyList = data.Value<JArray>("proxy_list");
            var proxyServer = proxyList[0].ToString();
            string[] arr = proxyServer.Split(':');
            proxyServer = $"http://{proxyServer}";
            string ip = arr[0];
            int port = int.Parse(arr[1]);
            _proxy = proxyServer;
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsName">可逗号拼接多个</param>
        /// <param name="outTime"></param>
        /// <returns></returns>
        public async Task<LoadUrlAsyncResponse> LoadUrlWaitJsInit(string url, string jsName, int outTime = 5000)
        {
            var taskDic = new Dictionary<string, TaskCompletionSource<bool>>();
            var jsNameArr = jsName.Split(',');
            for (int i = 0; i < jsNameArr.Length; i++)
            {
                var key = jsNameArr[i];
                taskDic.Add(key, new TaskCompletionSource<bool>());
            }
            onJsInitCallBack = (result) =>
            {
                if (jsNameArr.Contains(result)) { taskDic[result].SetResult(true); }
            };

            var res = await _bro.LoadUrlAsync(url);
            if (res.Success)
            {
                await Task.WhenAll(taskDic.Values.Select(p => p.Task));
                onJsInitCallBack = null;
                return res;
            }
            else
            {
                onJsInitCallBack = null;
                throw new Exception($"载入页面{url}失败");
            }
        }
        public async Task<JavascriptResponse> CallJs(string jsFunc)
        {
            var aa = await _bro.EvaluateScriptAsync(jsFunc);
            if (!aa.Success)
            {
                throw new Exception("CallJs执行失败" + aa.Result);
            }
            return aa;
        }

        public async Task<JavascriptResponse> CallJsWithReload(string jsFunc, string jsName)
        {
            var jsTask = new TaskCompletionSource<bool>();
            onJsInitCallBack = (result) =>
            {
                if (jsName == string.Empty || jsName == result) { jsTask.SetResult(true); onJsInitCallBack = null; }
            };
            var response = await _bro.EvaluateScriptAsync(jsFunc);

            await Task.Delay(2000);
            P.Log("Start Reload With Js CallFunc Finished!");
            _bro.Reload();

            await jsTask.Task;
            return response;
        }
        public async Task<JavascriptResponse> CallJsWaitReload(string jsFunc, string jsName)
        {
            var taskDic = new Dictionary<string, TaskCompletionSource<bool>>();
            var jsNameArr = jsName.Split(',');
            for (int i = 0; i < jsNameArr.Length; i++)
            {
                var key = jsNameArr[i];
                taskDic.Add(key, new TaskCompletionSource<bool>());
            }
            onJsInitCallBack = (result) =>
            {
                if (jsNameArr.Contains(result)) { taskDic[result].SetResult(true); }
            };
            var response = await _bro.EvaluateScriptAsync(jsFunc);
            if (!response.Success)
            {
                onJsInitCallBack = null;
                throw new Exception("CallJsWaitReload执行失败" + response.Result);

            }

            await Task.WhenAll(taskDic.Values.Select(p => p.Task));
            onJsInitCallBack = null;
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
                    onSignalCallBack = null;
                    tcs2.SetResult(true);

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
                    onSignalCallBack = null;
                    tcs2.SetResult(true);

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
        private ChromiumWebBrowser InitializeChromium(string name, string url)
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
            browser.RequestHandler = new MyRequestHandler();
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
                browser.RequestHandler = new MyRequestHandler("d4064113710", "ih16bhar");

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
            _proxy = "";
        }

        private void OnFrameLoadStart(object sender, FrameLoadStartEventArgs e, string name, string jumpToUrl)
        {
            var bro = sender as ChromiumWebBrowser;

            P.Log($"On {name} FrameLoadStart");

            P.Log($"On {name} FrameLoadStart");
            EventMa.InvokeEvent(emEventType.OnBrowserFrameLoadStart, bro.Address);
        }

        private void OnPostFailed(params object[] args)
        {
            string errorMsg = args[0].ToString();
            P.Log(errorMsg, emLogType.Error);
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
                if (!string.IsNullOrWhiteSpace(_proxy)) RemoveProxy(bro);
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

        public MyRequestHandler()
        {

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
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return new CustomResourceRequestHandler();
        }


    }

    public class CustomResourceRequestHandler : ResourceRequestHandler
    {
        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            if (request.Url.Contains("Popup.js?"))
            {
                return new CustomResponseFilter();
            }
            return null;
        }
    }
    public class CustomResponseFilter : IResponseFilter
    {
        private MemoryStream memoryStream;
        private bool isDisposed;

        // 必须实现 InitFilter 方法（不是 Initialize）
        public bool InitFilter()
        {
            memoryStream = new MemoryStream();
            return true; // 返回 true 表示初始化成功
        }

        public FilterStatus Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException("CustomResponseFilter");
            }

            dataInRead = 0;
            dataOutWritten = 0;

            if (dataIn == null)
            {
                return FilterStatus.Done;
            }

            // 读取输入数据
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = dataIn.Read(buffer, 0, buffer.Length)) > 0)
            {
                memoryStream.Write(buffer, 0, bytesRead);
                dataInRead += bytesRead;
            }

            // 如果这是最后一块数据

            // 处理完整内容
            string originalContent = Encoding.UTF8.GetString(memoryStream.ToArray());
            string modifiedContent = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts/js", "popup.js")); ;
            byte[] modifiedData = Encoding.UTF8.GetBytes(modifiedContent);

            // 写入输出
            dataOut.Write(modifiedData, 0, modifiedData.Length);
            dataOutWritten = modifiedData.Length;

            return FilterStatus.Done;


            return FilterStatus.NeedMoreData;
        }

        // 必须实现 Dispose 方法
        public void Dispose()
        {
            if (!isDisposed)
            {
                memoryStream?.Dispose();
                isDisposed = true;
            }
        }


    }

}
