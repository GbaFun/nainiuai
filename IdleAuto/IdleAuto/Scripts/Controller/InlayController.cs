using IdleAuto.Scripts.Wrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Controller
{
    public class InlayController : BaseController
    {
        public InlayController(BroWindow win) : base(win)
        {

        }

        public async Task<string> GetEquipContent()
        {
            var content = await _win.CallJs<String>("_inlay.getEquipContent()");
            return content;
        }
    }
}
