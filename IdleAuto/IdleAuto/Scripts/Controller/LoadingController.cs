using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LoadingController
{
    private static LoadingController instance;
    public static LoadingController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new LoadingController();
            }
            return instance;
        }
    }

    //public void 
}

