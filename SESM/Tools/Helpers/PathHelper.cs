using System;
using System.Configuration;
using System.Web.Configuration;
using SESM.DTO;

namespace SESM.Tools.Helpers
{
    public static class PathHelper
    {
        public static string GetLastDirName(string path)
        {
            if (path == String.Empty)
            {
                return null;
            }
            string[] pathSplitted = path.Split('\\');
            return pathSplitted[pathSplitted.Length - 1];
        }
        public static string GetPrefix()
        {
            Configuration conf = WebConfigurationManager.OpenWebConfiguration("/web");
            return conf.AppSettings.Settings["Prefix"].Value;
        }
        public static string GetInstancePath(EntityServer server)
        {
            Configuration conf = WebConfigurationManager.OpenWebConfiguration("/web");
            return conf.AppSettings.Settings["SESavePath"].Value + GetPrefix() + "_" + server.Id + "_" + server.Name + @"\";
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
    }
}