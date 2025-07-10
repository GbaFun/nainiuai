using FreeSql;
using IdleAuto.Db;
using IdleAuto.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


public class EquipUtil
{
    /// <summary>
    /// 查询仓库中的装备 不包含有配装的 但是可以包括自己的配装
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public static ISelect<EquipModel, EquipSuitModel> QueryEquipInRepo(
     int roleId = 0)
    {
        // 1. 创建连接查询
        var q = FreeDb.Sqlite.Select<EquipModel, EquipSuitModel>()
            .LeftJoin((a, b) => a.EquipID == b.EquipId);

        // 3. 追加角色条件
        if (roleId > 0)
            q = q.Where((a, b) => b.RoleId == roleId || b.SuitName == null);
        else
            q = q.Where((a, b) => b.SuitName == null);

        return q;
    }

    /// <summary>
    /// 获取当前装备的套装类型 全命中才算命中
    /// </summary>
    /// <param name="curEquips"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    public static emSuitType GetEquipSuitType(Dictionary<emEquipSort, EquipModel> curEquips, RoleModel role)
    {
        var eqids = curEquips.Values.Select(s => s.EquipID);
        var suitList = FreeDb.Sqlite.Select<EquipSuitModel>().Where(p => p.RoleId == role.RoleId).ToList();
        var lookup = suitList.ToLookup(s => s.SuitType);
        var result = lookup
            .Where(g => g.Select(x => x.EquipId).ToHashSet().IsSupersetOf(eqids))
         .ToDictionary(d => d.Key, d => d.ToList());
        foreach (var item in result)
        {
            if (item.Value.Count > 0) return item.Key;
        }

        return emSuitType.未知;


    }

    public static EquipModel GetOne(Expression<Func<EquipModel, bool>> exp)
    {
        return FreeDb.Sqlite.Select<EquipModel>().Where(exp).First();
    }


}

