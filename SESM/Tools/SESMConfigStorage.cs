using System;
using Westwind.Utilities.Configuration;

namespace SESM.Tools
{
    public class SESMConfigStorage : AppConfiguration
    {
        // Server Settings
        public string DBConnString { get; set; }
        public string Prefix { get; set; }
        public string SESavePath { get; set; }
        public string MESavePath { get; set; }
        public string SEDataPath { get; set; }
        public string MEDataPath { get; set; }
        public string Arch { get; set; }
        
        // SE Auto-Update Settings
        public bool SEAutoUpdateEnabled { get; set; }
        public string SEAutoUpdateCron { get; set; }
        public string SEAutoUpdateBetaPassword { get; set; }

        // SE Auto-Update Settings
        public bool MEAutoUpdateEnabled { get; set; }
        public string MEAutoUpdateCron { get; set; }
        public string MEAutoUpdateBetaPassword { get; set; }

        // SESE Auto-Update Settings
        public string SESEUpdateURL { get; set; }
        public bool SESEAutoUpdateEnabled { get; set; }
        public bool SESEAutoUpdateUseDev { get; set; }
        public string SESEAutoUpdateCron { get; set; }

        // Backups Settings
        public bool AutoBackupLvl1Enabled { get; set; }
        public string AutoBackupLvl1Cron { get; set; }
        public int AutoBackupLvl1NbToKeep { get; set; }
        public bool AutoBackupLvl2Enabled { get; set; }
        public string AutoBackupLvl2Cron { get; set; }
        public int AutoBackupLvl2NbToKeep { get; set; }
        public bool AutoBackupLvl3Enabled { get; set; }
        public string AutoBackupLvl3Cron { get; set; }
        public int AutoBackupLvl3NbToKeep { get; set; }

        // Misc
        public bool DiagnosisEnabled { get; set; }
        public bool BlockDll { get; set; }
        public bool LowPriorityStart { get; set; }
        public bool PerfMonitorEnabled { get; set; }

        public SESMConfigStorage()
        {
            // Server Settings
            DBConnString = @"Server=.\SQLEXPRESS;Database=SESM;User Id=sa;Password=MyPassword;MultipleActiveResultSets=true;";
            Prefix = "SESM";
            SESavePath = @"C:\ProgramData\SpaceEngineersDedicated\";
            MESavePath = @"C:\ProgramData\MedievalEngineersDedicated\";
            SEDataPath = @"C:\SpaceEngineers\";
            MEDataPath = @"C:\MedievalEngineers\";
            Arch = "x64";
            
            // SE Auto-Update Settings
            SEAutoUpdateEnabled = true;
            SEAutoUpdateCron = "0 0/10 * * * ?";
            SEAutoUpdateBetaPassword = string.Empty;

            // ME Auto-Update Settings
            MEAutoUpdateEnabled = false;
            MEAutoUpdateCron = "0 5/10 * * * ?";
            MEAutoUpdateBetaPassword = string.Empty;

            // SESE Auto-Update Settings
            SESEUpdateURL = "https://api.github.com/repos/SEServerExtender/SEServerExtender/releases";
            SESEAutoUpdateEnabled = false;
            SESEAutoUpdateUseDev = true;
            SESEAutoUpdateCron = "0 5/10 * * * ?";

            // Backups Settings
            AutoBackupLvl1Enabled = false;
            AutoBackupLvl1Cron = "0 0/10 * * * ?";
            AutoBackupLvl1NbToKeep = 6;

            AutoBackupLvl2Enabled = false;
            AutoBackupLvl2Cron = "0 0 * * * ?";
            AutoBackupLvl2NbToKeep = 24;

            AutoBackupLvl3Enabled = false;
            AutoBackupLvl3Cron = "0 0 0 * * ?";
            AutoBackupLvl3NbToKeep = 14;

            // Misc
            DiagnosisEnabled = false;
            BlockDll = true;
            LowPriorityStart = false;
            PerfMonitorEnabled = true;
        }
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            ConfigurationFileConfigurationProvider<SESMConfigStorage> provider = new ConfigurationFileConfigurationProvider<SESMConfigStorage>()
            {
                ConfigurationFile = AppDomain.CurrentDomain.BaseDirectory + @"\SESM.config",
                ConfigurationSection = sectionName,
            };

            return provider;
        }
    }
}