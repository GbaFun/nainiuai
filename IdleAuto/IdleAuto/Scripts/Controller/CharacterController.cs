using CefSharp;
using IdleAuto.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public CharacterController()
        {

        }

        public async void OnCharLoaded(params object[] args)
        {
            await GetCharAtt();
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

        private void UpdateAttribute(CharAttributeModel data)
        {
            var r = AccountController.Instance.User.Roles.Find(p => p.RoleId == data.RoleId);
            r.Attribute = data;
        }
    }
}
