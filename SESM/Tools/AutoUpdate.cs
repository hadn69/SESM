using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
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
            if (!SESMConfigHelper.GetAutoUpdate())
                return;

            Process si = new Process();
            si.StartInfo.WorkingDirectory = SESMConfigHelper.GetSEDataPath() + @"\SteamCMD\";
            si.StartInfo.UseShellExecute = false;
            si.StartInfo.FileName = "cmd.exe";
            si.StartInfo.Arguments = "/c \"" + SESMConfigHelper.GetSEDataPath() + @"\SteamCMD\steamcmd.exe +login " + SESMConfigHelper.GetAUUsername() 
                + " " + SESMConfigHelper.GetAUPassword() + " +force_install_dir " 
                + SESMConfigHelper.GetSEDataPath() + "\\AutoUpdateData\\ +app_update 244850 validate +quit \"";
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
            if (!File.Exists(SESMConfigHelper.GetSEDataPath() + @"\AutoUpdateData\Tools\DedicatedServer.zip"))
                return;

            // Checking if the file has been modified since last check
            FileInfo fi = new FileInfo(SESMConfigHelper.GetSEDataPath() + @"\AutoUpdateData\Tools\DedicatedServer.zip");
            if(fi.LastWriteTime.ToString("g") == SESMConfigHelper.GetLastAU())
                return;
            SESMConfigHelper.SetLastAU(fi.LastWriteTime.ToString("g"));

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

            using (ZipFile zip = ZipFile.Read(SESMConfigHelper.GetSEDataPath() + @"\AutoUpdateData\Tools\DedicatedServer.zip"))
            {
                if (!Directory.Exists(SESMConfigHelper.GetSEDataPath()))
                    Directory.CreateDirectory(SESMConfigHelper.GetSEDataPath());
                if (Directory.Exists(SESMConfigHelper.GetSEDataPath() + @"Content\"))
                    Directory.Delete(SESMConfigHelper.GetSEDataPath() + @"Content\", true);
                if (Directory.Exists(SESMConfigHelper.GetSEDataPath() + @"DedicatedServer\"))
                    Directory.Delete(SESMConfigHelper.GetSEDataPath() + @"DedicatedServer\", true);
                if (Directory.Exists(SESMConfigHelper.GetSEDataPath() + @"DedicatedServer64\"))
                    Directory.Delete(SESMConfigHelper.GetSEDataPath() + @"DedicatedServer64\", true);
                //Directory.Delete(SESMConfigHelper.GetSEDataPath(), true);
                //Directory.CreateDirectory(SESMConfigHelper.GetSEDataPath());
                zip.ExtractAll(SESMConfigHelper.GetSEDataPath());
            }

            foreach (EntityServer item in listStartedServ)
            {
                ServiceHelper.StartService(ServiceHelper.GetServiceName(item));
            }
        }
    }
}