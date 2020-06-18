using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Kvh.Kaleidoscope
{
    public class MVC_Controller
    {
        private MVC_View view;
        private MVC_Model model;

        public MVC_Controller(MVC_View view, MVC_Model model)
        {
            this.view = view;
            this.model = model;

            this.view.Controller = this;
            this.view.Model = this.model;

            //TODO
            model.GraphicsInterpolationMode = InterpolationMode.HighQualityBicubic;
            model.GraphicsPixelOffsetMode = PixelOffsetMode.HighQuality;
            model.GraphicsSmoothingMode = SmoothingMode.AntiAlias;
            SetKaleidoscope(new KaleidoscopeEquilateralTriangle() {
                InterpolationMode = model.GraphicsInterpolationMode,
                PixelOffsetMode = model.GraphicsPixelOffsetMode,
                SmoothingMode = model.GraphicsSmoothingMode,
            } );
        }

        public void SetKaleidoscope(IKaleidoscope kaleidoscope)
        {
            model.Kaleidoscope = kaleidoscope;
        }


        public void LoadSourceImage(string imgPath)
        {
            try
            {
                var tmp = new Bitmap(imgPath);
                model.SourceImage = tmp.Clone() as Bitmap;
                tmp.Dispose();
                model.SourceImageFullPath = imgPath;
                view.UpdateSourceImageInfo();
            }
            catch (Exception ex)
            {
                view.DisplayError("Error Loading Image", ex);
            }
        }

        public void UpdateTemplateExtractionParametersFromViewToModel()
        {
            model.TemplateExtractionSize = view.PatternSize;
            model.TemplateExtractionOffsetX = view.PatternXOffset;
            model.TemplateExtractionOffsetY = view.PatternYOffset;
            model.TemplateExtractionRotaion = view.PatternRotation;
        }

        public void UpdateTemplateExtractionParameterFromModelToView()
        {
            view.PatternSize = model.TemplateExtractionSize;
            view.PatternXOffset = model.TemplateExtractionOffsetX;
            view.PatternYOffset = model.TemplateExtractionOffsetY;
            view.PatternRotation = model.TemplateExtractionRotaion;
        }

        public void ScaleSourceImage(int targetWidth, int targetHeight)
        {
            model.ScaledImage = GraphicsExtensions.ResizeImage((Image)model.SourceImage, targetWidth, targetHeight);
            view.UpdateScaledImage();
        }

        public void UpdateClippingPathOnTemplateFinder()
        {
            // use a temp bitmap to avoid flickering
            Console.WriteLine("Scaled Image: " + model.ScaledImage.PhysicalDimension + model.ScaledImage.HorizontalResolution);
            var templateFinderBitmap = model.ScaledImage.Clone() as Bitmap;
            var clippingPath = model.Kaleidoscope.GetUntransformedTemplateClippingPath(
                model.TemplateExtractionSize);
            var graphics = Graphics.FromImage(templateFinderBitmap);
            
            graphics.SmoothingMode = model.GraphicsSmoothingMode;
            graphics.PixelOffsetMode = model.GraphicsPixelOffsetMode;
            graphics.InterpolationMode = model.GraphicsInterpolationMode;

            graphics.TranslateTransform(
                model.TemplateExtractionOffsetX,
                model.TemplateExtractionOffsetY);
            graphics.RotateTransform(
                model.TemplateExtractionRotaion);
            graphics.DrawPath(
                new Pen(Brushes.DimGray),
                clippingPath);
            graphics.DrawPath(
                new Pen(Brushes.White) { DashPattern = new float[] { 4, 2 } },
                clippingPath);
            graphics.Dispose();

            view.UpdateTemplateFinder(templateFinderBitmap);
        }

        public void ScaleSourceImageByWidth(int targetWidth)
        {
            var targetHeight = targetWidth * model.SourceImage.Height / model.SourceImage.Width;
            ScaleSourceImage(targetWidth, targetHeight);
        }

        public void ScaleSourceImageByHeight(int targetHeight)
        {
            var targetWidth = targetHeight * model.SourceImage.Width / model.SourceImage.Height;
            ScaleSourceImage(targetWidth, targetHeight);
        }

        public void ExtractTemplate()
        {
            model.Template = model.Kaleidoscope.ExtractTemplate(
                model.ScaledImage,
                model.TemplateExtractionSize,
                model.TemplateExtractionOffsetX,
                model.TemplateExtractionOffsetY,
                model.TemplateExtractionRotaion
                );
            view.UpdateTemplatePreviewWindow();
        }

        public void RenderKaleidoscopeImage(int renderingWidth, int renderingHeight)
        {
            model.RenderingWidth = renderingWidth;
            model.RenderingHeight = renderingHeight;
            model.RectangularPattern = model.Kaleidoscope.
                GetTileableRectangularPattern(model.Template);
            model.RenderedImage = GraphicsExtensions.CentreAlignedTile(
                model.RectangularPattern,
                model.RenderingWidth,
                model.RenderingHeight);
            view.UpdateRenderedImage();
        }

        public void SaveRenderedImage(string imgPath)
        {
            try
            {
                model.RenderedImageFullPath = imgPath;
                model.RenderedImage.SaveAsFile(model.RenderedImageFullPath);
                view.UpdateSavedImageInfo();
            }
            catch (Exception ex)
            {
                view.DisplayError("Error Saving File", ex);
            }
        }
    }
}

