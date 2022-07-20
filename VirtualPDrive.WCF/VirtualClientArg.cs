using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualPDrive.WCF
{
    public class VirtualClientArg
    {
        public Type Type { get; set; }

        public VirtualClientArg(Type type)
        {
            Type = type;
        }
    }
}
