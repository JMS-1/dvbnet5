using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.ProgramGuide;

namespace JMS.DVB.NET.Recording.Services.Planning;

public class ProfileStateFactory(
    IProgramGuideManagerFactory guideManagerFactory,
    IVCRProfiles profiles,
    ILogger logger
) : IProfileStateFactory
{
    public IProfileState Create(IProfileStateCollection collection, string profileName)
        => new ProfileState(collection, profileName, guideManagerFactory, profiles, logger);
}

