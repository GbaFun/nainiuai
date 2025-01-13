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
}
public static class P
{
    public static void Log(string message, emLogType logType = emLogType.Debug)
    {
        Console.WriteLine($"{logType}_log:{message}");

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
