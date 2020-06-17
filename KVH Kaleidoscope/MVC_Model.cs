using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    public class MVC_Model
    {
        public bool IsAutoFillGaps;
        public int IsLockAspectRatio;
        public IKaleidoscope Kaleidoscope;
        public Bitmap RectangularPattern;
        public Bitmap RenderedImage;
        public string RenderedImageFullPath;
        public int RenderedImageHeight;
        public int RenderedImageWidth;
        public Bitmap ScaledImage;
        public int ScaledImageHeight;
        public int ScaledImageWidth;
        public Bitmap SourceImage;
        public string SourceImageFullPath;
        public int TemplateExtractionSize;
        public Bitmap Template;
        public int TemplateExtractionOffsetX;
        public int TemplateExtractionOffsetY;
        public float TemplateExtractionRotaion;
    }
}
