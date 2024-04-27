using JMS.DVB.NET.Recording.Server;

namespace JMS.DVB.NET.Recording.Services.Planning;

public interface IProfileStateFactory
{
    IProfileState Create(IVCRServer collection, string profileName);
}

