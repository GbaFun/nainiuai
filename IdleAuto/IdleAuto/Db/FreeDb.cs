using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IdleAuto.Db
{
  public  class FreeDb
    {
        static Lazy<IFreeSql> sqliteLazy = new Lazy<IFreeSql>(() =>
        {
            var fsql = new FreeSql.FreeSqlBuilder()
                .UseMonitorCommand(cmd => Trace.WriteLine($"Sql：{cmd.CommandText}"))
                .UseAdoConnectionPool(true)
                .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=Idle.db")
                .UseAutoSyncStructure(true) //自动同步实体结构到数据库，只有CRUD时才会生成表
                .Build();
            return fsql;
        });
        public static IFreeSql Sqlite => sqliteLazy.Value;
    }
}
