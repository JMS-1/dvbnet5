using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Planning;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Logging;

namespace JMS.DVB.NET.Recording.Requests;

public class ProgramGuideProxyFactory(
    IVCRConfiguration configuration,
    IVCRProfiles profiles,
    IJobManager jobManager,
    IExtensionManager extensionManager,
    ILogger<ProgramGuideProxy> logger
) : IProgramGuideProxyFactory
{
    /// <inheritdoc/>
    public ProgramGuideProxy Create(IProfileState state, VCRRecordingInfo recording)
    {
        // Validate
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(recording);

        // Forward
        return new ProgramGuideProxy(state, recording, logger, jobManager, configuration, profiles, extensionManager);
    }
}
