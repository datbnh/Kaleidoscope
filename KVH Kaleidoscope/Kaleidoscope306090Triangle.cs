using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    class Kaleidoscope306090Triangle : IKaleidoscope
    {
        public SmoothingMode SmoothingMode;
        public PixelOffsetMode PixelOffsetMode;
        public InterpolationMode InterpolationMode;

        //TODO: add external documentation for these magic numbers :)
        private readonly float[] XOffsetFactors = { 0, 1, 1.5f, 1.5f, 2, 3, 4, 4.5f, 5, 4, 4.5f, 5, 1, 1.5f, 2, 1, 1.5f, 2, 3, 4, 4.5f, 4.5f, 5, 6 };
        private readonly float[] YOffsetFactors = { 0, 0, 0.5f, 0.5f, 1, 1, 1, 0.5f, 0, 1, 0.5f, 0, 2, 1.5f, 1, 2, 1.5f, 1, 1, 1, 1.5f, 1.5f, 2, 2 };
        private readonly float[] RotationAngles = { 0, 60, 60, -120, -120, 180, 180, 120, 120, -60, -60, 0, 180, 120, 120, -60, -60, 0, 0, 60, 60, -120, -120, 180 };
        private readonly int[] PatternIndices = { 0, 1, 0, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 0, 1, 0, 0, 1, 0 };


        public Bitmap ExtractTemplate(Bitmap image, int size, int x, int y, float angle)
        {
            var pSize = GetUntransformedTemplateRectangularSize(size);
            var clippingPath = GetUntransformedTemplateClippingPath(size);

            Bitmap template = new Bitmap((int)Math.Round(pSize.X), (int)Math.Round(pSize.Y, 0));
            template.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            var gTemplate = Graphics.FromImage(template);
            gTemplate.SmoothingMode = SmoothingMode;
            gTemplate.PixelOffsetMode = PixelOffsetMode;
            gTemplate.InterpolationMode = InterpolationMode;

            gTemplate.Clip = new Region(clippingPath);

            gTemplate.RotateTransform(-angle);
            gTemplate.TranslateTransform(-x, -y);
            gTemplate.DrawImage(image, new PointF(0, 0));
            gTemplate.Dispose();

            return template;
        }

        public Bitmap GetTileableRectangularPattern(Bitmap template)
        {
            var floatSize = GetUntransformedTemplateRectangularSize(template.Width);
            Bitmap bitmap = new Bitmap(template.Width * 6, template.Height * 2);
            bitmap.SetResolution(template.HorizontalResolution, template.VerticalResolution);
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
                new PointF(0, pSize.Y)});
            return clippingPath;
        }

        public PointF GetUntransformedTemplateRectangularSize(int size)
        {
            return new PointF(size, size * (float)Math.Sqrt(3));
        }
    }
}
