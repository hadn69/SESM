using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using Ionic.Zip;
using SESM.Controllers.ActionFilters;
using SESM.DAL;
using SESM.DTO;
using SESM.Models.Views.Settings;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    public class SettingsController : Controller
    {
        readonly DataContext _context = new DataContext();

        //
        // GET: Settings
        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        public ActionResult Index()
        {
            SettingsViewModel model = new SettingsViewModel();

            model.Prefix = SESMConfigHelper.GetPrefix();
            model.SESavePath = SESMConfigHelper.GetSESavePath();
            model.SEDataPath = SESMConfigHelper.GetSEDataPath();
            model.Arch = SESMConfigHelper.GetArch();
            model.AddDateToLog = SESMConfigHelper.GetAddDateToLog();
            model.SendLogToKeen = SESMConfigHelper.GetSendLogToKeen();

            return View(model);
        }

        //
        // POST: Settings
        [HttpPost]
        [LoggedOnly]
        [SuperAdmin]
        public ActionResult Index(SettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                ServerProvider srvPrv = new ServerProvider(_context);
                bool flag = false;
                if (model.SEDataPath.Substring(model.SEDataPath.Length - 1, 1) != @"\")
                {
                    flag = true;
                    ModelState.AddModelError("DataPath", @"The server path must end with \");
                }
                if (model.SESavePath.Substring(model.SESavePath.Length - 1, 1) != @"\")
                {
                    flag = true;
                    ModelState.AddModelError("SavePath", @"The save path must end with \");
                }

                if (flag)
                    return View(model);

                if (model.Prefix != SESMConfigHelper.GetPrefix()
                    || model.SEDataPath != SESMConfigHelper.GetSEDataPath()
                    || model.SESavePath != SESMConfigHelper.GetSESavePath()
                    || model.Arch != SESMConfigHelper.GetArch()
                    || model.AddDateToLog != SESMConfigHelper.GetAddDateToLog()
                    || model.SendLogToKeen != SESMConfigHelper.GetSendLogToKeen())
                {
                    // Getting started server list
                    List<EntityServer> listStartedServ = srvPrv.GetAllServers().Where(item => srvPrv.GetState(item) == ServiceState.Running).ToList();

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

                    foreach (EntityServer item in srvPrv.GetAllServers())
                    {
                        ServiceHelper.UnRegisterService(ServiceHelper.GetServiceName(item));
                    }

                    if (model.SEDataPath != SESMConfigHelper.GetSEDataPath())
                    {
                        if (!Directory.Exists(model.SEDataPath))
                            Directory.CreateDirectory(model.SEDataPath);
                        if (Directory.Exists(model.SEDataPath + @"Content\"))
                            Directory.Delete(model.SEDataPath + @"Content\", true);
                        if (Directory.Exists(model.SEDataPath + @"DedicatedServer\"))
                            Directory.Delete(model.SEDataPath + @"DedicatedServer\", true);
                        if (Directory.Exists(model.SEDataPath + @"DedicatedServer64\"))
                            Directory.Delete(model.SEDataPath + @"DedicatedServer64\", true);

                        Directory.Move(SESMConfigHelper.GetSEDataPath() + @"Content\", model.SEDataPath + @"Content\");
                        Directory.Move(SESMConfigHelper.GetSEDataPath() + @"DedicatedServer\", model.SEDataPath + @"DedicatedServer\");
                        Directory.Move(SESMConfigHelper.GetSEDataPath() + @"DedicatedServer64\", model.SEDataPath + @"DedicatedServer64\");
                        SESMConfigHelper.SetSEDataPath(model.SEDataPath);
                    }

                    if (model.Prefix != SESMConfigHelper.GetPrefix())
                    {
                        foreach (EntityServer item in srvPrv.GetAllServers())
                        {
                            Directory.Move(SESMConfigHelper.GetSEDataPath() + ServiceHelper.GetServiceName(SESMConfigHelper.GetPrefix(), item),
                                            SESMConfigHelper.GetSEDataPath() + ServiceHelper.GetServiceName(model.Prefix, item));
                        }
                        SESMConfigHelper.SetPrefix(model.Prefix);
                    }

                    if (model.SESavePath != SESMConfigHelper.GetSESavePath())
                    {
                        Directory.Move(SESMConfigHelper.GetSESavePath(), model.SESavePath);
                        SESMConfigHelper.SetSESavePath(model.SESavePath);
                    }
                    SESMConfigHelper.SetArch(model.Arch);
                    SESMConfigHelper.SetAddDateToLog(model.AddDateToLog);
                    SESMConfigHelper.SetSendLogToKeen(model.SendLogToKeen);

                    foreach (EntityServer item in srvPrv.GetAllServers())
                    {
                        ServiceHelper.RegisterService(ServiceHelper.GetServiceName(item));
                    }

                    foreach (EntityServer item in listStartedServ)
                    {
                        ServiceHelper.StartService(ServiceHelper.GetServiceName(item));
                    }
                    RedirectToAction("Index", "Server");
                }
            }
            return View(model);
        }

        //
        // GET: Settings/UploadBin
        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        public ActionResult UploadBin()
        {
            return View();
        }

        //
        // POST: Settings/UploadBin
        [HttpPost]
        [LoggedOnly]
        [SuperAdmin]
        public ActionResult UploadBin(UploadBinViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!ZipFile.IsZipFile(model.ServerZip.InputStream, false))
                {
                    ModelState.AddModelError("ZipError", "Your File is not a valid zip file");
                    return View(model);
                }

                ServerProvider srvPrv = new ServerProvider(_context);

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

                model.ServerZip.InputStream.Seek(0, SeekOrigin.Begin);

                using (ZipFile zip = ZipFile.Read(model.ServerZip.InputStream))
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

                return RedirectToAction("Index", "Server");
            }
            return View(model);
        }

        //
        // GET: Settings/Diagnosis
        [HttpGet]
        public ActionResult Diagnosis()
        {
            if (!SESMConfigHelper.GetDiagnosis())
            {
                return RedirectToAction("Index", "Home");
            }
            DiagnosisViewModel model = new DiagnosisViewModel();

            
            if (_context.Database.Exists())
            {
                model.DatabaseConnexion.State = true;
                model.DatabaseConnexion.Message = "Connection to database successful";
            }
            else
            {
                model.DatabaseConnexion.State = false;
                model.DatabaseConnexion.Message = "Connection to database failed. <br/> Check your connexion string in SESM.config";
            }
            
            switch (SESMConfigHelper.GetArch())
            {
                case ArchType.x64:
                    if (System.Environment.Is64BitOperatingSystem)
                    {
                        model.ArchMatch.State = true;
                        model.ArchMatch.Message = "You are on a 64 Bits computer and you want to run 64 Bits servers. It will work !";
                    }
                    else
                    {
                        model.ArchMatch.State = false;
                        model.ArchMatch.Message = "You are on a 32 Bits computer and you want to run 64 Bits servers. It won't work ! <br/>Please consider changing your Architecture variable to x86";
                    }
                    break;
                case ArchType.x86:
                    if (System.Environment.Is64BitOperatingSystem)
                    {
                        model.ArchMatch.State = true;
                        model.ArchMatch.Message = "You are on a 64 Bits computer and you want to run 32 Bits servers. It will work ! <br/>(but you should consider switching your arch variable to x64 for better performances)";
                    }
                    else
                    {
                        model.ArchMatch.State = true;
                        model.ArchMatch.Message = "You are on a 32 Bits computer and you want to run 32 Bits servers. It will work !";
                    }
                    break;
            }


            if (System.IO.File.Exists(SESMConfigHelper.GetSEDataPath() + @"DedicatedServer\SpaceEngineersDedicated.exe"))
            {
                model.Binariesx86.State = true;
                model.Binariesx86.Message = "32 Bits Space Engineers bianries found at " + SESMConfigHelper.GetSEDataPath() + @"DedicatedServer\SpaceEngineersDedicated.exe";
            }
            else
            {
                model.Binariesx86.State = false;
                model.Binariesx86.Message = "32 Bits Space Engineers bianries not found at " + SESMConfigHelper.GetSEDataPath() + @"DedicatedServer\SpaceEngineersDedicated.exe<br/>You should try to reupload your game files or activate the auto update";
            }


            if (System.IO.File.Exists(SESMConfigHelper.GetSEDataPath() + @"DedicatedServer64\SpaceEngineersDedicated.exe"))
            {
                model.Binariesx64.State = true;
                model.Binariesx64.Message = "64 Bits Space Engineers bianries found at " + SESMConfigHelper.GetSEDataPath() + @"DedicatedServer64\SpaceEngineersDedicated.exe";
            }
            else
            {
                model.Binariesx64.State = false;
                model.Binariesx64.Message = "64 Bits Space Engineers bianries not found at " + SESMConfigHelper.GetSEDataPath() + @"DedicatedServer64\SpaceEngineersDedicated.exe<br/>You should try to reupload your game files or activate the auto update";
            }


            ServiceHelper.RegisterService("SESMDiagTest");
            if (ServiceHelper.DoesServiceExist("SESMDiagTest"))
            {
                model.ServiceCreation.State = true;
                model.ServiceCreation.Message = "Creation of the service \"SESMDiagTest\" successful";

                ServiceHelper.UnRegisterService("SESMDiagTest");
                if (!ServiceHelper.DoesServiceExist("SESMDiagTest"))
                {
                    model.ServiceDeletion.State = true;
                    model.ServiceDeletion.Message = "Deletion of the service \"SESMDiagTest\" successful";
                }
                else
                {
                    model.ServiceDeletion.State = false;
                    model.ServiceDeletion.Message = "Deletion of the service \"SESMDiagTest\" failed<br/>Check if the application pool have admin rights";
                }
            }
            else
            {
                model.ServiceCreation.State = false;
                model.ServiceCreation.Message = "Creation of the service \"SESMDiagTest\" failed<br/>Check if the application pool have admin rights";
                
                model.ServiceDeletion.State = null;
                model.ServiceDeletion.Message = "Deletion of the service \"SESMDiagTest\" irrelevant";
            }

            try
            {
                FileStream stream = System.IO.File.Create(@"C:\SESMDiagTest.bin");
                stream.Close();
                stream.Dispose();
                model.FileCreation.State = true;
                model.FileCreation.Message = @"Creation of the file C:\SESMDiagTest.bin successful";

                try
                {
                    System.IO.File.Delete(@"\testDiag.bin");
                    model.FileDeletion.State = true;
                    model.FileDeletion.Message = @"Deletion of the file C:\SESMDiagTest.bin successful";
                }
                catch (Exception)
                {
                    model.FileDeletion.State = false;
                    model.FileDeletion.Message = @"Deletion of the file C:\SESMDiagTest.bin failed <br/>Check if the application pool have admin rights";
                }


            }
            catch (Exception)
            {
                model.FileCreation.State = false;
                model.FileCreation.Message = @"Creation of the file C:\SESMDiagTest.bin failed <br/>Check if the application pool have admin rights";

                model.FileDeletion.State = null;
                model.FileDeletion.Message = @"Deletion of the file C:\SESMDiagTest.bin irrelevant";
            }

            UserProvider usrPrv = new UserProvider(_context);

            if (model.DatabaseConnexion.State == true)
            {
                model.SuperAdmin.State = false;
                model.SuperAdmin.Message = @"No super administrator found";

                foreach (EntityUser item in usrPrv.GetUsers())
                {
                    if (item.IsAdmin)
                    {
                        model.SuperAdmin.State = true;
                        model.SuperAdmin.Message = @"At least 1 super administrator found";
                    }
                }
            }
            else
            {
                model.SuperAdmin.State = null;
                model.SuperAdmin.Message = @"Super administrator search irrelevant";
            }
            return View(model);
        }

        [HttpGet]
        [LoggedOnly]
        [SuperAdmin]
        public ActionResult AutoUpdate()
        {
            AutoUpdateViewModel model = new AutoUpdateViewModel();
            model.AutoUpdate = SESMConfigHelper.GetAutoUpdate();
            model.UserName = SESMConfigHelper.GetAUUsername();

            return View(model);
        }

        [HttpPost]
        [LoggedOnly]
        [SuperAdmin]
        public ActionResult AutoUpdate(AutoUpdateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            SESMConfigHelper.SetAUUsername(model.UserName);
            SESMConfigHelper.SetAutoUpdate(model.AutoUpdate);

            if (!string.IsNullOrEmpty(model.Password))
            {
                SESMConfigHelper.SetAUPassword(model.Password);
            }
            model.Password = "";
            return RedirectToAction("Index", "Server");
        }
    }
}