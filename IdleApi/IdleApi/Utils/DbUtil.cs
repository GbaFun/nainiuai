using IdleAuto.Db;
using IdleAuto.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleApi.Utils
{
    public class DbUtil
    {
        public static bool InsertOrUpdate<T>(T data, bool isAuto = false)
            where T : class, IModel
        {
            IFreeSql db = FreeDb.Sqlite;
            if (isAuto)
            {
                db = FreeDb.SqliteAuto;
            }
            var r = db.Select<T>(data).First();
            if (r == null)
            {
                int rows = db.InsertOrUpdate<T>().SetSource(data).ExecuteAffrows();
                if (rows != 1)
                {
                    throw new Exception("保存失败");
                }
            }
            else
            {
                //乐观锁仅在update 时生效 所以这样操作
                int rows = db.Update<T>().SetSource(data).ExecuteAffrows();
                if (rows != 1)
                {
                    throw new Exception("保存失败");
                }
            }
            return true;
        }
        public static bool InsertOrUpdate<T>(IEnumerable<T> data, bool isAuto = false) where T : class
        {
            IFreeSql db = FreeDb.Sqlite;
            if (isAuto)
            {
                db = FreeDb.SqliteAuto;
            }
            int rows = db.InsertOrUpdate<T>().SetSource(data).ExecuteAffrows();
            if (rows != data.Count())
            {
                throw new Exception("保存失败");
            }
            else return true;
        }


        public static bool Delete<T>(T data, bool isAuto = false) where T : class
        {
            IFreeSql db = FreeDb.Sqlite;
            if (isAuto)
            {
                db = FreeDb.SqliteAuto;
            }
            int rows = db.Delete<T>(data).ExecuteAffrows();
            if (rows != 1)
            {
                throw new Exception("删除失败");
            }
            else return true;
        }
        public static bool Delete<T>(IEnumerable<T> data, bool isAuto = false) where T : class
        {
            IFreeSql db = FreeDb.Sqlite;
            if (isAuto)
            {
                db = FreeDb.SqliteAuto;
            }
            int rows = db.Delete<T>(data).ExecuteAffrows();
            if (rows != data.Count())
            {
                throw new Exception("删除失败");
            }
            else return true;
        }
    }
}
