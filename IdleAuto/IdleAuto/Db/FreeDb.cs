using FreeSql;
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
                //.UseMonitorCommand(cmd => Trace.WriteLine($"Sql：{cmd.CommandText}"))
                .UseAdoConnectionPool(true)
                .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=Idle.db")
                .UseAutoSyncStructure(true) //自动同步实体结构到数据库，只有CRUD时才会生成表
                .Build();
            return fsql;
        });
        static Lazy<IFreeSql> sqliteLazy2 = new Lazy<IFreeSql>(() =>
        { // 直接使用当前工作目录

            var dbPath2 = "D:\\git\\idle\\IdleinfinityTools\\IdleApi\\IdleApi\\bin\\Debug\\net8.0\\Idle.db";

            Console.WriteLine($"数据库文件路径: {dbPath2}");

            var fsql = new FreeSqlBuilder()
                .UseAdoConnectionPool(true)
                .UseConnectionString(FreeSql.DataType.Sqlite, $"Data Source={dbPath2}")
                .UseAutoSyncStructure(true) // 自动同步实体结构到数据库
                .Build();
            return fsql;
        });
        public static IFreeSql Sqlite => sqliteLazy.Value;

        public static IFreeSql SqliteApi => sqliteLazy2.Value;
        
    }
}
