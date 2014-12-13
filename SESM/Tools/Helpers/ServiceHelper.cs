using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using NLog;
using Quartz;
using Quartz.Impl;
using SESM.DTO;
using SESM.Models.Views.Settings;

namespace SESM.Tools.Helpers
{
    public class ServiceHelper
    {

        public static string GetServiceName(EntityServer server)
        {
            return GetServiceName(PathHelper.GetPrefix(), server);
        }

        public static string GetServiceName(string prefix, EntityServer server)
        {
            return prefix + "_" + server.Id + "_" + server.Name;
        }

        public static void StopService(EntityServer server)
        {
            string serviceName = GetServiceName(server);
            try
            {
                ServiceController svcController = new ServiceController(serviceName);
                if(svcController.Status == ServiceControllerStatus.Running)
                {
                    svcController.Stop();
                    IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                    scheduler.DeleteJob(new JobKey("LowPriorityStart" + server.Id + "Job", "LowPriorityStart"));
                }

            }
            catch(Exception ex)
            {
                return;
            }
        }

        public static void StopServiceAndWait(EntityServer server)
        {
            string serviceName = GetServiceName(server);
            try
            {
                ServiceController svcController = new ServiceController(serviceName);

                if(svcController.Status == ServiceControllerStatus.Running)
                {
                    StopService(server);
                    WaitForStopped(server);
                }
            }
            catch(Exception ex)
            {
                return;
            }
        }

        public static void WaitForStopped(EntityServer server)
        {
            string serviceName = GetServiceName(server);
            try
            {
                ServiceController svcController = new ServiceController(serviceName);
                svcController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 10));
            }
            catch(Exception ex)
            {
                return;
            }
        }

        public static void StartService(EntityServer server)
        {
            string serviceName = GetServiceName(server);
            try
            {
                ServiceController svcController = new ServiceController(serviceName);
                if(svcController.Status != ServiceControllerStatus.Running)
                {
                    if(!server.UseServerExtender)
                    {
                        svcController.Start();
                    }
                    else
                    {
                        string[] argsStr = new string[6];
                        argsStr[0] = "nogui";
                        argsStr[1] = "noconsole";
                        argsStr[2] = "wcfport=" + server.ServerExtenderPort;
                        argsStr[3] = "autosave=" + server.AutoSaveInMinutes;
                        argsStr[4] = "instance=" + serviceName;
                        argsStr[5] = "gamepath=" + SESMConfigHelper.SEDataPath;
                        svcController.Start(argsStr);
                    }

                    if(SESMConfigHelper.LowPriorityStart)
                    {
                        SetBelowNormalPriority(serviceName);
                        IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                        scheduler.DeleteJob(new JobKey("LowPriorityStart" + server.Id + "Job", "LowPriorityStart"));
                        IJobDetail lowPriorityStartJob = JobBuilder.Create<ResetPriorityJob>()
                            .WithIdentity("LowPriorityStart" + server.Id + "Job", "LowPriorityStart")
                            .UsingJobData("id", server.Id)
                            .Build();

                        ITrigger lowPriorityStartTrigger = TriggerBuilder.Create()
                            .WithIdentity("LowPriorityStart" + server.Id + "Trigger", "LowPriorityStart")
                            .StartAt(DateBuilder.FutureDate(3, IntervalUnit.Minute))
                            .Build();

                        scheduler.ScheduleJob(lowPriorityStartJob, lowPriorityStartTrigger);
                    }
                }
            }
            catch(Exception ex)
            {
                return;
            }
        }

        public static void RestartService(EntityServer server)
        {
            string serviceName = GetServiceName(server);
            try
            {
                ServiceController svcController = new ServiceController(serviceName);
                if(svcController.Status == ServiceControllerStatus.Running)
                {
                    StopService(server);
                    WaitForStopped(server);
                    StartService(server);
                }
            }
            catch(Exception ex)
            {
                return;
            }
        }

        public static void SetBelowNormalPriority(string serviceName)
        {
            try
            {
                if(DoesServiceExist(serviceName))
                {
                    uint? pid = GetServicePID(serviceName);
                    if(pid == null)
                        return;

                    Process process = Process.GetProcessById((int)pid);
                    process.PriorityClass = ProcessPriorityClass.BelowNormal;
                }
            }
            catch(Exception)
            {

            }
        }

        public static void SetNormalPriority(string serviceName)
        {
            try
            {
                if(DoesServiceExist(serviceName))
                {
                    uint? pid = GetServicePID(serviceName);
                    if(pid == null)
                        return;

                    Process process = Process.GetProcessById((int)pid);
                    process.PriorityClass = ProcessPriorityClass.Normal;
                }
            }
            catch(Exception)
            {

            }
        }

        public static void RegisterService(string serviceName)
        {
            return;
            if(DoesServiceExist(serviceName))
                return;

            string dataPath = SESMConfigHelper.SEDataPath;
            if(SESMConfigHelper.Arch == ArchType.x86)
                dataPath += @"DedicatedServer\SpaceEngineersDedicated.exe";
            if(SESMConfigHelper.Arch == ArchType.x64)
                dataPath += @"DedicatedServer64\SpaceEngineersDedicated.exe";

            Process si = new Process();
            si.StartInfo.WorkingDirectory = @"c:\";
            si.StartInfo.UseShellExecute = false;
            si.StartInfo.FileName = "cmd.exe";
            si.StartInfo.Arguments = "/c \"sc create " + serviceName + " start= auto binPath= \\\"" + dataPath + "\\\"\"";
            si.StartInfo.CreateNoWindow = true;
            si.StartInfo.RedirectStandardInput = true;
            si.StartInfo.RedirectStandardOutput = true;
            si.StartInfo.RedirectStandardError = true;
            si.Start();
            string output = si.StandardOutput.ReadToEnd();
            si.Close();
        }

        public static void UnRegisterService(string serviceName)
        {
            if(DoesServiceExist(serviceName))
            {
                Process si = new Process();
                si.StartInfo.WorkingDirectory = @"c:\";
                si.StartInfo.UseShellExecute = false;
                si.StartInfo.FileName = "cmd.exe";
                si.StartInfo.Arguments = "/c \"sc delete " + serviceName + "\"";
                si.StartInfo.CreateNoWindow = true;
                si.StartInfo.RedirectStandardInput = true;
                si.StartInfo.RedirectStandardOutput = true;
                si.StartInfo.RedirectStandardError = true;
                si.Start();
                string output = si.StandardOutput.ReadToEnd();
                si.Close();
            }
        }

        public static void RegisterServerExtenderService(EntityServer server)
        {
            return;
            if(DoesServiceExist(server))
                return;

            string dataPath = SESMConfigHelper.SEDataPath + @"DedicatedServer64\SEServerExtender.exe";

            Process si = new Process();
            si.StartInfo.WorkingDirectory = @"c:\";
            si.StartInfo.UseShellExecute = false;
            si.StartInfo.FileName = "cmd.exe";
            si.StartInfo.Arguments = "/c \"sc create " + GetServiceName(server) + " start= auto binPath= ^\"" + dataPath
                + " ^\" \"";
            si.StartInfo.CreateNoWindow = true;
            si.StartInfo.RedirectStandardInput = true;
            si.StartInfo.RedirectStandardOutput = true;
            si.StartInfo.RedirectStandardError = true;
            si.Start();
            string output = si.StandardOutput.ReadToEnd();
            si.Close();
        }

        public static void KillService(EntityServer server)
        {
            KillService(GetServiceName(server));
        }

        // Source : http://stackoverflow.com/a/566089
        public static void KillService(string serviceName)
        {
            if(DoesServiceExist(serviceName))
            {
                uint? pid = GetServicePID(serviceName);
                if(pid == null)
                    return;

                Process process = Process.GetProcessById((int)pid);

                try
                {
                    process.Kill();
                }
                catch(Win32Exception)
                {
                    // Thrown if process is already terminating,
                    // the process is a Win16 exe or the process
                    // could not be terminated.
                }
                catch(InvalidOperationException)
                {
                    // Thrown if the process has already terminated.
                }

            }
        }

        public static void KillAllService()
        {
            foreach(Process proc in Process.GetProcessesByName("SpaceEngineersDedicated"))
            {
                try
                {
                    proc.Kill();
                }
                catch(Exception)
                {

                }

            }
            KillAllSESEService();
        }
        public static void KillAllSESEService()
        {
            foreach(Process proc in Process.GetProcessesByName("SEServerExtender"))
            {
                try
                {
                    proc.Kill();
                }
                catch(Exception)
                {

                }

            }
        }
        public static uint? GetServicePID(string serviceName)
        {
            if(!DoesServiceExist(serviceName))
                return null;

            Process si = new Process();
            si.StartInfo.WorkingDirectory = @"c:\";
            si.StartInfo.UseShellExecute = false;
            si.StartInfo.FileName = "cmd.exe";
            si.StartInfo.Arguments = "/c \"sc queryex " + serviceName + "\"";
            si.StartInfo.CreateNoWindow = true;
            si.StartInfo.RedirectStandardInput = true;
            si.StartInfo.RedirectStandardOutput = true;
            si.StartInfo.RedirectStandardError = true;
            si.Start();
            string output = si.StandardOutput.ReadToEnd();
            si.Close();
            string[] outputSplitted = output.Replace("\r", "").Split('\n');
            string outputLine = string.Empty;
            foreach(string item in outputSplitted.Where(item => item.Contains("PID")))
            {
                outputLine = item;
            }
            string pid = outputLine.Substring(28).Trim();
            uint processId = uint.Parse(pid);
            if(processId == 0)
                return null;
            else
                return processId;
        }

        public static Ressources? GetCurrentRessourceUsage(string serviceName)
        {
            uint? pid = GetServicePID(serviceName);
            Ressources ressources = new Ressources();
            if(pid == null || pid == 0)
            {
                ressources.CPU = 0;
                ressources.Memory = 0;
                return ressources;
            }

            try
            {
                SelectQuery query = new SelectQuery("select PercentProcessorTime, WorkingSet " +
                                                  "from Win32_PerfFormattedData_PerfProc_Process " +
                                                  "where IDProcess = " + pid);

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                ManagementObjectCollection collection = searcher.Get();
                IEnumerator enumerator = collection.GetEnumerator();
                enumerator.MoveNext();
                ManagementObject managementObject = (ManagementObject)enumerator.Current;
                ressources.CPU = int.Parse(managementObject["PercentProcessorTime"].ToString()) / Environment.ProcessorCount;
                ressources.Memory = (int)Math.Floor(long.Parse(managementObject["WorkingSet"].ToString()) / (1024.0 * 2));
            }
            catch(Exception ex)
            {
                Logger exceptionLogger = LogManager.GetLogger("GenericExceptionLogger");
                exceptionLogger.Fatal("Caught Exception in GetCurrentRessourceUsage :", ex);

            }
            return ressources;
        }

        public static bool DoesServiceExist(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            var service = services.FirstOrDefault(s => s.ServiceName == serviceName);
            return service != null;
        }

        public static bool DoesServiceExist(EntityServer server)
        {
            return DoesServiceExist(GetServiceName(server));
        }

        public struct Ressources
        {
            public int CPU;
            public int Memory;
        }
    }

    public enum ServiceState
    {
        Running,
        Starting,
        Stopping,
        Stopped,
        Unknow
    }
}