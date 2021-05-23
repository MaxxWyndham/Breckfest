using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace breckFest
{
    public class BMAP
    {
        public string Path { get; set; }

        public int Mode { get; set; }

        public DDS DDS { get; set; }

        public Bitmap Raw { get; set; }

        public static BMAP Load(string path, bool dump = false)
        {
            BMAP bmap = new BMAP();

            byte[] buff = new byte[536870912];
            int size = 0;

            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(path)))
            using (BinaryReader br = new BinaryReader(ms))
            {
                if (!IsBMAP(br)) { return null; }

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    int length = (int)br.ReadUInt32();

                    using (MemoryStream lzs = new MemoryStream(br.ReadBytes(length)))
                    using (LZ4Decompress lz4 = new LZ4Decompress(lzs))
                    {
                        size += lz4.Read(buff, size, buff.Length);
                    }
                }
            }

            if (dump)
            {
                using (BinaryWriter bw = new BinaryWriter(new FileStream(path.Replace(".bmap", ".raw"), FileMode.Create)))
                {
                    bw.Write(buff, 0, size);
                }
            }

            using (MemoryStream ms = new MemoryStream(buff, 0, size))
            using (BinaryReader br = new BinaryReader(ms))
            {
                bmap.Mode = (int)br.ReadUInt32();
                bmap.Path = br.ReadString((int)br.ReadUInt32());
                int dataSize = (int)br.ReadUInt32();

                switch (bmap.Mode)
                {
                    case 0:
                        bmap.DDS = DDS.Load(br.ReadBytes(dataSize));
                        break;

                    case 1:
                        br.ReadUInt16();
                        br.ReadUInt16();
                        br.ReadUInt32();
                        br.ReadUInt32();
                        bmap.Raw = new Bitmap(br.ReadUInt16(), br.ReadUInt16(), PixelFormat.Format32bppArgb);
                        br.ReadNullTerminatedString();

                        BitmapData bmpdata = bmap.Raw.LockBits(new Rectangle(0, 0, bmap.Raw.Width, bmap.Raw.Height), ImageLockMode.ReadWrite, bmap.Raw.PixelFormat);
                        dataSize = (int)(br.BaseStream.Length - br.BaseStream.Position);
                        Marshal.Copy(br.ReadBytes(dataSize), 0, bmpdata.Scan0, dataSize);
                        break;
                }
            }

            return bmap;
        }

        public void Save(string path, bool compress = true)
        {
            // Wreckfest doesn't seem to support uncompressed files.
            // compress is just for eyeballing things

            using (BinaryWriter bw = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                bw.Write(4);
                bw.Write((byte)0x70);
                bw.Write((byte)0x61);
                bw.Write((byte)0x6D);
                bw.Write((byte)0x62);
                bw.Write(3);

                byte[] input = getAllBytes();

                if (compress)
                {
                    int[] hashTable = new int[1 << (14 - 2)];
                    byte[] output = new byte[LZ4Compress.CalculateChunkSize(input.Length)];
                    int i = 0;

                    while (i < input.Length)
                    {
                        byte[] chunk = new byte[Math.Min(input.Length - i, output.Length)];

                        Array.Copy(input, i, chunk, 0, chunk.Length);
                        Array.Clear(hashTable, 0, hashTable.Length);

                        int size = LZ4Compress.Compress(hashTable, chunk, output, chunk.Length, chunk.Length + 4);

                        bw.Write(size);
                        bw.Write(output, 0, size);

                        i += chunk.Length;
                    }
                }
                else
                {
                    bw.Write(input);
                }
            }
        }

        public void SaveAs(string path, OutputFormat outputFormat)
        {
            if (Mode == 0)
            {
                switch (outputFormat)
                {
                    case OutputFormat.PNG:
                        DDS.Decompress().Save(path, ImageFormat.Png);
                        break;

                    case OutputFormat.DDS:
                        DDS.Save(path);
                        break;

                    case OutputFormat.TGA:
                        TGA.FromBitmap(DDS.Decompress()).Save(path);
                        break;
                }
                
            }
            else
            {
                Raw.Save(path, ImageFormat.Png);
            }
        }

        public static bool IsBMAP(string path)
        {
            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(path)))
            using (BinaryReader br = new BinaryReader(ms))
            {
                return IsBMAP(br);
            }
        }

        public static bool IsBMAP(BinaryReader br)
        {
            return (br.ReadUInt32() == 4 &&
                    br.ReadByte() == 0x70 && // p
                    br.ReadByte() == 0x61 && // a
                    br.ReadByte() == 0x6D && // m
                    br.ReadByte() == 0x62 && // b
                    br.ReadUInt32() == 3);
        }

        private byte[] getAllBytes()
        {
            byte[] b;

            if (Mode == 0)
            {
                int ddsSize = 128;

                for (int i = 0; i < DDS.MipMaps.Count; i++)
                {
                    ddsSize += DDS.MipMaps[i].Data.Length;
                }

                b = new byte[4 + 4 + Path.Length + 4 + ddsSize];

                using (MemoryStream ms = new MemoryStream(b))
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(Mode);                 // 4
                    bw.Write(Path.Length);          // 4
                    bw.Write(Path.ToCharArray());   // path.length
                    bw.Write(ddsSize);              // 4
                    DDS.Save(bw, DDS);         // ddsSize
                }
            }
            else
            {
                int rawSize = 46 + (Raw.Width * Raw.Height * 4);
                int offset = 0;
                b = new byte[4 + 4 + Path.Length + 4 + rawSize];

                using (MemoryStream ms = new MemoryStream(b))
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(Mode);                 // 4
                    bw.Write(Path.Length);          // 4
                    bw.Write(Path.ToCharArray());   // path.length
                    bw.Write(rawSize);              // 4
                    bw.Write((short)0x1c);
                    bw.Write((short)0x02);
                    bw.Write(0);
                    bw.Write(0);
                    bw.Write((short)Raw.Width);
                    bw.Write((short)Raw.Height);
                    bw.Write(" (Bugbear Entertainment Ltd. ".ToCharArray());
                    bw.Write((byte)0);

                    offset = (int)bw.BaseStream.Position;
                }

                BitmapData bmpdata = Raw.LockBits(new Rectangle(0, 0, Raw.Width, Raw.Height), ImageLockMode.ReadOnly, Raw.PixelFormat);
                Marshal.Copy(bmpdata.Scan0, b, offset, bmpdata.Stride * bmpdata.Height);
                Raw.UnlockBits(bmpdata);
            }

            return b;
        }
    }
}
