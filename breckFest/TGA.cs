using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace breckFest
{
    public class TGA
    {
        Bitmap bitmap;

        public Bitmap Bitmap
        {
            get { return bitmap; }
        }

        public static TGA Load(string path)
        {
            return TGA.Load(File.ReadAllBytes(path));
        }

        public static TGA Load(byte[] data)
        {
            TGA tga = new TGA();

            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader br = new BinaryReader(ms))
            {
                BitmapData bmpdata;
                PixelFormat format = PixelFormat.Format32bppArgb;

                byte idLength = br.ReadByte();
                byte colourMapType = br.ReadByte();
                byte imageType = br.ReadByte();

                if (idLength > 0) { throw new NotImplementedException("No support for TGA files with ID sections!"); }
                if (colourMapType == 0) { br.ReadBytes(5); } else { throw new NotImplementedException("No support for TGA files with ColourMaps!"); }

                int xOrigin = br.ReadInt16();
                int yOrigin = br.ReadInt16();
                int width = br.ReadInt16();
                int height = br.ReadInt16();
                byte pixelDepth = br.ReadByte();
                byte size = (byte)(pixelDepth / 8);
                byte imageDescriptor = br.ReadByte();

                if (size == 3) { format = PixelFormat.Format24bppRgb; }

                tga.bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                bmpdata = tga.bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, format);

                if (imageType == 10)
                {
                    const int iRAWSection = 127;
                    uint j = 0;
                    int iStep = 0;
                    int bpCount = 0;
                    int currentPixel = 0;
                    int pixelCount = width * height;
                    var colorBuffer = new byte[size];
                    byte chunkHeader = 0;
                    byte[] buffer = br.ReadBytes((int)br.BaseStream.Length - 13);

                    using (var nms = new MemoryStream())
                    {
                        while (currentPixel < pixelCount)
                        {
                            chunkHeader = buffer[iStep];
                            iStep++;

                            if (chunkHeader <= iRAWSection)
                            {
                                chunkHeader++;
                                bpCount = size * chunkHeader;
                                nms.Write(buffer, iStep, bpCount);
                                iStep += bpCount;

                                currentPixel += chunkHeader;
                            }
                            else
                            {
                                chunkHeader -= iRAWSection;
                                Array.Copy(buffer, iStep, colorBuffer, 0, size);
                                iStep += size;
                                for (j = 0; j < chunkHeader; j++) { nms.Write(colorBuffer, 0, size); }
                                currentPixel += chunkHeader;
                            }
                        }

                        var contentBuffer = new byte[nms.Length];
                        nms.Position = 0;
                        nms.Read(contentBuffer, 0, contentBuffer.Length);

                        Marshal.Copy(contentBuffer, 0, bmpdata.Scan0, contentBuffer.Length);
                    }
                }
                else
                {
                    Marshal.Copy(br.ReadBytes(width * height * size), 0, bmpdata.Scan0, width * height * size);
                }

                tga.bitmap.UnlockBits(bmpdata);
            }

            return tga;
        }
    }
}
