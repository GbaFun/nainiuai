using CefSharp;
using IdleAuto.Db;
using IdleAuto.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql;

namespace IdleAuto.Scripts.Controller
{
    public class CharacterController
    {
        private static CharacterController instance;
        public static CharacterController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CharacterController();
                }
                return instance;
            }
        }
        /// <summary>
        /// 是否在自动初始化状态
        /// </summary>
        public bool IsAutoInit { get; set; }

        public CharacterController()
        {
            EventManager.Instance.SubscribeEvent(emEventType.OnInitChar, OnInitChar);
        }

        /// <summary>
        /// js主动调用继续执行初始化
        /// </summary>
        /// <param name="args"></param>
        private async void OnInitChar(params object[] args)
        {

            if (IsAutoInit)
            {
                await StartAutoJob();
            }
        }
        /// <summary>
        /// 开始
        /// </summary>
        public async void StartInit()
        {
            IsAutoInit = true;
            await StartAutoJob();
        }

        public void Stop()
        {
            IsAutoInit = false;
        }
        /// <summary>
        /// 开始自动执行
        /// </summary>
        private async Task StartAutoJob()
        {
            if (IsAutoInit)
            {
                if (MainForm.Instance.browser.Address.IndexOf(PageLoadHandler.HomePage) > -1)
                {
                    var roles = await GetRoles();
                    if (roles.Count >= 1)
                    {
                        IsAutoInit = false;
                        return;
                    }
                    await Task.Delay(1500);
                    MainForm.Instance.browser.LoadUrl("https://www.idleinfinity.cn/Character/Create");
                }
                else if (MainForm.Instance.browser.Address.IndexOf(PageLoadHandler.CharCreate) > -1)
                {
                    await CreateRole();
                }
            }
        }


        /// <summary>
        /// 获取人物属性
        /// </summary>
        /// <returns></returns>
        public async Task<CharAttributeModel> GetCharAtt()
        {
            if (MainForm.Instance.browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"_char.getAttribute();");
                return d.Result?.ToObject<CharAttributeModel>();
            }
            else return null;

        }


        /// <summary>
        /// 创建角色
        /// </summary>
        /// <returns></returns>
        public async Task CreateRole()
        {
            await Task.Delay(1500);
            if (MainForm.Instance.browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"_init.createRole();");

            }

        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <returns></returns>
        public async Task<List<RoleModel>> GetRoles()
        {
            await Task.Delay(1500);
            if (MainForm.Instance.browser.CanExecuteJavascriptInMainFrame)
            {
                var d = await MainForm.Instance.browser.EvaluateScriptAsync($@"_init.getRoleInfo();");
                return d.Result?.ToObject<List<RoleModel>>();
            }
            return null;
        }


    }
}
