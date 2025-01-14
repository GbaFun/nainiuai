using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum emLogType
{
    Debug = 0,
    AutoEquip = 1,
    AhScan = 2,
    Error = 3,
    Warning = 4,
    RuneUpgrate = 5,
}
public static class P
{
    public static void Log(string message, emLogType logType = emLogType.Debug)
    {
        string lable = string.Empty;
        switch (logType)
        {
            case emLogType.Error:
                lable = "错!!!!误："; break;
            case emLogType.Warning:
                lable = "警!!!!告："; break;
            case emLogType.AutoEquip:
                lable = "一键修车："; break;
            case emLogType.AhScan:
                lable = "自动扫拍："; break;
            case emLogType.RuneUpgrate:
                lable = "自动符文："; break;
            default:
                lable = "消!!!!息："; break;
        }
        //Console.ForegroundColor = color;
        Console.WriteLine($"{lable}:{message}");

        if (logType == emLogType.Debug)
        {
            return;
        }
        string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", logType.ToString());
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        string logFileName = $"{DateTime.Now:yyyyMMdd}_{logType}.log";
        string logFilePath = Path.Combine(logDirectory, logFileName);

        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            writer.Dispose();
            writer.Close();
        }
    }
}
