using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IdleAuto.Scripts.Utils
{
  public  class ReflectUtil
    {
        public static void  Invoke(Type t,string methodName,params object[] data)
        {
            MethodInfo method = t.GetMethod(methodName);
            method.Invoke(null, data);
        }

    }
}
