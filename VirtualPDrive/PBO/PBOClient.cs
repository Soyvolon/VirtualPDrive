using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualPDrive.PBO;
public static class PBOClient
{
    public static async Task<PBOReader> LoadPBOAsync(string path, string[] extensions)
    {
        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        var reader = new PBOReader(fs, extensions);

        await reader.InitalizeAsync();

        return reader;
    }
}
