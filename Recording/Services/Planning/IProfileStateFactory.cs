namespace JMS.DVB.NET.Recording.Services.Planning;

public interface IProfileStateFactory
{
    IProfileState Create(IProfileStateCollection collection, string profileName);
}

