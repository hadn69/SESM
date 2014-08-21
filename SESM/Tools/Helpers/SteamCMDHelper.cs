using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web.ModelBinding;
using Ionic.Zip;
using NLog;
using SESM.DAL;
using SESM.DTO;

namespace SESM.Tools.Helpers
{
    public class SteamCMDHelper
    {
        public static void Initialise(Logger logger, string login, string password, string steamGuardCode)
        {
            // Checking if steamCMD.exe exist in the right location
            CheckSteamCMD(logger);

            Process si = new Process();
            si.StartInfo.WorkingDirectory = SESMConfigHelper.SEDataPath + @"\SteamCMD\";
            si.StartInfo.UseShellExecute = false;
            si.StartInfo.FileName = "cmd.exe";
            si.StartInfo.Arguments = "/c \"" + SESMConfigHelper.SEDataPath + @"\SteamCMD\steamcmd.exe +@NoPromptForPassword 1 +login " + login
                + " " + password + " " + steamGuardCode + " +quit \"";
            si.StartInfo.CreateNoWindow = true;
            si.StartInfo.RedirectStandardInput = true;
            si.StartInfo.RedirectStandardOutput = true;
            si.StartInfo.RedirectStandardError = true;

            si.Start();
            si.WaitForExit(30000);
            logger.Info("SteamCMD execution finished");
            logger.Info("SteamCMD Standard output :");
            while (si.StandardOutput.Peek() > -1)
            {
                logger.Info("    " + si.StandardOutput.ReadLine());
            }
            logger.Info("End of SteamCMD Standard output");
            logger.Info("SteamCMD Error output :");
            while (si.StandardError.Peek() > -1)
            {
                logger.Info("    " + si.StandardError.ReadLine());
            }
            logger.Info("End of SteamCMD Error output");
            si.Close();

        }

        public static SteamCMDResult Update(Logger logger)
        {
            // Checking if steamCMD.exe exist in the right location
            CheckSteamCMD(logger);

            string AUPassword = string.Empty;

            try
            {
                logger.Info("Decoding credentials...");
                AUPassword = SESMConfigHelper.AUPassword;
            }
            catch (CryptographicException)
            {
                logger.Error("Error decoding the credentials, to solve the issue, please re-input them in the auto update configuration. If it occur more than once, please report the bug.");
                return SteamCMDResult.Fail_Unknow;
            }
            catch
            {
                logger.Error("Error decoding the credentials, please report the bug !");
                return SteamCMDResult.Fail_Unknow;
            }

            string dedicatedZipPath = SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip";

            FileInfo fiBefore = null;
            MemoryStream original = null;
            // Checking if the file has been modified since last check
            if (File.Exists(dedicatedZipPath))
            {
                fiBefore = new FileInfo(dedicatedZipPath);

                logger.Info("Backing up original DedicatedServer.zip");
                original = new MemoryStream();
                using (Stream input = File.OpenRead(dedicatedZipPath))
                {
                    input.CopyTo(original);
                }
                original.Position = 0;

            }

            Process si = new Process();
            si.StartInfo.WorkingDirectory = SESMConfigHelper.SEDataPath + @"\SteamCMD\";
            si.StartInfo.UseShellExecute = false;
            si.StartInfo.FileName = "cmd.exe";
            si.StartInfo.Arguments = "/c \"" + SESMConfigHelper.SEDataPath + @"\SteamCMD\steamcmd.exe +login " + SESMConfigHelper.AUUsername
                + " " + AUPassword + " +force_install_dir "
                + SESMConfigHelper.SEDataPath + "\\AutoUpdateData\\ +app_update 244850 validate +quit \"";
            si.StartInfo.CreateNoWindow = true;
            si.StartInfo.RedirectStandardInput = true;
            si.StartInfo.RedirectStandardOutput = true;
            si.StartInfo.RedirectStandardError = true;

            logger.Info("Starting SteamCMD (90 secs Max)");
            si.Start();
            bool exited = si.WaitForExit(90000);
            if (exited)
                logger.Info("Process ended sooner than timeout");
            else
                logger.Warn("Process ended by timeout");

            logger.Info("SteamCMD Standard output :");
            string output = string.Empty;
            while (si.StandardOutput.Peek() > -1)
            {
                string val = si.StandardOutput.ReadLine();
                output += val;
                logger.Info("    " + val);
            }
            logger.Info("End of SteamCMD Standard output");
            logger.Info("SteamCMD Error output :");
            while (si.StandardError.Peek() > -1)
            {
                logger.Info("    " + si.StandardError.ReadLine());
            }
            logger.Info("End of SteamCMD Error output");

            if (!exited)
            {
                logger.Info("SteamCMD execution timouted, killing process...");
                try
                {
                    si.Kill();
                    logger.Info("Killing process sucessful");
                }
                catch (Exception ex)
                {
                    logger.Error("Error while killing process", ex);
                }
                logger.Info("Restoring original DedicatedServer.zip");
                File.Delete(dedicatedZipPath);

                using (FileStream fs = new FileStream(dedicatedZipPath,FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    original.CopyTo(fs);
                }
                original.Dispose();
                File.SetLastWriteTimeUtc(dedicatedZipPath, fiBefore.LastWriteTimeUtc);
                return SteamCMDResult.Fail_Unknow;
            }

            if (output.Contains("Login Failure:"))
            {
                if (output.Contains("Login Failure: Invalid Password"))
                {
                    logger.Error("Invalid Steam Credentials");
                    return SteamCMDResult.Fail_Credentials;
                }
                if (output.Contains("Login Failure: Account Logon Denied"))
                {
                    logger.Error("SteamGuard Active, please input code in the panel");
                    return SteamCMDResult.Fail_SteamGuardMissing;
                }
                if (output.Contains("Login Failure: Invalid Login Auth Code"))
                {
                    logger.Error("Wrong SteamGuard Code, please input the right one in the panel");
                    return SteamCMDResult.Fail_SteamGuardBadCode;
                }
            }

            // Checking DedicatedServer.zip exist 
            if (!File.Exists(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip"))
                return SteamCMDResult.Fail_Unknow;

            // Checking if the file has been modified since last check
            FileInfo fi = new FileInfo(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip");
            if (fiBefore != null)
                if (fi.LastWriteTimeUtc.ToString("g") == fiBefore.LastWriteTimeUtc.ToString("g"))
                {
                    logger.Info("DedicatedServer.zip haven't changed, exiting");
                    return SteamCMDResult.Success_NothingToDo;
                }
            logger.Info("DedicatedServer.zip have changed, initiating lockdown mode ...");
            SESMConfigHelper.Lockdown = true;
            logger.Info("Waiting 30 secs for all requests to end ...");

            System.Threading.Thread.Sleep(30000);

            DataContext context = new DataContext();
            ServerProvider srvPrv = new ServerProvider(context);

            logger.Info("Stopping the running server ...");
            // Getting started server list
            List<EntityServer> listStartedServ =
                srvPrv.GetAllServers().Where(item => srvPrv.GetState(item) == ServiceState.Running).ToList();

            foreach (EntityServer item in srvPrv.GetAllServers())
            {
                logger.Info("Sending stop order to " + item.Name);
                ServiceHelper.StopService(ServiceHelper.GetServiceName(item));
            }

            foreach (EntityServer item in srvPrv.GetAllServers())
            {
                ServiceHelper.WaitForStopped(ServiceHelper.GetServiceName(item));
            }

            logger.Info("Killing ghosts processes");
            // Killing some ghost processes that might still exists
            ServiceHelper.KillAllService();


            using (ZipFile zip = ZipFile.Read(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip"))
            {
                logger.Info("Deleting old game files");
                if (!Directory.Exists(SESMConfigHelper.SEDataPath))
                    Directory.CreateDirectory(SESMConfigHelper.SEDataPath);
                if (Directory.Exists(SESMConfigHelper.SEDataPath + @"Content\"))
                    Directory.Delete(SESMConfigHelper.SEDataPath + @"Content\", true);
                if (Directory.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer\"))
                    Directory.Delete(SESMConfigHelper.SEDataPath + @"DedicatedServer\", true);
                if (Directory.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer64\"))
                    Directory.Delete(SESMConfigHelper.SEDataPath + @"DedicatedServer64\", true);

                logger.Info("Unzipping new game files");
                zip.ExtractAll(SESMConfigHelper.SEDataPath);
            }
            logger.Info("Game file Update finished, lifting lockdown");
            SESMConfigHelper.Lockdown = false;

            foreach (EntityServer item in listStartedServ)
            {
                logger.Info("Restarting " + item.Name);
                ServiceHelper.StartService(ServiceHelper.GetServiceName(item));
            }
            return SteamCMDResult.Success_UpdateInstalled;
        }

        public static void CheckSteamCMD(Logger logger)
        {
            if (!File.Exists(SESMConfigHelper.SEDataPath + @"\SteamCMD\steamcmd.exe"))
            {
                logger.Info("SteamCMD.exe not present, downloading it...");
                if (!Directory.Exists(SESMConfigHelper.SEDataPath + @"\SteamCMD\"))
                    Directory.CreateDirectory(SESMConfigHelper.SEDataPath + @"\SteamCMD\");

                if (!Directory.Exists(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\"))
                    Directory.CreateDirectory(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\");

                WebRequest objRequest = HttpWebRequest.Create("http://media.steampowered.com/installer/steamcmd.zip");
                WebResponse objResponse = objRequest.GetResponse();
                MemoryStream memoryStream = new MemoryStream();
                objResponse.GetResponseStream().CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (Stream input = memoryStream)
                {
                    using (ZipFile zip = ZipFile.Read(input))
                    {
                        zip.ExtractAll(SESMConfigHelper.SEDataPath + @"\SteamCMD\");
                    }
                }
                logger.Info("SteamCMD.exe Downloaded !");
            }
        }

        public enum SteamCMDResult
        {
            Fail_Unknow,
            Fail_Credentials,
            Fail_SteamGuardMissing,
            Fail_SteamGuardBadCode,
            Success_UpdateInstalled,
            Success_NothingToDo

        }
    }
}