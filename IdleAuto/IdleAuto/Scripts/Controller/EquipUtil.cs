using IdleAuto.Db;
using IdleAuto.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Controller
{
    public class EquipUtil
    {
        /// <summary>
        /// 查询仓库中的装备 不包含有配装的 但是可以包括自己的配装
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static List<EquipModel> QueryEquipInRepo(Expression<Func<EquipModel, bool>> exp, int roleId = 0)
        {
            if (roleId > 0)
            {
                var q = FreeDb.Sqlite.Select<EquipSuitModel, EquipModel>()
                   .LeftJoin((a, b) => a.EquipId == b.EquipID);
                var list = q.Where((a, b) => a.RoleId == roleId).ToList().Select(s => s.EquipId).Distinct();
                exp.And(a => a.IsInSuit == false || list.Contains(a.EquipID));

            }
            else
            {
                exp.And(a => a.IsInSuit == false);
            }
            return FreeDb.Sqlite.Select<EquipModel>().Where(exp).ToList();
        }


    }
}
