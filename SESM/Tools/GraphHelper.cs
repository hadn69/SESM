using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using SESM.Tools.Helpers;

namespace SESM.Tools
{
    public class GraphHelper
    {
        public static PrivateFontCollection privateFontCollection;
        [Flags]
        public enum Corners
        {
            NONE = 0,
            TOP_LEFT = 1,
            TOP_RIGHT = 2,
            BOTTOM_RIGHT = 4,
            BOTTOM_LEFT = 8,
            ALL = 15
        }

        public static void CornerRectangle(Graphics graphics, 
            Point topLeftCorner, 
            Size size, 
            Pen outline,
            Brush filling,
            Corners cuttedCorners,
            int cornerSize)
        {
            List<Point> outlinePoints = new List<Point>();

            if (cuttedCorners.HasFlag(Corners.TOP_LEFT))
            {
                outlinePoints.Add(new Point(topLeftCorner.X, topLeftCorner.Y + cornerSize));
                outlinePoints.Add(new Point(topLeftCorner.X + cornerSize, topLeftCorner.Y));
            }
            else
                outlinePoints.Add(new Point(topLeftCorner.X, topLeftCorner.Y));

            Point topRightCorner = new Point(topLeftCorner.X + size.Width, topLeftCorner.Y);

            if (cuttedCorners.HasFlag(Corners.TOP_RIGHT))
            {
                outlinePoints.Add(new Point(topRightCorner.X - cornerSize, topRightCorner.Y));
                outlinePoints.Add(new Point(topRightCorner.X, topRightCorner.Y + cornerSize));
            }
            else
                outlinePoints.Add(new Point(topRightCorner.X, topRightCorner.Y));

            Point bottomRightCorner = new Point(topLeftCorner.X + size.Width, topLeftCorner.Y + size.Height);

            if (cuttedCorners.HasFlag(Corners.BOTTOM_RIGHT))
            {
                outlinePoints.Add(new Point(bottomRightCorner.X, bottomRightCorner.Y - cornerSize));
                outlinePoints.Add(new Point(bottomRightCorner.X - cornerSize, bottomRightCorner.Y));
            }
            else
                outlinePoints.Add(new Point(bottomRightCorner.X, bottomRightCorner.Y));

            Point bottomLeftCorner = new Point(topLeftCorner.X, topLeftCorner.Y + size.Height);

            if (cuttedCorners.HasFlag(Corners.BOTTOM_LEFT))
            {
                outlinePoints.Add(new Point(bottomLeftCorner.X + cornerSize, bottomLeftCorner.Y));
                outlinePoints.Add(new Point(bottomLeftCorner.X, bottomLeftCorner.Y - cornerSize));
            }
            else
                outlinePoints.Add(new Point(bottomLeftCorner.X, bottomLeftCorner.Y));

            graphics.FillPolygon(filling, outlinePoints.ToArray());
            graphics.DrawPolygon(outline, outlinePoints.ToArray());

        }

        public static void Template1(Graphics graphics, SignParams signParams)
        {
            graphics.Clear(signParams.BgColor);

            int border = 10;
            int imageSize = 60;

            if (signParams.Logo)
            {
                CornerRectangle(graphics, new Point(imageSize + border*2, border),
                    new Size(signParams.SignSize.Width - imageSize - border*3, signParams.SignSize.Height - 2*border),
                    new Pen(Color.FromArgb(0), 1), new SolidBrush(signParams.PrimColor),
                    Corners.TOP_LEFT | Corners.BOTTOM_RIGHT, 15);

                graphics.FillRectangle(new SolidBrush(signParams.PrimColor), border, imageSize + border*2,
                    imageSize + border + 1, signParams.SignSize.Height - (imageSize + border*3));

                graphics.DrawImage(Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + @"\Content\logo.png"), 10, 10,
                    60, 60);
            }
            else
            {
                CornerRectangle(graphics, new Point(border, border),
                    new Size(signParams.SignSize.Width - border * 2, signParams.SignSize.Height - 2 * border),
                    new Pen(Color.FromArgb(0), 1), new SolidBrush(signParams.PrimColor),
                    Corners.TOP_LEFT | Corners.BOTTOM_RIGHT, 15);
            }

        }

        public static void Template2(Graphics graphics, SignParams signParams)
        {
            graphics.Clear(signParams.BgColor);
            Random rnd = new Random();
            Pen starPen = new Pen(signParams.PrimColor);
            double nbStars = Math.Round(signParams.SignSize.Width*signParams.SignSize.Height/500.0);
            for (int i = 0; i < nbStars; i++)
            {
                graphics.DrawEllipse(starPen, rnd.Next(0, signParams.SignSize.Width), rnd.Next(0, signParams.SignSize.Height), 1, 1);
            }

            if (signParams.Logo)
            {
                graphics.DrawImage(Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + @"\Content\logo.png"), 10, 10,
                    60, 60);
            }
        }

        public static void ErrorSign(Graphics graphics)
        {
            graphics.Clear(Color.Black);

            int width = 200;
            int height = 100;


            Font myFont = new Font("Lato", 24);
            SizeF measure = graphics.MeasureString("Error", myFont);


            graphics.DrawString("Error", myFont, new SolidBrush(Color.White), (width - measure.Width) / 2, (height - measure.Height) / 2);

        }
    }
}