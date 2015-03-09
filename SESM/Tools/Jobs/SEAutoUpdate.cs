using NLog;
using Quartz;

namespace SESM.Tools.Jobs
{
    [DisallowConcurrentExecution]
    public class SEAutoUpdate : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Logger logger = LogManager.GetLogger("SEAutoUpdateLogger");
            Run(logger, false, false);
        }


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