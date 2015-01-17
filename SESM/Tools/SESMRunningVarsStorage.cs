using System;
using Westwind.Utilities.Configuration;

namespace SESM.Tools
{
    public class SESMRunningVarsStorage : AppConfiguration
    {
        public bool Lockdown;
        public SESMRunningVarsStorage()
        {
            Lockdown = false;

        }
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            ConfigurationFileConfigurationProvider<SESMRunningVarsStorage> provider = new ConfigurationFileConfigurationProvider<SESMRunningVarsStorage>()
            {
                ConfigurationFile = System.AppDomain.CurrentDomain.BaseDirectory + @"\SESM.RunVar",
                ConfigurationSection = sectionName,
            };

            return provider;
        }
    }
}