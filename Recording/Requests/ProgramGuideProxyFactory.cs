using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Planning;
using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.Requests;

public class ProgramGuideProxyFactory(
    IVCRConfiguration configuration,
    IVCRProfiles profiles,
    IJobManager jobManager,
    IExtensionManager extensionManager,
    ILogger logger
) : IProgramGuideProxyFactory
{
    /// <inheritdoc/>
    public ProgramGuideProxy Create(IProfileState state, VCRRecordingInfo recording)
    {
        // Validate
        if (state == null)
            throw new ArgumentNullException(nameof(state));
        if (recording == null)
            throw new ArgumentNullException(nameof(recording));

        // Forward
        return new ProgramGuideProxy(state, recording, logger, jobManager, configuration, profiles, extensionManager);
    }
}
