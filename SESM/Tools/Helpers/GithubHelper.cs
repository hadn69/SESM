using System;
using System.IO;
using System.Net;
using System.Web.Helpers;
using Ionic.Zip;

namespace SESM.Tools.Helpers
{
    public class GithubHelper
    {
        public static string UpdateIsAvailable()
        {
            string[] files = Directory.GetFiles(SESMConfigHelper.SEDataPath, "SEServerExtender_*.zip",
                SearchOption.TopDirectoryOnly);
            string data = GetGithubData();
            if (files.Length == 0)
            {
                if (SESMConfigHelper.SESEDev)
                {
                    return GetLastetDevVersion(data);
                }
                else
                {
                    return GetLastetVersion(data);
                }
            }
            if(files.Length != 1)
            {
                return null;
            }

            if(SESMConfigHelper.SESEDev)
            {
                string last = GetLastetDevVersion(data);
                if (string.IsNullOrEmpty(last) || last == PathHelper.GetLastLeaf(files[0]))
                    return null;
                return GetLastetDevVersionURL(data);
            }
            else
            {
                string last = GetLastetVersion(data);
                if(string.IsNullOrEmpty(last) || last == PathHelper.GetLastLeaf(files[0]))
                    return null;
                return GetLastetVersionURL(data);
            }
        }

        public static string GetLastetVersion(string data)
        {
            dynamic result;
            try
            {
                result = Json.Decode(data);
            }
            catch(Exception)
            {
                return null;
            }
            foreach(dynamic item in result)
            {
                if(!item.prerelease)
                {
                    return item.assets[0].name;
                }
            }
            return null;
        }

        public static string GetLastetVersion()
        {
            string data = GetGithubData();
            return GetLastetVersion(data);
        }

        public static string GetLastetVersionURL(string data)
        {
            dynamic result;
            try
            {
                result = Json.Decode(data);
            }
            catch(Exception)
            {
                return null;
            }
            foreach(dynamic item in result)
            {
                if(!item.prerelease)
                {
                    return item.assets[0].browser_download_url;
                }
            }
            return null;
        }

        public static string GetLastetDevVersion(string data)
        {
            dynamic result;
            try
            {
                result = Json.Decode(data);
            }
            catch(Exception)
            {
                return null;
            }
            foreach(dynamic item in result)
            {
                if(item.prerelease)
                {
                    return item.assets[0].name;
                }
            }
            return null;
        }

        public static string GetLastetDevVersion()
        {
            string data = GetGithubData();
            return GetLastetDevVersion(data);
        }

        public static string GetLastetDevVersionURL(string data)
        {
            dynamic result;
            try
            {
                result = Json.Decode(data);
            }
            catch(Exception)
            {
                return null;
            }
            foreach(dynamic item in result)
            {
                if(item.prerelease)
                {
                    return item.assets[0].browser_download_url;
                }
            }
            return null;
        }

        private static string GetGithubData()
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "SESM V2");
            return client.DownloadString("https://api.github.com/repos/SEModCommunity/SE-Community-Mod-API/releases");
        }

        public static void CleanupUpdate()
        {
            string[] files = Directory.GetFiles(SESMConfigHelper.SEDataPath, "SEServerExtender_*.zip", SearchOption.TopDirectoryOnly);
            foreach(var item in files)
            {
                File.Delete(item);
            }
        }

        public static void DownloadUpdate(string url)
        {
            WebRequest objRequest = HttpWebRequest.Create(url);
            WebResponse objResponse = objRequest.GetResponse();
            FileStream fileStream = new FileStream(SESMConfigHelper.SEDataPath + PathHelper.GetLastLeaf(url), FileMode.Create, FileAccess.Write, FileShare.None);
            objResponse.GetResponseStream().CopyTo(fileStream);
        }

        public static void ApplyUpdate()
        {
            string[] files = Directory.GetFiles(SESMConfigHelper.SEDataPath, "SEServerExtender_*.zip", SearchOption.TopDirectoryOnly);
            if(files.Length != 1)
            {
                // If multiple zip available, we don't risk an update 
                return;
            }
            using(ZipFile zip = new ZipFile(files[0]))
            {
                zip.ExtractAll(SESMConfigHelper.SEDataPath + "DedicatedServer64");
            }
        }
    }
}