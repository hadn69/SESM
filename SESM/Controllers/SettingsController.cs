﻿using System;
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
            model.EnumArch = SESMConfigHelper.GetArch();
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
                    || model.EnumArch != SESMConfigHelper.GetArch()
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
                            Directory.Move(model.SEDataPath + ServiceHelper.GetServiceName(SESMConfigHelper.GetPrefix(), item),
                                model.SEDataPath + ServiceHelper.GetServiceName(model.Prefix, item));
                        }
                        SESMConfigHelper.SetPrefix(model.Prefix);
                    }

                    if (model.SESavePath != SESMConfigHelper.GetSESavePath())
                    {
                        Directory.Move(SESMConfigHelper.GetSESavePath(), model.SESavePath);
                        SESMConfigHelper.SetSEDataPath(model.SESavePath);
                    }
                    SESMConfigHelper.SetArch(model.EnumArch);
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

                model.ServerZip.InputStream.Seek(0, SeekOrigin.Begin);

                using (ZipFile zip = ZipFile.Read(model.ServerZip.InputStream))
                {
                    Directory.Delete(SESMConfigHelper.GetSEDataPath(), true);
                    Directory.CreateDirectory(SESMConfigHelper.GetSEDataPath());
                    zip.ExtractAll(SESMConfigHelper.GetSEDataPath());
                }

                foreach (EntityServer item in listStartedServ)
                {
                    ServiceHelper.StartService(ServiceHelper.GetServiceName(item));
                }

                return RedirectToAction("Index","Server");
            }
            return View(model);
        }

        //
        // GET: Settings/Diagnosis
        [HttpGet]
        public ActionResult Diagnosis()
        {
            Configuration conf = WebConfigurationManager.OpenWebConfiguration("/web");

            if (conf.AppSettings.Settings["Diagnosis"].Value.ToLower() != "true")
            {
                return RedirectToAction("Index", "Home");
            }
            DiagnosisViewModel model = new DiagnosisViewModel();

            if (_context.Database.Exists())
                model.DatabaseConnexion = true;

            switch (SESMConfigHelper.GetArch())
            {
                case EnumArchType.X64:
                    if (System.Environment.Is64BitOperatingSystem)
                        model.ArchMatch = true;
                    break;
                case EnumArchType.X86:
                    model.ArchMatch = true;
                    break;
            }

            if (System.IO.File.Exists(SESMConfigHelper.GetSEDataPath() + @"DedicatedServer\SpaceEngineersDedicated.exe"))
                model.Binariesx86 = true;

            if (System.IO.File.Exists(SESMConfigHelper.GetSEDataPath() + @"DedicatedServer64\SpaceEngineersDedicated.exe"))
                model.Binariesx64 = true;

            ServiceHelper.RegisterService("SESMDiagTest");
            if (ServiceHelper.DoesServiceExist("SESMDiagTest"))
                model.ServiceCreation = true;

            ServiceHelper.UnRegisterService("SESMDiagTest");
            if (!ServiceHelper.DoesServiceExist("SESMDiagTest"))
                model.ServiceDeletion = true;

            try
            {
                FileStream stream = System.IO.File.Create(conf.AppSettings.Settings["SEDataPath"].Value + @"\testDiag.bin");
                stream.Close();
                stream.Dispose();
                model.FileCreation = true;
            }
            catch (Exception)
            {
            }

            try
            {
                System.IO.File.Delete(conf.AppSettings.Settings["SEDataPath"].Value + @"\testDiag.bin");
                model.FileDeletion = true;
            }
            catch (Exception)
            {
            }
            UserProvider usrPrv = new UserProvider(_context);
            foreach (EntityUser item in usrPrv.GetUsers())
            {
                if (item.IsAdmin)
                {
                    model.SuperAdmin = true;
                    break;
                }
            }

            return View(model);
        }
    }
}