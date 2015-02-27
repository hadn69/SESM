using Quartz;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.Helpers;

namespace SESM.Tools
{
    public class ResetPriorityJob : IJob
    {
        public void Execute(IJobExecutionContext jobContext)
        {
            JobDataMap dataMap = jobContext.JobDetail.JobDataMap;
            int serverId = dataMap.GetInt("id");

            DataContext context = new DataContext();
            ServerProvider srvPrv = new ServerProvider(context);

            EntityServer server = srvPrv.GetServer(serverId);

            ServiceHelper.SetPriority(server);
        }
    }
}