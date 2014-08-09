using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Quartz;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools.Helpers;

namespace SESM.Tools
{
    public class AutoUpdate :IJob
    {
        public void Execute(IJobExecutionContext jobContext)
        {
            // Checking auto update is enable
            if (!SESMConfigHelper.AutoUpdate)
                return;



            FileInfo fiBefore = null;
            // Checking if the file has been modified since last check
            if (File.Exists(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip"))
                fiBefore = new FileInfo(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip");

            Process si = new Process();
            si.StartInfo.WorkingDirectory = SESMConfigHelper.SEDataPath + @"\SteamCMD\";
            si.StartInfo.UseShellExecute = false;
            si.StartInfo.FileName = "cmd.exe";
            si.StartInfo.Arguments = "/c \"" + SESMConfigHelper.SEDataPath + @"\SteamCMD\steamcmd.exe +login " + SESMConfigHelper.AUUsername 
                + " " + SESMConfigHelper.AUPassword + " +force_install_dir " 
                + SESMConfigHelper.SEDataPath + "\\AutoUpdateData\\ +app_update 244850 validate +quit \"";
            si.StartInfo.CreateNoWindow = true;
            si.StartInfo.RedirectStandardInput = true;
            si.StartInfo.RedirectStandardOutput = true;
            si.StartInfo.RedirectStandardError = true;
            si.Start();
            si.WaitForExit(60000);
            var output = new List<string>();

            while (si.StandardOutput.Peek() > -1)
            {
                output.Add(si.StandardOutput.ReadLine());
            }

            while (si.StandardError.Peek() > -1)
            {
                output.Add(si.StandardError.ReadLine());
            }

            
            si.Close();

            // Checking DedicatedServer.zip exist 
            if (!File.Exists(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip"))
                return;

            // Checking if the file has been modified since last check
            FileInfo fi = new FileInfo(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip");
            if(fiBefore != null)
                if (fi.LastWriteTime.ToString("g") == fiBefore.LastWriteTime.ToString("g"))
                    return;

            DataContext context = new DataContext();
            ServerProvider srvPrv = new ServerProvider(context);

            // Getting started server list
            List<EntityServer> listStartedServ =
                srvPrv.GetAllServers().Where(item => srvPrv.GetState(item) == ServiceState.Running).ToList();

            foreach (EntityServer item in srvPrv.GetAllServers())
            {
                ServiceHelper.StopService(ServiceHelper.GetServiceName(item));
            }

            foreach (EntityServer item in srvPrv.GetAllServers())
            {
                ServiceHelper.WaitForStopped(ServiceHelper.GetServiceName(item));
            }

            // Killing some ghost processes that might still exists
            ServiceHelper.KillAllService();

            using (ZipFile zip = ZipFile.Read(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip"))
            {
                if (!Directory.Exists(SESMConfigHelper.SEDataPath))
                    Directory.CreateDirectory(SESMConfigHelper.SEDataPath);
                if (Directory.Exists(SESMConfigHelper.SEDataPath + @"Content\"))
                    Directory.Delete(SESMConfigHelper.SEDataPath + @"Content\", true);
                if (Directory.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer\"))
                    Directory.Delete(SESMConfigHelper.SEDataPath + @"DedicatedServer\", true);
                if (Directory.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer64\"))
                    Directory.Delete(SESMConfigHelper.SEDataPath + @"DedicatedServer64\", true);
                zip.ExtractAll(SESMConfigHelper.SEDataPath);
            }

            foreach (EntityServer item in listStartedServ)
            {
                ServiceHelper.StartService(ServiceHelper.GetServiceName(item));
            }
        }
    }
}