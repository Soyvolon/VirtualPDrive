
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;

using VirtualPDrive.Core.Client;

namespace VirtualPDrive.WCF
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class VirtualPDriveService : IVirtualPDriveService
    {
        IVirtualPDriveCallback _callback;
        
        private VirtualClientBase _client;
        private readonly Type _clientType;
        
        public VirtualPDriveService(VirtualClientArg arg)
        {
            _clientType = arg.Type;
        }

        public async Task InitalizeAsync(string armaPath, string[] mods, bool ignoreMods, string output, string local)
        {
            var settings = new VirtualClientSettings()
            {
                ArmAPath = armaPath,
                ModsFilter = mods,
                NoMods = ignoreMods,
                OutputPath = output,
                Local = local
            };

            _client = (VirtualClientBase)Activator.CreateInstance(_clientType, settings);

            _client.OnStart += Client_OnStart;
            _client.OnShutdown += Client_OnShutdown;

            await _client.StartAsync();
        }

        private async Task Client_OnStart(object sender, VirtualClientEventArgs args)
        {
            await Task.Run(() =>
            {
                _callback.ClientStarted(_client.Settings.OutputPath);
            });
        }

        private async Task Client_OnShutdown(object sender)
        {
            await Task.Run(() =>
            {
                _callback.ClientStopped();
            });
        }

        public void Stop()
        {
            _client.Cancellation.Cancel(true);
        }
    }
}
