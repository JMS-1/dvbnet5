using JMS.DVB.NET.Recording.Services.Configuration;
using Newtonsoft.Json;

namespace JMS.DVB.NET.Recording.RestWebApi;

public class UserProfileStore(IVCRConfigurationExePathProvider configPath) : IUserProfileStore
{
    private readonly string _profilePath = Path.Join(Path.GetDirectoryName(configPath.Path), "profile.json");

    /// <summary>
    /// Erstellt die Informationen des aktuellen Anwenders.
    /// </summary>
    /// <returns>Die gewünschten Einstellungen.</returns>
    public UserProfile Load()
    {
        return new UserProfile();
    }

    /// <summary>
    /// Aktualisiert die Einstellungen des Anwenders.
    /// </summary>
    public void Save(UserProfile profile)
    {
        File.WriteAllText(_profilePath, JsonConvert.SerializeObject(profile));

        var recover = JsonConvert.DeserializeObject<UserProfile>(File.ReadAllText(_profilePath));

        Console.WriteLine(recover);
    }
}

