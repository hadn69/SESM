using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using Ionic.Zip;
using NLog;

namespace SESM.Tools.Helpers
{
    public class SESEHelper
    {
        public static string GetLastRemoteURL(string data, bool useDev)
        {
            dynamic result;
            try
            {
                result = Json.Decode(data);
            }
            catch (Exception)
            {
                return null;
            }
            foreach (dynamic item in result)
            {
                if ((useDev && item.prerelease) || (!useDev && !item.prerelease))
                {
                    return item.assets[0].browser_download_url;
                }
            }
            return null;
        }

        public static Version GetLastRemoteVersion(bool useDev)
        {
            string githubData = GetGithubData();
            return GetLastRemoteVersion(githubData, useDev);
        }

        public static Version GetLastRemoteVersion(string data, bool useDev)
        {
            dynamic result;
            try
            {
                result = Json.Decode(data);
            }
            catch (Exception)
            {
                return null;
            }
            foreach (dynamic item in result)
            {
                if ((useDev && item.prerelease) || (!useDev && !item.prerelease))
                {
                    Match extract = Regex.Match(item.assets[0].name, "^.*v(.*)-.*$");
                    if (extract.Groups.Count != 2)
                        return null;
                    VersionCache.SESERemoteVersion = extract.Groups[1].Value;
                    return new Version(extract.Groups[1].Value);
                }
            }
            return new Version(0, 0);
        }

        public static string GetGithubData()
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "SESM V3");
            try
            {
                string response = client.DownloadString(SESMConfigHelper.SESEUpdateURL);
                return string.IsNullOrWhiteSpace(response) ? null : response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Version GetLocalVersion()
        {
            string SESELocPath = SESMConfigHelper.SEDataPath + "DedicatedServer64\\SEServerExtender.exe";
            if (File.Exists(SESELocPath))
            {
                Version ver = AssemblyName.GetAssemblyName(SESELocPath).Version;
                VersionCache.SESELocalVersion = ver.ToString();
                return ver;
            }
            return new Version(0, 0, 0, 0);
        }

        public static void CleanupUpdate()
        {
            string[] files = Directory.GetFiles(SESMConfigHelper.SEDataPath, "SEServerExtender*.zip", SearchOption.TopDirectoryOnly);
            foreach (var item in files)
            {
                try
                {
                    File.Delete(item);
                }
                catch (Exception ex)
                {
                    Logger exceptionLogger = LogManager.GetLogger("GenericExceptionLogger");
                    exceptionLogger.Fatal("Caught Exception in CleanupUpdate when deleting " + item, ex);
                }
            }
        }

        public static void DownloadUpdate(string url)
        {
            WebRequest objRequest = HttpWebRequest.Create(url);
            WebResponse objResponse = objRequest.GetResponse();
            FileStream fileStream = new FileStream(SESMConfigHelper.SEDataPath + PathHelper.GetLastLeaf(url), FileMode.Create, FileAccess.Write, FileShare.None);
            objResponse.GetResponseStream().CopyTo(fileStream);
            fileStream.Close();
        }

        public static void ApplyUpdate(Logger logger = null)
        {
            try
            {
                string[] files = Directory.GetFiles(SESMConfigHelper.SEDataPath, "SEServerExtender*.zip", SearchOption.TopDirectoryOnly);
                if (files.Length != 1)
                {
                    // If multiple zip available or no zip, we don't risk an update 
                    return;
                }

                if (logger != null)
                    logger.Info("Extracting " + PathHelper.GetLastLeaf(files[0]));

                using (ZipFile zip = new ZipFile(files[0]))
                {
                    zip.ExtractAll(SESMConfigHelper.SEDataPath + "DedicatedServer64",
                        ExtractExistingFileAction.OverwriteSilently);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}