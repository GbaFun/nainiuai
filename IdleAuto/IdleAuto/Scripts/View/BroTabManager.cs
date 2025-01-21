using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdleAuto.Scripts.View
{
	public class BroTabManager
	{
		private TabControl Tab;

		/// <summary>
		/// 登录账号调取cookie用
		/// </summary>
		private string _accountName { get; set; }

		public ChromiumWebBrowser browser
		{
			get;
			private set;
		}

		public event Action<string, string> AddTabPageEvent;

		private void InitializeChromium(string name, string url)
		{
			// 创建 CefRequestContextSettings 并指定缓存路径
			var requestContextSettings = new RequestContextSettings
			{
				CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"cache/{name}")
			};
			// 创建 CefRequestContext
			var requestContext = new RequestContext(requestContextSettings);
			// 创建 ChromiumWebBrowser 实例，并指定 RequestContext
			browser = new ChromiumWebBrowser(url, requestContext) { Dock = DockStyle.Fill };
			// 绑定对象
			browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
			browser.JavascriptObjectRepository.Register("Bridge", new Bridge(), isAsync: true, options: BindingOptions.DefaultBinder);
			browser.KeyboardHandler = new CEFKeyBoardHandler();
			// 等待页面加载完成后执行脚本
			browser.FrameLoadEnd += OnFrameLoadEnd;
			browser.FrameLoadStart += OnFrameLoadStart;
		}


		public BroTabManager(TabControl tab)
		{
			this.Tab = tab;
			AddTabPageEvent += OnAddTabPage;

		}
		private void OnAddTabPage(string title, string url)
		{
			// 检查是否需要调用 Invoke
			if (Tab.InvokeRequired)
			{
				// 使用 Invoke 方法将操作委托给创建控件的线程
				Tab.Invoke(new Action(() => AddTabPage(title, url)));
			}
			else
			{
				AddTabPage(title, url);
			}
		}

		private void AddTabPage(string name, string url)
		{
			_accountName = name;
			InitializeChromium(name, url);
			// 创建 TabPage
			TabPage tabPage = new TabPage(name);
			tabPage.Controls.Add(browser);
			// 将 TabPage 添加到 TabControl
			Tab.TabPages.Add(tabPage);
		}
		public void TriggerAddTabPage(string title, string url)
		{
			// 触发事件
			AddTabPageEvent?.Invoke(title, url);

		}

		private void OnFrameLoadStart(object sender, FrameLoadStartEventArgs e)
		{
			var bro = sender as ChromiumWebBrowser;

		}

		private void OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
		{
			var bro = sender as ChromiumWebBrowser;
			string url = bro.Address;
			Console.WriteLine(url);
			if (PageLoadHandler.ContainsUrl(url, PageLoadHandler.LoginPage))
			{
				Task.Run(async () =>
				{
					await PageLoadHandler.LoadCookieAndCache(browser,_accountName);
				});
			}
			Task.Run(async () =>
			{
				await PageLoadHandler.LoadJsByUrl(browser);
			});

			if (!PageLoadHandler.ContainsUrl(url, PageLoadHandler.LoginPage))
			{
				PageLoadHandler.SaveCookieAndCache(browser);
			}
		}
	}
}
