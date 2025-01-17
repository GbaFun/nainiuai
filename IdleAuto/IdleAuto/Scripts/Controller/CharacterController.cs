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

        public CharacterController()
        {

        }

        public async void OnCharLoaded(params object[] args)
        {
            var c = await GetCharAtt();
            AccountController.Instance.User.Roles.ForEach(p => { p.Attribute = new List<CharAttributeModel>() { c }; });

            FreeDb.Sqlite.Insert(AccountController.Instance.User.Roles).ExecuteAffrows();
            var attList = AccountController.Instance.User.Roles.SelectMany(p => p.Attribute);
            FreeDb.Sqlite.Insert(attList).ExecuteAffrows();



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


    }
}
