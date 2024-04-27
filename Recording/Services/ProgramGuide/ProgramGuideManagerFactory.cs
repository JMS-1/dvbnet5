using JMS.DVB.NET.Recording.Server;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Services.ProgramGuide;

public class ProgramGuideManagerFactory(
    IVCRProfiles profiles,
    IRegistry registry,
    IJobManager jobs,
    ILogger logger
) : IProgramGuideManagerFactory
{
    public IProgramGuideManager Create(IVCRServer server, string profileName)
        => new ProgramGuideManager(server, profileName, registry, profiles, jobs, logger);
}

