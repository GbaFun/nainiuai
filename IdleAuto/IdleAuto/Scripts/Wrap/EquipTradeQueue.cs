using IdleAuto.Db;
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
                Console.WriteLine("处理装备登记");
                var l = FreeDb.Sqlite.Select<TradeModel>().Where(p =>(list.Select(s => s.EquipId).Contains(p.EquipId) )||( list.Select(s => s.DemandRoleId).Contains(p.DemandRoleId))).ToList();
                //确保套装中没有被登记掉的装备
                if (l.Count == 0)
                {
                    DbUtil.InsertOrUpdate(list);
                }
            };
        }

        public static void Enqueue(List<TradeModel> list)
        {
            _queue.Enqueue(list);
        }

    }
}
