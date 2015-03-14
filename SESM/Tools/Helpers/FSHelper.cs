using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SESM.Tools.Helpers
{
    public static class FSHelper
    {
        public static IEnumerable<NodeInfo> GetDirectories(string path)
        {
            if(!Directory.Exists(path))
                return null;

            return Directory.GetDirectories(path).Select(GetDirSizeInfo).OrderBy(x => x.Name).ToList();
        }
        public static NodeInfo GetDirSizeInfo(string path)
        {
            if(!Directory.Exists(path))
                return null;


            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            long totalSize = 0;
            DateTime lastModifTime = dirInfo.LastWriteTime;

            foreach(FileInfo info in files.Select(filePath => new FileInfo(filePath)))
            {
                totalSize += info.Length;
                if(lastModifTime < info.LastWriteTime)
                    lastModifTime = info.LastWriteTime;
            }

            return new NodeInfo()
            {
                Name = PathHelper.GetLastLeaf(path),
                Size = totalSize,
                Timestamp = lastModifTime
            };
        }

        public static IEnumerable<NodeInfo> GetFiles(string path)
        {
            string[] files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            return files.Select(GetFileSizeInfo).OrderBy(x => x.Name).ToList();
        }
        public static NodeInfo GetFileSizeInfo(string path)
        {
            if(!File.Exists(path))
                return null;

            FileInfo info = new FileInfo(path);

            return new NodeInfo()
            {
                Name = info.Name,
                Size = info.Length,
                Timestamp = info.LastWriteTime
            };
        }

        public static void SaveStream(Stream stream, string path, bool overwrite)
        {
            if(!overwrite)
            {
                if(File.Exists(path))
                    throw new Exception("File " + PathHelper.GetLastLeaf(path) + " already exist");
                if(Directory.Exists(path))
                    throw new Exception("Directory " + PathHelper.GetLastLeaf(path) + " already exist");
            }
            else
            {
                if(File.Exists(path))
                    File.Delete(path);
                if(Directory.Exists(path))
                    Directory.Delete(path, true);
            }

            FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);

            stream.CopyTo(fileStream);


            stream.Close();
            fileStream.Close();
        }

        public static void DirectoryCopy(string sourcePath, string destPath, bool recursive)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourcePath);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourcePath);
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destPath, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (recursive)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destPath, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, recursive);
                }
            }
        }
    }

    public class NodeInfo
    {
        public string Name = "";
        public long Size = 0;
        public DateTime Timestamp = DateTime.MinValue;
    }
}