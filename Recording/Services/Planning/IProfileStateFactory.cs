namespace JMS.DVB.NET.Recording.Services.Planning;

public interface IProfileStateFactory
{
    IProfileState Create(string profileName);
}

