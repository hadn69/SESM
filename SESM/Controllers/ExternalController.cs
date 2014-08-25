using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using SESM.Tools;
using SESM.Tools.Helpers;

namespace SESM.Controllers
{
    public class ExternalController : Controller
    {
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
            return View();
        }

        public ActionResult Signature(string id)
        {
            SignParams signParams = new SignParams();
            bool importStatus = signParams.Decode(id);
            if (!importStatus)
            {
                return null;
            }

            Bitmap myBitmap = new Bitmap(signParams.SignSize.Width, signParams.SignSize.Height);
            Graphics graph = Graphics.FromImage(myBitmap);

            graph.SmoothingMode = SmoothingMode.HighQuality;

            GraphHelper.Template1(graph, signParams);

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