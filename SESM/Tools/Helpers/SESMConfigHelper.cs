using System;
using System.Configuration;
using System.Web.Configuration;
using Microsoft.Win32;
using SESM.Models.Views.Settings;

namespace SESM.Tools.Helpers
{
    public class SESMConfigHelper
    {
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

        public static string GetPrefix()
        {
            Configuration conf = WebConfigurationManager.OpenWebConfiguration("/web");
            return conf.AppSettings.Settings["Prefix"].Value;
        }

        public static void SetPrefix(string prefix)
        {
            Configuration conf = WebConfigurationManager.OpenWebConfiguration("/web");
            conf.AppSettings.Settings["Prefix"].Value = prefix;
        }

        public static string GetSESavePath()
        {
            Configuration conf = WebConfigurationManager.OpenWebConfiguration("/web");
            return conf.AppSettings.Settings["SESavePath"].Value;
        }

        public static void SetSESavePath(string SESavePath)
        {
            Configuration conf = WebConfigurationManager.OpenWebConfiguration("/web");
            conf.AppSettings.Settings["SESavePath"].Value = SESavePath;
        }

        public static string GetSEDataPath()
        {
            Configuration conf = WebConfigurationManager.OpenWebConfiguration("/web");
            return conf.AppSettings.Settings["SEDataPath"].Value;
        }

        public static void SetSEDataPath(string sEDataPath)
        {
            Configuration conf = WebConfigurationManager.OpenWebConfiguration("/web");
            conf.AppSettings.Settings["SEDataPath"].Value = sEDataPath;
        }

        public static EnumArchType GetArch()
        {
            Configuration conf = WebConfigurationManager.OpenWebConfiguration("/web");
            switch (conf.AppSettings.Settings["Arch"].Value)
            {
                case "x86":
                    return EnumArchType.X86;
                    break;
                case "x64":
                    return EnumArchType.X64;
                    break;
            }
            throw new SystemException("ArchError");
        }

        public static void SetArch(EnumArchType enumArch)
        {
            Configuration conf = WebConfigurationManager.OpenWebConfiguration("/web");
            switch (enumArch)
            {
                case EnumArchType.X86:
                    conf.AppSettings.Settings["Arch"].Value = "x86";
                    break;
                case EnumArchType.X64:
                    conf.AppSettings.Settings["Arch"].Value = "x64";
                    break;
            }
        }
    }
}