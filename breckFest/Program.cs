using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace breckFest
{
    public enum WreckfestExtensions
    {
        agan = 1,
        agen = 0,
        aist = 0,
        apbl = 0,
        bmap = 3,
        ccms = 0,
        ccps = 0,
        ccrs = 0,
        clgs = 0,
        clhe = 1,
        clmx = 0,
        cwea = 0,
        dcty = 0,
        dfsc = 3,
        dmsd = 0,
        dntp = 1,
        dset = 4,
        enga = 3,
        engs = 6,
        evss = 0,
        fnts = 1,
        fxac = 1,
        fxaf = 0,
        fxbl = 0,
        fxbp = 0,
        fxcn = 0,
        fxdf = 0,
        fxlf = 0,
        fxpm = 0,
        fxsf = 0,
        fxss = 0,
        fxtg = 0,
        fxtr = 0,
        gmpl = 0xd,
        grge = 3,
        irsu = 0,
        jobs = 0,
        jodb = 0,
        jofi = 0,
        johi = 0,
        jopi = 0,
        joun = 0,
        mchc = 0,
        panl = 4,
        prfb = 0,
        rlao = 0,
        rlod = 0,
        scne = 8,
        srfl = 0,
        surs = 1,
        tcat = 2,
        teli = 0,
        teno = 2,
        upgb = 0,
        upgr = 0xd,
        upss = 1,
        vail = 0,
        vdst = 0,
        vean = 2,
        vebr = 1,
        vech = 3,
        vedi = 3,
        veen = 8,
        vees = 1,
        vege = 3,
        vehi = 5,
        vesh = 1,
        vest = 4,
        vesu = 5,
        veti = 7,
        vetr = 0,
        vhae = 0,
        vhcl = 2,
        vhcp = 0,
        vpdl = 0,
        vpdp = 0,
        vpdr = 1,
        vpst = 2,
        vsbd = 2,
        vsbp = 1,
        vsks = 1,
        vstd = 1,
        weat = 3,
        weli = 0
    }

    public enum OutputFormat
    {
        PNG,
        DDS,
        TGA
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
                if (Enum.TryParse(extension, out WreckfestExtensions we))
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
                                bw.Write((int)we);

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