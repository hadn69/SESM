using NLog;
using Quartz;
using SESM.Tools.Helpers;

namespace SESM.Tools
{
    public class AutoUpdate :IJob
    {
        private static Logger _logger = LogManager.GetLogger("AutoUpdateLogger");
        public void Execute(IJobExecutionContext jobContext)
        {
            // Checking auto update is enable
            if (!SESMConfigHelper.AutoUpdate)
                return;

            _logger.Info("----Starting AutoUpdateJob----");

            SteamCMDHelper.Update(_logger, 200);
            
            _logger.Info("----End of AutoUpdateJob----");
        }
    }
}