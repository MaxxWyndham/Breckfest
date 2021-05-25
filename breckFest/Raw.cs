using System;
using System.IO;

namespace breckFest
{
    public class Raw
    {
        public static void Load(string path, bool dump = false)
        {
            byte[] buff = new byte[536870912];
            int size = 0;

            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(path)))
            using (BinaryReader br = new BinaryReader(ms))
            {
                if (!IsValid(br, Path.GetExtension(path).Substring(1).ToLower())) { return; }

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    int length = (int)br.ReadUInt32();
 
                    using (MemoryStream lzs = new MemoryStream(br.ReadBytes(length)))
                    using (LZ4Decompress lz4 = new LZ4Decompress(lzs))
                    {
                        size += lz4.Read(buff, size, buff.Length);
                    }

                    if (Path.GetExtension(path).ToLower() == ".pckd") 
                    {
                        using (MemoryStream contentstream = new MemoryStream(buff, 0, size))
                        using (BinaryReader contents = new BinaryReader(contentstream))
                        {
                            contents.ReadBytes(8);

                            int contentCount = contents.ReadInt32();

                            for (int i = 0; i < contentCount; i++)
                            {
                                br.ReadInt32();

                                int filenameLength = contents.ReadInt32();
                                string filename = contents.ReadString(filenameLength);

                                string outFile = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path)), filename.Replace("/", @"\"));

                                if (!Directory.Exists(Path.GetDirectoryName(outFile))) { Directory.CreateDirectory(Path.GetDirectoryName(outFile)); }

                                Console.WriteLine($"   Extracting: {filename}");

                                using (FileStream fs = new FileStream(outFile, FileMode.Create))
                                using (BinaryWriter bw = new BinaryWriter(fs))
                                {
                                    bw.Write(br.ReadBytes(contents.ReadInt32()));

                                    while (br.PeekInt32() != 0 && br.BaseStream.Position < br.BaseStream.Length) { bw.Write(br.ReadByte()); }
                                }

                                if (dump) { Load(outFile, true); }

                                contents.ReadInt32();
                            }
                        }
                    }
                }
            }

            if (dump)
            {
                using (BinaryWriter bw = new BinaryWriter(new FileStream($"{path}.raw", FileMode.Create)))
                {
                    bw.Write(buff, 0, size);
                }
            }
        }

        public static bool IsValid(string path)
        {
            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(path)))
            using (BinaryReader br = new BinaryReader(ms))
            {
                return IsValid(br, Path.GetExtension(path).Substring(1).ToLower());
            }
        }

        public static bool IsValid(BinaryReader br, string extension)
        {
            return (br.ReadUInt32() == 4 &&
                    br.ReadByte() == extension[3] &&
                    br.ReadByte() == extension[2] &&
                    br.ReadByte() == extension[1] &&
                    br.ReadByte() == extension[0] &&
                    br.ReadUInt32() >= 0);
        }
    }
}