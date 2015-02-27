using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Web.Mvc;
using SESM.DAL;
using SESM.DTO;
using SESM.Models.Views.Server;
using SESM.Tools;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    public class ExternalController : Controller
    {
        // GET: External/Version
        public ActionResult Version()
        {
            return Content(Constants.GetVersion());
        }

        // GET: External/Legal
        public ActionResult Legal()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SESM Version " + Constants.GetVersion());
            sb.AppendLine("This software is Licenced under the SESM End User Licence Agreement, for personal, non commercial use only");
            sb.AppendLine();
            sb.AppendLine("-- SESM EULA --");
            sb.AppendLine("PLEASE READ THIS CAREFULLY BEFORE YOU DOWNLOAD/INSTALL THIS SOFTWARE");
            sb.AppendLine("");
            sb.AppendLine("1. Introduction ");
            sb.AppendLine("	1.1. THIS IS A LEGALLY BINDING MULTI PARTY AGREEMENT BETWEEN YOU AND RÉMY GRANDIN AND HOSTASPACE. BY INSTALLING, COPYING OR OTHERWISE USING SPACE ENGINEERS SERVER MANAGER (HERINAFTER KNOWN AS \"SESM\") SOFTWARE INCLUDING UPGRADES AND RELATED DOCUMENTATION. FOR ANY PURPOSE, YOU ARE AGREEING TO BE BOUND BY THIS LICENCE INCLUDING WITHOUT LIMITATION THE EXCLUSIONS AND LIMITATIONS OF OUR LIABILITY CONTAINED THEREIN. IF YOU DO NOT AGREE WITH THE TERMS AND CONDITIONS OF THIS LICENCE YOU MAY NOT INSTALL, COPY OR OTHERWISE USE SPACE ENGINEERS SERVER MANAGER IN ANY WAY. IF YOU HAVE A DIFFERENT LICENSE AGREEMENT SIGNED BY RÉMY GRANDIN AND HOSTASPACE YOU WILL THEN BE BOUND UNDER THAT LICENSE AGREEMENT. ");
            sb.AppendLine("	1.2. Space Engineers Server Manager is protected by copyright laws and international copyright treaties, as well as other intellectual property laws and treaties. Any unauthorized use of third software will result in forfeiture of any and all rights to said software.");
            sb.AppendLine("");
            sb.AppendLine("2. Grant of license ");
            sb.AppendLine("	2.1. You are not permitted to install or use SESM except in accordance with this license. In consideration of your agreement to the terms of this License, Rémy Grandin grants you a non-exclusive right ('License') to install and use SESM as permitted by this License. You are not permitted to use SESM for any and all commercial and business purposes, your licence to use SESM only permits you to use SESM for non-commercial, personal use.");
            sb.AppendLine("	2.2. YOU ARE PERMITTED:");
            sb.AppendLine("		2.2.1. To install, store and use SESM on one or more computers at any one time.");
            sb.AppendLine("		2.2.2. Use SESM personal use for you and/or your group, which you use for non-commercial purposes, whereby you make no monetary value through any and all use of SESM.");
            sb.AppendLine("		2.2.3. To make necessary back-up copies of SESM in support of your permitted use of SESM provided you label the back-up copy with the, Rémy Grandin copyright notice (SESM EULA). This License does not allow you to make any other copies of the whole or any part of SESM.");
            sb.AppendLine("	2.3. YOU ARE NOT PERMITTED OR MAY PERMIT OR ALLOW OTHERS TO:");
            sb.AppendLine("		2.3.1. Install, use or copy SESM except as permitted by this License.");
            sb.AppendLine("		2.3.2. Transfer, assign or sub-license SESM or this License on a permanent or temporary basis to any other party.");
            sb.AppendLine("		2.3.3. Sell, distribute, rent, loan, lease, or sub-license SESM.");
            sb.AppendLine("		2.3.4. Allow any party to resell services through this software.");
            sb.AppendLine("		2.3.5. Alter, adapt, merge, modify or translate SESM in any way for any purpose, including, without limitation, for error correction");
            sb.AppendLine("		2.3.6. Reverse-engineer, disassemble or decompile SESM.");
            sb.AppendLine("		2.3.7. Copy or distribute the documentation accompanying SESM except as reasonably required for your own use in accordance with the terms of this License. ");
            sb.AppendLine("		2.3.8. Share your registration information with others.");
            sb.AppendLine("		2.3.9. Allow any competing software company, employees or affiliate of said companies access to the SESM software, including setup files and/or panel website access.");
            sb.AppendLine("		2.3.10. Install if you are an owner, employee or affiliate of any competing company or software product.");
            sb.AppendLine("		2.3.11 Use SESM as a business, corporation or any other entity who makes a profit or whose purpose it is to make a profit.");
            sb.AppendLine("		2.3.12 Use SESM for any and all commercial purposes, whereby money is made through the use of SESM.");
            sb.AppendLine("3. Free copies of SESM");
            sb.AppendLine("	3.1. SESM is available as free for non-commercial, personal use.");
            sb.AppendLine("");
            sb.AppendLine("4. Limited Liability Disclaimer");
            sb.AppendLine("	4.1. Rémy Grandin will not be held responsible for any damages consequential and otherwise arising from the use of said software including but not limited, financial loss or damage caused by misuse or a fault in the software.");
            sb.AppendLine("	4.2. You agree to hold Rémy Grandin, and its affiliates harmless of any legal action taken against you and/or your company arising from the use or fault of this software.");
            sb.AppendLine("	4.3. Furthermore you agree to pay any legal fees and expenses allowed by law which may be a result of legal proceedings against Rémy Grandin and/or its affiliates due to the use of this software.");
            sb.AppendLine("");
            sb.AppendLine("5. International Export Disclaimer");
            sb.AppendLine("	5.1. Due to the nature of the encryption methods that the software uses you must agree that you will not export, install or run this software to or within countries which the United States does not have open trade agreements with.");
            sb.AppendLine("	5.2. If the software is knowingly exported, installed or run on hardware not within the United States or countries it has trade agreements with, the person or persons doing so shall accept all liability for their actions, and hold Rémy Grandin and their affiliates harmless from any legal proceedings resulting in such actions.");
            sb.AppendLine("");
            sb.AppendLine("6. Closing Statement");
            sb.AppendLine("	6.1. This license agreement supersedes any verbal agreement which has been previously granted. Any written agreement may supersede this agreement if signed by both Rémy Grandin and HostASpace.");
            sb.AppendLine("	6.2. This license agreement and its term are governed by the United Kingdom. You hereby irrevocably attorn and submit to the non-exclusive jurisdiction of the courts of United Kingdom therefrom. If any provision shall be considered unlawful, void or otherwise unenforceable, then that provision shall be deemed severable from this License and not affect the validity and enforceability of any other provisions.");
            sb.AppendLine("	6.3. Any violation of the current license terms will be grounds for immediate license termination. Rémy Grandin and HostASpace also reserves the right to pursue legal proceedings against any and all parties involved in the breech of this license agreement.");
            sb.AppendLine(
                "	6.4. HostASpace reserves the right to change this license agreement without notice, but you will be asked to accept it again upon a new version being released.");
            sb.AppendLine("-- END OF SESM EULA --");
            sb.AppendLine();
            sb.AppendLine("Currently allowed business and commercial users : ");
            sb.AppendLine("- Hostaspace");

            return Content(sb.ToString());
        }

        // GET: External
        public ActionResult RandomRGB(int id = 10)
        {
            if (SESMConfigHelper.DiagnosisEnabled)
            {
                int size = id;
                Bitmap myBitmap = new Bitmap(size, size);
                Random rnd = new Random();

                for(int i = 0; i < size; i++)
                {
                    for(int j = 0; j < size; j++)
                    {
                        Byte[] b = new Byte[3];
                        rnd.NextBytes(b);
                        myBitmap.SetPixel(i, j, Color.FromArgb(b[0], b[1], b[2]));
                    }
                }
                Response.Clear();
                Response.ContentType = "image/png";
                MemoryStream memStream = new MemoryStream();
                myBitmap.Save(memStream, ImageFormat.Png);
                memStream.Position = 0;
                memStream.CopyTo(Response.OutputStream);
                Response.End();
            }

            return null;
        }
        public ActionResult RandomBW(int id = 10)
        {
            if (SESMConfigHelper.DiagnosisEnabled)
            {
                int size = id;
                Bitmap myBitmap = new Bitmap(size, size);
                Random rnd = new Random();

                for(int i = 0; i < size; i++)
                {
                    for(int j = 0; j < size; j++)
                    {
                        Byte[] b = new Byte[1];
                        rnd.NextBytes(b);
                        myBitmap.SetPixel(i, j, Color.FromArgb(b[0], b[0], b[0]));
                    }
                }
                Response.Clear();
                Response.ContentType = "image/png";
                MemoryStream memStream = new MemoryStream();
                myBitmap.Save(memStream, ImageFormat.Png);
                memStream.Position = 0;
                memStream.CopyTo(Response.OutputStream);
                Response.End();
            }
            return null;
        }

        public ActionResult SignGen(int id)
        {
            DataContext _context = new DataContext();
            ServerProvider srvPrv = new ServerProvider(_context);

            EntityServer serv = srvPrv.GetServer(id);

            ServerConfigHelper serverConfig = new ServerConfigHelper();
            serverConfig.LoadFromServConf(PathHelper.GetConfigurationFilePath(serv));

            ServerViewModel serverView = new ServerViewModel();
            serverView = serverConfig.ParseOut(serverView);
            serverView.Name = serv.Name;
            serverView.IsLvl1BackupEnabled = serv.IsLvl1BackupEnabled;
            serverView.IsLvl2BackupEnabled = serv.IsLvl2BackupEnabled;
            serverView.IsLvl3BackupEnabled = serv.IsLvl3BackupEnabled;
            ViewData["ID"] = id;
            ViewData["State"] = srvPrv.GetState(serv);
            return View(serverView);
        }

        public ActionResult Signature(string id)
        {
            bool flagPass = false;
            SignParams signParams = new SignParams();
            bool importStatus = signParams.Decode(id);
            Bitmap myBitmap;
            if(importStatus)
            {
                flagPass = true;
                myBitmap = new Bitmap(signParams.SignSize.Width, signParams.SignSize.Height);
            }
            else
            {
                myBitmap = new Bitmap(200, 100);
            }

            Graphics graph = Graphics.FromImage(myBitmap);
            graph.SmoothingMode = SmoothingMode.HighQuality;
            graph.TextRenderingHint = TextRenderingHint.AntiAlias;

            if(flagPass)
            {
                switch(signParams.TemplateID)
                {
                    case 1:
                        GraphHelper.Template1(graph, signParams);
                        break;
                    case 2:
                        GraphHelper.Template2(graph, signParams);
                        break;
                }

            }
            else
                GraphHelper.ErrorSign(graph);



            Response.Clear();
            Response.ContentType = "image/png";
            MemoryStream memStream = new MemoryStream();
            myBitmap.Save(memStream, ImageFormat.Png);
            memStream.Position = 0;
            memStream.CopyTo(Response.OutputStream);
            Response.End();

            return null;
        }
    }
}