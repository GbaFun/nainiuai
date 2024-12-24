using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class Bridge
{
    public void ShowMessage(params object[] a)
    {
        foreach(var item in a)
        {
            Console.WriteLine(a.ToString());
        }
    }
    public string GetMessage()
    {
        Console.WriteLine("JavaScript Called Cs");
        return "Message from C#";
    }
}

