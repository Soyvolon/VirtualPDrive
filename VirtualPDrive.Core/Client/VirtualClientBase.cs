using Serilog;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualPDrive.Core.Client
{
    public abstract class VirtualClientBase : IDisposable
    {
        #region Events
        public delegate Task OnStartEventHandler(object sender, VirtualClientEventArgs args);
        public virtual event OnStartEventHandler OnStart;

        public delegate Task OnShutdownEventHandler(object sender);
        public virtual event OnShutdownEventHandler OnShutdown;
        #endregion

        public VirtualClientSettings Settings { get; protected set; }
        public CancellationTokenSource Cancellation { get; protected set; }

        public VirtualClientBase(VirtualClientSettings settings)
        {
            Settings = settings;
        }

        public abstract Task StartAsync();

        public void Dispose()
        {
            Cancellation?.Cancel();
            Settings = null;
            OnStart = null;
            OnShutdown = null;
        }
    }
}