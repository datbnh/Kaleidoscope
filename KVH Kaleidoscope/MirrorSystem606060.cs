using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    public class MirrorSystem606060 : MirrorSystem
    {
        private readonly TransformationSet transformations = new TransformationSet(
            new float[] { -.5f, 0, 1f, 1.5f, 2.5f, 3, 2.5f, 0, 1, 1.5f, 1, 1.5f, 2.5f, 3 },
            new float[] { 2, 1, 1, 2, 2, 1, 2, 1, 1, 0, 1, 0, 0, 1 },
            new float[] { -60, 0, 60, -120, 180, 120, -60, -120, 180, 120, -60, 0, 60, -120 },
            new int[] { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0 }
            );

        public override TransformationSet TransformationSetForTilableRectangularPattern => transformations;

        public override int TilableRectangularPatternHorizontalSpan => 3;

        public override int TilableRectangularPatternVerticalSpan => 2;

        public override PointF[] GetUntransformedTemplateClippingPolygon(float size, float boundaryOverlapping)
        {
            var pSize = GetUntransformedTemplateRectangularSize(size);
            var overlap = GetUntransformedTemplateRectangularSize(boundaryOverlapping);
            return new[] {
                new PointF(-overlap.X, -overlap.Y),
                new PointF(pSize.X + overlap.X, -overlap.Y),
                new PointF(pSize.X/2, pSize.Y + overlap.Y)};
        }

        public override PointF GetUntransformedTemplateRectangularSize(float size)
        {
            return new PointF(size, size * (float)Math.Sqrt(3) / 2);
        }
    }
}
