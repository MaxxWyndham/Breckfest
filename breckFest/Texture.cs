using System;
using System.Drawing;

namespace breckFest
{
    public class Texture
    {
        protected Bitmap bitmap;

        public Bitmap Bitmap
        {
            get { return bitmap; }
        }

        public static Texture Load(string path)
        {
            Texture texture = new Texture();

            texture.bitmap = new Bitmap(path);

            return texture;
        }
    }
}