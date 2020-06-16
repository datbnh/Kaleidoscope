using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    class KaleidoscopeEquilateralTriangle : IKaleidoscope
    {
        public SmoothingMode SmoothingMode;
        public PixelOffsetMode PixelOffsetMode;
        public InterpolationMode InterpolationMode;

        //TODO: add external documentation for these magic numbers :)
        private readonly float[] XOffsetFactors = { -.5f, 0, 1f, 1.5f, 2.5f, 3, 2.5f, 0, 1, 1.5f, 1, 1.5f, 2.5f, 3 };
        private readonly float[] YOffsetFactors = { 2, 1, 1, 2, 2, 1, 2, 1, 1, 0, 1, 0, 0, 1 };
        private readonly float[] RotationAngles = { -60, 0, 60, -120, 180, 120, -60, -120, 180, 120, -60, 0, 60, -120 };
        private readonly int[] PatternIndices = { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 };

        public Bitmap ExtractTemplate(Bitmap originalImage, int size, int x, int y, float angle)
        {
            var pSize = GetUntransformedTemplateRectangularSize(size);
            var clippingPath = GetUntransformedTemplateClippingPath(size);

            Bitmap template = new Bitmap((int)Math.Round(pSize.X), (int)Math.Round(pSize.Y, 0));
            var gTemplate = Graphics.FromImage(template);
            gTemplate.SmoothingMode = SmoothingMode;
            gTemplate.PixelOffsetMode = PixelOffsetMode;
            gTemplate.InterpolationMode = InterpolationMode;

            gTemplate.Clip = new Region(clippingPath);

            gTemplate.RotateTransform(-angle);
            gTemplate.TranslateTransform(-x, -y);
            gTemplate.DrawImage(originalImage, new PointF(0, 0));
            gTemplate.Dispose();

            return template;
        }

        public Bitmap GetTileableRectangularPattern(Bitmap template)
        {
            var floatSize = GetUntransformedTemplateRectangularSize(template.Width);
            Bitmap bitmap = new Bitmap(template.Width * 3, template.Height * 2);
            var flippedXTemplate = template.Clone() as Bitmap;
            flippedXTemplate.RotateFlip(RotateFlipType.RotateNoneFlipX);

            Bitmap[] patterns = { template, flippedXTemplate };

            var g = Graphics.FromImage(bitmap);
            g.InterpolationMode = InterpolationMode;
            g.SmoothingMode = SmoothingMode;
            g.PixelOffsetMode = PixelOffsetMode;

            g.DrawSet(patterns,
                floatSize.X, floatSize.Y,
                XOffsetFactors, YOffsetFactors,
                RotationAngles, PatternIndices);

            GraphicsExtensions.FillGaps(bitmap);

            return bitmap;
        }

        public GraphicsPath GetUntransformedTemplateClippingPath(int size)
        {
            var pSize = GetUntransformedTemplateRectangularSize(size);
            var clippingPath = new GraphicsPath();
            clippingPath.AddPolygon(new[] {
                new PointF(0, 0),
                new PointF(pSize.X, 0),
                new PointF(pSize.X, pSize.Y)});
            return clippingPath;
        }

        public PointF GetUntransformedTemplateRectangularSize(int size)
        {
            return new PointF(size, size * (float)Math.Sqrt(3) / 2);
        }
    }
}
