﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using Ionic.Zip;
using NLog;

namespace SESM.Tools.Helpers
{
    public class GithubHelper
    {
        public static string UpdateIsAvailable()
        {
            string[] files = Directory.GetFiles(SESMConfigHelper.SEDataPath, "SEServerExtender*.zip", SearchOption.TopDirectoryOnly);
            string data = GetGithubData();
            if(files.Length == 0)
            {
                return GetLastURL(data, SESMConfigHelper.SESEAutoUpdateUseDev).ToString();
            }
            if(files.Length != 1)
            {
                List<FileInfo> fileInfos = files.Select(file => new FileInfo(file)).OrderBy(x => x.LastWriteTime).ToList();

                for(int i = 1; i < fileInfos.Count; i++)
                {
                    try
                    {
                        File.Delete(fileInfos[i].FullName);
                    }
                    catch(Exception ex)
                    {
                        Logger exceptionLogger = LogManager.GetLogger("GenericExceptionLogger");
                        exceptionLogger.Fatal("Caught Exception in UpdateIsAvailable when deleting " + fileInfos[i].Name, ex);
                    }
                }
                files = Directory.GetFiles(SESMConfigHelper.SEDataPath, "SEServerExtender*.zip", SearchOption.TopDirectoryOnly);

            }

            string last = GetLastVersion(data, SESMConfigHelper.SESEAutoUpdateUseDev).ToString();
            if(string.IsNullOrEmpty(last) || last == PathHelper.GetLastLeaf(files[0]))
                return null;
            return GetLastURL(data, SESMConfigHelper.SESEAutoUpdateUseDev);
        }

        public static string GetLastURL(string data, bool useDev)
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
                if((useDev && item.prerelease) || (!useDev && !item.prerelease))
                {
                    return item.assets[0].browser_download_url;
                }
            }
            return null;
        }

        public static Version GetLastVersion(string data, bool useDev)
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
                if((useDev && item.prerelease) || (!useDev && !item.prerelease))
                {
                    Match extract = Regex.Match(item.assets[0].name, "^.*v(.*)-.*$");
                    if (extract.Groups.Count != 2)
                        return null;
                    return new Version("0." + extract.Groups[1].Value);
                }
            }
            return null;
        }

        public static string GetGithubData()
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "SESM V2");
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

        public static void CleanupUpdate()
        {
            string[] files = Directory.GetFiles(SESMConfigHelper.SEDataPath, "SEServerExtender*.zip", SearchOption.TopDirectoryOnly);
            foreach(var item in files)
            {
                try
                {
                    File.Delete(item);
                }
                catch(Exception ex)
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

        public static void ApplyUpdate()
        {
            string[] files = Directory.GetFiles(SESMConfigHelper.SEDataPath, "SEServerExtender*.zip", SearchOption.TopDirectoryOnly);
            if(files.Length != 1)
            {
                // If multiple zip available or no zip, we don't risk an update 
                return;
            }
            using(ZipFile zip = new ZipFile(files[0]))
            {
                zip.ExtractAll(SESMConfigHelper.SEDataPath + "DedicatedServer64",
                    ExtractExistingFileAction.OverwriteSilently);
            }
        }
    }
}