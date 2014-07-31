using System;
using System.Configuration;
using System.Text;
using System.Web.Configuration;
using System.Web.Security;
using Microsoft.Win32;
using SESM.Models.Views.Settings;
using Westwind.Utilities.Configuration;

namespace SESM.Tools.Helpers
{
    public class SESMConfigHelper
    {
        // TODO : refactor getter/setter as properties

        public static SESMConfigStorage ConfigStorage;

        static SESMConfigHelper()
        {
            ConfigStorage = new SESMConfigStorage();
            ConfigStorage.Initialize();
        }

        private static void InitializeRegistry()
        {
            RegistryKey wow6432Node = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node", true);
            RegistryKey ksh = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\KeenSoftwareHouse", true);
            if (ksh == null)
            {
                wow6432Node.CreateSubKey("KeenSoftwareHouse");
                ksh = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\KeenSoftwareHouse", true);
            }
            RegistryKey seds = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\KeenSoftwareHouse\SpaceEngineersDedicatedServer", true);
            if (seds == null)
            {
                ksh.CreateSubKey("SpaceEngineersDedicatedServer");
                seds = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\KeenSoftwareHouse\SpaceEngineersDedicatedServer", true);
                seds.SetValue("AddDateToLog", "False", RegistryValueKind.String);
                seds.SetValue("SendLogToKeen", "False", RegistryValueKind.String);
            }
        }

        public static bool GetAddDateToLog()
        {
            InitializeRegistry();
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Wow6432Node\KeenSoftwareHouse\SpaceEngineersDedicatedServer");

            string addDateToLog = (string)regKey.GetValue("AddDateToLog");

            if (addDateToLog == "False")
                return false;
            if (addDateToLog == "True")
                return true;

            throw new SystemException("RegKeyError");
        }

        public static void SetAddDateToLog(bool value)
        {
            InitializeRegistry();
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Wow6432Node\KeenSoftwareHouse\SpaceEngineersDedicatedServer", true);

            regKey.SetValue("AddDateToLog", value ? "True" : "False", RegistryValueKind.String);
        }

        public static bool GetSendLogToKeen()
        {
            InitializeRegistry();
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Wow6432Node\KeenSoftwareHouse\SpaceEngineersDedicatedServer");

            string addDateToLog = (string)regKey.GetValue("SendLogToKeen");

            if (addDateToLog == "False")
                return false;
            if (addDateToLog == "True")
                return true;

            throw new SystemException("RegKeyError");
        }

        public static void SetSendLogToKeen(bool value)
        {
            InitializeRegistry();
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Wow6432Node\KeenSoftwareHouse\SpaceEngineersDedicatedServer", true);

            regKey.SetValue("SendLogToKeen", value ? "True" : "False", RegistryValueKind.String);
        }

        public static string GetDBConnString()
        {
            ConfigStorage.Read();
            return ConfigStorage.DBConnString;
        }

        public static void SetDBConnString(string DBConnString)
        {
            ConfigStorage.DBConnString = DBConnString;
            ConfigStorage.Write();
        }

        public static string GetPrefix()
        {
            ConfigStorage.Read();
            return ConfigStorage.Prefix;
        }

        public static void SetPrefix(string prefix)
        {
            ConfigStorage.Prefix = prefix;
            ConfigStorage.Write();
        }

        public static string GetSESavePath()
        {
            ConfigStorage.Read();
            return ConfigStorage.SESavePath;
        }

        public static void SetSESavePath(string SESavePath)
        {
            ConfigStorage.SESavePath = SESavePath;
            ConfigStorage.Write();
        }

        public static string GetSEDataPath()
        {
            ConfigStorage.Read();
            return ConfigStorage.SEDataPath;
        }

        public static void SetSEDataPath(string SEDataPath)
        {
            ConfigStorage.SEDataPath = SEDataPath;
            ConfigStorage.Write();
        }

        public static bool GetDiagnosis()
        {
            ConfigStorage.Read();
            return ConfigStorage.Diagnosis;
        }

        public static void SetDiagnosis(bool diagnosis)
        {
            ConfigStorage.Diagnosis = diagnosis;
            ConfigStorage.Write();
        }

        public static bool GetAutoUpdate()
        {
            ConfigStorage.Read();
            return ConfigStorage.AutoUpdate;
        }

        public static void SetAutoUpdate(bool autoUpdate)
        {
            ConfigStorage.AutoUpdate = autoUpdate;
            ConfigStorage.Write();
        }

        public static string GetAUUsername()
        {
            ConfigStorage.Read();
            return ConfigStorage.AUUsername;
        }

        public static void SetAUUsername(string aUUsername)
        {
            ConfigStorage.AUUsername = aUUsername;
            ConfigStorage.Write();
        }

        public static string GetAUPassword()
        {
            ConfigStorage.Read();
            string value = ConfigStorage.AUPassword;
            return Encoding.UTF8.GetString(MachineKey.Unprotect(Convert.FromBase64String(value), "SteamPassword"));
        }

        public static void SetAUPassword(string aUPassword)
        {
            string value = Convert.ToBase64String(MachineKey.Protect(Encoding.UTF8.GetBytes(aUPassword), "SteamPassword"));
            ConfigStorage.AUPassword = value;
            ConfigStorage.Write();
        }

        public static string GetLastAU()
        {
            ConfigStorage.Read();
            return ConfigStorage.LastAU;
        }

        public static void SetLastAU(string lastAU)
        {
            ConfigStorage.LastAU = lastAU;
            ConfigStorage.Write();
        }

        public static ArchType GetArch()
        {
            switch (ConfigStorage.Arch)
            {
                case "x86":
                    return ArchType.x86;
                    break;
                case "x64":
                    return ArchType.x64;
                    break;
            }
            throw new SystemException("ArchError");
        }

        public static void SetArch(ArchType arch)
        {
            switch (arch)
            {
                case ArchType.x86:
                    ConfigStorage.Arch = "x86";
                    break;
                case ArchType.x64:
                    ConfigStorage.Arch = "x64";
                    break;
            }
        }
    
    
    
    }
}