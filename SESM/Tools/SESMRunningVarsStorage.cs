using System;
using Westwind.Utilities.Configuration;

namespace SESM.Tools
{
    public static class SESMRunningVarsStorage
    {
        public static bool Lockdown;
        public static bool SEUpdating;
        public static bool MEUpdating;
        public static bool SESEUpdating;
        /*
        public SESMRunningVarsStorage()
        {
            Lockdown = false;
            SEUpdating = false;
            MEUpdating = false;
            SESEUpdating = false;

        }
        */
        static SESMRunningVarsStorage()
        {
            Lockdown = false;
            SEUpdating = false;
            MEUpdating = false;
            SESEUpdating = false;
        }
        /*
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            ConfigurationFileConfigurationProvider<SESMRunningVarsStorage> provider = new ConfigurationFileConfigurationProvider<SESMRunningVarsStorage>()
            {
                ConfigurationFile = AppDomain.CurrentDomain.BaseDirectory + @"\SESM.RunVar",
                ConfigurationSection = sectionName,
            };

            return provider;
        }
        */
    }
}