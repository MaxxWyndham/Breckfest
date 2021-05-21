using System.IO;

namespace breckFest
{
    public static class ExtensionMethods
    {
        public static string ReadString(this BinaryReader br, int length)
        {
            if (length == 0) { return null; }

            char[] c = br.ReadChars(length);
            int l = length;

            for (int i = 0; i < length; i++)
            {
                if (c[i] == 0)
                {
                    l = i;
                    break;
                }
            }

            return new string(c, 0, l);
        }

        public static string ReadNullTerminatedString(this BinaryReader br)
        {
            string r = "";
            char c;

            do
            {
                c = br.ReadChar();
                if (c > 0) { r += c; }
            } while (c > 0);

            return r;
        }
    }
}
