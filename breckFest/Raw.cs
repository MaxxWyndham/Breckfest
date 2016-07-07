/*
Copyright 2016 Maxx Wyndham

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using System.IO;

namespace breckFest
{
    public class Raw
    {
        public static void Load(string path, bool bDump = false)
        {
            byte[] buff = new byte[134217728];
            int size = 0;

            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(path)))
            using (BinaryReader br = new BinaryReader(ms))
            {
                if (!IsValid(br, Path.GetExtension(path).Substring(1).ToLower())) { return; }

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    int length = (int)br.ReadUInt32();

                    using (var lzs = new MemoryStream(br.ReadBytes(length)))
                    using (var lz4 = new LZ4Decompress(lzs))
                    {
                        size += lz4.Read(buff, size, buff.Length);
                    }
                }
            }

            if (bDump)
            {
                using (BinaryWriter bw = new BinaryWriter(new FileStream(path + ".raw", FileMode.Create)))
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
