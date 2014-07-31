using System.IO;
using System.Web;
using Westwind.Utilities.Configuration;

namespace SESM.Tools
{
    public class SESMConfigStorage : Westwind.Utilities.Configuration.AppConfiguration
    {
        public string DBConnString { get; set; }
        public string Prefix { get; set; }
        public string SESavePath { get; set; }
        public string SEDataPath { get; set; }
        public string Arch { get; set; }
        public bool Diagnosis { get; set; }
        public bool AutoUpdate { get; set; }
        public string AUUsername { get; set; }
        public string AUPassword { get; set; }
        public string LastAU { get; set; }

        public SESMConfigStorage()
        {
            DBConnString = @"Provider=System.Data.SqlClient;Provider Connection String= 'Server=.\SQLEXPRESS;Database=SESM;User Id=sa;Password=MyPassword;MultipleActiveResultSets=true;'";
            Prefix = "SESM";
            SESavePath = @"C:\ProgramData\SpaceEngineersDedicated\";
            SEDataPath = @"C:\SpaceEngineer\";
            Arch = "x64";
            Diagnosis = false;
            AutoUpdate = false;
            AUUsername = "SteamUsername";
            AUPassword = "";
            LastAU = "xxx";

        }
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            var provider = new ConfigurationFileConfigurationProvider<SESMConfigStorage>()
            {
                //ConfigurationFile = HttpContext.Current.Server.MapPath("/") + @"\SESM.config",
                ConfigurationFile = System.AppDomain.CurrentDomain.BaseDirectory + @"\SESM.config",
                ConfigurationSection = sectionName,
            };
            
            return provider;
        }    
    }
}