using IdleAuto.Logic.Enum;
using IdleAuto.Logic.ViewModel.Output;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IdleAuto.ExtMethod.DynamicExtMethod;

namespace IdleAuto.Logic.Serivce
{
    public class JsInvoker
    {
        public static object HandleMessage(params object[] data)
        {
            string type = data[0] as string;
            switch (type)
            {
                case BridgeMsgType.AhData:
                    return BuyEquip(data[1] as ExpandoObject);
                    break;
            }
            return "";
        }

        public static List<EquipToBuy> BuyEquip(ExpandoObject data)
        {
            var d = data.ToObject<Dictionary<int, EquipToBuy>>();
            foreach (var item in d)
            {
                item.Value.eid = item.Key;
            }

            return d.Values.ToList();

        }
    }
}
