using System;
using Microsoft.Win32;
using SESM.Models;

namespace SESM.Tools.Helpers
{
    public class SESMConfigHelper
    {
        private static readonly SESMConfigStorage ConfigStorage;

        static SESMConfigHelper()
        {
            ConfigStorage = new SESMConfigStorage();
            ConfigStorage.Initialize();
        }

        // Registry Settings
        public static void InitializeRegistry()
        {
            RegistryKey wow6432Node = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node", true);
            RegistryKey ksh = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\KeenSoftwareHouse", true);
            if (ksh == null)
            {
                wow6432Node.CreateSubKey("KeenSoftwareHouse");
                ksh = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\KeenSoftwareHouse", true);
            }
            RegistryKey seds = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\KeenSoftwareHouse\SpaceEngineersDedicated", true);
            if (seds == null)
            {
                ksh.CreateSubKey("SpaceEngineersDedicated");
                seds = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\KeenSoftwareHouse\SpaceEngineersDedicated", true);
                seds.SetValue("AddDateToLog", "True", RegistryValueKind.String);
                seds.SetValue("SendLogToKeen", "False", RegistryValueKind.String);
            }

            seds = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\KeenSoftwareHouse\MedievalEngineersDedicated", true);
            if (seds == null)
            {
                ksh.CreateSubKey("MedievalEngineersDedicated");
                seds = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\KeenSoftwareHouse\MedievalEngineersDedicated", true);
                seds.SetValue("AddDateToLog", "True", RegistryValueKind.String);
                seds.SetValue("SendLogToKeen", "False", RegistryValueKind.String);
            }
        }

        public static bool SEAddDateToLog
        {
            get
            {
                InitializeRegistry();
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Wow6432Node\KeenSoftwareHouse\SpaceEngineersDedicated");

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
                    @"SOFTWARE\Wow6432Node\KeenSoftwareHouse\SpaceEngineersDedicated", true);

                regKey.SetValue("AddDateToLog", value ? "True" : "False", RegistryValueKind.String);
            }
        }

        public static bool SESendLogToKeen
        {
            get
            {
                InitializeRegistry();
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Wow6432Node\KeenSoftwareHouse\SpaceEngineersDedicated");

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
                    @"SOFTWARE\Wow6432Node\KeenSoftwareHouse\SpaceEngineersDedicated", true);

                regKey.SetValue("SendLogToKeen", value ? "True" : "False", RegistryValueKind.String);
            }
        }

        public static bool MEAddDateToLog
        {
            get
            {
                InitializeRegistry();
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Wow6432Node\KeenSoftwareHouse\MedievalEngineersDedicated");

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
                    @"SOFTWARE\Wow6432Node\KeenSoftwareHouse\MedievalEngineersDedicated", true);

                regKey.SetValue("AddDateToLog", value ? "True" : "False", RegistryValueKind.String);
            }
        }

        public static bool MESendLogToKeen
        {
            get
            {
                InitializeRegistry();
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Wow6432Node\KeenSoftwareHouse\MedievalEngineersDedicated");

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
                    @"SOFTWARE\Wow6432Node\KeenSoftwareHouse\MedievalEngineersDedicated", true);

                regKey.SetValue("SendLogToKeen", value ? "True" : "False", RegistryValueKind.String);
            }
        }

        // Server Settings

        public static string DBConnString
        {
            get
            {
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

        public static string MESavePath
        {
            get
            {
                if (!ConfigStorage.MESavePath.EndsWith(@"\"))
                {
                    ConfigStorage.MESavePath += @"\";
                    ConfigStorage.Write();
                }

                return ConfigStorage.MESavePath;
            }
            set
            {
                ConfigStorage.MESavePath = value;
                ConfigStorage.Write();
            }
        }

        public static string MEDataPath
        {
            get
            {
                if (!ConfigStorage.MEDataPath.EndsWith(@"\"))
                {
                    ConfigStorage.MEDataPath += @"\";
                    ConfigStorage.Write();
                }

                return ConfigStorage.MEDataPath;
            }
            set
            {
                ConfigStorage.MEDataPath = value;
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
                    case "x64":
                        return ArchType.x64;
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

        public static bool PerfMonitorEnabled
        {
            get
            {
                return ConfigStorage.PerfMonitorEnabled;
            }
            set
            {
                ConfigStorage.PerfMonitorEnabled = value;
                ConfigStorage.Write();
            }
        }

        // Running Vars

        public static bool Lockdown
        {
            get
            {
                return SESMRunningVarsStorage.Lockdown;
            }
            set
            {
                SESMRunningVarsStorage.Lockdown = value;
            }
        }

        public static bool SEUpdating
        {
            get
            {
                return SESMRunningVarsStorage.SEUpdating;
            }
            set
            {
                SESMRunningVarsStorage.SEUpdating = value;
            }
        }

        public static bool MEUpdating
        {
            get
            {
                return SESMRunningVarsStorage.MEUpdating;
            }
            set
            {
                SESMRunningVarsStorage.MEUpdating = value;
            }
        }

        public static bool SESEUpdating
        {
            get
            {
                return SESMRunningVarsStorage.SESEUpdating;
            }
            set
            {
                SESMRunningVarsStorage.SESEUpdating = value;
            }
        }

        // SE Auto-Update Settings

        public static bool SEAutoUpdateEnabled
        {
            get
            {
                return ConfigStorage.SEAutoUpdateEnabled;
            }
            set
            {
                ConfigStorage.SEAutoUpdateEnabled = value;
                ConfigStorage.Write();
            }
        }

        public static string SEAutoUpdateCron
        {
            get
            {
                return ConfigStorage.SEAutoUpdateCron;
            }
            set
            {
                ConfigStorage.SEAutoUpdateCron = value;
                ConfigStorage.Write();
            }
        }

        public static string SEAutoUpdateBranch
        {
            get
            {
                return ConfigStorage.SEAutoUpdateBranch;
            }
            set
            {
                ConfigStorage.SEAutoUpdateBranch = value;
                ConfigStorage.Write();
            }
        }

        public static string SEAutoUpdateBetaPassword
        {
            get
            {
                return ConfigStorage.SEAutoUpdateBetaPassword;
            }
            set
            {
                ConfigStorage.SEAutoUpdateBetaPassword = value;
                ConfigStorage.Write();
            }
        }

        // ME Auto-Update Settings

        public static bool MEAutoUpdateEnabled
        {
            get
            {
                return ConfigStorage.MEAutoUpdateEnabled;
            }
            set
            {
                ConfigStorage.MEAutoUpdateEnabled = value;
                ConfigStorage.Write();
            }
        }

        public static string MEAutoUpdateCron
        {
            get
            {
                return ConfigStorage.MEAutoUpdateCron;
            }
            set
            {
                ConfigStorage.MEAutoUpdateCron = value;
                ConfigStorage.Write();
            }
        }

        public static string MEAutoUpdateBranch
        {
            get
            {
                return ConfigStorage.MEAutoUpdateBranch;
            }
            set
            {
                ConfigStorage.MEAutoUpdateBranch = value;
                ConfigStorage.Write();
            }
        }

        public static string MEAutoUpdateBetaPassword
        {
            get
            {
                return ConfigStorage.MEAutoUpdateBetaPassword;
            }
            set
            {
                ConfigStorage.MEAutoUpdateBetaPassword = value;
                ConfigStorage.Write();
            }
        }

        // SESE Auto-Update Settings

        public static string SESEUpdateURL
        {
            get
            {
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
                return ConfigStorage.SESEAutoUpdateCron;
            }
            set
            {
                ConfigStorage.SESEAutoUpdateCron = value;
                ConfigStorage.Write();
            }
        }

        // Backups Settings
        public static bool AutoBackupLvl1Enabled
        {
            get
            {
                return ConfigStorage.AutoBackupLvl1Enabled;
            }
            set
            {
                ConfigStorage.AutoBackupLvl1Enabled = value;
                ConfigStorage.Write();
            }
        }

        public static string AutoBackupLvl1Cron
        {
            get
            {
                return ConfigStorage.AutoBackupLvl1Cron;
            }
            set
            {
                ConfigStorage.AutoBackupLvl1Cron = value;
                ConfigStorage.Write();
            }
        }

        public static int AutoBackupLvl1NbToKeep
        {
            get
            {
                return ConfigStorage.AutoBackupLvl1NbToKeep;
            }
            set
            {
                ConfigStorage.AutoBackupLvl1NbToKeep = value;
                ConfigStorage.Write();
            }
        }

        public static bool AutoBackupLvl2Enabled
        {
            get
            {
                return ConfigStorage.AutoBackupLvl2Enabled;
            }
            set
            {
                ConfigStorage.AutoBackupLvl2Enabled = value;
                ConfigStorage.Write();
            }
        }

        public static string AutoBackupLvl2Cron
        {
            get
            {
                return ConfigStorage.AutoBackupLvl2Cron;
            }
            set
            {
                ConfigStorage.AutoBackupLvl2Cron = value;
                ConfigStorage.Write();
            }
        }

        public static int AutoBackupLvl2NbToKeep
        {
            get
            {
                return ConfigStorage.AutoBackupLvl2NbToKeep;
            }
            set
            {
                ConfigStorage.AutoBackupLvl2NbToKeep = value;
                ConfigStorage.Write();
            }
        }

        public static bool AutoBackupLvl3Enabled
        {
            get
            {
                return ConfigStorage.AutoBackupLvl3Enabled;
            }
            set
            {
                ConfigStorage.AutoBackupLvl3Enabled = value;
                ConfigStorage.Write();
            }
        }

        public static string AutoBackupLvl3Cron
        {
            get
            {
                return ConfigStorage.AutoBackupLvl3Cron;
            }
            set
            {
                ConfigStorage.AutoBackupLvl3Cron = value;
                ConfigStorage.Write();
            }
        }

        public static int AutoBackupLvl3NbToKeep
        {
            get
            {
                return ConfigStorage.AutoBackupLvl3NbToKeep;
            }
            set
            {
                ConfigStorage.AutoBackupLvl3NbToKeep = value;
                ConfigStorage.Write();
            }
        }

        // Misc

        public static bool DiagnosisEnabled
        {
            get
            {
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