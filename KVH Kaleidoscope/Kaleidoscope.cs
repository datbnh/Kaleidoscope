using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Kvh.Kaleidoscope
{
    public static class Kaleidoscope
    {
        /// <summary>
        /// Renders kaleidoscope image as a bitmap with specified size using specified template.
        /// </summary>
        /// <param name="width">Width of the output bitmap</param>
        /// <param name="height">Height of the output bitmap</param>
        /// <param name="template">Input template</param>
        /// <returns></returns>
        public static Bitmap Render(int width, int height, KaleidoscopeTemplate template)
        {
            if (template == null)
                return null;

            var k = new KaleidoscopeEquilateralTriangle()
            {
                InterpolationMode = InterpolationMode.HighQualityBicubic,
                PixelOffsetMode = PixelOffsetMode.HighQuality,
                SmoothingMode = SmoothingMode.HighQuality,
            };

            var pattern = k.GetTileableRectangularPattern(template.Bitmap);

            Bitmap bitmap = new Bitmap(width, height);

            var nTotalCols = (int)Math.Round((float)width / pattern.Width + 0.5f, 0);
            var nTotalRows = (int)Math.Round((float)height / pattern.Height + 0.5f, 0);

            var initOffsetX = (width - nTotalCols * pattern.Width) / 2;
            var initOffsetY = (height - nTotalRows * pattern.Height) / 2;

            var g = Graphics.FromImage(bitmap);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);

            for (int i = 0; i < nTotalRows; i++)
            {
                for (int j = 0; j < nTotalCols; j++)
                {
                    var x = j * pattern.Width + initOffsetX;
                    var y = i * pattern.Height + initOffsetY;
                    g.DrawImage(pattern, new PointF(x, y));
                    Console.WriteLine("> " + x + " " + y);
                }
            }

            g.Dispose();
            return bitmap;
        }
    }
}