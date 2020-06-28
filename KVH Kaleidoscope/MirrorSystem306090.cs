using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    class MirrorSystem306090 : MirrorSystem
    {
        private readonly TransformationSet transformations = new TransformationSet(
            new float[] { 0, 1, 1.5f, 1.5f, 2, 3, 4, 4.5f, 5, 4, 4.5f, 5, 1, 1.5f, 2, 1, 1.5f, 2, 3, 4, 4.5f, 4.5f, 5, 6 },
            new float[] { 0, 0, 0.5f, 0.5f, 1, 1, 1, 0.5f, 0, 1, 0.5f, 0, 2, 1.5f, 1, 2, 1.5f, 1, 1, 1, 1.5f, 1.5f, 2, 2 },
            new float[] { 0, 60, 60, -120, -120, 180, 180, 120, 120, -60, -60, 0, 180, 120, 120, -60, -60, 0, 0, 60, 60, -120, -120, 180 },
            new int[] { 0, 1, 0, 0, 1, 0, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 0, 1, 0, 0, 1, 0 }
            );
        public override TransformationSet TransformationSetForTilableRectangularPattern => transformations;

        public override int TilableRectangularPatternHorizontalSpan => 6;

        public override int TilableRectangularPatternVerticalSpan => 2;

        public override PointF[] GetUntransformedTemplateClippingPolygon(int size)
        {
            var pSize = GetUntransformedTemplateRectangularSize(size);
            return new[] {
                new PointF(0, 0),
                new PointF(pSize.X, 0),
                new PointF(0, pSize.Y)};
        }

        public override PointF GetUntransformedTemplateRectangularSize(int size)
        {
            return new PointF(size, size * (float)Math.Sqrt(3));
        }
    }
}
