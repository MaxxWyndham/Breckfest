using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace breckFest
{
    public enum OutputFormat
    {
        PNG,
        DDS,
        TGA,
        TIF
    }

    class Program
    {
        static BreckfestSettings settings = new BreckfestSettings();

        static void Main(string[] args)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            string suppliedPath = null;
            string suppliedFile = null;

            Console.WriteLine($"Breckfest v{version.Major}.{version.Minor}.{version.Build}");

            if (File.Exists("extensions.bowl")) { WreckfestExtensions.Load(Path.GetFullPath("extensions.bowl")); }

            if (args.Length > 0)
            {
                foreach (string s in args)
                {
                    if (s.StartsWith("-"))
                    {
                        // functionality
                        switch (s)
                        {
                            case "-clutter":
                            case "-c":
                                settings.Clutter = true;
                                break;

                            case "-force":
                            case "-f":
                                settings.ForceOverwrite = true;
                                break;

                            case "-dump":
                                settings.Raw = true;
                                break;

                            case "-compress":
                                settings.Compress = true;
                                break;

                            case "-dxt1":
                                settings.Format = D3DFormat.DXT1;
                                break;

                            case "-dxt5":
                                settings.Format = D3DFormat.DXT5;
                                break;

                            case "-bc5u":
                                settings.Format = D3DFormat.BC5U;
                                settings.GenerateMipMaps = false;
                                break;

                            case "-raw":
                                settings.Format = D3DFormat.A8R8G8B8;
                                break;

                            case "-png":
                                settings.SaveAs = OutputFormat.PNG;
                                break;

                            case "-dds":
                                settings.SaveAs = OutputFormat.DDS;
                                break;

                            case "-tga":
                                settings.SaveAs = OutputFormat.TGA;
                                break;

                            case "-tif":
                                settings.SaveAs = OutputFormat.TIF;
                                break;

                            case "-norename":
                            case "-nr":
                                settings.NoRename = true;
                                break;

                            case "-nomipmaps":
                                settings.GenerateMipMaps = false;
                                break;

                            default:
                                Console.WriteLine($"Unknown argument: {s}");
                                return;
                        }
                    }
                    else
                    {
                        if (File.Exists(s))
                        {
                            suppliedFile = s;
                        }
                        else if (Directory.Exists(s))
                        {
                            suppliedPath = s;
                        }
                    }
                }
            }

            if (suppliedPath != null)
            {
                processFolder(suppliedPath);
            }
            else if (suppliedFile != null)
            {
                processFile(suppliedFile);
            }
            else
            {
                processFolder(Environment.CurrentDirectory);
            }
        }

        private static void processFolder(string path)
        {
            List<string> processedFiles = new List<string>();

            foreach (string file in Directory.GetFiles(path).OrderBy(f => f))
            {
                if (processedFiles.Contains(Path.GetDirectoryName(path) + Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file))))
                {
                    Console.WriteLine($"Skipping : {Path.GetFileName(file)}");
                    continue;
                }

                string processed = processFile(file);

                if (processed != null)
                {
                    processedFiles.Add(Path.GetDirectoryName(path) + Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file)));
                }
            }
        }

        private static string processFile(string path)
        {
            path = Path.GetFullPath(path);

            BMAP bmap = new BMAP();
            string extension = Path.GetExtension(path).Substring(1).ToLower();
            string outputName;

            if (settings.Raw)
            {
                if (Raw.IsValid(path))
                {
                    Console.WriteLine($"Loading  : {Path.GetFileName(path)}");
                    Raw.Load(path, true);

                    return path;
                }
            }
            else if (settings.Compress)
            {
                if (WreckfestExtensions.Contains(extension))
                {
                    using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(path)))
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        if (
                            br.ReadInt32() != 4 ||
                            br.ReadByte() != extension[3] ||
                            br.ReadByte() != extension[2] ||
                            br.ReadByte() != extension[1] ||
                            br.ReadByte() != extension[0])
                        {
                            br.BaseStream.Position = 0;
                            byte[] input = br.ReadBytes((int)ms.Length);

                            File.Move(path, $"{path}.bak");

                            using (BinaryWriter bw = new BinaryWriter(new FileStream(path, FileMode.Create)))
                            {
                                bw.Write(4);
                                bw.Write(extension[3]);
                                bw.Write(extension[2]);
                                bw.Write(extension[1]);
                                bw.Write(extension[0]);
                                bw.Write(WreckfestExtensions.HeaderFor(extension));

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
                        }
                        else
                        {
                            Console.WriteLine($"Skipping : {Path.GetFileName(path)} is already compressed");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error    : unsupported extension '{extension}'.");
                }
            }
            else if (extension == "bmap")
            {
                if (BMAP.IsBMAP(path))
                {
                    Console.WriteLine($"Loading  : {Path.GetFileName(path)}");
                    bmap = BMAP.Load(path, false);

                    outputName = $"{Path.GetFileNameWithoutExtension(path)}.{(bmap.Mode == 1 ? "clutter" : (bmap.DDS.Format == D3DFormat.A8R8G8B8 ? "raw" : bmap.DDS.Format.ToString().ToLower()))}.{settings.SaveAs}";

                    Console.WriteLine($"Saving   : {outputName}");
                    if (!overwrite($@"{Path.GetDirectoryName(path)}\{outputName}")) { return null; }
                    bmap.SaveAs(outputName, settings.SaveAs);

                    return path;
                }
            }
            else if (extension == "dds")
            {
                string newExtension = $"{(settings.NoRename ? "" : ".x")}.bmap";
                string outFile = $"{Path.GetFileNameWithoutExtension(path)}{newExtension}";

                Console.WriteLine($"Loading  : {Path.GetFileName(path)}");
                Console.WriteLine($"Saving   : {outFile}");

                bmap.Path = path.Contains(@"\data\") ? path.Substring(path.IndexOf(@"data\")).Replace(@"\", "/") : Path.GetFileName(path);
                bmap.DDS = DDS.Load(path);
                if (!overwrite(outFile)) { return null; }
                bmap.Save(outFile);

                return path;
            }
            else if (Array.IndexOf(new string[] { "png", "tga", "tif" }, extension) > -1)
            {
                Texture texture = null;
                Console.WriteLine($"Loading   : {Path.GetFileName(path)}");

                switch (extension)
                {
                    case "tga":
                        texture = TGA.Load(path);
                        break;

                    case "tif":
                    case "png":
                        texture = Texture.Load(path);
                        break;
                }

                BreckfestSettings original = settings.Clone();

                if (Path.GetFileNameWithoutExtension(path).EndsWith(".clutter", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Clutter = true;
                    path = path.Replace(".clutter", "", true);
                }
                else if (Path.GetFileNameWithoutExtension(path).EndsWith(".raw", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Format = D3DFormat.A8R8G8B8;
                    path = path.Replace(".raw", "", true);
                }
                else if (Path.GetFileNameWithoutExtension(path).EndsWith(".dxt1", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Format = D3DFormat.DXT1;
                    path = path.Replace(".dxt1", "", true);
                }
                else if (Path.GetFileNameWithoutExtension(path).EndsWith(".dxt5", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Format = D3DFormat.DXT5;
                    path = path.Replace(".dxt5", "", true);
                }
                else if (Path.GetFileNameWithoutExtension(path).EndsWith(".bc5u", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Format = D3DFormat.BC5U;
                    settings.GenerateMipMaps = false;
                    path = path.Replace(".bc5u", "", true);
                }

                bmap.Path = path.Contains(@"\data\") ? path.Substring(path.IndexOf(@"data\")).Replace(@"\", "/") : Path.GetFileName(path);

                if (settings.Clutter)
                {
                    Console.WriteLine($"Cluttering: {texture.Bitmap.Width}x{texture.Bitmap.Height}");

                    bmap.Mode = 1;
                    bmap.Raw = texture.Bitmap;
                }
                else
                {
                    bmap.DDS = new DDS(settings.Format, texture.Bitmap, settings.GenerateMipMaps);
                }

                string newExtension = $"{(settings.NoRename ? "" : ".x")}.bmap";
                string outFile = $"{Path.GetFileNameWithoutExtension(path)}{newExtension}";

                Console.WriteLine($"Saving    : {outFile}");
                if (!overwrite(outFile)) { return null; }
                bmap.Save(outFile, true);

                settings = original.Clone();

                return path;
            }

            return null;
        }

        private static bool overwrite(string path)
        {
            if (!File.Exists(path))
            {
                return true;
            }
            else
            {
                if (!settings.ForceOverwrite)
                {
                    Console.WriteLine($"Warning  : {path} already exists.");
                    Console.WriteLine("Overwrite?");

                    while (true)
                    {
                        ConsoleKey key = Console.ReadKey(true).Key;

                        if (key == ConsoleKey.N)
                        {
                            Console.WriteLine("N");
                            return false;
                        }
                        else if (key == ConsoleKey.Y)
                        {
                            Console.WriteLine("Y");
                            return true;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }
        }
    }

    public class BreckfestSettings
    {
        public bool Clutter { get; set; }

        public bool Raw { get; set; }

        public bool Compress { get; set; }

        public D3DFormat Format { get; set; } = D3DFormat.DXT5;

        public bool ForceOverwrite { get; set; }

        public bool NoRename { get; set; }

        public OutputFormat SaveAs { get; set; } = OutputFormat.PNG;

        public bool GenerateMipMaps { get; set; } = true;

        public BreckfestSettings Clone()
        {
            return new BreckfestSettings
            {
                Clutter = Clutter,
                Raw = Raw,
                Compress = Compress,
                ForceOverwrite = ForceOverwrite,
                Format = Format,
                NoRename = NoRename,
                SaveAs = SaveAs,
                GenerateMipMaps = GenerateMipMaps
            };
        }
    }
}