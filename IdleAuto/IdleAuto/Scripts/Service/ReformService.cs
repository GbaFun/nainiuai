using AttributeMatch;
using IdleAuto.Configs.CfgExtension;
using IdleAuto.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Service
{
    public class ReformService
    {
        private readonly string _accountName;
        Equipment con21 = EmEquipCfg.Instance.GetEquipCondition(emEquip.死灵圣衣洗点21);
        Equipment con25 = EmEquipCfg.Instance.GetEquipCondition(emEquip.死灵圣衣洗点25);

        public ReformService(string accountName)
        {
            _accountName = accountName;

        }

        // JavaScript 可调用的方法
        public Dictionary<int, int> GetEquipCountToReform()
        {
            var list = FreeDb.Sqlite.Select<EquipModel>().Where(p => p.AccountName == _accountName && p.RoleID > 0 && p.Content.Contains("蛇夫座")).ToList();

            var dic = new Dictionary<int, int>();
            var count21 = 0;
            var count25 = 0;

            foreach (var item in list)
            {
                if (!AttributeMatchUtil.Match(item, con21, out _))
                {
                    count21++;
                }
                else
                {
                    if (!AttributeMatchUtil.Match(item, con25, out _))
                    {
                        count25++;
                    }
                }
            }
            dic.Add(21, count21);
            dic.Add(25, count25);
            return dic;
        }
    }
}
