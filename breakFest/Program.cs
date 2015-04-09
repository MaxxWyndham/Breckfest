using System;
using System.IO;

namespace breckFest
{
    class Program
    {
        static void Main(string[] args)
        {
            Version version = new Version(1, 1, 0);

            BMAP bmap = new BMAP();

            Console.WriteLine("Breckfest v{0}.{1}.{2}", version.Major, version.Minor, version.Build);

            if (args.Length > 0)
            {
                if (File.Exists(args[0]))
                {
                    string extension = Path.GetExtension(args[0]).Substring(1);

                    if (extension == "bmap")
                    {
                        if (BMAP.IsBMAP(args[0]))
                        {
                            Console.WriteLine("Loading: {0}", Path.GetFileName(args[0]));
                            bmap = BMAP.Load(args[0], true);
                            Console.WriteLine("Saving : {0}", Path.GetFileName(args[0].Replace(".bmap", ".png")));
                            bmap.SaveAsPNG(args[0].Replace(".bmap", ".png"));
                        }
                        else
                        {
                            Console.WriteLine("This file isn't a bmap!");
                            Console.WriteLine("Press any key to exit");
                            Console.ReadKey();
                        }
                    }
                    else if (Array.IndexOf(new string[] { "dds", "png", "tga", "tif" }, extension) > -1)
                    {
                        switch (extension)
                        {
                            case "dds":
                                Console.WriteLine("Loading  : {0}", Path.GetFileName(args[0]));
                                bmap.Path = Path.GetFileName(args[0]);
                                bmap.DDS = DDS.Load(args[0]);
                                Console.WriteLine("Saving   : {0}", Path.GetFileName(args[0].Replace(".dds", ".bmap")));
                                bmap.Save(args[0].Replace(".dds", ".x.bmap"));
                                break;

                            case "png":
                                Console.WriteLine("Loading  : {0}", Path.GetFileName(args[0]));
                                PNG png = PNG.Load(args[0]);
                                bmap.Path = Path.GetFileName(args[0]);
                                Console.WriteLine("Squishing: {0}x{1} (this might take awhile)", png.Bitmap.Width, png.Bitmap.Height);
                                bmap.DDS = new DDS(D3DFormat.DXT5, png.Bitmap);
                                Console.WriteLine("Saving   : {0}", Path.GetFileName(args[0].Replace(".png", ".bmap")));
                                bmap.Save(args[0].Replace(".png", ".x.bmap"));
                                break;

                            case"tga":
                                Console.WriteLine("Loading  : {0}", Path.GetFileName(args[0]));
                                TGA tga = TGA.Load(args[0]);
                                bmap.Path = Path.GetFileName(args[0]);
                                Console.WriteLine("Squishing: {0}x{1} (this might take awhile)", tga.Bitmap.Width, tga.Bitmap.Height);
                                bmap.DDS = new DDS(D3DFormat.DXT5, tga.Bitmap);
                                Console.WriteLine("Saving   : {0}", Path.GetFileName(args[0].Replace(".tga", ".bmap")));
                                bmap.Save(args[0].Replace(".tga", ".x.bmap"));
                                break;

                            case "tif":
                                Console.WriteLine("Loading  : {0}", Path.GetFileName(args[0]));
                                TIF tif = TIF.Load(args[0]);
                                bmap.Path = Path.GetFileName(args[0]);
                                Console.WriteLine("Squishing: {0}x{1} (this might take awhile)", tif.Bitmap.Width, tif.Bitmap.Height);
                                bmap.DDS = new DDS(D3DFormat.DXT5, tif.Bitmap);
                                Console.WriteLine("Saving   : {0}", Path.GetFileName(args[0].Replace(".tif", ".bmap")));
                                bmap.Save(args[0].Replace(".tif", ".x.bmap"));
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Not entirely sure what you're trying to achieve here.");
                        Console.WriteLine("Press any key to exit");
                        Console.ReadKey();
                    }
                }
            }
            else
            {
                // file all files in current folder and convert
            }
        }
    }
}
