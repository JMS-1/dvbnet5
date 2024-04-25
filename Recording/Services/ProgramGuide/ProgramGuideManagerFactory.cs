using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Services.ProgramGuide;

public class ProgramGuideManagerFactory(
    IVCRProfiles profiles,
    IJobManager jobs,
    ILogger logger
) : IProgramGuideManagerFactory
{
    public IProgramGuideManager Create(IProfileStateCollection states, string profileName)
        => new ProgramGuideManager(states, profileName, profiles, jobs, logger);
}

