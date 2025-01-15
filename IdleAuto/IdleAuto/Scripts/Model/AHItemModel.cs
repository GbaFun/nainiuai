using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AHItemModel
{
    /// <summary>
    /// 装备id
    /// </summary>
    public int Eid { get; set; }

    /// <summary>
    /// 逻辑价格 通过一套衡量算法给出一个价值方便扫货
    /// </summary>
    public decimal LogicPrice { get; set; }

    /// <summary>
    /// 装备名称
    /// </summary>
    public string ETitle { get; set; }

    /// <summary>
    /// 装备文字描述 用于匹配词缀
    /// </summary>
    public string Content { get; set; }

    public int Lv { get; set; }

    public int GoldCoinPrice { get; set; }
    /// <summary>
    /// 符文价格 多个符文的情况所以是数组
    /// </summary>
    public int[] RunePriceArr { get; set; }

    public int[] RuneCountArr { get; set; }

    public bool CanBuy { get; set; }

    public string ToPriceStr()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < RunePriceArr.Length; i++)
        {
            sb.Append(RunePriceArr[i].ToString());
            sb.Append("*");
            sb.Append(RuneCountArr[i]);
            sb.Append(" ");
        }
        return sb.ToString();
    }

}
