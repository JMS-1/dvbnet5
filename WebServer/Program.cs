using System.Diagnostics;
using JMS.DVB.NET.Recording;

namespace JMS.VCR.NET;

public class Program
{
    public static void Main(string[] args)
    {
        for (; ; Thread.Sleep(5000))
            using (var host = CreateHostBuilder(args).Build())
            {
                var restart = new CancellationTokenSource();

                host.Services.StartRecording(restart);

                host.RunAsync(restart.Token).Wait();

                if (!restart.IsCancellationRequested) break;
            }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host
            .CreateDefaultBuilder(args)
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureAppConfiguration(c => c.AddEnvironmentVariables("VCRNET_"))
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
}
