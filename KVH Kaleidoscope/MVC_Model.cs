using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    public class MVC_Model
    {
        public bool IsAutoFillGaps;
        public int IsLockAspectRatio;
        
        public MirrorSystem MirrorSystem;
        
        public Bitmap RectangularPattern;
        
        public Bitmap RenderedImage;
        public string RenderedImageFullPath;
        public int RenderingHeight;
        public int RenderingWidth;
        
        public Bitmap ScaledImage;
        
        public Bitmap SourceImage;
        public string SourceImageFullPath;
        
        public Bitmap Template;
        public int TemplateExtractionOffsetX;
        public int TemplateExtractionOffsetY;
        public float TemplateExtractionRotaion;
        public int TemplateExtractionSize;
    }
}
