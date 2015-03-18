using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using NLog;
using Quartz;
using Quartz.Impl;
using SESM.DTO;
using SESM.Models;
using SESM.Tools.Jobs;

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
                svcController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 30));
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
                        SetLowPriority(serviceName);
                        IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                        scheduler.DeleteJob(ResetPriorityJob.GetJobKey(server));
                        IJobDetail lowPriorityStartJob = JobBuilder.Create<ResetPriorityJob>()
                            .WithIdentity(ResetPriorityJob.GetJobKey(server))
                            .UsingJobData("id", server.Id)
                            .Build();

                        ITrigger lowPriorityStartTrigger = TriggerBuilder.Create()
                            .WithIdentity(ResetPriorityJob.GetTriggerKey(server))
                            .StartAt(DateBuilder.FutureDate(3, IntervalUnit.Minute))
                            .Build();

                        scheduler.ScheduleJob(lowPriorityStartJob, lowPriorityStartTrigger);
                    }
                    else
                    {
                        SetPriority(server);
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

        public static void SetLowPriority(string serviceName)
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

        public static void SetHighPriority(string serviceName)
        {
            try
            {
                if(DoesServiceExist(serviceName))
                {
                    uint? pid = GetServicePID(serviceName);
                    if(pid == null)
                        return;

                    Process process = Process.GetProcessById((int)pid);
                    process.PriorityClass = ProcessPriorityClass.High;
                }
            }
            catch(Exception)
            {

            }
        }

        public static void SetPriority(EntityServer server)
        {
            string serviceName = GetServiceName(server);

            switch (server.ProcessPriority)
            {
                case EnumProcessPriority.Low :
                    SetLowPriority(serviceName);
                    break;
                case EnumProcessPriority.Normal:
                    SetNormalPriority(serviceName);
                    break;
                case EnumProcessPriority.High:
                    SetHighPriority(serviceName);
                    break;
            }
        }

        public static void RegisterService(EntityServer server)
        {
            if(DoesServiceExist(server))
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
            si.StartInfo.Arguments = "/c \"sc create " + GetServiceName(server) + " start= auto binPath= \\\"" + dataPath + "\\\"\"";
            si.StartInfo.CreateNoWindow = true;
            si.StartInfo.RedirectStandardInput = true;
            si.StartInfo.RedirectStandardOutput = true;
            si.StartInfo.RedirectStandardError = true;
            si.Start();
            string output = si.StandardOutput.ReadToEnd();
            si.Close();
        }

        public static void RegisterServerExtenderService(EntityServer server)
        {
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

        public static void UnRegisterService(EntityServer server)
        {
            UnRegisterService(GetServiceName(server));
            return;
        }

        public static void UnRegisterService(string serviceName)
        {
            if (DoesServiceExist(serviceName))
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


        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        private static void KillProcessAndChildren(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach(ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch(ArgumentException)
            {
                // Process already exited.
            }
        }

        public static void KillService(EntityServer server)
        {
            KillService(GetServiceName(server));
        }

        public static void KillService(string serviceName)
        {
            if(DoesServiceExist(serviceName))
            {
                uint? pid = GetServicePID(serviceName);
                if(pid == null)
                    return;
                KillProcessAndChildren((int)pid);
            }
        }

        public static void KillAllServices()
        {
            foreach(Process proc in Process.GetProcessesByName("SpaceEngineersDedicated"))
            {
                try
                {
                    KillProcessAndChildren(proc.Id);
                }
                catch(Exception) { }
            }
            KillAllSESEServices();
        }

        public static void KillAllSESEServices()
        {
            foreach(Process proc in Process.GetProcessesByName("SEServerExtender"))
            {
                try
                {
                    KillProcessAndChildren(proc.Id);
                }
                catch(Exception) { }
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