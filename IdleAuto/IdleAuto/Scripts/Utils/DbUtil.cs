using IdleAuto.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Utils
{
    public class DbUtil
    {
        public static bool InsertOrUpdate<T>(T data) where T:class
        {
            var r = FreeDb.Sqlite.Select<T>(data).First();
            if (r == null)
            {
                int rows = FreeDb.Sqlite.InsertOrUpdate<T>().SetSource(data).ExecuteAffrows();
                if (rows != 1)
                {
                    throw new Exception("保存失败");
                }
            }
            else
            {
                //乐观锁仅在update 时生效 所以这样操作
                int rows = FreeDb.Sqlite.Update<T>().SetSource(data).ExecuteAffrows();
                if (rows != 1)
                {
                    throw new Exception("保存失败");
                }
            }
             return true;
        }
        public static bool InsertOrUpdate<T>(IEnumerable<T> data) where T : class
        {
            int rows = FreeDb.Sqlite.InsertOrUpdate<T>().SetSource(data).ExecuteAffrows();
            if (rows != data.Count())
            {
                throw new Exception("保存失败");
            }
            else return true;
        }

     
        public static bool Delete<T>(T data) where T : class
        {
            int rows = FreeDb.Sqlite.Delete<T>(data).ExecuteAffrows();
            if (rows != 1)
            {
                throw new Exception("删除失败");
            }
            else return true;
        }
        public static bool Delete<T>(IEnumerable<T> data) where T:class
        {
            int rows = FreeDb.Sqlite.Delete<T>(data).ExecuteAffrows();
            if (rows != data.Count())
            {
                throw new Exception("删除失败");
            }
            else return true;
        }
    }
}
