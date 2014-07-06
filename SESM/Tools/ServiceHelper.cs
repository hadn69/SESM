using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using SESM.DTO;
using SESM.Models;
using System.Management;

namespace SESM.Tools
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

            string dataPath = SESMConfigHelper.GetSEDataPath();
            if (SESMConfigHelper.GetArch() == ArchType.x86)
                dataPath += @"DedicatedServer\SpaceEngineersDedicated.exe";
            if (SESMConfigHelper.GetArch() == ArchType.x64)
                dataPath += @"DedicatedServer64\SpaceEngineersDedicated.exe";

            System.Diagnostics.Process si = new System.Diagnostics.Process();
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
                System.Diagnostics.Process si = new System.Diagnostics.Process();
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
                string query = string.Format("SELECT ProcessId FROM Win32_Service WHERE Name='{0}'", serviceName);
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                foreach (ManagementObject obj in searcher.Get())
                {
                    uint processId = (uint) obj["ProcessId"];
                    Process process = null;
                    try
                    {
                        process = Process.GetProcessById((int) processId);
                    }
                    catch (ArgumentException)
                    {
                        // Thrown if the process specified by processId
                        // is no longer running.
                    }
                    try
                    {
                        if (process != null)
                        {
                            process.Kill();
                        }
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
        }

        public static bool DoesServiceExist(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            var service = services.FirstOrDefault(s => s.ServiceName == serviceName);
            return service != null;
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