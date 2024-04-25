using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.ProgramGuide;

namespace JMS.DVB.NET.Recording.Services.Planning;

public class ProfileStateFactory(
    IProgramGuideManagerFactory guideManagerFactory,
    IVCRProfiles profiles,
    IVCRConfiguration configuration,
    ILogger logger,
    IJobManager jobManager,
    IExtensionManager extensionManager
) : IProfileStateFactory
{
    public IProfileState Create(IVCRServer collection, string profileName)
        => new ProfileState(collection, profileName, guideManagerFactory, profiles, configuration, jobManager, extensionManager, logger);
}

