using JMS.DVB.NET.Recording.Actions;
using JMS.DVB.NET.Recording.Planning;
using JMS.DVB.NET.Recording.Requests;
using JMS.DVB.NET.Recording.RestWebApi;
using JMS.DVB.NET.Recording.Server;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Logging;
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
        services.AddTransient(typeof(ILogger<>), typeof(Logger<>));

        services.AddTransient<IChangeExceptions, ChangeExceptions>();
        services.AddTransient<IConfigurationUpdater, ConfigurationUpdater>();
        services.AddTransient<ILogQuery, LogQuery>();
        services.AddTransient<IProfileStateFactory, ProfileStateFactory>();
        services.AddTransient<IProgramGuideEntries, ProgramGuideEntries>();
        services.AddTransient<IProgramGuideManagerFactory, ProgramGuideManagerFactory>();
        services.AddTransient<IProgramGuideProxyFactory, ProgramGuideProxyFactory>();
        services.AddTransient<IRecordingInfoFactory, RecordingInfoFactory>();
        services.AddTransient<IRecordingPlannerFactory, RecordingPlannerFactory>();
        services.AddTransient<IRecordingProxyFactory, RecordingProxyFactory>();
        services.AddTransient<IRuleUpdater, RuleUpdater>();
        services.AddTransient<ISourceScanProxyFactory, SourceScanProxyFactory>();
        services.AddTransient<IUserProfileStore, UserProfileStore>();
        services.AddTransient<IZappingProxyFactory, ZappingProxyFactory>();

        services.AddSingleton<IExtensionManager, ExtensionManager>();
        services.AddSingleton<IJobManager, JobManager>();
        services.AddSingleton<IRecordings, Recordings>();
        services.AddSingleton<IRegistry, Registry>();
        services.AddSingleton<IVCRConfiguration, VCRConfiguration>();
        services.AddSingleton<IVCRProfiles, VCRProfiles>();
        services.AddSingleton<IVCRServer, VCRServer>();

        services.AddSingleton<IVCRConfigurationExePathProvider>((ctx) => new ConfigurationPathProvider());
    }

    public static void StartRecording(this IServiceProvider services, CancellationTokenSource restart)
    {
        Environment.CurrentDirectory = Tools.ApplicationDirectory.FullName;

        services.GetRequiredService<IVCRServer>().Startup(() => restart.Cancel());
    }
}
