using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;


public class BaseController
{
    public static object HandleMessage(params object[] data)
    {
        string type = data[0] as string;
        switch (type)
        {
         
        }
        return "";
    }


}
