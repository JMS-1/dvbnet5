using JMS.DVB.NET.Recording.Actions;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Requests;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Planning;
using JMS.DVB.NET.Recording.Services.ProgramGuide;
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
        services.AddTransient<ServiceFactory>();

        services.AddTransient(typeof(Lazy<>));

        services.AddTransient<IConfigurationUpdater, ConfigurationUpdater>();
        services.AddTransient<ILogger, Logger>();
        services.AddTransient<IProfileStateFactory, ProfileStateFactory>();
        services.AddTransient<IProgramGuideManagerFactory, ProgramGuideManagerFactory>();
        services.AddTransient<IRuleUpdater, RuleUpdater>();

        services.AddSingleton<IVCRConfigurationExePathProvider>((ctx) => new ConfigurationPathProvider());

        services.AddSingleton<IJobManager, JobManager>();
        services.AddSingleton<IProfileStateCollection, ProfileStateCollection>();
        services.AddSingleton<IVCRConfiguration, VCRConfiguration>();
        services.AddSingleton<IVCRProfiles, VCRProfiles>();

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
