using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace breckFest
{
    class Program
    {
        static void Main(string[] args)
        {
            Version version = new Version(1, 2, 0);

            Console.WriteLine("Breckfest v{0}.{1}.{2}", version.Major, version.Minor, version.Build);

            if (args.Length > 0)
            {
                if (File.Exists(args[0]))
                {
                    processFile(args[0]);
                }
                else if (Directory.Exists(args[0]))
                {
                    processFolder(args[0]);
                }
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
            else if (Array.IndexOf(new string[] { "dds", "png", "tga", "tif" }, extension) > -1)
            {
                switch (extension)
                {
                    case "dds":
                        Console.WriteLine("Loading  : {0}", Path.GetFileName(path));
                        bmap.Path = Path.GetFileName(path);
                        bmap.DDS = DDS.Load(path);
                        Console.WriteLine("Saving   : {0}", Path.GetFileName(path.Replace(".dds", ".bmap")));
                        bmap.Save(path.Replace(".dds", ".x.bmap"));
                        break;

                    case "png":
                        Console.WriteLine("Loading  : {0}", Path.GetFileName(path));
                        PNG png = PNG.Load(path);
                        bmap.Path = Path.GetFileName(path);
                        Console.WriteLine("Squishing: {0}x{1} (this might take awhile)", png.Bitmap.Width, png.Bitmap.Height);
                        bmap.DDS = new DDS(D3DFormat.DXT5, png.Bitmap);
                        Console.WriteLine("Saving   : {0}", Path.GetFileName(path.Replace(".png", ".bmap")));
                        bmap.Save(path.Replace(".png", ".x.bmap"));
                        break;

                    case "tga":
                        Console.WriteLine("Loading  : {0}", Path.GetFileName(path));
                        TGA tga = TGA.Load(path);
                        bmap.Path = Path.GetFileName(path);
                        Console.WriteLine("Squishing: {0}x{1} (this might take awhile)", tga.Bitmap.Width, tga.Bitmap.Height);
                        bmap.DDS = new DDS(D3DFormat.DXT5, tga.Bitmap);
                        Console.WriteLine("Saving   : {0}", Path.GetFileName(path.Replace(".tga", ".bmap")));
                        bmap.Save(path.Replace(".tga", ".x.bmap"));
                        break;

                    case "tif":
                        Console.WriteLine("Loading  : {0}", Path.GetFileName(path));
                        TIF tif = TIF.Load(path);
                        bmap.Path = Path.GetFileName(path);
                        Console.WriteLine("Squishing: {0}x{1} (this might take awhile)", tif.Bitmap.Width, tif.Bitmap.Height);
                        bmap.DDS = new DDS(D3DFormat.DXT5, tif.Bitmap);
                        Console.WriteLine("Saving   : {0}", Path.GetFileName(path.Replace(".tif", ".bmap")));
                        bmap.Save(path.Replace(".tif", ".x.bmap"));
                        break;
                }

                return path;
            }

            return null;
        }
    }
}
