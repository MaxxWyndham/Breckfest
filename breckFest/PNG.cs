using System;
using System.Drawing;

namespace breckFest
{
    public class PNG
    {
        Bitmap bitmap;

        public Bitmap Bitmap
        {
            get { return bitmap; }
        }

        public static PNG Load(string path)
        {
            PNG png = new PNG();

            png.bitmap = new Bitmap(path);

            return png;
        }
    }
}
