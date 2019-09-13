using System.Drawing;

namespace Kvh.Kaleidoscope
{
    public class KaleidoscopeTemplate
    {
        public Bitmap Bitmap { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }

        internal KaleidoscopeTemplate(Bitmap bitmap, float width, float height)
        {
            Bitmap = bitmap;
            Width = width;
            Height = height;
        }
    }
}