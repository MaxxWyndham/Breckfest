using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace breckFest
{
    class Program
    {
        static BreckfestSettings settings = new BreckfestSettings();

        static void Main(string[] args)
        {
            Version version = new Version(1, 4, 0);

            Dictionary<string, string> commands = new Dictionary<string, string>();

            string lastCommand;
            string suppliedPath = null;
            string suppliedFile = null;

            Console.WriteLine("Breckfest v{0}.{1}.{2}", version.Major, version.Minor, version.Build);

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

                            case "-dxt1":
                                settings.Format = D3DFormat.DXT1;
                                break;

                            case "-dxt5":
                                settings.Format = D3DFormat.DXT5;
                                break;

                            case "-raw":
                                settings.Format = D3DFormat.A8R8G8B8;
                                break;

                            default:
                                Console.WriteLine("Unknown argument: {0}", s);
                                return;
                        }
                    }
                    else
                    {
                        lastCommand = "";

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
                    Console.WriteLine("Skipping : {0}", Path.GetFileName(file));
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
            BMAP bmap = new BMAP();
            string extension = Path.GetExtension(path).Substring(1);
            string outputName = "";

            if (settings.Raw)
            {
                if (Raw.IsValid(path))
                {
                    Console.WriteLine("Loading  : {0}", Path.GetFileName(path));
                    Raw.Load(path, true);

                    return path;
                }
            }
            else if (extension == "bmap")
            {
                if (BMAP.IsBMAP(path))
                {
                    Console.WriteLine("Loading  : {0}", Path.GetFileName(path));
                    bmap = BMAP.Load(path, false);

                    outputName = string.Format("{0}.{1}.png", Path.GetFileNameWithoutExtension(path), (bmap.Mode == 1 ? "clutter" : (bmap.DDS.Format == D3DFormat.A8R8G8B8 ? "raw" : bmap.DDS.Format.ToString().ToLower())));

                    Console.WriteLine("Saving   : {0}", outputName);
                    if (!Overwrite(string.Format(@"{0}\{1}", Path.GetDirectoryName(path), outputName))) { return null; }
                    bmap.SaveAsPNG(outputName);

                    return path;
                }
            }
            else if (extension == "dds")
            {
                Console.WriteLine("Loading  : {0}", Path.GetFileName(path));
                bmap.Path = Path.GetFileName(path);
                bmap.DDS = DDS.Load(path);
                Console.WriteLine("Saving   : {0}", Path.GetFileName(path.Replace(".dds", ".bmap")));
                bmap.Save(path.Replace(".dds", ".x.bmap"));

                return path;
            }
            else if (Array.IndexOf(new string[] { "png", "tga", "tif" }, extension) > -1)
            {
                Texture texture = null;
                Console.WriteLine("Loading   : {0}", Path.GetFileName(path));

                switch (extension)
                {
                    case "png":
                        texture = PNG.Load(path);
                        break;

                    case "tga":
                        texture = TGA.Load(path);
                        break;

                    case "tif":
                        texture = TIF.Load(path);
                        break;
                }

                if (Path.GetFileNameWithoutExtension(path).EndsWith(".clutter", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Clutter = true;
                    path = path.Replace(".clutter", "");
                }
                else if (Path.GetFileNameWithoutExtension(path).EndsWith(".raw", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Format = D3DFormat.A8R8G8B8;
                    path = path.Replace(".raw", "");
                }
                else if (Path.GetFileNameWithoutExtension(path).EndsWith(".dxt1", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Format = D3DFormat.DXT1;
                    path = path.Replace(".dxt1", "");
                }
                else if (Path.GetFileNameWithoutExtension(path).EndsWith(".dxt5", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Format = D3DFormat.DXT5;
                    path = path.Replace(".dxt5", "");
                }

                bmap.Path = Path.GetFileName(path);

                if (settings.Clutter)
                {
                    Console.WriteLine("Cluttering: {0}x{1}", texture.Bitmap.Width, texture.Bitmap.Height);

                    bmap.Mode = 1;
                    bmap.Raw = texture.Bitmap;
                }
                else
                {
                    if (settings.Format == D3DFormat.A8R8G8B8)
                    {
                        Console.WriteLine("Formatting: {0}x{1} (this might take awhile)", texture.Bitmap.Width, texture.Bitmap.Height);
                    }
                    else
                    {
                        Console.WriteLine("Squishing : {0}x{1} (this might take awhile)", texture.Bitmap.Width, texture.Bitmap.Height);
                    }

                    bmap.DDS = new DDS(settings.Format, texture.Bitmap);
                }

                Console.WriteLine("Saving    : {0}", Path.GetFileName(path.Replace(string.Format(".{0}", extension), ".bmap")));
                bmap.Save(path.Replace(string.Format(".{0}", extension), ".x.bmap"), true);

                return path;
            }

            return null;
        }

        private static bool Overwrite(string path)
        {
            if (!File.Exists(path))
            {
                return true;
            }
            else
            {
                if (!settings.ForceOverwrite)
                {
                    Console.WriteLine("Warning  : {0} already exists.", path);
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
        bool clutter;
        bool raw;
        D3DFormat format;
        bool force;

        public bool Clutter
        {
            get { return clutter; }
            set { clutter = value; }
        }

        public bool Raw
        {
            get { return raw; }
            set { raw = value; }
        }

        public D3DFormat Format
        {
            get { return format; }
            set { format = value; }
        }

        public bool ForceOverwrite
        {
            get { return force; }
            set { force = value; }
        }

        public BreckfestSettings()
        {
            this.clutter = false;
            this.raw = false;
            this.force = false;
            this.format = D3DFormat.DXT5;
        }
    }
}