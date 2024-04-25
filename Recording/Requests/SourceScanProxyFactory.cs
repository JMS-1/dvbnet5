using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Requests;

public class SourceScanProxyFactory(
    IVCRConfiguration configuration,
    IVCRProfiles profiles,
    IJobManager jobManager,
    IExtensionManager extensionManager,
    ILogger logger
) : ISourceScanProxyFactory
{
    /// <inheritdoc/>
    public SourceScanProxy Create(IProfileState state, VCRRecordingInfo recording)
    {
        // Validate
        if (state == null)
            throw new ArgumentNullException(nameof(state));
        if (recording == null)
            throw new ArgumentNullException(nameof(recording));

        // Forward
        return new SourceScanProxy(state, recording, logger, jobManager, configuration, profiles, extensionManager);
    }
}

