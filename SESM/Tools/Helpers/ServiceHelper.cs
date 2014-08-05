﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using SESM.DTO;
using SESM.Models.Views.Settings;

namespace SESM.Tools.Helpers
{
    public class ServiceHelper
    {
        
        public static string GetServiceName(EntityServer server)
        {
            return PathHelper.GetPrefix() + "_" + server.Id + "_" + server.Name;
        }

        public static string GetServiceName(string prefix, EntityServer server)
        {
            return prefix + "_" + server.Id + "_" + server.Name;
        }

        public static void StopService(string serviceName)
        {
            try
            {
                ServiceController svcController = new ServiceController(serviceName);
                if (svcController.Status == ServiceControllerStatus.Running)
                    svcController.Stop();
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public static void StopServiceAndWait(string serviceName)
        {
            try
            {
                ServiceController svcController = new ServiceController(serviceName);

                if (svcController.Status == ServiceControllerStatus.Running)
                {
                    svcController.Stop();
                    svcController.WaitForStatus(ServiceControllerStatus.Stopped);
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public static void WaitForStopped(string serviceName)
        {
            try
            {
                ServiceController svcController = new ServiceController(serviceName);
                svcController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 10));
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public static void StartService(string serviceName)
        {
            try
            {
                ServiceController svcController = new ServiceController(serviceName);

                if (svcController.Status == ServiceControllerStatus.Stopped)
                    svcController.Start();
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public static void RestartService(string serviceName)
        {
            try
            {
                ServiceController svcController = new ServiceController(serviceName);
                if (svcController.Status == ServiceControllerStatus.Running)
                {
                    svcController.Stop();
                    svcController.WaitForStatus(ServiceControllerStatus.Stopped);
                    svcController.Start();
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public static void RegisterService(string serviceName)
        {
            if (DoesServiceExist(serviceName)) 
                return;

            string dataPath = SESMConfigHelper.SEDataPath;
            if (SESMConfigHelper.Arch == ArchType.x86)
                dataPath += @"DedicatedServer\SpaceEngineersDedicated.exe";
            if (SESMConfigHelper.Arch == ArchType.x64)
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
            if (DoesServiceExist(serviceName))
            {
                Process si = new Process();
                si.StartInfo.WorkingDirectory = @"c:\";
                si.StartInfo.UseShellExecute = false;
                si.StartInfo.FileName = "cmd.exe";
                si.StartInfo.Arguments = "/c \"sc delete " + serviceName  + "\"";
                si.StartInfo.CreateNoWindow = true;
                si.StartInfo.RedirectStandardInput = true;
                si.StartInfo.RedirectStandardOutput = true;
                si.StartInfo.RedirectStandardError = true;
                si.Start();
                string output = si.StandardOutput.ReadToEnd();
                si.Close();
            }
        }
        
        // Source : http://stackoverflow.com/a/566089
        public static void KillService(string serviceName)
        {
            if (DoesServiceExist(serviceName))
            {
                uint? pid = GetServicePID(serviceName);
                if (pid == null)
                    return;

                Process process = Process.GetProcessById((int)pid);

                try
                {
                    process.Kill();
                }
                catch (Win32Exception)
                {
                    // Thrown if process is already terminating,
                    // the process is a Win16 exe or the process
                    // could not be terminated.
                }
                catch (InvalidOperationException)
                {
                    // Thrown if the process has already terminated.
                }

            }
        }

        public static void KillAllService()
        {
            foreach (Process proc in Process.GetProcessesByName("SpaceEngineersDedicated"))
            {
                try
                {
                    proc.Kill();
                }
                catch (Exception)
                {
                    throw;
                }
                
            }
        }

        public static uint? GetServicePID(string serviceName)
        {
            if (!DoesServiceExist(serviceName))
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
            foreach (string item in outputSplitted.Where(item => item.Contains("PID")))
            {
                outputLine = item;
            }
            string pid = outputLine.Substring(28).Trim();
            uint processId = uint.Parse(pid);
            if (processId == 0)
                return null;
            else
                return processId;
        }

        public static Ressources? GetCurrentRessourceUsage(string serviceName)
        {
            uint? pid = GetServicePID(serviceName);
            Ressources ressources = new Ressources();
            if (pid == null || pid == 0)
            {
                ressources.CPU = 0;
                ressources.Memory = 0;
                return ressources;
            }
                
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
            return ressources;
        }

        public static bool DoesServiceExist(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            var service = services.FirstOrDefault(s => s.ServiceName == serviceName);
            return service != null;
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