using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Ionic.Zip;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Tools;
using SESM.Tools.API;
using SESM.Tools.Helpers;

namespace SESM.Controllers.API
{
    [APICheckLockdown]
    public class APIExplorerController : Controller, IAPIController
    {
        public DataContext CurrentContext { get; set; }

        public EntityServer RequestServer { get; set; }

        public APIExplorerController()
        {
            CurrentContext = new DataContext();
        }

        // POST: API/Explorer/GetDirectoryContent
        [HttpPost]
        [APIServerAccess("EXP-GDC", "SERVER_EXPLORER_LIST")]
        public ActionResult GetDirectoryContent()
        {
            // ** PARSING / ACCESS **
            string path = Request.Form["Path"];
            if (string.IsNullOrWhiteSpace(path))
                path = string.Empty;

            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(RequestServer), path));
            if (path.Substring(path.Length - 1) != "\\")
                path += "\\";

            if (!path.Contains(PathHelper.GetInstancePath(RequestServer)))
                return Content(XMLMessage.Error("EXP-GDC-BADPTH", "The directory isn't accessible for you, bad boy !").ToString());

            if (!Directory.Exists(path))
                return Content(XMLMessage.Error("EXP-GDC-BADDIR", "The directory don't exist").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("EXP-GDC-OK");

            IEnumerable<NodeInfo> directories = FSHelper.GetDirectories(path);

            foreach (NodeInfo directory in directories)
            {
                response.AddToContent(new XElement("Item",
                    new XElement("Type", "Directory"),
                    new XElement("Name", directory.Name),
                    new XElement("Size", directory.Size),
                    new XElement("Date", directory.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss"))));
            }

            IEnumerable<NodeInfo> files = FSHelper.GetFiles(path);

            foreach (NodeInfo file in files)
            {
                response.AddToContent(new XElement("Item",
                    new XElement("Type", "File"),
                    new XElement("Name", file.Name),
                    new XElement("Size", file.Size),
                    new XElement("Date", file.Timestamp.ToString("yyyy/MM/dd-HH:mm:ss"))));
            }

            return Content(response.ToString());
        }

        // POST: API/Explorer/GetDirectoryDirectories
        [HttpPost]
        [APIServerAccess("EXP-GDD", "SERVER_EXPLORER_LIST")]
        public ActionResult GetDirectoryDirectories()
        {
            // ** PARSING / ACCESS **
            string path = Request.Form["Path"];
            if (string.IsNullOrWhiteSpace(path))
                path = string.Empty;

            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(RequestServer), path));
            if (path.Substring(path.Length - 1) != "\\")
                path += "\\";


            if (!path.Contains(PathHelper.GetInstancePath(RequestServer)))
                return Content(XMLMessage.Error("EXP-GDD-BADPTH", "The directory isn't accessible for you, bad boy !").ToString());

            if (!Directory.Exists(path))
                return Content(XMLMessage.Error("EXP-GDD-BADDIR", "The directory don't exist").ToString());

            // ** PROCESS **
            XMLMessage response = new XMLMessage("EXP-GDD-OK");

            IEnumerable<NodeInfo> directories = FSHelper.GetDirectories(path);

            foreach (NodeInfo directory in directories)
            {
                response.AddToContent(new XElement("Item",
                    new XElement("Name", directory.Name)));
            }
            return Content(response.ToString());
        }

        // POST: API/Explorer/Delete
        [HttpPost]
        [APIServerAccess("EXP-DEL", "SERVER_EXPLORER_DELETE")]
        public ActionResult Delete()
        {
            // ** PARSING / ACCESS **
            string[] paths = Request.Form["Paths"].Split(':');

            for (int i = 0; i < paths.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(paths[i]))
                    paths[i] = string.Empty;
                paths[i] = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(RequestServer), paths[i]));

                if (!paths[i].Contains(PathHelper.GetInstancePath(RequestServer)))
                    return Content(XMLMessage.Error("EXP-DEL-BADPTH", "The object isn't accessible for you, bad boy !").ToString());

                if (!(Directory.Exists(paths[i]) || System.IO.File.Exists(paths[i])))
                    return Content(XMLMessage.Error("EXP-DEL-BADEX", "The object don't exist").ToString());
            }

            // ** PROCESS **
            foreach (string item in paths)
            {
                if (Directory.Exists(item))
                {
                    try
                    {
                        Directory.Delete(item, true);
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (System.IO.File.Exists(item))
                {
                    try
                    {
                        System.IO.File.Delete(item);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return Content(XMLMessage.Success("EXP-DEL-OK", "Object(s) deleted").ToString());
        }

        // POST: API/Explorer/Rename
        [HttpPost]
        [APIServerAccess("EXP-RNM", "SERVER_EXPLORER_RENAME")]
        public ActionResult Rename()
        {
            // ** PARSING / ACCESS **
            string path = Request.Form["Path"];
            string newName = Request.Form["NewName"];
            string oldname;

            path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(RequestServer), path));

            if (!path.Contains(PathHelper.GetInstancePath(RequestServer)))
                return Content(XMLMessage.Error("EXP-RNM-BADPTH", "The object isn't accessible for you, bad boy !").ToString());

            if (!(Directory.Exists(path) || System.IO.File.Exists(path)))
                return Content(XMLMessage.Error("EXP-RNM-BADEX", "The object don't exist").ToString());

            if (!Regex.IsMatch(newName, "^[^:/\\*?\"<>|]*$"))
                return Content(XMLMessage.Error("EXP-RNM-BADNAM", "Invalid new name, the new name of the object can't contain the folowing characters : \\ / : * ? \" < > |").ToString());

            // ** PROCESS **
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                oldname = di.Name;
                path = di.Parent.FullName;
                if (Directory.Exists(path + "\\" + newName) || System.IO.File.Exists(path + "\\" + newName))
                    return Content(XMLMessage.Error("EXP-RNM-DIREX", "Invalid new name, a directory with the same name already exist").ToString());

                try
                {
                    Directory.Move(path + "\\" + oldname, path + "\\" + newName);
                }
                catch (Exception ex)
                {
                    return Content(XMLMessage.Error("EXP-RNM-DEX", "Directory failed to be renamed (ex : " + ex.Message + ")").ToString());
                }
                return Content(XMLMessage.Success("EXP-RNM-OK", "Directory renamed successfuly").ToString());
            }
            else if (System.IO.File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                oldname = fi.Name;
                path = fi.DirectoryName;

                string oldPath = path + "\\" + oldname;
                string newPath = path + "\\" + newName;

                if (Directory.Exists(newPath) || System.IO.File.Exists(newPath))
                    return Content(XMLMessage.Error("EXP-RNM-FILEX", "Invalid new name, a file with the same name already exist").ToString());

                if (SESMConfigHelper.BlockDll &&
                    !AuthHelper.HasAccess(RequestServer, "SERVER_EXPLORER_RENAMETODLL") &&
                    !oldPath.Contains(PathHelper.GetInstancePath(RequestServer) + @"Mods") &&
                    newPath.Contains(PathHelper.GetInstancePath(RequestServer) + @"Mods") &&
                    Directory.GetFiles(oldPath).Any(file => file.ToLower().EndsWith(".dll")))
                    return Content(XMLMessage.Error("EXP-RNM-BADDLL", "The old folder contain dll and can't be moved to anywhere insid ethe mod folder").ToString());

                try
                {
                    if (newName.Split('.').Last().ToLower() == "dll" && SESMConfigHelper.BlockDll && path.Contains(PathHelper.GetInstancePath(RequestServer) + @"Mods") && !AuthHelper.HasAccess(RequestServer, "SERVER_EXPLORER_RENAMETODLL"))
                        return Content(XMLMessage.Error("EXP-RNM-BADDLL", "Invalid new name, you can't rename a file to a .dll one").ToString());

                    System.IO.File.Move(oldPath, newPath);

                }
                catch (Exception ex)
                {
                    return Content(XMLMessage.Error("EXP-RNM-FEX", "File failed to be renamed (ex : " + ex.Message + ")").ToString());
                }
                return Content(XMLMessage.Success("EXP-RNM-OK", "File renamed successfuly").ToString());
            }
            return Content(XMLMessage.Error("EXP-RNM-TRD", "Object of the 3rd Types (please report this ASAP)").ToString());
        }

        // POST: API/Explorer/Download
        [HttpPost]
        [APIServerAccess("EXP-DL", "SERVER_EXPLORER_DOWNLOAD")]
        public ActionResult Download()
        {
            // ** PARSING / ACCESS **
            string[] paths = Request.Form["Paths"].Split(':');

            if (paths.Length == 1 && string.IsNullOrWhiteSpace(paths[0]))
                return Content(XMLMessage.Error("EXP-DL-MISPTH", "The Paths field must be provided").ToString());

            for (int i = 0; i < paths.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(paths[i]))
                    paths[i] = string.Empty;
                paths[i] = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(RequestServer), paths[i]));

                if (!paths[i].Contains(PathHelper.GetInstancePath(RequestServer)))
                    return Content(XMLMessage.Error("EXP-DL-BADPTH", "The object isn't accessible for you, bad boy !").ToString());

                if (!(Directory.Exists(paths[i]) || System.IO.File.Exists(paths[i])))
                    return Content(XMLMessage.Error("EXP-DL-BADEX", "The object don't exist").ToString());
            }

            // ** PROCESS **
            try
            {
                Response.Clear();
                if (paths.Length == 1)
                {

                    if (System.IO.File.Exists(paths[0])) // File
                    {
                        Response.ContentType = MimeMapping.GetMimeMapping(PathHelper.GetLastLeaf(paths[0]));
                        Response.AddHeader("Content-Disposition",
                            String.Format("attachment; filename={0}", PathHelper.GetLastLeaf(paths[0])));

                        using (FileStream fileStream = new FileStream(paths[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (StreamReader streamReader = new StreamReader(fileStream))
                            {
                                fileStream.CopyTo(Response.OutputStream);
                                Response.OutputStream.Flush();
                            }
                        }
                        Response.End();
                        return null;
                    }
                    else // Directory
                    {
                        Response.ContentType = "application/zip";

                        Response.AddHeader("Content-Disposition",
                            String.Format("attachment; filename={0}", RequestServer.Name + "-" + PathHelper.GetLastLeaf(paths[0]) + "-"
                                                                      + DateTime.Now.ToString("yyyyMMddHHmm") + ".zip"));
                        using (ZipFile zip = new ZipFile())
                        {
                            zip.AddDirectory(paths[0]);

                            zip.Save(Response.OutputStream);
                            Response.End();
                            return null;
                        }
                    }
                }
                else
                {
                    using (ZipFile zip = new ZipFile())
                    {
                        foreach (string item in paths)
                        {
                            if ((Directory.Exists(item))) // Folder
                                zip.AddDirectory(item, new DirectoryInfo(item).Name);
                            else // File
                                zip.AddFile(item, string.Empty);
                        }
                        Response.ContentType = "application/zip";
                        string zipName = string.Empty;
                        zipName = Directory.Exists(paths[0])
                            ? new DirectoryInfo(paths[0]).Parent.Name
                            : new FileInfo(paths[0]).Directory.Name;
                        if (zipName == PathHelper.GetFSDirName(RequestServer))
                        {
                            Response.AddHeader("Content-Disposition",
                                String.Format("attachment; filename={0}", RequestServer.Name + "-"
                                                                          + DateTime.Now.ToString("yyyyMMddHHmm") +
                                                                          ".zip"));
                        }
                        else
                        {
                            Response.AddHeader("Content-Disposition",
                                String.Format("attachment; filename={0}", RequestServer.Name + "-" + zipName + "-"
                                                                          + DateTime.Now.ToString("yyyyMMddHHmm") +
                                                                          ".zip"));
                        }
                        zip.Save(Response.OutputStream);
                        Response.End();
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(XMLMessage.Error("EXP-DL-EX", "Error Occured (ex : " + ex.Message + ")").ToString());
            }
        }

        // POST: API/Explorer/NewFolder
        [HttpPost]
        [APIServerAccess("EXP-NFO", "SERVER_EXPLORER_CREATE_FOLDER")]
        public ActionResult NewFolder()
        {
            // ** PARSING / ACCESS **
            string path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(RequestServer), Request.Form["Path"]));

            if (!path.Contains(PathHelper.GetInstancePath(RequestServer)))
                return Content(XMLMessage.Error("EXP-NFO-BADPTH", "The object isn't accessible for you, bad boy !").ToString());

            if ((Directory.Exists(path) || System.IO.File.Exists(path)))
                return Content(XMLMessage.Error("EXP-NFO-BADEX", "An object with the same name already exist").ToString());

            if (!Regex.IsMatch(PathHelper.GetLastLeaf(path), "^[^:/\\*?\"<>|]*$"))
                return Content(XMLMessage.Error("EXP-NFO-BADNAM", "Invalid name, the name of the folder can't contain the folowing characters : \\ / : * ? \" < > |").ToString());

            // ** PROCESS **
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                return Content(XMLMessage.Error("EXP-NFO-EX", "An error occurend while creating the folder (exception : " + ex.Message + ")").ToString());
            }
            return Content(XMLMessage.Success("EXP-NFO-OK", "Folder created").ToString());
        }

        // POST: API/Explorer/NewFile
        [HttpPost]
        [APIServerAccess("EXP-NFI", "SERVER_EXPLORER_CREATE_FILE")]
        public ActionResult NewFile()
        {
            // ** PARSING / ACCESS **
            string path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(RequestServer), Request.Form["Path"]));

            if (!path.Contains(PathHelper.GetInstancePath(RequestServer)))
                return Content(XMLMessage.Error("EXP-NFI-BADPTH", "The object isn't accessible for you, bad boy !").ToString());

            if ((Directory.Exists(path) || System.IO.File.Exists(path)))
                return Content(XMLMessage.Error("EXP-NFI-BADEX", "An object with the same name already exist").ToString());

            if (!Regex.IsMatch(PathHelper.GetLastLeaf(path), "^[^:/\\*?\"<>|]*$"))
                return Content(XMLMessage.Error("EXP-NFI-BADNAM", "Invalid name, the name of the file can't contain the folowing characters : \\ / : * ? \" < > |").ToString());

            // ** PROCESS **
            try
            {
                System.IO.File.Create(path);
            }
            catch (Exception ex)
            {
                return Content(XMLMessage.Error("EXP-NFI-EX", "An error occurend while creating the file (exception : " + ex.Message + ")").ToString());
            }
            return Content(XMLMessage.Success("EXP-NFI-OK", "File created").ToString());
        }

        // POST: API/Explorer/Upload
        [HttpPost]
        [APIServerAccess("EXP-UP", "SERVER_EXPLORER_UPLOAD")]
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> Files)
        {
            // ** INIT **
            EntityUser user = Session["User"] as EntityUser;


            // ** PARSING / ACCESS **
            string path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(RequestServer), Request.Form["Path"]));

            if (!path.Contains(PathHelper.GetInstancePath(RequestServer)))
                return Content(XMLMessage.Error("EXP-UP-BADPTH", "The object isn't accessible for you, bad boy !").ToString());

            bool overwriteFiles = Request.Form["Overwrite"].ToLower() == "true";
            bool extractZip = Request.Form["Extract"].ToLower() == "true";

            bool blockDll = SESMConfigHelper.BlockDll && !AuthHelper.HasAccess(RequestServer, "SERVER_EXPLORER_UPLOADDLL");

            // ** PROCESS **
            try
            {
                foreach (HttpPostedFileBase file in Files)
                {
                    if (extractZip && file.FileName.Split('.').Last().ToLower() == "zip")
                    {
                        using (ZipFile zip = ZipFile.Read(file.InputStream))
                        {
                            if (blockDll)
                            {
                                bool testDll = false;
                                foreach (ZipEntry item in zip)
                                {
                                    if ((path + item.FileName.Replace("/", @"\")).Contains(PathHelper.GetInstancePath(RequestServer) + @"Mods")
                                        && item.FileName.Split('.').Last().ToLower() == "dll")
                                    {
                                        testDll = true;
                                        break;
                                    }
                                }
                                if (testDll)
                                    continue;
                            }

                            zip.ExtractAll(path,
                                overwriteFiles
                                    ? ExtractExistingFileAction.OverwriteSilently
                                    : ExtractExistingFileAction.Throw);
                        }
                    }
                    else
                    {
                        if (blockDll && file.FileName.Split('.').Last().ToLower() == "dll" && path.Contains(PathHelper.GetInstancePath(RequestServer) + @"Mods"))
                            continue;
                        FSHelper.SaveStream(file.InputStream, path + "\\" + file.FileName, overwriteFiles);
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(XMLMessage.Error("EXP-UP-EX", "An error occurend while writing file (exception : " + ex.Message + ")").ToString());
            }
            return Content(XMLMessage.Success("EXP-UP-OK", "File(s) uploaded").ToString());
        }

        // POST: API/Explorer/GetFileContent
        [HttpPost]
        [APIServerAccess("EXP-GFC", "SERVER_EXPLORER_READFILE")]
        public ActionResult GetFileContent()
        {
            // ** PARSING / ACCESS **
            string path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(RequestServer), Request.Form["Path"]));

            if (!path.Contains(PathHelper.GetInstancePath(RequestServer)))
                return Content(XMLMessage.Error("EXP-GFC-BADPTH", "The object isn't accessible for you, bad boy !").ToString());

            if (!System.IO.File.Exists(path))
                return Content(XMLMessage.Error("EXP-GFC-BADEX", "File doesn't exist").ToString());

            if (new FileInfo(path).Length >= (512 * 1024))
                return Content(XMLMessage.Error("EXP-GFC-TOOBIG", "File is too big").ToString());

            // ** PROCESS **
            //string content = System.IO.File.ReadAllText(path);

            XMLMessage response = new XMLMessage("EXP-GFC-OK");
            try
            {
                string data = string.Empty;
                using (FileStream fileStream = new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        data = streamReader.ReadToEnd();
                    }
                }
                response.AddToContent(new XCData(data));
                return Content(response.ToString());
            }
            catch (Exception)
            {
                return Content(XMLMessage.Error("EXP-GFC-EX", "Failed to read file").ToString());
            }
        }

        // POST: API/Explorer/SetFileContent
        [HttpPost]
        [ValidateInput(false)]
        [APIServerAccess("EXP-SFC", "SERVER_EXPLORER_WRITEFILE")]
        public ActionResult SetFileContent()
        {
            // ** PARSING / ACCESS **
            string path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(RequestServer), Request.Form["Path"]));

            if (!path.Contains(PathHelper.GetInstancePath(RequestServer)))
                return Content(XMLMessage.Error("EXP-SFC-BADPTH", "The object isn't accessible for you, bad boy !").ToString());

            if (!System.IO.File.Exists(path))
                return Content(XMLMessage.Error("EXP-SFC-BADEX", "File doesn't exist").ToString());

            string data = Request.Form["Data"];

            // ** PROCESS **
            try
            {
                System.IO.File.WriteAllText(path, data, new UTF8Encoding(false));
                return Content(XMLMessage.Success("EXP-SFC-OK", "Data writed").ToString());
            }
            catch (Exception)
            {
                return Content(XMLMessage.Error("EXP-SFC-EX", "Failed to write file").ToString());
            }
        }

        // POST: API/Explorer/GetDllVersion
        [HttpPost]
        [APIServerAccess("EXP-GDV", "SERVER_EXPLORER_READFILE")]
        public ActionResult GetDllVersion()
        {
            // ** PARSING / ACCESS **
            string path = Path.GetFullPath(Path.Combine(PathHelper.GetInstancePath(RequestServer), Request.Form["Path"]));

            if (!path.Contains(PathHelper.GetInstancePath(RequestServer)))
                return Content(XMLMessage.Error("EXP-GDV-BADPTH", "The object isn't accessible for you, bad boy !").ToString());

            if (!System.IO.File.Exists(path))
                return Content(XMLMessage.Error("EXP-GDV-BADEX", "File doesn't exist").ToString());

            if (new FileInfo(path).Extension.ToLower() != ".dll")
                return Content(XMLMessage.Error("EXP-GDV-NOTDLL", "File is not a dll").ToString());

            // ** PROCESS **
            try
            {
                return Content(XMLMessage.Success("EXP-GDV-OK", AssemblyName.GetAssemblyName(path).Version.ToString()).ToString());
            }
            catch (Exception)
            {
                return Content(XMLMessage.Error("EXP-GDV-EX", "Failed to read file").ToString());
            }
        }
    }
}