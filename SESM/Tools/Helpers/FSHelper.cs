using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SESM.Tools.Helpers
{
    public static class FSHelper
    {
        public static List<NodeInfo> GetDirectories(string path)
        {
            if (!Directory.Exists(path))
                return null;
            
            return Directory.GetDirectories(path).Select(GetDirSizeInfo).OrderBy(x => x.Name).ToList();
        }
        public static NodeInfo GetDirSizeInfo(string path)
        {
            if (!Directory.Exists(path))
                return null;


            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            long totalSize = 0;
            DateTime lastModifTime = dirInfo.LastWriteTime;

            foreach (FileInfo info in files.Select(filePath => new FileInfo(filePath)))
            {
                totalSize += info.Length;
                if (lastModifTime < info.LastWriteTime)
                    lastModifTime = info.LastWriteTime;
            }

            return new NodeInfo()
            {
                Name = PathHelper.GetLastLeaf(path),
                Size = totalSize,
                Timestamp = lastModifTime 
            };
        }

        public static List<NodeInfo> GetFiles(string path)
        {
            string[] files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            return files.Select(GetFileSizeInfo).OrderBy(x => x.Name).ToList();
        }
        public static NodeInfo GetFileSizeInfo(string path)
        {
            if (!File.Exists(path))
                return null;

            FileInfo info = new FileInfo(path);

            return new NodeInfo()
            {
                Name = info.Name,
                Size = info.Length,
                Timestamp = info.LastWriteTime
            };
        }
    }
    

    public class NodeInfo
    {
        public string Name = "";
        public long Size = 0;
        public DateTime Timestamp = DateTime.MinValue;
    }
}