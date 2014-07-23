using System.Web.Mvc;
using System.Web.Routing;
using Quartz;
using Quartz.Impl;
using SESM.Tools.Monitor;

namespace SESM
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail collectorJob = JobBuilder.Create<Collector>()
                .WithIdentity("CollectorJob", "Monitor")
                .Build();

            ITrigger collectorTrigger = TriggerBuilder.Create()
                .WithIdentity("CollectorTrigger", "Monitor")
                .WithCronSchedule("0 * * * * ?")
                .StartNow()
                .Build();

            scheduler.ScheduleJob(collectorJob, collectorTrigger);

            IJobDetail hourlyCrusherJob = JobBuilder.Create<HourlyCrusher>()
                .WithIdentity("HourlyCrusherJob", "Monitor")
                .Build();

            ITrigger hourlyCrusherTrigger = TriggerBuilder.Create()
                .WithIdentity("HourlyCrusherTrigger", "Monitor")
                .WithCronSchedule("0 0 * * * ?")
                .StartNow()
                .Build();

            scheduler.ScheduleJob(hourlyCrusherJob, hourlyCrusherTrigger);
        }
    }
}
