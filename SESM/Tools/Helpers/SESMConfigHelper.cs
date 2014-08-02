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

        public static bool AddDateToLog
        {
            get
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
            set
            {
                InitializeRegistry();
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Wow6432Node\KeenSoftwareHouse\SpaceEngineersDedicatedServer", true);

                regKey.SetValue("AddDateToLog", value ? "True" : "False", RegistryValueKind.String);
            }
        }

        public static bool SendLogToKeen
        {
            get
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
            set
            {
                InitializeRegistry();
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Wow6432Node\KeenSoftwareHouse\SpaceEngineersDedicatedServer", true);

                regKey.SetValue("SendLogToKeen", value ? "True" : "False", RegistryValueKind.String);
            }
        }

        public static string DBConnString
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.DBConnString;
            }
            set
            {
                ConfigStorage.DBConnString = value;
                ConfigStorage.Write();        
            }

        }

        public static string Prefix
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.Prefix;        
            }
            set
            {
                ConfigStorage.Prefix = value;
                ConfigStorage.Write();
            }
        }

        public static string SESavePath
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.SESavePath;                
            }
            set
            {
                ConfigStorage.SESavePath = value;
                ConfigStorage.Write();
            }
        }

        public static string SEDataPath
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.SEDataPath;                
            }
            set
            {
                ConfigStorage.SEDataPath = value;
                ConfigStorage.Write();
            }
        }

        public static bool Diagnosis
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.Diagnosis;
            }
            set
            {
                ConfigStorage.Diagnosis = value;
                ConfigStorage.Write();
            }
        }

        public static bool AutoUpdate
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.AutoUpdate;
            }
            set
            {
                ConfigStorage.AutoUpdate = value;
                ConfigStorage.Write();        
            }
        }

        public static string AUUsername
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.AUUsername;                
            }
            set
            {
                ConfigStorage.AUUsername = value;
                ConfigStorage.Write();        
            }
        }

        public static string AUPassword
        {
            get
            {
                ConfigStorage.Read();
                string value = ConfigStorage.AUPassword;
                return Encoding.UTF8.GetString(MachineKey.Unprotect(Convert.FromBase64String(value), "SteamPassword"));                
            }
            set
            {
                string val = Convert.ToBase64String(MachineKey.Protect(Encoding.UTF8.GetBytes(value), "SteamPassword"));
                ConfigStorage.AUPassword = val;
                ConfigStorage.Write();            
            }
        }

        public static string LastAU
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.LastAU;
            }
            set
            {
                ConfigStorage.LastAU = value;
                ConfigStorage.Write();
            }
        }

        public static ArchType Arch
        {
            get
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
            set
            {
                switch (value)
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
}