using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Requests;
using JMS.DVB.NET.Recording.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JMS.DVB.NET.Recording;

public static class RecordingExtensions
{
    private class ConfigurationPathProvider : IVCRConfigurationExePathProvider
    {
        private readonly string m_Path = Path.Combine(RunTimeLoader.GetDirectory("Recording").FullName, "Service");

        string IVCRConfigurationExePathProvider.Path => m_Path;
    }

    public static void UseRecording(this IServiceCollection services)
    {
        services.AddTransient<ILogger, Logger>();
        services.AddTransient<ServiceFactory>();

        services.AddSingleton<IVCRConfigurationExePathProvider>((ctx) => new ConfigurationPathProvider());

        services.AddSingleton<JobManager>();
        services.AddSingleton<IVCRConfiguration, VCRConfiguration>();
        services.AddSingleton<VCRProfiles>();
        services.AddSingleton<VCRServer>();

        /* Vorläufig Lösung für das 'static-Problem', da muss noch deutlich mehr passieren. */
        services.AddSingleton<VCRScheduleExtensions.Initializer>();
        services.AddSingleton<VCRJobExtensions.Initializer>();
    }

    public static void StartRecording(this IServiceProvider services, CancellationTokenSource restart)
    {
        /* Vorläufig Lösung für das 'static-Problem', da muss noch deutlich mehr passieren. */
        services.GetRequiredService<VCRScheduleExtensions.Initializer>();
        services.GetRequiredService<VCRJobExtensions.Initializer>();

        services.GetRequiredService<VCRServer>().Restart = () => restart.Cancel();
    }
}
