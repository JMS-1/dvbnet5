using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Services.ProgramGuide;

public class ProgramGuideManagerFactory(
    Lazy<IProfileStateCollection> states,
    IVCRProfiles profiles,
    IJobManager jobs,
    ILogger logger
) : IProgramGuideManagerFactory
{
    public IProgramGuideManager Create(string profileName)
        => new ProgramGuideManager(states, profileName, profiles, jobs, logger);
}

