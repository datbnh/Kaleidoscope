using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Kvh.Kaleidoscope
{
    public static class Kaleidoscope
    {
        public static Bitmap Render(int width, int height, KaleidoscopeTemplate pattern)
        {
            if (pattern == null)
                return null;

            Bitmap bitmap = new Bitmap(width, height);
            var flippedXPattern = pattern.Bitmap.Clone() as Bitmap;
            flippedXPattern.RotateFlip(RotateFlipType.RotateNoneFlipX);

            Bitmap[] patterns = { pattern.Bitmap, flippedXPattern };

            var nTotalCols = (int)Math.Round(width / pattern.Width / 3, 0) + 2;
            var nTotalRows = (int)Math.Round(height / pattern.Height, 0) + 2;
            var nTop = (int)Math.Round(nTotalRows / 2f, 0);
            var nLeft = (int)Math.Round(nTotalCols / 2f, 0); ;

            var g = Graphics.FromImage(bitmap);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);

            g.TranslateTransform(width / 2, height / 2); // move to image centre

            if (nTop > 0)
            {
                g.TranslateTransform(0, -nTop * pattern.Height);
            }
            for (var i = 0; i < nTotalRows; i++)
            {
                if (i % 2 != 0)
                {
                    g.TranslateTransform(-Kaleidoscope2D.RowXOffsetFactor * pattern.Width, 0);
                    DrawRow(g, patterns, nLeft, nTotalCols + 1, pattern.Width, pattern.Height,
                        Kaleidoscope2D.XOffsetFactors, Kaleidoscope2D.YOffsetFactors,
                        Kaleidoscope2D.RotationAngles, Kaleidoscope2D.PatternIndices);
                    g.TranslateTransform(Kaleidoscope2D.RowXOffsetFactor * pattern.Width, 0);
                }
                else
                {
                    DrawRow(g, patterns, nLeft, nTotalCols, pattern.Width, pattern.Height,
                        Kaleidoscope2D.XOffsetFactors, Kaleidoscope2D.YOffsetFactors,
                        Kaleidoscope2D.RotationAngles, Kaleidoscope2D.PatternIndices);
                }
                g.TranslateTransform(0, pattern.Height);
            }
            g.Dispose();
            //FillVoid(bitmap);
            return bitmap;
        }

        public static void FillVoid(Bitmap bmp)
        {
            int[] xIdx = new int[] { };
            int[] yIdx = new int[] { };
            int r = 0, g = 0, b = 0;

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color clr = bmp.GetPixel(x, y);
                    if (clr.A < 255)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
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
                        for (var i = 0; i < xIdx.Length; i++)
                        {
                            r += bmp.GetPixel(x + xIdx[i], y + yIdx[i]).R;
                            g += bmp.GetPixel(x + xIdx[i], y + yIdx[i]).G;
                            b += bmp.GetPixel(x + xIdx[i], y + yIdx[i]).B;
                        }
                        r = r / xIdx.Length;
                        g = g / xIdx.Length;
                        b = b / xIdx.Length;
                        bmp.SetPixel(x, y, Color.FromArgb(255, r, g, b));
                    }
                }
            }
        }

        private static void DrawRow(Graphics g, Bitmap[] patterns,
            int nLeft, int nTotalCols, float patternWidth, float patternHeight,
            float[] xOffsetFactors, float[] yOffsetFactors, float[] rotationAngles, int[] patternIndices)
        {
            var setWidth = patternWidth * 3;
            if (nLeft > 0)
            {
                g.TranslateTransform(-nLeft * setWidth, 0);
            }
            for (var i = 0; i < nTotalCols; i++)
            {
                DrawSet(g, patterns, patternWidth, patternHeight,
                    xOffsetFactors, yOffsetFactors, rotationAngles, patternIndices);
                g.TranslateTransform(setWidth, 0);
            }
            g.TranslateTransform(-(nTotalCols - nLeft) * setWidth, 0);
        }

        private static void DrawSet(Graphics g, Bitmap[] patterns,
            float patternWidth, float patternHeight,
            float[] xOffsetFactors, float[] yOffsetFactors, float[] rotationAngles, int[] patternIndices)
        {
            if (!((xOffsetFactors.Length == yOffsetFactors.Length) &&
                (yOffsetFactors.Length == rotationAngles.Length) &&
                (rotationAngles.Length == patternIndices.Length)))
                throw new Exception("Array sizes mismatched.");

            for (var i = 0; i < xOffsetFactors.Length; i++)
            {
                DrawImageAtPointAndAngle(g, patterns[patternIndices[i]],
                    patternWidth * xOffsetFactors[i], patternHeight * yOffsetFactors[i], rotationAngles[i]);
            }
        }

        private static void DrawImageAtPointAndAngle(Graphics g, Bitmap bmp, float x, float y, float degree)
        {
            g.TranslateTransform(x, y);
            g.RotateTransform(degree);
            g.DrawImage(bmp, 0, 0);
            g.RotateTransform(-degree);
            g.TranslateTransform(-x, -y);
        }
    }

    // TODO Abstract/Factory class: KaleidoscopeType
    public static class Kaleidoscope2D
    {
        public static float[] XOffsetFactors = { 0.0f, 1.0f, 1.5f, 2.5f, 3.0f, 2.5f };
        public static float[] YOffsetFactors = { 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 1.0f };
        public static float[] RotationAngles = { 0, 60, 240, 180, 120, 300 };
        public static int[] PatternIndices = { 0, 1, 0, 1, 0, 1 };
        public static float SetWidthFactor = 3;
        public static float RowXOffsetFactor = 1.5f;
    }
}