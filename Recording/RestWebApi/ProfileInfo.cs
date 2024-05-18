using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt ein einzelnen Geräteprofil.
    /// </summary>
    public class ProfileInfo
    {
        /// <summary>
        /// Der Name des Geräteprofils.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="profile">Das zu beschreibende Geräteprofil.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static ProfileInfo Create(IProfileState profile)
        {
            // Validate
            ArgumentNullException.ThrowIfNull(profile);

            // Create
            return new ProfileInfo { Name = profile.ProfileName };
        }
    }
}
