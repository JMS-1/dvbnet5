using JMS.DVB.NET.Recording.Requests;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.ProgramGuide;

namespace JMS.DVB.NET.Recording.Services.Planning;

public class ProfileStateFactory(
    Lazy<IProfileStateCollection> collection,
    IProgramGuideManagerFactory guideManagerFactory,
    IVCRProfiles profiles,
    ServiceFactory factory,
    ILogger logger
) : IProfileStateFactory
{
    public IProfileState Create(string profileName)
        => new ProfileState(collection, profileName, guideManagerFactory, profiles, factory, logger);
}

