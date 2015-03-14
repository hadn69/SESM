using System;
using Microsoft.Win32;
using SESM.Models.Views.Settings;

namespace SESM.Tools.Helpers
{
    public class SESMConfigHelper
    {
        private static readonly SESMConfigStorage ConfigStorage;
        private static readonly SESMRunningVarsStorage RunningVars;

        static SESMConfigHelper()
        {
            ConfigStorage = new SESMConfigStorage();
            ConfigStorage.Initialize();

            RunningVars = new SESMRunningVarsStorage();
            RunningVars.Initialize();
        }

        // Registry Settings
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

        // Server Settings

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

                if (!ConfigStorage.SESavePath.EndsWith(@"\"))
                {
                    ConfigStorage.SESavePath += @"\";
                    ConfigStorage.Write();
                }

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

                if (!ConfigStorage.SEDataPath.EndsWith(@"\"))
                {
                    ConfigStorage.SEDataPath += @"\";
                    ConfigStorage.Write();
                }

                return ConfigStorage.SEDataPath;                
            }
            set
            {
                ConfigStorage.SEDataPath = value;
                ConfigStorage.Write();
            }
        }

        public static ArchType Arch
        {
            get
            {
                switch(ConfigStorage.Arch)
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
                switch(value)
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

        // Running Vars

        public static bool Lockdown
        {
            get
            {
                RunningVars.Read();
                return RunningVars.Lockdown;
            }
            set
            {
                RunningVars.Lockdown = value;
                RunningVars.Write();
            }
        }

        public static bool SEUpdating
        {
            get
            {
                RunningVars.Read();
                return RunningVars.SEUpdating;
            }
            set
            {
                RunningVars.SEUpdating = value;
                RunningVars.Write();
            }
        }

        public static bool SESEUpdating
        {
            get
            {
                RunningVars.Read();
                return RunningVars.SESEUpdating;
            }
            set
            {
                RunningVars.SESEUpdating = value;
                RunningVars.Write();
            }
        }

        // Auto-Update Settings

        public static bool AutoUpdateEnabled
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.AutoUpdateEnabled;
            }
            set
            {
                ConfigStorage.AutoUpdateEnabled = value;
                ConfigStorage.Write();
            }
        }

        public static string AutoUpdateCron
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.AutoUpdateCron;
            }
            set
            {
                ConfigStorage.AutoUpdateCron = value;
                ConfigStorage.Write();
            }
        }

        public static string AutoUpdateBetaPassword
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.AutoUpdateBetaPassword;
            }
            set
            {
                ConfigStorage.AutoUpdateBetaPassword = value;
                ConfigStorage.Write();
            }
        }

        // SESE Auto-Update Settings

        public static string SESEUpdateURL
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.SESEUpdateURL;
            }
            set
            {
                ConfigStorage.SESEUpdateURL = value;
                ConfigStorage.Write();
            }
        }

        public static bool SESEAutoUpdateEnabled
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.SESEAutoUpdateEnabled;
            }
            set
            {
                ConfigStorage.SESEAutoUpdateEnabled = value;
                ConfigStorage.Write();
            }
        }

        public static bool SESEAutoUpdateUseDev
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.SESEAutoUpdateUseDev;
            }
            set
            {
                ConfigStorage.SESEAutoUpdateUseDev = value;
                ConfigStorage.Write();
            }
        }

        public static string SESEAutoUpdateCron
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.SESEAutoUpdateCron;
            }
            set
            {
                ConfigStorage.SESEAutoUpdateCron = value;
                ConfigStorage.Write();
            }
        }

        // Misc

        public static bool DiagnosisEnabled
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.DiagnosisEnabled;
            }
            set
            {
                ConfigStorage.DiagnosisEnabled = value;
                ConfigStorage.Write();
            }
        }

        public static bool BlockDll
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.BlockDll;
            }
            set
            {
                ConfigStorage.BlockDll = value;
                ConfigStorage.Write();
            }
        }

        public static bool LowPriorityStart
        {
            get
            {
                ConfigStorage.Read();
                return ConfigStorage.LowPriorityStart;
            }
            set
            {
                ConfigStorage.LowPriorityStart = value;
                ConfigStorage.Write();
            }
        }        
    }
}