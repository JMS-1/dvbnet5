using JMS.DVB.NET.Recording.Actions;
using JMS.DVB.NET.Recording.Requests;
using JMS.DVB.NET.Recording.Server;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Configuration;
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
        services.AddTransient(typeof(Lazy<>));

        services.AddTransient<IChangeExceptions, ChangeExceptions>();
        services.AddTransient<IConfigurationUpdater, ConfigurationUpdater>();
        services.AddTransient<ILogger, Logger>();
        services.AddTransient<ILogQuery, LogQuery>();
        services.AddTransient<IProfileStateFactory, ProfileStateFactory>();
        services.AddTransient<IProgramGuideEntries, ProgramGuideEntries>();
        services.AddTransient<IProgramGuideManagerFactory, ProgramGuideManagerFactory>();
        services.AddTransient<IProgramGuideProxyFactory, ProgramGuideProxyFactory>();
        services.AddTransient<IRecordingProxyFactory, RecordingProxyFactory>();
        services.AddTransient<IRecordings, Recordings>();
        services.AddTransient<IRuleUpdater, RuleUpdater>();
        services.AddTransient<ISourceScanProxyFactory, SourceScanProxyFactory>();
        services.AddTransient<IZappingProxyFactory, ZappingProxyFactory>();

        services.AddSingleton<IVCRConfigurationExePathProvider>((ctx) => new ConfigurationPathProvider());

        services.AddSingleton<IExtensionManager, ExtensionManager>();
        services.AddSingleton<IJobManager, JobManager>();
        services.AddSingleton<IVCRServer, VCRServer>();
        services.AddSingleton<IVCRConfiguration, VCRConfiguration>();
        services.AddSingleton<IVCRProfiles, VCRProfiles>();
    }

    public static void StartRecording(this IServiceProvider services, CancellationTokenSource restart)
    {
        Environment.CurrentDirectory = Tools.ApplicationDirectory.FullName;

        services.GetRequiredService<IVCRServer>().Startup(() => restart.Cancel());
    }
}
