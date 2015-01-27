using Westwind.Utilities.Configuration;

namespace SESM.Tools
{
    public class SESMConfigStorage : AppConfiguration
    {
        // Server Settings
        public string DBConnString { get; set; }
        public string Prefix { get; set; }
        public string SESavePath { get; set; }
        public string SEDataPath { get; set; }
        public string Arch { get; set; }

        // Auto-Update Settings
        public bool AutoUpdateEnabled { get; set; }
        public string AutoUpdateCron { get; set; }
        public string AutoUpdateBetaPassword { get; set; }

        // SESE Auto-Update Settings
        public string SESEUpdateURL { get; set; }
        public bool SESEAutoUpdateEnabled { get; set; }
        public bool SESEAutoUpdateUseDev { get; set; }
        public string SESEAutoUpdateCron { get; set; }

        // Misc
        public bool DiagnosisEnabled { get; set; }
        public bool BlockDll { get; set; }
        public bool LowPriorityStart { get; set; }

        public SESMConfigStorage()
        {
            // Server Settings
            DBConnString = @"Server=.\SQLEXPRESS;Database=SESM;User Id=sa;Password=MyPassword;MultipleActiveResultSets=true;";
            Prefix = "SESM";
            SESavePath = @"C:\ProgramData\SpaceEngineersDedicated\";
            SEDataPath = @"C:\SpaceEngineers\";
            Arch = "x64";

            // Auto-Update Settings
            AutoUpdateEnabled = true;
            AutoUpdateCron = "0 0/10 * * * ?";
            AutoUpdateBetaPassword = string.Empty;

            // SESEAuto-Update Settings
            SESEUpdateURL = "https://api.github.com/repos/Tyrsis/SE-Community-Mod-API/releases";
            SESEAutoUpdateEnabled = false;
            SESEAutoUpdateUseDev = true;
            SESEAutoUpdateCron = "0 5/10 * * * ?";

            // Misc
            DiagnosisEnabled = false;
            BlockDll = true;
            LowPriorityStart = false;
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