﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    public static class Kaleidoscope
    {
        public static Bitmap Render(int width, int height, Bitmap pattern)
        {
            Bitmap bitmap = new Bitmap(width, height);
            var flippedXPattern = pattern.Clone() as Bitmap;
            flippedXPattern.RotateFlip(RotateFlipType.RotateNoneFlipX);

            Bitmap[] patterns = { pattern, flippedXPattern };

            float patternWidth = pattern.Width;
            float patternHeight = patternWidth * (float)Math.Sqrt(3) / 2;


            var nTotalRows = (int)Math.Round(height / patternHeight, 0) + 2;
            var nTotalCols = (int)Math.Round(width / patternWidth / 3, 0) + 2;
            var nTop = (int)Math.Round(nTotalRows / 2f, 0);
            var nLeft = (int)Math.Round(nTotalCols / 2f, 0); ;

            var g = Graphics.FromImage(bitmap);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Gray);

            g.TranslateTransform(width / 2, height / 2);
            //g.RotateTransform(clipPathRotation);
            if (nTop > 0)
            {
                g.TranslateTransform(0, -nTop * patternHeight);
            }
            for (var i = 0; i < nTotalRows; i++)
            {
                if (i % 2 != 0)
                {
                    g.TranslateTransform(-Kaleidoscope2D.RowXOffsetFactor * patternWidth, 0);
                    //DrawRow(g, nLeft, nTotalCols + 1, xOffset, yOffset);
                    DrawRow(g, patterns, nLeft, nTotalCols + 1, patternWidth, patternHeight,
                        Kaleidoscope2D.XOffsetFactor, Kaleidoscope2D.YOffsetFactor,
                        Kaleidoscope2D.RotationAngle, Kaleidoscope2D.PatternIndex);
                    g.TranslateTransform(Kaleidoscope2D.RowXOffsetFactor * patternWidth, 0);
                }
                else
                {
                    //DrawRow(g, nLeft, nTotalCols, xOffset, yOffset);
                    DrawRow(g, patterns, nLeft, nTotalCols, patternWidth, patternHeight,
                        Kaleidoscope2D.XOffsetFactor, Kaleidoscope2D.YOffsetFactor,
                        Kaleidoscope2D.RotationAngle, Kaleidoscope2D.PatternIndex);
                }
                g.TranslateTransform(0, patternHeight);
            }
            g.Dispose();

            return bitmap;
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
                //DrawSet(g, xOffset, yOffset);
                DrawSet(g, patterns, patternWidth, patternHeight,
                    xOffsetFactors, yOffsetFactors, rotationAngles, patternIndices);
                g.TranslateTransform(setWidth, 0);
            }
            g.TranslateTransform(-(nTotalCols - nLeft) * setWidth, 0);
        }

        private static void DrawSet(Graphics g, Bitmap[] patterns, 
            float patternWidth, float patternHeight, 
            float[] xOffsetFactors, float [] yOffsetFactors, float[] rotationAngles, int[]patternIndices)
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

            //DrawImageAtPointAndAngle(g, patternOriginal, 0, 0, 0);
            //DrawImageAtPointAndAngle(g, patternFlippedLR, 1 * xOffset, 0, 60);
            //DrawImageAtPointAndAngle(g, patternOriginal, 1.5f * xOffset, yOffset, 240);
            //DrawImageAtPointAndAngle(g, patternFlippedLR, 2.5f * xOffset, yOffset, 180);
            //DrawImageAtPointAndAngle(g, patternOriginal, 3 * xOffset, 0, 120);
            //DrawImageAtPointAndAngle(g, patternFlippedLR, 2.5f * xOffset, yOffset, 300);
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
        public static float[] XOffsetFactor = { 0.0f, 1.0f, 1.5f, 2.5f, 3.0f, 2.5f };
        public static float[] YOffsetFactor = { 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 1.0f };
        public static float[] RotationAngle = { 0, 60, 240, 180, 120, 300 };
        public static int[] PatternIndex = { 0, 1, 0, 1, 0, 1 };
        public static float SetWidthFactor = 3;
        public static float RowXOffsetFactor = 1.5f;
    }
}
