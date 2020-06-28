using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    class MirrorSystem459045 : MirrorSystem
    {
        private readonly TransformationSet transformations = new TransformationSet(
            new float[] { 0, 1, 1, 0, 1, 2, 2, 1, 0, 1, 1, 0, 1, 2, 2, 1 },
            new float[] { 0, 0, 2, 2, 0, 0, 2, 2, 2, 2, 4, 4, 2, 2, 4, 4 },
            new float[] { 0, 90, 180, 270, 0, 90, 180, 270, 0, 90, 180, 270, 0, 90, 180, 270 },
            new int[] { 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1 }
            );

        public override TransformationSet TransformationSetForTilableRectangularPattern => transformations;

        public override int TilableRectangularPatternHorizontalSpan => 2;

        public override int TilableRectangularPatternVerticalSpan => 4;

        public override PointF[] GetUntransformedTemplateClippingPolygon(int size)
        {
            var pSize = GetUntransformedTemplateRectangularSize(size);
            return new[] {
                new PointF(0, 0),
                new PointF(pSize.X, 0),
                new PointF(pSize.X/2, pSize.Y)};
        }

        public override PointF GetUntransformedTemplateRectangularSize(int size)
        {
            return new PointF(size, size / 2);
        }
    }
}
