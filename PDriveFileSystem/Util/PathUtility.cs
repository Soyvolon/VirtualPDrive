using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDriveFileSystem.Util;
public static class PathUtility
{
    public static string CombineWithPrefix(this string file, string prefix, string ignoreFront, bool forceLowercase)
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
                if (!subsection[fileOffset].Equals(fileTree[fileOffset],
                    forceLowercase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                {
                    invalid = true;
                    break;
                }
            }

            if (!invalid)
                break;
        }

        if (invalid)
        {
            var adjustedPrefix = string.Join(Path.DirectorySeparatorChar,
                prefix.Split(Path.DirectorySeparatorChar)
                .SkipWhile(x => x.Equals(ignoreFront,
                    forceLowercase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)));

            return Path.Combine(adjustedPrefix, file);
        }

#if DEBUG
        if (prefix.StartsWith("a3"))
        { }
#endif

        var pathParts = prefixTree[1..pathOffset].ToList();
        pathParts.AddRange(fileTree);

        var output = Path.Combine(pathParts.ToArray());
        return output;
    }
}
