using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
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

        // GET: External
        public ActionResult RandomRGB(int id = 10)
        {
            if (SESMConfigHelper.Diagnosis)
            {
                int size = id;
                Bitmap myBitmap = new Bitmap(size, size);
                Random rnd = new Random();

                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
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
            if (SESMConfigHelper.Diagnosis)
            {
                int size = id;
                Bitmap myBitmap = new Bitmap(size, size);
                Random rnd = new Random();

                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
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
            if (importStatus)
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

            if (flagPass)
            {
                switch (signParams.TemplateID)
                {
                    case 1 :
                        GraphHelper.Template1(graph, signParams);
                        break;
                    case 2 :
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