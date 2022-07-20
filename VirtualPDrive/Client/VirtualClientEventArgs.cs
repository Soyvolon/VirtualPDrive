using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualPDrive.Client
{
    public class VirtualClientEventArgs : EventArgs
    {
        public CancellationTokenSource ClientCancellationToken { get; private set; }

        public VirtualClientEventArgs(CancellationTokenSource clientCancellationToken)
        {
            ClientCancellationToken = clientCancellationToken;
        }
    }
}