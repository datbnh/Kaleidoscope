using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    public class TransformationSet
    {
        public readonly float[] XOffsetFactors;
        public readonly float[] YOffsetFactors;
        public readonly float[] Rotations;
        /// <summary>
        /// 0: none, 1: X-flipped
        /// </summary>
        public readonly int[] TemplateIndices;

        public readonly int Length;

        public TransformationSet(float[] xOffsetFactors, float[] yOffsetFactors,
            float[] rotations, int[] templateIndices)
        {
            if (xOffsetFactors.Length != yOffsetFactors.Length ||
                yOffsetFactors.Length != rotations.Length ||
                rotations.Length != templateIndices.Length ||
                templateIndices.Length != xOffsetFactors.Length)
                throw new Exception("Array sizes mismatched.");

            if (templateIndices.Max() > 1 || templateIndices.Min() < 0)
                throw new Exception("Specified templateIndices contains unsupported unsupported value.");

            XOffsetFactors = xOffsetFactors;
            YOffsetFactors = yOffsetFactors;
            Rotations = rotations;
            TemplateIndices = templateIndices;
            Length = XOffsetFactors.Length;
        }
    }
}
