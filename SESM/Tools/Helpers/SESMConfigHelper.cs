using System;
using System.Text;
using System.Web.Security;
using Microsoft.Win32;
using SESM.Models.Views.Settings;

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

        public static bool Lockdown
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.Lockdown;
            }
            set
            {
                ConfigStorage.Lockdown = value;
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

        public static bool StatusAutoRefresh
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.StatusAutoRefresh;
            }
            set
            {
                ConfigStorage.StatusAutoRefresh = value;
                ConfigStorage.Write();
            }
        }

        public static bool PerfMonitor
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.PerfMonitor;
            }
            set
            {
                ConfigStorage.PerfMonitor = value;
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

        public static string AUInterval
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.AUInterval;
            }
            set
            {
                ConfigStorage.AUInterval = value;
                ConfigStorage.Write();
            }
        }

        public static string AUBetaName
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.AUBetaName;
            }
            set
            {
                ConfigStorage.AUBetaName = value;
                ConfigStorage.Write();
            }
        }

        public static string AUBetaPassword
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.AUBetaPassword;
            }
            set
            {
                ConfigStorage.AUBetaPassword = value;
                ConfigStorage.Write();
            }
        }

        public static bool UseSESE
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.UseSESE;
            }
            set
            {
                ConfigStorage.UseSESE = value;
                ConfigStorage.Write();
            }
        }

        public static bool SESEDev
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.SESEDev;
            }
            set
            {
                ConfigStorage.SESEDev = value;
                ConfigStorage.Write();
            }
        }

        public static bool AutoBackupLvl1
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.AutoBackupLvl1;
            }
            set
            {
                ConfigStorage.AutoBackupLvl1 = value;
                ConfigStorage.Write();
            }
        }

        public static string ABIntervalLvl1
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.ABIntervalLvl1;
            }
            set
            {
                ConfigStorage.ABIntervalLvl1 = value;
                ConfigStorage.Write();
            }
        }

        public static int ABNbToKeepLvl1
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.ABNbToKeepLvl1;
            }
            set
            {
                ConfigStorage.ABNbToKeepLvl1 = value;
                ConfigStorage.Write();
            }
        }

        public static bool AutoBackupLvl2
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.AutoBackupLvl2;
            }
            set
            {
                ConfigStorage.AutoBackupLvl2 = value;
                ConfigStorage.Write();
            }
        }

        public static string ABIntervalLvl2
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.ABIntervalLvl2;
            }
            set
            {
                ConfigStorage.ABIntervalLvl2 = value;
                ConfigStorage.Write();
            }
        }

        public static int ABNbToKeepLvl2
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.ABNbToKeepLvl2;
            }
            set
            {
                ConfigStorage.ABNbToKeepLvl2 = value;
                ConfigStorage.Write();
            }
        }

        public static bool AutoBackupLvl3
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.AutoBackupLvl3;
            }
            set
            {
                ConfigStorage.AutoBackupLvl3 = value;
                ConfigStorage.Write();
            }
        }

        public static string ABIntervalLvl3
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.ABIntervalLvl3;
            }
            set
            {
                ConfigStorage.ABIntervalLvl3 = value;
                ConfigStorage.Write();
            }
        }

        public static int ABNbToKeepLvl3
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.ABNbToKeepLvl3;
            }
            set
            {
                ConfigStorage.ABNbToKeepLvl3 = value;
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