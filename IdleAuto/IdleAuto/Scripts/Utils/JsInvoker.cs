using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;


public class JsInvoker
{
    public static object HandleMessage(params object[] data)
    {
        string type = data[0] as string;
        switch (type)
        {
            case BridgeMsgType.AhData:
                return AuctionControl.BuyEquip(data[1] as ExpandoObject);
                break;
        }
        return "";
    }


}
