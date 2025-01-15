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

        public void OnCharLoaded(params object[] args)
        {
            //更新人物属性
            CharAttributeModel data = args[0].ToObject<CharAttributeModel>();
            UpdateAttribute(data);
        }

        private void UpdateAttribute(CharAttributeModel data)
        {
            var r = AccountController.Instance.User.Roles.Find(p => p.RoleId == data.RoleId);
            r.Attribute = data;
        }
    }
}
