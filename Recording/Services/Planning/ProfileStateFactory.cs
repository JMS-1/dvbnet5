using JMS.DVB.NET.Recording.Requests;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.ProgramGuide;

namespace JMS.DVB.NET.Recording.Services.Planning;

public class ProfileStateFactory(
    IProgramGuideManagerFactory guideManagerFactory,
    IVCRProfiles profiles,
    ILogger logger,
    IRecordingProxyFactory recordingFactory,
    IZappingProxyFactory zappingFactory
) : IProfileStateFactory
{
    public IProfileState Create(IVCRServer collection, string profileName)
        => new ProfileState(collection, profileName, guideManagerFactory, profiles, logger, recordingFactory, zappingFactory);
}

