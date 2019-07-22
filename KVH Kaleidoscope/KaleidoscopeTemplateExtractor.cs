using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
        public bool UseAlphaBlend;

        public KaleidoscopeTemplateExtractor(SmoothingMode smoothingMode,
            PixelOffsetMode pixelOffsetMode, InterpolationMode interpolationMode, bool useAlphaBlend = true)
        {
            SmoothingMode = smoothingMode;
            PixelOffsetMode = pixelOffsetMode;
            InterpolationMode = interpolationMode;
            UseAlphaBlend = useAlphaBlend;
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


            if (UseAlphaBlend)
            {
                var overlapping = 0;
                var maskWidth = templateSize + 2* overlapping;
                var maskHeight = maskWidth * (float)Math.Sqrt(3) / 2;

                var maskPath = new GraphicsPath();
                maskPath.AddPolygon(new[] {
                new PointF(-overlapping, -overlapping*(float)Math.Sqrt(3)/2),
                new PointF(maskWidth, -overlapping*(float)Math.Sqrt(3)/2),
                new PointF(maskWidth/2, maskHeight)});


                var alphaMask = new Bitmap(patternWidth, (int)(Math.Round(patternHeight)));
                var gMask = Graphics.FromImage(alphaMask);
                gMask.SmoothingMode = SmoothingMode;
                gMask.PixelOffsetMode = PixelOffsetMode;
                gMask.InterpolationMode = InterpolationMode;
                gMask.Clear(Color.White);
                gMask.FillPath(Brushes.Black, maskPath);
                gMask.Dispose();

                gPattern.RotateTransform(-angle);
                gPattern.TranslateTransform(-x, -y);
                gPattern.DrawImage(originalImage, scaledImgRect, imgRect, GraphicsUnit.Pixel);
                gPattern.Dispose();

                ApplyAlphaMask(pattern, alphaMask);
            }
            else
            {
                gPattern.Clip = new Region(clippingPath);

                gPattern.RotateTransform(-angle);
                gPattern.TranslateTransform(-x, -y);
                gPattern.DrawImage(originalImage, scaledImgRect, imgRect, GraphicsUnit.Pixel);
                gPattern.Dispose();
            }

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


        // 
        public static void ApplyAlphaMask(Bitmap bmp, Bitmap alphaMaskImage)
        {
            int width = bmp.Width;
            int height = bmp.Height;

            BitmapData dataAlphaMask = alphaMaskImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                try
                {
                    unsafe //using pointer requires the unsafe keyword
                    {
                        byte* pData0Mask = (byte*)dataAlphaMask.Scan0;
                        byte* pData0 = (byte*)data.Scan0;

                        for (int x = 0; x < width; x++)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                byte* pData = pData0 + (y * data.Stride) + (x * 4);
                                byte* pDataMask = pData0Mask + (y * dataAlphaMask.Stride) + (x * 4);

                                byte maskBlue = pDataMask[0];
                                byte maskGreen = pDataMask[1];
                                byte maskRed = pDataMask[2];

                                //the closer the color is to black the more opaque it will be.
                                byte alpha = (byte)(255 - (maskRed + maskBlue + maskGreen) / 3);

                                //respect the original alpha value
                                byte originalAlpha = pData[3];
                                pData[3] = (byte)(alpha * originalAlpha / 255f);
                            }
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(data);
                }
            }
            finally
            {
                alphaMaskImage.UnlockBits(dataAlphaMask);
            }
        }
    }
}
