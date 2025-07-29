using IdleAuto.Db;
using IdleAuto.Scripts.Model;
using IdleAuto.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Wrap
{
    public class EquipTradeQueue
    {
        private static RateControlledQueue<List<TradeModel>> _queue = new RateControlledQueue<List<TradeModel>>(1, TimeSpan.FromSeconds(1));

        public static void AppDomainInitializer()
        {
            _queue.ItemProcessed += (sender, list) =>
            {
                var toTradeList = list.Where(p => p.TradeStatus == emTradeStatus.Register).ToList();
                var toLockList = list.Where(p => p.TradeStatus == emTradeStatus.Locked).ToList();
                Console.WriteLine("处理装备登记");
                var l = FreeDb.Sqlite.Select<TradeModel>().Where(p => (toTradeList.Select(s => s.EquipId).Contains(p.EquipId)) || (list.Select(s => s.DemandRoleId).Contains(p.DemandRoleId))).ToList();
                var lockList = FreeDb.Sqlite.Select<LockEquipModel>().Where(p => toLockList.Select(s => s.EquipId).Contains(p.EquipId)).ToList();
                if (lockList.Count > 0) return;
                //确保套装中没有被登记掉的装备
                if (l.Count == 0&&toTradeList.Count>0)
                {
                    FreeDb.Sqlite.Transaction(() =>
                    {
                        //fsql.Ado.TransactionCurrentThread 获得当前事务对象

                        DbUtil.InsertOrUpdate(toTradeList);

                        DbUtil.InsertOrUpdate(toLockList.ToObject<List<LockEquipModel>>());
                        FreeDb.Sqlite.Ado.TransactionCurrentThread.Commit();
                    });


                }
            };
        }

        public static void Enqueue(List<TradeModel> list)
        {
            _queue.Enqueue(list);
        }

    }
}
