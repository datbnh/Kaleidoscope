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
            
            KaleidoscopeRenderer.SetGraphicsModes(
                SmoothingMode.AntiAlias,
                PixelOffsetMode.HighQuality,
                InterpolationMode.HighQualityBicubic);
            
            SetMirorrSystem(new MirrorSystem306090());
        }

        public void SetMirorrSystem(MirrorSystem mirrorSystem)
        {
            model.MirrorSystem = mirrorSystem;
            view.UpdateMirrorSystem();
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

        public void SetTemplateExtractionParameters(int size, int xOffset, int yOffset, float rotation)
        {
            model.TemplateExtractionSize = size;
            model.TemplateExtractionOffsetX = xOffset;
            model.TemplateExtractionOffsetY = yOffset;
            model.TemplateExtractionRotaion = rotation;
        }

        public void UpdateTemplateExtractionParameterFromModelToView()
        {
            view.TemplateSize = model.TemplateExtractionSize;
            view.TemplateXOffset = model.TemplateExtractionOffsetX;
            view.TemplateYOffset = model.TemplateExtractionOffsetY;
            view.TemplateRotation = model.TemplateExtractionRotaion;
        }

        public void ScaleSourceImage(int targetWidth, int targetHeight)
        {
            model.ScaledImage = GraphicsExtensions.ResizeImage((Image)model.SourceImage, targetWidth, targetHeight);
            view.UpdateScaledImage();
        }

        public void UpdateClippingPathOnTemplateFinder()
        {
            if (model.ScaledImage == null)
                return;
            // use a temp bitmap to avoid flickering
            var templateFinderBitmap = model.ScaledImage.Clone() as Bitmap;
            var clippingPath = model.MirrorSystem.GetUntransformedTemplateClippingPolygon(
                model.TemplateExtractionSize, 0).ToGraphicsPath();
            var graphics = Graphics.FromImage(templateFinderBitmap);
            
            graphics.SmoothingMode = KaleidoscopeRenderer.SmoothingMode;
            graphics.PixelOffsetMode = KaleidoscopeRenderer.PixelOffsetMode;
            graphics.InterpolationMode = KaleidoscopeRenderer.InterpolationMode;

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
            model.Template = KaleidoscopeRenderer.ExtractTemplate(
                model.ScaledImage,
                model.MirrorSystem,
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
            model.RectangularPattern = KaleidoscopeRenderer.
                GetTileableRectangularPattern(model.Template, model.MirrorSystem);
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

        internal void SetMirorrSystem(Type type)
        {
            if (type.IsEquivalentTo(typeof(MirrorSystem306090)))
                SetMirorrSystem(new MirrorSystem306090());
            else if (type.IsEquivalentTo(typeof(MirrorSystem606060)))
                SetMirorrSystem(new MirrorSystem606060());
            else if (type.IsEquivalentTo(typeof(MirrorSystem459045)))
                SetMirorrSystem(new MirrorSystem459045());
            else
                return;
            UpdateClippingPathOnTemplateFinder();
        }
    }
}

