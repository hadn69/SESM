using System.Collections.Generic;
using System.Linq;
using Quartz;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.Helpers;

namespace SESM.Tools
{
    public class AutoStartJob : IJob
    {
        public void Execute(IJobExecutionContext jobContext)
        {
            if(!SESMConfigHelper.Lockdown)
            {
                DataContext context = new DataContext();
                ServerProvider srvPrv = new ServerProvider(context);

                List<EntityServer> listServ = srvPrv.GetAllServers()
                    .Where(x => x.IsAutoStartEnabled && srvPrv.GetState(x) != ServiceState.Running)
                    .ToList();
                foreach (EntityServer server in listServ)
                {
                    ServiceHelper.StartService(server);
                }
            }
        }
    }
}