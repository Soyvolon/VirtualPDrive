using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMemoryProvider.FileSystem;
public class MemoryFile : MemoryItem
{
    public string Extension { get; set; }

    public MemoryFile(string name, string extension)
    {
        Name = name;
        Extension = extension;
    }
}
