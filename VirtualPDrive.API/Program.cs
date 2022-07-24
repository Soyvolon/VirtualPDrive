using Microsoft.OpenApi.Models;

using Serilog;
using Serilog.Events;

using VirtualPDrive.API.Services.VC;

namespace VirtualPDrive.API;

public class Program
{
    public static int Main(string[] args)
    {
        var cfg = new ConfigurationBuilder()
#if DEBUG
            .AddJsonFile("appsettings.Development.json")
#else
            .AddJsonFile("appsettings.json")
#endif
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(cfg)
            .CreateLogger();

        try
        {
            Log.Information("Starting web host");
            CreateHostBuilder(args).Build().Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            File.WriteAllText("api-error.log", ex.ToString());
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(builder =>
            {
                builder.UseStartup<Startup>();
            })
            .UseWindowsService();
}
