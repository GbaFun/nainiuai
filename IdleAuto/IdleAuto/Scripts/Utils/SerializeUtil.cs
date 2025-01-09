using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class SerializeUtil
{
    public static T ToObject<T>(this ExpandoObject dynamicObject)
    {
        // 将dynamic对象序列化为JSON字符串
        string jsonString = JsonConvert.SerializeObject(dynamicObject);
        // 将JSON字符串反序列化为指定类型的对象
        return JsonConvert.DeserializeObject<T>(jsonString);
    }

    public static T ToObject<T>(this object obj)
    {
        // 将dynamic对象序列化为JSON字符串
        string jsonString = JsonConvert.SerializeObject(obj);
        // 将JSON字符串反序列化为指定类型的对象
        return JsonConvert.DeserializeObject<T>(jsonString);
    }
}
