using Westwind.Utilities.Configuration;

namespace SESM.Tools
{
    public class SESMConfigStorage : AppConfiguration
    {
        public string DBConnString { get; set; }
        public string Prefix { get; set; }
        public string SESavePath { get; set; }
        public string SEDataPath { get; set; }
        public string Arch { get; set; }
        public bool Lockdown { get; set; }

        public bool Diagnosis { get; set; }
        public bool StatusAutoRefresh { get; set; }
        public bool PerfMonitor { get; set; }

        public bool AutoUpdate { get; set; }
        public string AUUsername { get; set; }
        public string AUPassword { get; set; }
        public string AUInterval { get; set; }
        public string AUBetaName { get; set; }
        public string AUBetaPassword { get; set; }

        public int SESEDelay { get; set; }
        public bool UseSESE { get; set; }
        public bool SESEDev { get; set; }

        public bool AutoBackupLvl1 { get; set; }
        public string ABIntervalLvl1 { get; set; }
        public int ABNbToKeepLvl1 { get; set; }

        public bool AutoBackupLvl2 { get; set; }
        public string ABIntervalLvl2 { get; set; }
        public int ABNbToKeepLvl2 { get; set; }
        
        public bool AutoBackupLvl3 { get; set; }
        public string ABIntervalLvl3 { get; set; }
        public int ABNbToKeepLvl3 { get; set; }

        public bool BlockDll { get; set; }

        public SESMConfigStorage()
        {
            DBConnString = @"Server=.\SQLEXPRESS;Database=SESM;User Id=sa;Password=MyPassword;MultipleActiveResultSets=true;";
            Prefix = "SESM";
            SESavePath = @"C:\ProgramData\SpaceEngineersDedicated\";
            SEDataPath = @"C:\SpaceEngineers\";
            Arch = "x64";
            Lockdown = false;

            Diagnosis = false;
            StatusAutoRefresh = true;
            PerfMonitor = false;

            AutoUpdate = false;
            AUUsername = "SteamUsername";
            AUPassword = "";
            AUInterval = "0 0/10 * * * ?";
            AUBetaName = "";
            AUBetaPassword = "";

            SESEDelay = 8;
            UseSESE = false;
            SESEDev = true;

            AutoBackupLvl1 = false;
            ABIntervalLvl1 = "0 0/10 * * * ?";
            ABNbToKeepLvl1 = 12;

            AutoBackupLvl2 = false;
            ABIntervalLvl2 = "0 0 * * * ?";
            ABNbToKeepLvl2 = 48;

            AutoBackupLvl3 = false;
            ABIntervalLvl3 = "0 0 0 * * ?";
            ABNbToKeepLvl3 = 30;

            BlockDll = true;
        }   
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            ConfigurationFileConfigurationProvider<SESMConfigStorage> provider = new ConfigurationFileConfigurationProvider<SESMConfigStorage>()
            {
                ConfigurationFile = System.AppDomain.CurrentDomain.BaseDirectory + @"\SESM.config",
                ConfigurationSection = sectionName,
            };
            
            return provider;
        }    
    }
}