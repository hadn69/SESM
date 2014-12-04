using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using Ionic.Zip;
using NLog;
using SESM.DAL;
using SESM.DTO;

namespace SESM.Tools.Helpers
{
    public class SteamCMDHelper
    {
        public static SteamCMDResult Initialise(Logger logger, string login, string password, string steamGuardCode)
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
            si.StartInfo.RedirectStandardError = false;

            logger.Info("Starting SteamCMD (30 secs Max)");
            si.Start();
            DateTime endTime = DateTime.Now.AddSeconds(30);
            string output = string.Empty;
            logger.Info("Start of SteamCMD output :");
            while(!si.HasExited && DateTime.Now <= endTime)
            {
                string val = si.StandardOutput.ReadLine();
                output += val;
                logger.Info("    " + val);
            }
            logger.Info("End of SteamCMD output");

            if(output.Contains("Login Failure:"))
            {
                if(output.Contains("Login Failure: Invalid Password"))
                {
                    logger.Error("Invalid Steam Credentials");
                    return SteamCMDResult.Fail_Credentials;
                }
                if(output.Contains("Login Failure: Account Logon Denied"))
                {
                    logger.Error("SteamGuard Active, please input code in the panel");
                    return SteamCMDResult.Fail_SteamGuardMissing;
                }
                if(output.Contains("Login Failure: Invalid Login Auth Code"))
                {
                    logger.Error("Wrong SteamGuard Code, please input the right one in the panel");
                    return SteamCMDResult.Fail_SteamGuardBadCode;
                }
            }

            if(si.HasExited)
                logger.Info("Process closed itself gracefully");
            else
            {
                logger.Warn("Process execution timeout");
                try
                {
                    si.Kill();
                    logger.Info("Killing process sucessful");
                    Thread.Sleep(2000);
                }
                catch(Exception ex)
                {
                    logger.Error("Error while killing process", ex);
                }
                return SteamCMDResult.Fail_TooLong;
            }

            return SteamCMDResult.Success_NothingToDo;
        }

        public static SteamCMDResult Update(Logger logger, int duration)
        {
            Logger serviceLogger = LogManager.GetLogger("ServiceLogger");
            MemoryStream original = null;
            try
            {
                bool SEUpdate = false;
                bool SESEUpdate = false;

                // ----------- Start of SE Update Detection -----------

                // Checking if steamCMD.exe exist in the right location
                CheckSteamCMD(logger);

                string AUPassword = string.Empty;

                try
                {
                    logger.Info("Decoding credentials...");
                    AUPassword = SESMConfigHelper.AUPassword;
                }
                catch(CryptographicException)
                {
                    logger.Error(
                        "Error decoding the credentials, to solve the issue, please re-input them in the auto update configuration. If it already occured, please report the bug.");
                    return SteamCMDResult.Fail_Unknow;
                }
                catch
                {
                    logger.Error("Error decoding the credentials, please report the bug !");
                    return SteamCMDResult.Fail_Unknow;
                }

                string dedicatedZipPath = SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip";

                string md5Original = null;


                if(File.Exists(dedicatedZipPath))
                {
                    logger.Info("Backing up original DedicatedServer.zip");
                    original = new MemoryStream();
                    using(Stream input = File.OpenRead(dedicatedZipPath))
                    {
                        using(MD5 md5 = MD5.Create())
                        {
                            md5Original = Convert.ToBase64String(md5.ComputeHash(input));
                        }
                        logger.Info("Original hash : " + md5Original);
                        original.Position = 0;
                        input.CopyTo(original);
                    }
                    original.Position = 0;
                }

                Process si = new Process();
                si.StartInfo.WorkingDirectory = SESMConfigHelper.SEDataPath + @"\SteamCMD\";
                si.StartInfo.UseShellExecute = false;
                si.StartInfo.FileName = SESMConfigHelper.SEDataPath + @"\SteamCMD\steamcmd.exe";
                si.StartInfo.Arguments = "+@NoPromptForPassword 1 +login " + SESMConfigHelper.AUUsername
                                         + " " + AUPassword + " +force_install_dir "
                                         + SESMConfigHelper.SEDataPath + "AutoUpdateData";
                if(string.IsNullOrEmpty(SESMConfigHelper.AUBetaName))
                {
                    si.StartInfo.Arguments += " +app_update 244850 -validate -beta public ";
                }
                else
                {
                    si.StartInfo.Arguments += " +app_update 244850 -validate -beta \""
                                              + SESMConfigHelper.AUBetaName + "\" -betapassword \"" +
                                              SESMConfigHelper.AUBetaPassword + "\" ";
                }

                si.StartInfo.Arguments += " +quit ";
                si.StartInfo.CreateNoWindow = false;
                si.StartInfo.RedirectStandardInput = true;
                si.StartInfo.RedirectStandardOutput = true;
                si.StartInfo.RedirectStandardError = false;

                logger.Info("Starting SteamCMD (" + duration + " secs Max)");
                si.Start();
                DateTime endTime = DateTime.Now.AddSeconds(duration);
                string output = string.Empty;
                logger.Info("Start of SteamCMD output :");
                while(!si.HasExited && DateTime.Now <= endTime)
                {
                    string val = si.StandardOutput.ReadLine();
                    output += val;
                    logger.Info("    " + val);
                }
                logger.Info("End of SteamCMD output");

                if(si.HasExited)
                    logger.Info("Process closed itself gracefully");
                else
                {
                    logger.Warn("Process execution timeout");
                    try
                    {
                        si.Kill();
                        logger.Info("Killing process sucessful");
                        Thread.Sleep(2000);
                    }
                    catch(Exception ex)
                    {
                        logger.Error("Error while killing process", ex);
                    }
                    if(File.Exists(dedicatedZipPath))
                        File.Delete(dedicatedZipPath);

                    if(original != null)
                    {
                        logger.Info("Restoring original DedicatedServer.zip");

                        using(FileStream fs = new FileStream(dedicatedZipPath, FileMode.Create, FileAccess.Write,
                            FileShare.None))
                        {
                            original.CopyTo(fs);
                        }
                        original.Dispose();
                    }
                    return SteamCMDResult.Fail_TooLong;
                }

                if(output.Contains("Login Failure:"))
                {
                    if(output.Contains("Login Failure: Invalid Password"))
                    {
                        logger.Error("Invalid Steam Credentials");
                        return SteamCMDResult.Fail_Credentials;
                    }
                    if(output.Contains("Login Failure: Account Logon Denied"))
                    {
                        logger.Error("SteamGuard Active, please input code in the panel");
                        return SteamCMDResult.Fail_SteamGuardMissing;
                    }
                    if(output.Contains("Login Failure: Invalid Login Auth Code"))
                    {
                        logger.Error("Wrong SteamGuard Code, please input the right one in the panel");
                        return SteamCMDResult.Fail_SteamGuardBadCode;
                    }
                }

                // Checking DedicatedServer.zip exist 
                if(!File.Exists(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip"))
                    return SteamCMDResult.Fail_Unknow;

                // Checking if the file has been modified since last check
                FileInfo fi = new FileInfo(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip");

                string md5New = "";
                using(Stream input = File.OpenRead(dedicatedZipPath))
                {
                    using(MD5 md5 = MD5.Create())
                    {
                        md5New = Convert.ToBase64String(md5.ComputeHash(input));
                    }
                    logger.Info("New File hash : " + md5New);
                }
                if(md5Original != null)
                {
                    if(md5Original == md5New)
                    {
                        logger.Info("DedicatedServer.zip haven't changed, no SE Update Detected");
                    }
                    else
                    {
                        logger.Info("DedicatedServer.zip have changed, update on the way !");
                        SEUpdate = true;
                    }
                }
                else
                {
                    logger.Info("DedicatedServer.zip detected,update on the way !");
                    SEUpdate = true;
                }
                // ----------- End of SE Update Detection -----------

                // ----------- Start of SESE Update Detection -----------
                if(SESMConfigHelper.UseSESE)
                {

                    string SESEUrl = GithubHelper.UpdateIsAvailable();
                    if(!string.IsNullOrEmpty(SESEUrl))
                    {
                        logger.Info("SE Server Extender Update detected");
                        logger.Info("URL : " + SESEUrl);
                        logger.Info("Cleaning up SESE Zip");
                        GithubHelper.CleanupUpdate();
                        logger.Info("Downloading New SESE Update Zip");
                        GithubHelper.DownloadUpdate(SESEUrl);
                        SESEUpdate = true;
                    }
                    else
                    {
                        logger.Info("No SE Server Extender Update detected");
                    }
                }

                // ----------- End of SESE Update Detection -----------
                if(!SEUpdate && !SESEUpdate)
                {
                    logger.Info("No Update detected, exiting ...");
                    return SteamCMDResult.Success_NothingToDo;
                }

                logger.Info("Initiating lockdown mode ...");
                SESMConfigHelper.Lockdown = true;
                logger.Info("Waiting 30 secs for all requests to end ...");

                Thread.Sleep(30000);

                DataContext context = new DataContext();
                ServerProvider srvPrv = new ServerProvider(context);


                List<EntityServer> listStartedServ;
                // Getting started server list
                if(SEUpdate)
                    listStartedServ = srvPrv.GetAllServers().Where(item => srvPrv.GetState(item) == ServiceState.Running).ToList();
                else
                    listStartedServ = srvPrv.GetAllServers().Where(item => item.UseServerExtender && srvPrv.GetState(item) == ServiceState.Running)
                        .ToList();

                if(SEUpdate)
                {
                    logger.Info("Stopping all running server ...");

                    foreach(EntityServer item in srvPrv.GetAllServers())
                    {
                        logger.Info("Sending stop order to " + item.Name);
                        serviceLogger.Info(item.Name + " stopped by Updater");
                        ServiceHelper.StopService(item);
                    }

                    foreach(EntityServer item in srvPrv.GetAllServers())
                    {
                        logger.Info("Waiting for stop of " + item.Name);
                        ServiceHelper.WaitForStopped(item);
                    }

                    logger.Info("Killing ghosts processes");
                    // Killing some ghost processes that might still exists
                    ServiceHelper.KillAllService();
                }
                else
                {
                    logger.Info("Stopping SESE running server ...");
                    foreach(EntityServer item in listStartedServ)
                    {
                        logger.Info("Sending stop order to " + item.Name);
                        serviceLogger.Info(item.Name + " stopped by Updater");
                        ServiceHelper.StopService(item);
                    }
                    logger.Info("Waiting 30 secs for server to stop");
                    Thread.Sleep(30000);
                    logger.Info("Killing ghosts processes");
                    // Killing some ghost processes that might still exists
                    ServiceHelper.KillAllSESEService();
                }
                logger.Info("Waiting 30 secs to clear minds");
                Thread.Sleep(30000);
                int tryCount = 3;
                if(SEUpdate)
                    for(int i = 0; i < tryCount; i++)
                    {
                        try
                        {
                            using(ZipFile zip = ZipFile.Read(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip"))
                            {
                                logger.Info("Deleting old game files (try " + i + " of " + tryCount + ")");
                                if(!Directory.Exists(SESMConfigHelper.SEDataPath))
                                    Directory.CreateDirectory(SESMConfigHelper.SEDataPath);
                                if(Directory.Exists(SESMConfigHelper.SEDataPath + @"Content\"))
                                    Directory.Delete(SESMConfigHelper.SEDataPath + @"Content\", true);
                                if(Directory.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer\"))
                                    Directory.Delete(SESMConfigHelper.SEDataPath + @"DedicatedServer\", true);
                                if(Directory.Exists(SESMConfigHelper.SEDataPath + @"DedicatedServer64\"))
                                    Directory.Delete(SESMConfigHelper.SEDataPath + @"DedicatedServer64\", true);

                                logger.Info("Unzipping new game files");
                                zip.ExtractAll(SESMConfigHelper.SEDataPath);
                                break;
                            }
                        }
                        catch(Exception exUnzip)
                        {
                            logger.Error("Fail Deleting/Unzipping new game files (Exception) : ", exUnzip);
                            logger.Info("Waiting 30 secs");
                            Thread.Sleep(30000);
                            if (i == tryCount - 1)
                                throw new Exception("Fail applying game files, aborting");
                        }
                    }

                logger.Info("Applying SESE Files if they exist");
                GithubHelper.ApplyUpdate();

                logger.Info("Game file Update finished, lifting lockdown");
                SESMConfigHelper.Lockdown = false;

                foreach(EntityServer item in listStartedServ)
                {
                    logger.Info("Restarting " + item.Name);
                    serviceLogger.Info(item.Name + " stopped by Updater");
                    ServiceHelper.StartService(item);
                }
                return SteamCMDResult.Success_UpdateInstalled;
            }
            catch(Exception ex)
            {
                logger.Fatal("AutoUpdate failed miserably ... (Exception)", ex);

                if(original == null)
                {
                    logger.Info("No previous zip, deleting one if it exist");
                    try
                    {
                        if(File.Exists(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip"))
                            File.Delete(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip");
                    }
                    catch(Exception ex2)
                    {
                        logger.Fatal("Fail in the fail ... (Exception)", ex2);
                    }
                }
                else
                {
                    logger.Info("Restoring zip, deleting ...");
                    try
                    {
                        if(File.Exists(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip"))
                            File.Delete(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip");
                    }
                    catch(Exception ex2)
                    {
                        logger.Fatal("Fail in the fail ... (Exception)", ex2);
                    }

                    logger.Info("Saving zip");
                    try
                    {
                        FileStream file = new FileStream(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip",
                            FileMode.Create,
                            FileAccess.ReadWrite,
                            FileShare.None);
                        original.Position = 0;
                        original.CopyTo(file);
                    }
                    catch(Exception ex2)
                    {
                        logger.Fatal("Fail in the fail ... (Exception)", ex2);
                    }
                }
                return SteamCMDResult.Fail_Unknow;
            }
        }

        public static SteamCMDResult ForceUpdate(Logger logger, int duration)
        {
            string zipPath = SESMConfigHelper.SEDataPath + @"\AutoUpdateData\Tools\DedicatedServer.zip";
            if(File.Exists(zipPath))
            {
                logger.Info("DedicatedServer.zip Detected, deleting it ...");
                try
                {
                    File.Delete(zipPath);
                }
                catch(Exception)
                {
                    logger.Error("Failed deleting DedicatedServer.zip !");
                }

                if(File.Exists(zipPath))
                {
                    logger.Error("DedicatedServer.zip still present, Manual Update Force Failed !");
                    return SteamCMDResult.Fail_Unknow;
                }

                logger.Info("Done !");
            }
            else
            {
                logger.Info("DedicatedServer.zip not present, (Why are you doing a manual update force ..., useless here !)");
            }
            return Update(logger, duration);
        }

        private static void CheckSteamCMD(Logger logger)
        {
            if(!File.Exists(SESMConfigHelper.SEDataPath + @"\SteamCMD\steamcmd.exe"))
            {
                logger.Info("SteamCMD.exe not present, downloading it...");
                if(!Directory.Exists(SESMConfigHelper.SEDataPath + @"\SteamCMD\"))
                    Directory.CreateDirectory(SESMConfigHelper.SEDataPath + @"\SteamCMD\");

                if(!Directory.Exists(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\"))
                    Directory.CreateDirectory(SESMConfigHelper.SEDataPath + @"\AutoUpdateData\");

                WebRequest objRequest = HttpWebRequest.Create("http://media.steampowered.com/installer/steamcmd.zip");
                WebResponse objResponse = objRequest.GetResponse();
                MemoryStream memoryStream = new MemoryStream();
                objResponse.GetResponseStream().CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using(Stream input = memoryStream)
                {
                    using(ZipFile zip = ZipFile.Read(input))
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
            Fail_TooLong,
            Fail_Credentials,
            Fail_SteamGuardMissing,
            Fail_SteamGuardBadCode,
            Success_UpdateInstalled,
            Success_NothingToDo

        }
    }
}