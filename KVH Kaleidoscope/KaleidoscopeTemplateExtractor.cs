using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kvh.Kaleidoscope
{
    public class KaleidoscopeTemplateExtractor
    {
        public SmoothingMode SmoothingMode;
        public PixelOffsetMode PixelOffsetMode;
        public InterpolationMode InterpolationMode;

        public KaleidoscopeTemplateExtractor(SmoothingMode smoothingMode,
            PixelOffsetMode pixelOffsetMode, InterpolationMode interpolationMode)
        {
            SmoothingMode = smoothingMode;
            PixelOffsetMode = pixelOffsetMode;
            InterpolationMode = interpolationMode;
        }

        public KaleidoscopeTemplate Extract(Bitmap originalImage, 
            int scaledWidth, int scaledHeight,
            int templateSize, int x, int y, float angle, PictureBox pictureBox)
        {
            var imgRect = new Rectangle(0, 0, originalImage.Width, originalImage.Height);
            var scaledImgRect = new RectangleF(0, 0, scaledWidth, scaledHeight);

            var patternWidth = templateSize;
            var patternHeight = patternWidth * (float)Math.Sqrt(3) / 2;

            var clippingPath = new GraphicsPath();
            clippingPath.AddPolygon(new[] {
                new PointF(0, 0),
                new PointF(patternWidth, 0),
                new PointF(patternWidth/2, patternHeight)});

            Bitmap pattern = new Bitmap(patternWidth, (int)Math.Round(patternHeight,0));
            var gPattern = Graphics.FromImage(pattern);
            gPattern.SmoothingMode = SmoothingMode;
            gPattern.PixelOffsetMode = PixelOffsetMode;
            gPattern.InterpolationMode = InterpolationMode;

            gPattern.Clip = new Region(clippingPath);
            gPattern.RotateTransform(-angle);
            gPattern.TranslateTransform(-x, -y);
            gPattern.DrawImage(originalImage, scaledImgRect, imgRect, GraphicsUnit.Pixel);
            gPattern.Dispose();

            if (pictureBox != null)
            {
                //var gPreviewBmp = pictureBox.CreateGraphics();
                //gPreviewBmp.Clear(Color.Transparent);
                // use a temp bitmap to avoid flickering
                var previewBmp = new Bitmap(scaledWidth, scaledHeight);
                var gPreviewBmp = Graphics.FromImage(previewBmp);
                gPreviewBmp.SmoothingMode = SmoothingMode;
                gPreviewBmp.PixelOffsetMode = PixelOffsetMode;
                gPreviewBmp.InterpolationMode = InterpolationMode;

                gPreviewBmp.DrawImage(originalImage, scaledImgRect, imgRect, GraphicsUnit.Pixel);
                gPreviewBmp.TranslateTransform(x, y);
                gPreviewBmp.RotateTransform(angle);
                gPreviewBmp.DrawPath(new Pen(Brushes.DimGray), clippingPath);
                gPreviewBmp.DrawPath(new Pen(Brushes.White) { DashPattern = new float[] { 4, 2 } }, clippingPath);
                gPreviewBmp.Dispose();

                //pictureBox.Width = scaledWidth;
                //pictureBox.Height = scaledHeight;
                pictureBox.Image = previewBmp;
            }

            return new KaleidoscopeTemplate(pattern, patternWidth, patternHeight);
        }
    }
}
