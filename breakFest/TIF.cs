using System;
using System.Drawing;

namespace breckFest
{
    public class TIF
    {
        Bitmap bitmap;

        public Bitmap Bitmap
        {
            get { return bitmap; }
        }

        public static TIF Load(string path)
        {
            TIF tif = new TIF();

            tif.bitmap = new Bitmap(path);

            return tif;
        }
    }
}
