**Breckfest v1.5.0**

![alt text](http://www.toxic-ragers.co.uk/images/misc/breckfest.png "Breckfest")

A drag'n'drop image converter for Wreckfest.

v1.5.0 released 2015-07-19  
Download binary [here](http://www.toxic-ragers.co.uk/files/tools/breckfest/Breckfest.v1.5.0.zip) (49.1KB)

Notes:  
About as barebones as you can possibly get.  Enhancements and optimisations coming in future versions.

How to use:  
Drop a bmap on Breckfest.exe to get a png file.  
Drop a png (dds, tga and tif also supported) on Breckfest.exe to get a bmap file.  
Image to BMAP will add a ".x" to the filename (ie Skin5_C5.png would become Skin5_C5.x.bmap), this is to prevent accidentally overwriting of original files. You'll need to rename the files manually and remove the ".x"  
Drop a directory on Breckfest.exe and all* supported files within that folder (not subfolders) will be processed.  
Doubleclicking Breckfest will process the current directory.

\* Files are sorted alphabetically and only the first supported extension will be processed.  ie, if skin5_c5.bmap is processed Breckfest will ignore skin5_c5.png.  Or, put another way, BMAP > PNG will be prioritised over PNG > BMAP

Commandline options:  
-c[lutter] : Generates clutter bmaps  
-dxt1 : Compresses using DXT1 compression  
-dxt5 : Compresses using DXT5 compression  
-raw : No compression  
-f[orce] : Breckfest will automatically overwrite files  
-dump : Decompresses any valid Wreckfest compressed file  
-compress : Compresses any valid Wreckfest decompressed file  
_Breckfest.exe -c "c:\path\to\file.png"_ will create clutter bmap file.x.bmap  
_Breckfest.exe -clutter "c:\path\to\file.png"_ will do the same thing

Filename options:  
Filename.clutter.png will be processed as -clutter and saved as Filename.x.bmap  
Filename.dxt1.png will be processed as -dxt1 and saved as Filename.x.bmap  
Filename.dxt5.png will be processed as -dxt5 and saved as Filename.x.bmap  
And so on

**Changelog**

**v1.5.0**  
Added -compress commandline option  
Fixed "options within the filename" carrying across multiple files

**v1.4.0**  
Breckfest no longer overwrites files automatically  
Added -f[orce] commandline option to force Breckfest to overwrite files automatically  
Added -dump commandline option  
Added -raw, -dxt1 and -dxt5 commandline options  
Added support for "options within the filename"

**v1.3.0**  
Added -c[lutter] commandline option  
Fixed red and blue channels becoming swapped when processing \menu\textures\ bmaps

**v1.2.0**  
Added directory support  
Added support for converting from A8R8G8B8 bmap files

**v1.1.0**  
Name change!  Breckfest looks less broken than Breakfest.  
Added support for DDS, TGA and TIF files  
Increased maximum supported image size to 8192x8192.  This takes bloomin' ages to squish.  
Now supports converting from BMAPs containing raw data (blend_proto_t.bmap is an example of these)

**v1.0.0**  
Initial release
