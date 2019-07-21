using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    public class KaleidoscopeTemplate
    {
        private Bitmap bitmap;
        private float width;
        private float height;

        public Bitmap Bitmap { get => bitmap; private set => bitmap = value; }
        public float Width { get => width; private set => width = value; }
        public float Height { get => height; private set => height = value; }

        internal KaleidoscopeTemplate(Bitmap bitmap, float width, float height)
        {
            Bitmap = bitmap;
            Width = width;
            Height = height;
        }
    }
}
