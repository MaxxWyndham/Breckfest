using System.Drawing;

namespace breckFest
{
    public class Texture
    {
        public virtual Bitmap Bitmap { get; protected set; }

        public static Texture Load(string path)
        {
            return new Texture
            {
                Bitmap = new Bitmap(path)
            };
        }
    }
}