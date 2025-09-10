﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class SerializeUtil
{
    private static readonly JsonSerializerSettings UpperSetting = new JsonSerializerSettings
    {
        // 设置枚举转换为字符串
        Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter(new UpperCamelCaseNamingStrategy()) },
        ContractResolver = new UpperCamelCaseContractResolver(),
        Formatting = Formatting.Indented
    };

    private static readonly JsonSerializerSettings LowerSetting = new JsonSerializerSettings
    {
        // 设置枚举转换为字符串
        Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter(new LowerCamelCaseNamingStrategy()) },
        ContractResolver = new LowerCamelCaseContractResolver(),
        Formatting = Formatting.Indented
    };
    public static T ToObject<T>(this ExpandoObject dynamicObject)
    {
        // 将dynamic对象序列化为JSON字符串
        string jsonString = JsonConvert.SerializeObject(dynamicObject);
        // 将JSON字符串反序列化为指定类型的对象
        return JsonConvert.DeserializeObject<T>(jsonString);
    }

    public static T ToObject<T>(this object obj)
    {
        try
        { // 将dynamic对象序列化为JSON字符串
            string jsonString = JsonConvert.SerializeObject(obj);
            // 将JSON字符串反序列化为指定类型的对象
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
        catch (Exception e)
        {
            throw e;
        }

    }

    public static T ToUpperCamelCase<T>(this string str)
    {

        return JsonConvert.DeserializeObject<T>(str, UpperSetting);
    }

    public static string ToLowerCamelCase(this object obj)
    {

        return JsonConvert.SerializeObject(obj, LowerSetting);
    }

    public class UpperCamelCaseNamingStrategy : NamingStrategy
    {
        protected override string ResolvePropertyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            return char.ToUpper(name[0], CultureInfo.InvariantCulture) + name.Substring(1);
        }
    }

    public class UpperCamelCaseContractResolver : DefaultContractResolver
    {
        public UpperCamelCaseContractResolver()
        {
            NamingStrategy = new UpperCamelCaseNamingStrategy();
        }
    }

    public class LowerCamelCaseNamingStrategy : NamingStrategy
    {
        protected override string ResolvePropertyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            return char.ToLower(name[0], CultureInfo.InvariantCulture) + name.Substring(1);
        }
    }

    public class LowerCamelCaseContractResolver : DefaultContractResolver
    {
        public LowerCamelCaseContractResolver()
        {
            NamingStrategy = new LowerCamelCaseNamingStrategy();
        }
    }

    public static T DeepCopy<T>(this T original)
    {
        
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(original));
    }
}
