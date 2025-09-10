﻿using FreeSql.DataAnnotations;
using IdleAuto.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TradeModel:IModel
{
    
    [Column(IsPrimary = true)]

    public long EquipId { get; set; }

    public emTradeStatus TradeStatus { get; set; }

    /// <summary>
    /// 同一个配置同一个角色只乞讨一次
    /// </summary>
    public string EquipName { get; set; }
    /// <summary>
    /// 同一个部位只乞讨一次
    /// </summary>
    public string EquipSortName { get; set; }



    /// <summary>
    /// 需求人roleid 方便直接跳转修车
    /// </summary>
    public int DemandRoleId { get; set; }
    /// <summary>
    /// 需求人
    /// </summary>
    public string DemandRoleName { get; set; }

    /// <summary>
    /// 所属账户
    /// </summary>
    public string OwnerAccountName { get; set; }
    /// <summary>
    /// 需求人账号
    /// </summary>
    public string DemandAccountName { get; set; }

    /// <summary>
    /// 乐观锁
    /// </summary>
    [Column(IsVersion =true)]
    public int version { get; set; }

}

public enum emTradeStatus
{
    /// <summary>
    /// 登记
    /// </summary>
    Register = 0,

    /// <summary>
    /// 已发送
    /// </summary>
    Sent = 1,

    /// <summary>
    /// 已接收
    /// </summary>
    Received = 2,

    /// <summary>
    /// 拒绝
    /// </summary>
    Rejected = 3,

    Locked=4

}
