using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Ionic.Zip;
using NLog;

namespace SESM.Tools.Helpers
{
    public class SteamCMDHelper
    {
        private const int AppId = 298740;
        private const int GetInfoDuration = 60;

        private static object CheckSteamCMDLock = new object();

        private static void CheckSteamCMD(Logger logger)
        {
            lock (CheckSteamCMDLock)
            {
                if (!File.Exists(PathHelper.GetSteamCMDPath() + @"steamcmd.exe"))
                {
                    logger?.Info("SteamCMD.exe not present, downloading it...");
                    if (!Directory.Exists(PathHelper.GetSteamCMDPath()))
                        Directory.CreateDirectory(PathHelper.GetSteamCMDPath());

                    if (!Directory.Exists(PathHelper.GetSyncDirPath()))
                        Directory.CreateDirectory(PathHelper.GetSyncDirPath());
                    try
                    {
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
                        logger?.Info("SteamCMD.exe Downloaded !");
                        logger?.Info("Launching for 15 seconds for initialisation");

                        Process si = new Process();
                        si.StartInfo.WorkingDirectory = PathHelper.GetSteamCMDPath();
                        si.StartInfo.UseShellExecute = false;
                        si.StartInfo.FileName = SESMConfigHelper.SEDataPath + @"\SteamCMD\steamcmd.exe";
                        si.StartInfo.CreateNoWindow = false;
                        si.StartInfo.RedirectStandardInput = false;
                        si.StartInfo.RedirectStandardOutput = false;
                        si.StartInfo.RedirectStandardError = false;
                        si.Start();
                        Thread.Sleep(15000);
                        if (!si.HasExited)
                            si.Kill();
                        logger?.Info("Done SteamCMD initialisation");
                    }

                    catch (Exception ex)
                    {
                        logger?.Error("Error while downloadin/unzipping !", ex);
                    }
                }
            }
        }

        private static string ExecuteSteamCMD(Logger logger, string arguments, int duration = GetInfoDuration)
        {
            try
            {
                // Check and DL SteamCMD
                CheckSteamCMD(logger);

                // Getting Info
                Process si = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = PathHelper.GetSteamCMDPath(),
                        UseShellExecute = false,
                        FileName = SESMConfigHelper.SEDataPath + @"\SteamCMD\steamcmd.exe",
                        Arguments = arguments,
                        CreateNoWindow = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = false
                    }
                };

                logger?.Info("Starting SteamCMD (" + GetInfoDuration + " secs Max)");
                logger?.Debug("Arguments : " + si.StartInfo.Arguments);
                si.Start();

                DateTime endTime = DateTime.Now.AddSeconds(duration);
                string output = string.Empty;

                logger?.Debug("Start of SteamCMD output :");
                while (!si.HasExited && DateTime.Now <= endTime)
                {
                    string val = si.StandardOutput.ReadLine();
                    if (val == null)
                        continue;
                    output += val + "\n";
                    logger?.Debug("    " + val);
                }
                logger?.Debug("End of SteamCMD output");

                if (si.HasExited)
                    logger?.Info("Process closed itself gracefully");
                else
                {
                    logger?.Warn("Process execution timeout");
                    try
                    {
                        ServiceHelper.KillProcessAndChildren(si.Id);
                        logger?.Info("Killing process sucessful");
                        Thread.Sleep(2000);
                    }
                    catch (Exception ex)
                    {
                        logger?.Error("Error while killing process", ex);
                    }
                }
                return output;
            }
            catch (Exception ex)
            {
                logger?.Fatal("Exception executing steamcmd : ", ex);
            }
            return null;
        }

        public static int? GetInstalledVersion(Logger logger = null)
        {
            string output = ExecuteSteamCMD(logger, " +@ShutdownOnFailedCommand 1"
                                                    + " +force_install_dir " + PathHelper.GetSyncDirPath()
                                                    + " +login Anonymous"
                                                    + " +app_status " + AppId
                                                    + " +quit");

            if (output.Contains("Login Failure"))
            {
                logger?.Error("Login Failure !");
                return null;
            }

            if (output.Contains("install state:"))
            {
                if (output.Contains("install state: uninstalled"))
                    return 0;
                Regex regex = new Regex(@"BuildID (\d+)");
                Match match = regex.Match(output);
                if (match.Success)
                {
                    try
                    {
                        return int.Parse(match.Groups[1].Value);
                    }
                    catch (Exception ex)
                    {
                        logger?.Error("Failed parsing int :", ex);
                        return null;
                    }
                }
                logger?.Error("Missing keyword !");
                return null;
            }
            logger?.Error("Missing keyword !");
            return null;
        }

        public static int? GetAvailableVersion(bool dev, Logger logger = null)
        {
            string output = ExecuteSteamCMD(logger, " +@ShutdownOnFailedCommand 1"
                                                    + " +force_install_dir " + PathHelper.GetSyncDirPath()
                                                    + " +login Anonymous"
                                                    + " +app_info_request " + AppId
                                                    + " +app_info_update" // Try to force info update
                                                    + " +app_info_update 1"
                                                    + " +app_info_print " + AppId
                                                    + " +quit");
            if (output.Contains("Login Failure"))
            {
                logger?.Error("Login Failure !");
                return null;
            }

            if (output.Contains("\"branches\""))
            {
                Regex regex = dev ?
                    new Regex("dev\"\\s*{\\s*\"buildid\"\\s*\"(\\d+)") :
                    new Regex("public\"\\s*{\\s*\"buildid\"\\s*\"(\\d+)");
                Match match = regex.Match(output);
                if (match.Success)
                {
                    try
                    {
                        return int.Parse(match.Groups[1].Value);
                    }
                    catch (Exception ex)
                    {
                        logger?.Error("Failed parsing int :", ex);
                        return 0;
                    }
                }
            }
            logger?.Error("Missing keyword !");
            return null;
        }

        public static void Update(Logger logger, bool dev)
        {
            string output = ExecuteSteamCMD(logger, " +login Anonymous"
                                                    + " +force_install_dir " + PathHelper.GetSyncDirPath()
                                                    + " +app_update " + AppId + " -validate "
                                                    + (dev ?
                                                        " -beta development -betapassword " + SESMConfigHelper.AutoUpdateBetaPassword :
                                                        " -beta public")
                                                    + " +quit", 120);
            logger.Info("Update output : " + output);

        }

    }
}