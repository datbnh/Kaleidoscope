using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    public static class KaleidoscopeRenderer
    {
        public static SmoothingMode SmoothingMode { get; private set; }
        public static PixelOffsetMode PixelOffsetMode { get; private set; }
        public static InterpolationMode InterpolationMode { get; private set; }

        public static void SetGraphicsModes(
            SmoothingMode smoothingMode, PixelOffsetMode pixelOffsetMode, InterpolationMode interpolationMode)
        {
            SmoothingMode = smoothingMode;
            PixelOffsetMode = pixelOffsetMode;
            InterpolationMode = interpolationMode;
        }

        public static Bitmap ExtractTemplate(Bitmap image, MirrorSystem mirrorSystem, 
            int size, int x, int y, float angle)
        {
            var pSize = mirrorSystem.GetUntransformedTemplateRectangularSize(size);
            
            Bitmap template = new Bitmap((int)Math.Round(pSize.X), (int)Math.Round(pSize.Y, 0));
            template.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            var gTemplate = Graphics.FromImage(template);
            gTemplate.SmoothingMode = SmoothingMode;
            gTemplate.PixelOffsetMode = PixelOffsetMode;
            gTemplate.InterpolationMode = InterpolationMode;

            gTemplate.Clip = new Region(
                mirrorSystem.GetUntransformedTemplateClippingPolygon(size).ToGraphicsPath());

            gTemplate.RotateTransform(-angle);
            gTemplate.TranslateTransform(-x, -y);
            gTemplate.DrawImage(image, new PointF(0, 0));
            gTemplate.Dispose();

            return template;
        }

        public static Bitmap GetTileableRectangularPattern(Bitmap template, MirrorSystem mirrorSystem)
        {
            var floatSize = mirrorSystem.GetUntransformedTemplateRectangularSize(template.Width);
            Bitmap bitmap = new Bitmap(
                template.Width * mirrorSystem.TilableRectangularPatternHorizontalSpan, 
                template.Height * mirrorSystem.TilableRectangularPatternVerticalSpan);
            bitmap.SetResolution(template.HorizontalResolution, template.VerticalResolution);

            var flippedXTemplate = template.Clone() as Bitmap;
            flippedXTemplate.RotateFlip(RotateFlipType.RotateNoneFlipX);

            Bitmap[] templates = { template, flippedXTemplate };

            var g = Graphics.FromImage(bitmap);
            g.InterpolationMode = InterpolationMode;
            g.SmoothingMode = SmoothingMode;
            g.PixelOffsetMode = PixelOffsetMode;

            g.DrawSet(templates, floatSize.X, floatSize.Y,
                mirrorSystem.TransformationSetForTilableRectangularPattern);

            GraphicsExtensions.FillGaps(bitmap);

            return bitmap;
        }
    }
}
