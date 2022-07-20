using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;

namespace VirtualPDrive.WCF
{
    [ServiceContract(Namespace = "http://Soyvolon.VirtaulPDrive.ServiceHost", 
        SessionMode = SessionMode.Required,
        CallbackContract = typeof(IVirtualPDriveCallback))]
    public interface IVirtualPDriveService
    {
        [OperationContract(IsOneWay = true)]
        Task InitalizeAsync(string armaPath, string[] mods, bool ignoreMods, string output, string local);
        [OperationContract(IsOneWay = true)]
        void Stop();
    }

    public interface IVirtualPDriveCallback
    {
        [OperationContract(IsOneWay = true)]
        void ClientStarted(string root);
        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void ClientStopped();
    }
}
