using JMS.DVB.CardServer;
using JMS.DVB.NET.Recording;

namespace JMS.VCR.NET;

public class Program
{
    public static void Main(string[] args)
    {
        for (; ; Thread.Sleep(5000))
        {
            var host = CreateHostBuilder(args).Build();

            var restart = new CancellationTokenSource();

            host.Services.StartRecording(restart);

            host.RunAsync(restart.Token).Wait();

            if (!restart.IsCancellationRequested) break;
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(c => c.AddEnvironmentVariables("VCRNET_"))
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
