using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualPDrive.Client;
public class VirtualClientErrorEventArgs
{
    public Exception? Exception { get; set; }
    public string Message { get; set; } = "";
}
