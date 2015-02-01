using System;
using SESM.DTO;

namespace SESM.Tools.Helpers
{
    public static class PathHelper
    {
        public static string GetLastLeaf(string path)
        {
            if (path == String.Empty)
            {
                return null;
            }
            if (path.Contains("\\"))
            {
                string[] pathSplitted = path.Split('\\');
                return pathSplitted[pathSplitted.Length - 1];
            }
            if(path.Contains("/"))
            {
                string[] pathSplitted = path.Split('/');
                return pathSplitted[pathSplitted.Length - 1];
            }
            return null;
        }
        public static string GetPrefix()
        {
            return SESMConfigHelper.Prefix;
        }

        public static string GetSteamCMDPath()
        {
            return SESMConfigHelper.SEDataPath + @"SteamCMD\";
        }
        public static string GetSyncDirPath()
        {
            return SESMConfigHelper.SEDataPath + @"SyncData\";
        }
        public static string GetInstancePath(EntityServer server)
        {
            return SESMConfigHelper.SESavePath + GetPrefix() + "_" + server.Id + "_" + server.Name + @"\";
        }
        public static string GetSavesPath(EntityServer server)
        {
            return GetInstancePath(server) + @"Saves\";
        }
        public static string GetModsPath(EntityServer server)
        {
            return GetInstancePath(server) + @"Mods\";
        }
        public static string GetSavePath(EntityServer server, string saveName)
        {
            return GetSavesPath(server) + saveName;
        }
        public static string GetModsPath(EntityServer server, string modName)
        {
            return GetModsPath(server) + modName;
        }
        public static string GetConfigurationFilePath(EntityServer server)
        {
            return GetInstancePath(server) + @"SpaceEngineers-Dedicated.cfg";
            
        }
        public static string GetBackupsPath(EntityServer server)
        {
            return GetInstancePath(server) + @"Backups\";
        }

        internal static string GetFSDirName(EntityServer server)
        {
            return GetPrefix() + "_" + server.Id + "_" + server.Name;
        }
    }
}