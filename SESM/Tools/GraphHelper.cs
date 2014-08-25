using System;
using System.Collections.Generic;
using System.Drawing;
using SESM.Tools.Helpers;

namespace SESM.Tools
{
    public class GraphHelper
    {
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
            graphics.Clear(Color.FromArgb(255, 78, 93, 108));

            int border = 10;
            int imageSize = 60;

            //Color.FromArgb(255, 233, 105, 26)
            CornerRectangle(graphics,
                new Point(imageSize + border * 2, border),
                new Size(signParams.SignSize.Width - imageSize - border * 3, signParams.SignSize.Height - 2 * border),
                new Pen(Color.FromArgb(0), 2),
                new SolidBrush(Color.FromArgb(255, 43, 62, 80)),
                Corners.TOP_LEFT | Corners.BOTTOM_RIGHT,
                15);

            CornerRectangle(graphics,
                new Point(border, imageSize + border * 2),
                new Size(signParams.SignSize.Width - border * 2, signParams.SignSize.Height - (imageSize + border * 3)),
                new Pen(Color.FromArgb(0), 2),
                new SolidBrush(Color.FromArgb(255, 43, 62, 80)),
                Corners.BOTTOM_RIGHT,
                15);

            graphics.DrawImage(Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + @"\Content\logo.png"), 10, 10, 60, 60);


        }
    }
}