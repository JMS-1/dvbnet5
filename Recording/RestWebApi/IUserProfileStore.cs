namespace JMS.DVB.NET.Recording.RestWebApi;

public interface IUserProfileStore
{
    UserProfile Load();

    void Save(UserProfile profile);
}

