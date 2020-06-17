using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;

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
            SetKaleidoscope(new KaleidoscopeEquilateralTriangle());
        }

        public void SetKaleidoscope(IKaleidoscope kaleidoscope)
        {
            model.Kaleidoscope = kaleidoscope;
        }


        public void LoadSourceImage(string sourceImageFullPath)
        {
            LoadImage(sourceImageFullPath);
            view.UpdateSourceImageInfo();
        }

        public void ScaleSourceImage(int targetWidth, int targetHeight)
        {
            model.ScaledImage = GraphicsExtensions.ResizeImage((Image)model.SourceImage, targetWidth, targetHeight);
            view.UpdateScaledImage();
        }

        public void UpdateClippingPathOnTemplateFinder()
        {
            // use a temp bitmap to avoid flickering
            var templateFinderBitmap = model.ScaledImage.Clone() as Bitmap;
            var gTemplateFinderBitmap = Graphics.FromImage(templateFinderBitmap);
            //gTemplateFinderBitmap.SmoothingMode = model.Kaleidoscope.SmoothingMode;
            //gTemplateFinderBitmap.PixelOffsetMode = model.Kaleidoscope.PixelOffsetMode;
            //gTemplateFinderBitmap.InterpolationMode = model.Kaleidoscope.InterpolationMode;

            //gTemplateFinderBitmap.DrawImage(originalImage, scaledImgRect, imgRect, GraphicsUnit.Pixel);
            gTemplateFinderBitmap.TranslateTransform(
                model.TemplateExtractionOffsetX, 
                model.TemplateExtractionOffsetY);
            gTemplateFinderBitmap.RotateTransform(
                model.TemplateExtractionRotaion);
            var clippingPath = model.Kaleidoscope.GetUntransformedTemplateClippingPath(
                model.TemplateExtractionSize);
            gTemplateFinderBitmap.DrawPath(new Pen(Brushes.DimGray), clippingPath);
            gTemplateFinderBitmap.DrawPath(new Pen(Brushes.White) { DashPattern = new float[] { 4, 2 } }, clippingPath);
            gTemplateFinderBitmap.Dispose();

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

        public void SetTemplateExtractionParameters(int size, int offsetX, int offsetY, float angle)
        {
            model.TemplateExtractionSize = size;
            model.TemplateExtractionOffsetX = offsetX;
            model.TemplateExtractionOffsetY = offsetY;
            model.TemplateExtractionRotaion = angle;
            //view.UpdateTemplateExtractionParameters();
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
            view.UpdateExtractedTemplate();
        }

        public void RenderKaleidoscopeImage(int renderingWidth, int renderingHeight)
        {
            model.RenderedImageWidth = renderingWidth;
            model.RenderedImageHeight = renderingHeight;
            model.RectangularPattern = model.Kaleidoscope.
                GetTileableRectangularPattern(model.Template);
            model.RenderedImage = GraphicsExtensions.CentreAlignedTile(
                model.RectangularPattern,
                model.RenderedImageWidth,
                model.RenderedImageHeight);
            view.UpdateRenderedImage();
        }


        private void LoadImage(string imgPath)
        {
            try
            {
                model.SourceImageFullPath = imgPath;
                var tmp = new Bitmap(model.SourceImageFullPath);
                model.SourceImage = tmp.Clone() as Bitmap;
                tmp.Dispose();
                view.UpdateSourceImageInfo();
            }
            catch (Exception ex)
            {
                view.DisplayError("Error Loading Image", ex);
            }
        }

        public void SaveRenderedImage(string imgPath)
        {
            if (model.RenderedImage == null)
                return;

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

