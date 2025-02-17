using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SingleManagerBase<T> where T : class, new()
{
    private static readonly Lazy<T> _instance = new Lazy<T>(() => new T());

    public static T Instance => _instance.Value;

    protected SingleManagerBase()
    {
        // 防止外部实例化
        if (_instance.IsValueCreated)
        {
            throw new InvalidOperationException("Cannot create another instance of this singleton class.");
        }
    }
}

