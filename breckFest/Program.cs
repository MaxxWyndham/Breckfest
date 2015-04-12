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
            Version version = new Version(1, 3, 0);

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
            else
            {
                suppliedPath = Environment.CurrentDirectory;
            }

            if (suppliedPath != null) { processFolder(suppliedPath); }
            if (suppliedFile != null) { processFile(suppliedFile); }
        }

        private static void processFolder(string path)
        {
            List<string> processedFiles = new List<string>();

            foreach (string file in Directory.GetFiles(path).OrderBy(f => f))
            {
                if (processedFiles.Contains(Path.GetDirectoryName(path) + Path.GetFileNameWithoutExtension(file)))
                {
                    Console.WriteLine("Skipping : {0}", Path.GetFileName(file));
                    continue;
                }

                string processed = processFile(file);

                if (processed != null)
                {
                    processedFiles.Add(Path.GetDirectoryName(path) + Path.GetFileNameWithoutExtension(file));
                }
            }
        }

        private static string processFile(string path)
        {
            BMAP bmap = new BMAP();
            string extension = Path.GetExtension(path).Substring(1);

            if (extension == "bmap")
            {
                if (BMAP.IsBMAP(path))
                {
                    Console.WriteLine("Loading  : {0}", Path.GetFileName(path));
                    bmap = BMAP.Load(path, false);
                    Console.WriteLine("Saving   : {0}", Path.GetFileName(path.Replace(".bmap", ".png")));
                    bmap.SaveAsPNG(path.Replace(".bmap", ".png"));

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
                Console.WriteLine("Loading  : {0}", Path.GetFileName(path));

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

                bmap.Path = Path.GetFileName(path);

                if (settings.Clutter)
                {
                    Console.WriteLine("Cluttering   : {0}x{1}", texture.Bitmap.Width, texture.Bitmap.Height);
                    bmap.Mode = 1;
                    bmap.Raw = texture.Bitmap;
                }
                else
                {
                    Console.WriteLine("Squishing: {0}x{1} (this might take awhile)", texture.Bitmap.Width, texture.Bitmap.Height);
                    bmap.DDS = new DDS(D3DFormat.DXT5, texture.Bitmap);
                }

                Console.WriteLine("Saving   : {0}", Path.GetFileName(path.Replace(string.Format(".{0}", extension), ".bmap")));
                bmap.Save(path.Replace(string.Format(".{0}", extension), ".x.bmap"));

                return path;
            }

            return null;
        }
    }

    public class BreckfestSettings
    {
        bool clutter;

        public bool Clutter
        {
            get { return clutter; }
            set { clutter = value; }
        }

        public BreckfestSettings()
        {
            this.clutter = false;
        }
    }
}