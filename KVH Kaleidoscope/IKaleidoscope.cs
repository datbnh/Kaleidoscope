using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    public interface IKaleidoscope
    {
        Bitmap ExtractTemplate(Bitmap originalImage, int templateSize, int xOffset, int yOffset, float angle);

        Bitmap GetTileableRectangularPattern(Bitmap template);

        GraphicsPath GetUntransformedTemplateClippingPath(int size);

        PointF GetUntransformedTemplateRectangularSize(int size);
    }
}
