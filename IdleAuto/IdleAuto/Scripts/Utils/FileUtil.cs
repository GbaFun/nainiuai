using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FileUtil
{
    public static void EnsureDirectoryExists(string path)
    {
        var directoryPath = Path.GetDirectoryName(path);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    public static void CopyDirectory(string sourceDir, string targetDir)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDir);

        // 创建目标目录
        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);

        // 复制所有文件
        foreach (FileInfo file in dir.GetFiles())
        {
            string destPath = Path.Combine(targetDir, file.Name);
            file.CopyTo(destPath, true);
        }

        // 递归复制子目录
        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            string destSubDir = Path.Combine(targetDir, subDir.Name);
            CopyDirectory(subDir.FullName, destSubDir);
        }
    }


}
