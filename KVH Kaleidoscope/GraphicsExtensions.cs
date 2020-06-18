using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    public static class GraphicsExtensions
    {

        /// <summary>
        /// Resize the image to the specified width and height.
        /// <para>
        /// Source: https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
        /// </para>
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Bitmap CentreAlignedTile(Bitmap image, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            var nTotalCols = (int)Math.Round((float)width / image.Width + 0.5f, 0);
            var nTotalRows = (int)Math.Round((float)height / image.Height + 0.5f, 0);

            var initOffsetX = (width - nTotalCols * image.Width) / 2;
            var initOffsetY = (height - nTotalRows * image.Height) / 2;

            var g = Graphics.FromImage(bitmap);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);

            for (int i = 0; i < nTotalRows; i++)
            {
                for (int j = 0; j < nTotalCols; j++)
                {
                    var x = j * (image.Width) + initOffsetX;
                    var y = i * (image.Height) + initOffsetY;
                    g.DrawImage(image, new PointF(x, y));
                }
            }

            g.Dispose();
            return bitmap;
        }

        /// <summary>
        /// Lets grpahic g draw the specified bitmap at location (x, y) rotated by an angle (degree).
        /// </summary>
        /// <param name="g"></param>
        /// <param name="bmp"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="degrees"></param>
        public static void DrawImageAtLoactionAndAngle(this Graphics g, Bitmap bmp, float x, float y, float degrees)
        {
            g.TranslateTransform(x, y);
            g.RotateTransform(degrees);
            g.DrawImage(bmp, 0, 0);
            g.RotateTransform(-degrees);
            g.TranslateTransform(-x, -y);
        }

        /// <summary>
        /// <para>Lets graphic g draw a set of patterns having the same size. This set is defined 
        /// by the pattern width and height, an array of x-offset factors and another one for y-offset 
        /// factors, another array for the rotation angle and an array containing the indices to 
        /// the related pattern.
        /// </para>
        /// <para>
        /// Graphic g will iterate through the length of xOffsetFactors array, 
        /// in each iteration i, the pattern at patternIndices[i] is selected from 
        /// the specified patterns array (e.g. patterns[patternIndices[i]]) and drawn 
        /// at coordinate (xOffsetFactors[i]*patternWidth, xOffsetFactors[i]*patternWidth) 
        /// rotated by an angle rotationAngles[i].
        /// </para>
        /// <para>
        /// Length of xOffsetFactors, yOffsetFactors, rotationAngles and patternIndices must 
        /// be the same.
        /// </para>
        /// <para>
        /// For example, if specified parameters are as below:
        /// <list type="bullet">
        /// <item>patterns = [image0, image1],</item>
        /// <item>xOffsetFactors = [1, 2, 3],</item>
        /// <item>yOffsetFactors = [11, 22, 33],</item>
        /// <item>rotationAngles = [angle1, angle2, angle3],</item>
        /// <item>patternIndices = [1, 0, 1],</item>
        /// </list>
        /// The drawing sequence will be:
        /// <list type="number">
        /// <item>draws image1 at (1*patternWidth, 11*patternHeight) with an angle of angle1,</item>
        /// <item>draws image0 at (2*patternWidth, 22*patternHeight) with an angle of angle2,</item>
        /// <item>draws image1 at (3*patternWidth, 33*patternHeight) with an angle of angle3.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="g"></param>
        /// <param name="patterns"></param>
        /// <param name="patternWidth"></param>
        /// <param name="patternHeight"></param>
        /// <param name="xOffsetFactors"></param>
        /// <param name="yOffsetFactors"></param>
        /// <param name="rotationAngles"></param>
        /// <param name="patternIndices"></param>
        public static void DrawSet(this Graphics g, Bitmap[] patterns,
            float patternWidth, float patternHeight,
            float[] xOffsetFactors, float[] yOffsetFactors, float[] rotationAngles, int[] patternIndices)
        {
            if (!((xOffsetFactors.Length == yOffsetFactors.Length) &&
                (yOffsetFactors.Length == rotationAngles.Length) &&
                (rotationAngles.Length == patternIndices.Length)))
                throw new Exception("Array sizes mismatched.");

            for (var i = 0; i < xOffsetFactors.Length; i++)
            {
                g.DrawImageAtLoactionAndAngle(
                    patterns[patternIndices[i]],
                    patternWidth * xOffsetFactors[i], 
                    patternHeight * yOffsetFactors[i], 
                    rotationAngles[i]);
            }
        }

        /// <summary>
        /// Fills all pixels that have alpha channel value less than 255 by averaging neighbour pixels.
        /// </summary>
        /// <param name="bmp"></param>
        public static void FillGaps(Bitmap bmp)
        {
            _ = new int[] { };
            _ = new int[] { };
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color clr = bmp.GetPixel(x, y);
                    if (clr.A < 255)
                    {
                        int r = 0;
                        int g = 0;
                        int b = 0;
                        GetCoordinateOfNeighbouringPixels(bmp, x, y, out int[] xIdx, out int[] yIdx);
                        for (var i = 0; i < xIdx.Length; i++)
                        {
                            r += bmp.GetPixel(x + xIdx[i], y + yIdx[i]).R;
                            g += bmp.GetPixel(x + xIdx[i], y + yIdx[i]).G;
                            b += bmp.GetPixel(x + xIdx[i], y + yIdx[i]).B;
                        }
                        r /= xIdx.Length;
                        g /= xIdx.Length;
                        b /= xIdx.Length;
                        bmp.SetPixel(x, y, Color.FromArgb(255, r, g, b));
                    }
                }
            }
        }

        /// <summary>
        /// Returns (x, y) coordinate 
        /// of all neighbouring pixels of a pixel 
        /// in the bitmap bmp 
        /// specified by its (x, y) coordinate to xIdx and yIdx arrays.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="xIdx"></param>
        /// <param name="yIdx"></param>
        private static void GetCoordinateOfNeighbouringPixels(Bitmap bmp, int x, int y, out int[] xIdx, out int[] yIdx)
        {
            if (x == 0)
            {
                if (y == 0)
                {
                    xIdx = new int[] { 0, 1, 1 };
                    yIdx = new int[] { 1, 0, 1 };
                }
                else if (y == bmp.Height - 1)
                {
                    xIdx = new int[] { 0, 1, 1 };
                    yIdx = new int[] { -1, -1, 0 };
                }
                else
                {
                    xIdx = new int[] { 0, 0, 1, 1, 1 };
                    yIdx = new int[] { -1, 1, -1, 0, 1 };
                }
            }
            else if (x == bmp.Width - 1)
            {
                if (y == 0)
                {
                    xIdx = new int[] { -1, -1, 0 };
                    yIdx = new int[] { 0, 1, 1 };
                }
                else if (y == bmp.Height - 1)
                {
                    xIdx = new int[] { -1, -1, 0 };
                    yIdx = new int[] { -1, 0, -1 };
                }
                else
                {
                    xIdx = new int[] { -1, -1, -1, 0, 0 };
                    yIdx = new int[] { -1, 0, 1, -1, 1, };
                }
            }
            else
            {
                if (y == 0)
                {
                    xIdx = new int[] { -1, -1, 0, 1, 1 };
                    yIdx = new int[] { 0, 1, 1, 0, 1 };
                }
                else if (y == bmp.Height - 1)
                {
                    xIdx = new int[] { -1, -1, 0, 1, 1 };
                    yIdx = new int[] { -1, 0, -1, -1, 0 };
                }
                else
                {
                    xIdx = new int[] { -1, -1, -1, 0, 0, 1, 1, 1 };
                    yIdx = new int[] { -1, 0, 1, -1, 1, -1, 0, 1 };
                }
            }
        }
    }
}
