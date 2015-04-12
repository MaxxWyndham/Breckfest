**Breckfest v1.3.0**

![alt text](http://www.toxic-ragers.co.uk/images/misc/breckfest.png "Breckfest")

A drag'n'drop image converter for Wreckfest.

v1.3.0 released 2015-04-12  
Download binary [here](http://www.toxic-ragers.co.uk/files/tools/breckfest/Breckfest.v1.3.0.zip) (46.5KB)

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
_Breckfest.exe -c "c:\path\to\file.png"_ will create clutter bmap file.x.bmap  
_Breckfest.exe -clutter "c:\path\to\file.png"_ will do the same thing

**Changelog**

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
