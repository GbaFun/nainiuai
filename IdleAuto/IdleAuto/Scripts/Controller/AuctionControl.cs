using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AuctionControl
{
    public static List<AHItemModel> BuyEquip(ExpandoObject data)
    {
        var d = data.ToObject<Dictionary<int, AHItemModel>>();
        foreach (var item in d)
        {
            item.Value.eid = item.Key;
        }

        return d.Values.ToList();

    }
}
