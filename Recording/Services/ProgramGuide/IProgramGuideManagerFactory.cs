namespace JMS.DVB.NET.Recording.Services.ProgramGuide;

public interface IProgramGuideManagerFactory
{
    IProgramGuideManager Create(string profileName);
}

