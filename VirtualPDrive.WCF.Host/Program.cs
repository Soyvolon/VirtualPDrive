// See https://aka.ms/new-console-template for more information

using Castle.Facilities.WcfIntegration;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.ComponentModel.Design;
using System.ServiceModel;

using VirtualPDrive.Client;

namespace VirtualPDrive.WCF.Host;

public class Program
{
    public static void Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(x => new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
                .Build());

        var service = serviceCollection.BuildServiceProvider();
        var configuration = service.GetRequiredService<IConfiguration>();

        var container = new WindsorContainer();
        container.AddFacility<WcfFacility>()
            .Register(Component.For<IVirtualPDriveService>()
                .DependsOn(
                    Dependency.OnValue<VirtualClientArg>(
                            new VirtualClientArg(typeof(VirtualClient))))
                .ImplementedBy<VirtualPDriveService>()
                .AsWcfService(new DefaultServiceModel()
                    .AddEndpoints(WcfEndpoint.BoundTo(new WSHttpBinding())
                        .At(configuration["host"]))));

        Task.Delay(-1).GetAwaiter().GetResult();
    }
}
