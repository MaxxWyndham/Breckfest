using System;
using System.Collections.Generic;
using System.IO;

namespace breckFest
{
    public static class WreckfestExtensions
    {
        private static Dictionary<string, int> Extensions { get; set; } = new Dictionary<string, int>
        {
            ["agan"] = 1,
            ["agen"] = 0,
            ["aist"] = 0,
            ["apbl"] = 0,
            ["bmap"] = 3,
            ["ccms"] = 0,
            ["ccps"] = 0,
            ["ccrs"] = 0,
            ["clgs"] = 0,
            ["clhe"] = 1,
            ["clmx"] = 0,
            ["cwea"] = 0,
            ["dcty"] = 0,
            ["dfsc"] = 3,
            ["dmsd"] = 0,
            ["dntp"] = 1,
            ["dset"] = 4,
            ["enga"] = 3,
            ["engs"] = 6,
            ["evss"] = 0,
            ["fnts"] = 1,
            ["fxac"] = 1,
            ["fxaf"] = 0,
            ["fxbl"] = 0,
            ["fxbp"] = 0,
            ["fxcn"] = 0,
            ["fxdf"] = 0,
            ["fxlf"] = 0,
            ["fxpm"] = 0,
            ["fxsf"] = 0,
            ["fxss"] = 0,
            ["fxtg"] = 0,
            ["fxtr"] = 0,
            ["gmpl"] = 0xd,
            ["grge"] = 3,
            ["irsu"] = 0,
            ["jobs"] = 0,
            ["jodb"] = 0,
            ["jofi"] = 0,
            ["johi"] = 0,
            ["jopi"] = 0,
            ["joun"] = 0,
            ["mchc"] = 0,
            ["panl"] = 4,
            ["prfb"] = 0,
            ["rlao"] = 0,
            ["rlod"] = 0,
            ["scne"] = 8,
            ["srfl"] = 0,
            ["surs"] = 1,
            ["tcat"] = 2,
            ["teli"] = 0,
            ["teno"] = 2,
            ["upgb"] = 0,
            ["upgr"] = 0xd,
            ["upss"] = 1,
            ["vail"] = 0,
            ["vdst"] = 0,
            ["vean"] = 2,
            ["vebr"] = 1,
            ["vech"] = 3,
            ["vedi"] = 3,
            ["veen"] = 8,
            ["vees"] = 1,
            ["vege"] = 3,
            ["vehi"] = 5,
            ["vesh"] = 1,
            ["vest"] = 4,
            ["vesu"] = 5,
            ["veti"] = 7,
            ["vetr"] = 0,
            ["vhae"] = 0,
            ["vhcl"] = 2,
            ["vhcp"] = 0,
            ["vpdl"] = 0,
            ["vpdp"] = 0,
            ["vpdr"] = 1,
            ["vpst"] = 2,
            ["vsbd"] = 2,
            ["vsbp"] = 1,
            ["vsks"] = 1,
            ["vstd"] = 1,
            ["weat"] = 3,
            ["weli"] = 0
        };

        public static bool Contains(string extension)
        {
            return Extensions.ContainsKey(extension.ToLower());
        }

        public static int HeaderFor(string extension)
        {
            return Extensions[extension.ToLower()];
        }

        public static void Load(string path)
        {
            foreach (string line in File.ReadAllLines(path))
            {
                if (line.Trim().StartsWith("#") || line.Trim().Length == 0) { continue; }

                string[] parts = line.Split('=');

                if (parts.Length != 2) { continue; }

                parts[0] = parts[0].Trim().ToLower();
                parts[1] = parts[1].Trim().ToLower();

                Extensions[parts[0].Trim().ToLower()] = Convert.ToInt32(parts[1], parts[1].StartsWith("0x") ? 16 : 10);
            }
        }
    }
}
