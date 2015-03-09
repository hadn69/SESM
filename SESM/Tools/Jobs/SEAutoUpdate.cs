using Quartz;

namespace SESM.Tools.Jobs
{
    public class SEAutoUpdate
    {

        public static JobKey GetJobKey()
        {
            return new JobKey("SESEAutoUpdate", "SESEAutoUpdate");
        }

        public static TriggerKey GetTriggerKey()
        {
            return new TriggerKey("SESEAutoUpdate", "SESEAutoUpdate");
        }
    }
}