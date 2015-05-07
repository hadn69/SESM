using System;
using Westwind.Utilities.Configuration;

namespace SESM.Tools
{
    public class SESMRunningVarsStorage : AppConfiguration
    {
        public bool Lockdown;
        public bool SEUpdating;
        public bool MEUpdating;
        public bool SESEUpdating;
        public SESMRunningVarsStorage()
        {
            Lockdown = false;
            SEUpdating = false;
            MEUpdating = false;
            SESEUpdating = false;

        }
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            ConfigurationFileConfigurationProvider<SESMRunningVarsStorage> provider = new ConfigurationFileConfigurationProvider<SESMRunningVarsStorage>()
            {
                ConfigurationFile = AppDomain.CurrentDomain.BaseDirectory + @"\SESM.RunVar",
                ConfigurationSection = sectionName,
            };

            return provider;
        }
    }
}