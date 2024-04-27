using JMS.DVB.NET.Recording.Server;

namespace JMS.DVB.NET.Recording.Services.ProgramGuide;

public interface IProgramGuideManagerFactory
{
    IProgramGuideManager Create(IVCRServer server, string profileName);
}

