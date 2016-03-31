using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Helpers;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using Quartz.Util;

namespace SESM.Tools.Helpers
{
    public class SteamCMDHelper
    {
        public const int SEAppId = 298740;
        public const int MEAppId = 367970;
        private const int GetInfoDuration = 60;

        private static object CheckSteamCMDLock = new object();

        private static object ExecuteSteamCMDLock = new object();

        private static void CheckSteamCMD(Logger logger)
        {
            lock (CheckSteamCMDLock)
            {
                if (!File.Exists(PathHelper.GetSteamCMDPath() + @"steamcmd.exe"))
                {
                    logger?.Info("SteamCMD.exe not present, downloading it...");
                    if (!Directory.Exists(PathHelper.GetSteamCMDPath()))
                        Directory.CreateDirectory(PathHelper.GetSteamCMDPath());

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

                        Process si = new Process
                        {
                            StartInfo =
                            {
                                WorkingDirectory = PathHelper.GetSteamCMDPath(),
                                UseShellExecute = false,
                                FileName = SESMConfigHelper.SEDataPath + @"\SteamCMD\steamcmd.exe",
                                CreateNoWindow = false,
                                RedirectStandardInput = false,
                                RedirectStandardOutput = false,
                                RedirectStandardError = false
                            }
                        };
                        si.Start();
                        si.WaitForExit(15000);
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
                lock (ExecuteSteamCMDLock)
                {
                    // Check and DL SteamCMD
                    CheckSteamCMD(logger);

                    // Force deleting 
                    if (Directory.Exists(PathHelper.GetSteamCMDPath() + @"appcache\"))
                        Directory.Delete(PathHelper.GetSteamCMDPath() + @"appcache\", true);
                    if (Directory.Exists(PathHelper.GetSteamCMDPath() + @"depotcache\"))
                        Directory.Delete(PathHelper.GetSteamCMDPath() + @"depotcache\", true);

                    // Getting Info
                    Process si = new Process
                    {
                        StartInfo =
                        {
                            WorkingDirectory = PathHelper.GetSteamCMDPath(),
                            UseShellExecute = false,
                            FileName = SESMConfigHelper.SEDataPath + @"\SteamCMD\steamcmd.exe",
                            Arguments = arguments,
                            CreateNoWindow = true,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        }
                    };

                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    {
                        si.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                                outputWaitHandle.Set();
                            else
                                output.AppendLine(e.Data);
                        };
                        si.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                                errorWaitHandle.Set();
                            else
                                error.AppendLine(e.Data);
                        };

                        logger?.Info("Starting SteamCMD (" + duration + " secs Max)");
                        //logger?.Debug("Arguments : " + si.StartInfo.Arguments);

                        si.Start();

                        si.BeginOutputReadLine();
                        si.BeginErrorReadLine();

                        if (si.WaitForExit(duration * 1000) &&
                            outputWaitHandle.WaitOne(10 * 1000) &&
                            errorWaitHandle.WaitOne(10 * 1000))
                        {
                            logger?.Info("Process closed itself gracefully");
                        }
                        else
                        {
                            logger?.Warn("Process execution timeout");
                            logger?.Warn("Trying to kill the process");
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

                        logger?.Debug("-- Start of SteamCMD standard output :");
                        foreach (string line in output.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                        {
                            logger?.Debug("    " + line);
                        }
                        logger?.Debug("-- End of SteamCMD standard output");

                        logger?.Debug("== Start of SteamCMD error output :");
                        foreach (string line in error.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                        {
                            logger?.Debug("    " + line);
                        }
                        logger?.Debug("== End of SteamCMD error output");

                        return output.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.Fatal("Exception executing steamcmd : ", ex);
            }
            return null;
        }

        public static SteamGameInfo GetSteamGameInfos(int appId, string syncDirPath, Logger logger)
        {
            try
            {
                string arguments = " +@ShutdownOnFailedCommand 1 " +
                                  " +force_install_dir " + syncDirPath +
                                  " +login Anonymous " +
                                  " +app_status " + appId +
                                  " +app_info_print " + appId +
                                  " +find a " +
                                  " +quit";


                string output = ExecuteSteamCMD(logger, arguments, 30);

                SteamGameInfo gameInfo = new SteamGameInfo();

                // Parsing installed buildid

                if (output.Contains("Login Failure"))
                {
                    logger?.Error("Login Failure !");
                    return null;
                }

                if (output.Contains("install state:"))
                {
                    if (output.Contains("install state: uninstalled"))
                        gameInfo.InstalledBuildId = 0;
                    else
                    {
                        Regex regex = new Regex(@"BuildID (\d+)");
                        Match match = regex.Match(output);
                        if (match.Success)
                        {
                            try
                            {
                                VersionCache.SELocalVersion = match.Groups[1].Value;
                                gameInfo.InstalledBuildId = int.Parse(match.Groups[1].Value);
                            }
                            catch (Exception ex)
                            {
                                logger?.Error("Failed parsing int :", ex);
                                return null;
                            }
                        }
                        else
                        {
                            logger?.Error("Missing BuildId keyword !");
                            return null;
                        }
                    }
                }
                else
                {
                    logger?.Error("Missing Basic keyword !");
                    return null;
                }

                // Parsing remote data

                int firstPos = output.IndexOf('{');
                int lastPos = output.LastIndexOf('}');

                output = output.Substring(firstPos, lastPos - firstPos + 1);

                output = output.Replace("\"\t\t\"", "\":\"");
                output = Regex.Replace(output, ":\"(\\d{1,9})\"", ":$1");
                output = Regex.Replace(output, "(})(\\s+[^}\\s])", "$1,$2");
                output = Regex.Replace(output, "(\"|\\d)(\\s+\")", "$1,$2");
                output = Regex.Replace(output, "(\")(\\s*{)", "$1:$2");

                if (string.IsNullOrWhiteSpace(output))
                    return null;

                var converter = new ExpandoObjectConverter();
                dynamic infos = JsonConvert.DeserializeObject<ExpandoObject>(output, converter);

                gameInfo.Name = infos.common.name;
                gameInfo.GameId = Convert.ToInt32(infos.common.gameid);

                foreach (KeyValuePair<String, object> kvp in (infos.depots.branches as IDictionary<String, object>))
                {
                    dynamic item = kvp.Value;

                    SteamGameInfo.BranchItem branch = new SteamGameInfo.BranchItem();

                    try
                    {
                        branch.BuildId = Convert.ToInt32(item.buildid);
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        branch.Description = item.description;
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        branch.PwdRequired = Convert.ToBoolean(item.pwdrequired);
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        branch.PwdTestGID = item.pwdtestgid;
                    }
                    catch (Exception)
                    {
                    }

                    gameInfo.Branches.Add(kvp.Key, branch);
                }


                return gameInfo;

            }
            catch (Exception ex)
            {
                logger?.Fatal("GetSteamGameInfos Exception", ex);
                return null;
            }
        }

        public static void Update(int appId, string syncDirPath, string branch, string password, Logger logger)
        {
            string output = ExecuteSteamCMD(logger, " +login Anonymous"
                                                    + " +force_install_dir " + syncDirPath
                                                    + " +app_update " + appId + " -validate "
                                                    + " -beta " + branch
                                                    + (!string.IsNullOrWhiteSpace(password) ? "-betapassword " + password : "")
                                                    + " +find a "
                                                    + " +quit", 180);
            //logger.Info("Update output : " + output);
        }

        public class SteamGameInfo
        {
            public string Name;
            public int GameId;
            public int InstalledBuildId;
            public Dictionary<string, BranchItem> Branches = new Dictionary<string, BranchItem>();

            public BranchItem this[string branch]
            {
                get { return Branches[branch]; }
                set
                {
                    if (Branches.ContainsKey(branch))
                        Branches[branch] = value;
                    else
                        Branches.Add(branch, value);
                }
            }

            public class BranchItem
            {
                public int BuildId;
                public string Description;
                public bool PwdRequired = false;
                public string PwdTestGID;

            }
        }
    }


}