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
    public int eid { get; set; }

    /// <summary>
    /// 逻辑价格 通过一套衡量算法给出一个价值方便扫货
    /// </summary>
    public int logicPrice { get; set; }

    /// <summary>
    /// 装备名称
    /// </summary>
    public string eTitle { get; set; }

    public int lv { get; set; }

    public int goldCoinPrice { get; set; }
    /// <summary>
    /// 符文价格 多个符文的情况所以是数组
    /// </summary>
    public int[] runePriceArr { get; set; }

    public int[] runeCountArr { get; set; }

}
