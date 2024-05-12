using System.Text.Json;
using JMS.DVB.NET.Recording.Services.Configuration;

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
        UserProfile? userProfile;

        try
        {
            userProfile = File.Exists(_profilePath) ? JsonSerializer.Deserialize<UserProfile>(File.ReadAllText(_profilePath)) : null;
        }
        catch (Exception)
        {
            userProfile = null;
        }

        return userProfile ?? new UserProfile();
    }

    /// <summary>
    /// Aktualisiert die Einstellungen des Anwenders.
    /// </summary>
    public void Save(UserProfile profile) => File.WriteAllText(_profilePath, JsonSerializer.Serialize(profile));

}

