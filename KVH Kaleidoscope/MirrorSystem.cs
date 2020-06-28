using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    public abstract class MirrorSystem
    {
        /// <summary>
        /// The TransformationSet to generate a tilable rectangular pattern.
        /// </summary>
        /// <returns></returns>
        public abstract TransformationSet TransformationSetForTilableRectangularPattern { get; }

        /// <summary>
        /// The horizontal span of the rectangular tilable pattern generated from 
        /// the TransformationSetForTilableRectangularPattern 
        /// in multiplication of template width.
        /// </summary>
        /// <returns></returns>
        public abstract int TilableRectangularPatternHorizontalSpan { get; }

        /// <summary>
        /// The vertical span of the rectangular tilable pattern generated from 
        /// the TransformationSetForTilableRectangularPattern 
        /// in multiplication of template width.
        /// </summary>
        /// <returns></returns>
        public abstract int TilableRectangularPatternVerticalSpan { get; }

        public abstract PointF[] GetUntransformedTemplateClippingPolygon(float size, float boundaryOverlapping=0.25f);

        public abstract PointF GetUntransformedTemplateRectangularSize(float size);
    }
}
