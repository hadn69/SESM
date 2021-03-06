﻿using System;
using System.IO;
using System.Linq;
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
        public static string GetSESyncDirPath()
        {
            return SESMConfigHelper.SEDataPath + @"SyncData\";
        }
        public static string GetMESyncDirPath()
        {
            return SESMConfigHelper.MEDataPath + @"SyncData\";
        }
        public static string GetInstancePath(string prefix, EntityServer server)
        {
            if(server.ServerType == EnumServerType.SpaceEngineers)
                return SESMConfigHelper.SESavePath + prefix + "_" + server.Id + "_" + server.Name + @"\";
            else
                return SESMConfigHelper.MESavePath + prefix + "_" + server.Id + "_" + server.Name + @"\";
        }
        public static string GetInstancePath(EntityServer server)
        {
            return GetInstancePath(GetPrefix(), server);
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
            if (server.ServerType == EnumServerType.SpaceEngineers)
                return GetInstancePath(server) + @"SpaceEngineers-Dedicated.cfg";
            else
                return GetInstancePath(server) + @"MedievalEngineers-Dedicated.cfg";

        }
        public static string GetBackupsPath(EntityServer server)
        {
            return GetInstancePath(server) + @"Backups\";
        }

        internal static string GetFSDirName(EntityServer server)
        {
            return GetPrefix() + "_" + server.Id + "_" + server.Name;
        }

        public static string SanitizeFSName(string FSName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(FSName, (current, item) => current.Replace(item.ToString(), ""));
        }
    }
}