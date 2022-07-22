using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMemoryProvider.Util;
public static class PathUtility
{
    public static string CombineWithPrefix(this string file, string prefix)
    {
        int pathOffset = 0;
        int fileOffset = 0;
        var fileTree = file.Split(Path.DirectorySeparatorChar);
        var prefixTree = prefix.Split(Path.DirectorySeparatorChar);

        bool invalid = false;
        for (pathOffset = 0; pathOffset < prefixTree.Length; pathOffset++)
        {
            var subsection = prefixTree[pathOffset..];
            invalid = false;
            for (fileOffset = 0; fileOffset < fileTree.Length && fileOffset < subsection.Length; fileOffset++)
            {
                if (subsection[fileOffset] != fileTree[fileOffset])
                {
                    invalid = true;
                    break;
                }
            }

            if (!invalid)
                break;
        }

        if (invalid)
            return Path.Combine(prefix, file);

        var pathParts = prefixTree[..(pathOffset)].ToList();
        pathParts.AddRange(fileTree[(fileOffset - 1)..]);

        var output = Path.Combine(pathParts.ToArray());
        return output;
    }
}
