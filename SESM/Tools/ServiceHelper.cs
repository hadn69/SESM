using System;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Web.Configuration;
using SESM.DTO;

namespace SESM.Tools
{
    public class ServiceHelper
    {
        
        public static string GetServiceName(EntityServer server)
        {
            Configuration conf = WebConfigurationManager.OpenWebConfiguration("/web");
            return PathHelper.GetPrefix() + "_" + server.Id + "_" + server.Name;
        }

        public static void StopService(string serviceName)
        {
            try
            {
                ServiceController svcController = new ServiceController(serviceName);
                if (svcController != null)
                {
                    if (svcController.Status != ServiceControllerStatus.Stopped &&
                        svcController.Status != ServiceControllerStatus.StopPending)
                        svcController.Stop();
  
                }
            }
            catch (Exception Ex)
            {
                return;
            }
        }

        public static void StopServiceAndWait(string serviceName)
        {
            try
            {
                ServiceController svcController = new ServiceController(serviceName);
                if (svcController != null)
                {
                    if (svcController.Status != ServiceControllerStatus.Stopped &&
                        svcController.Status != ServiceControllerStatus.StopPending)
                    {
                        svcController.Stop();
                        svcController.WaitForStatus(ServiceControllerStatus.Stopped);
                    }
                }
            }
            catch (Exception Ex)
            {
                return;
            }
        }

        public static void WaitForStopped(string serviceName)
        {
            try
            {
                ServiceController svcController = new ServiceController(serviceName);
                if (svcController != null)
                {
                    svcController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 10));
                }
            }
            catch (Exception Ex)
            {
                return;
            }
        }
        public static void StartService(string serviceName)
        {
            try
            {
                ServiceController svcController = new ServiceController(serviceName);
                if (svcController != null)
                {
                    if (svcController.Status != ServiceControllerStatus.Running &&
                        svcController.Status != ServiceControllerStatus.StartPending)
                        svcController.Start();

                }
            }
            catch (Exception Ex)
            {
                return;
            }
        }

        public static void RestartService(string serviceName)
        {
            try
            {
                ServiceController svcController = new ServiceController(serviceName);
                if (svcController != null)
                {
                    if (svcController.Status != ServiceControllerStatus.Stopped 
                        && svcController.Status != ServiceControllerStatus.StartPending 
                        && svcController.Status != ServiceControllerStatus.StopPending)
                    {
                        svcController.Stop();
                        svcController.WaitForStatus(ServiceControllerStatus.Stopped);
                        svcController.Start();
                    }

                }
            }
            catch (Exception Ex)
            {
                return;
            }
        }

        public static void RegisterService(string serviceName)
        {
            if (!DoesServiceExist(serviceName))
            {
                Configuration conf = WebConfigurationManager.OpenWebConfiguration("/web");
                string dataPath = conf.AppSettings.Settings["SEDataPath"].Value;
                if (conf.AppSettings.Settings["Arch"].Value == "x86")
                    dataPath += @"DedicatedServer\SpaceEngineersDedicated.exe";
                if (conf.AppSettings.Settings["Arch"].Value == "x64")
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