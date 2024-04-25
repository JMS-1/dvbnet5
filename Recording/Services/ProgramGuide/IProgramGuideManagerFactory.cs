using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Services.ProgramGuide;

public interface IProgramGuideManagerFactory
{
    IProgramGuideManager Create(IVCRServer server, string profileName);
}

