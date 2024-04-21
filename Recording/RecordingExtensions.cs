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
        services.AddSingleton<IVCRConfigurationExePathProvider>((ctx) => new ConfigurationPathProvider());
        services.AddSingleton<VCRConfiguration>();
        services.AddSingleton<VCRProfiles>();
        services.AddSingleton<VCRServer>();
    }

    public static void StartRecording(this IServiceProvider services, CancellationToken restart)
    {
        services.GetRequiredService<VCRServer>().RestartToken = restart;
    }
}
