using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class SingleManagerBase<T> where T : SingleManagerBase<T>, new()
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
                _instance.OnInit();
            }
            return _instance;
        }
    }

    protected virtual void OnInit() { }
}

