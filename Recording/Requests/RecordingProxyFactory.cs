using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Requests;

public class RecordingProxyFactory(
    IVCRConfiguration configuration,
    IVCRProfiles profiles,
    IJobManager jobManager,
    IExtensionManager extensionManager,
    ILogger logger
) : IRecordingProxyFactory
{
    /// <inheritdoc/>
    public RecordingProxy Create(IProfileState state, VCRRecordingInfo firstRecording)
        => new RecordingProxy(state, firstRecording, logger, jobManager, configuration, profiles, extensionManager);
}

