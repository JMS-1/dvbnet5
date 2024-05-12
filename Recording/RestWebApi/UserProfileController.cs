using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Pflegt die Benutzerkonfiguration.
    /// </summary>
    [ApiController]
    [Route("api/user")]
    public class UserProfileController(IUserProfileStore store) : ControllerBase
    {
        /// <summary>
        /// Meldet die aktuelle Benutzerkonfiguration.
        /// </summary>
        /// <returns>Die Einstellungen des Anwenders.</returns>
        [HttpGet]
        public UserProfile GetCurrentProfile()
        {
            var profile = store.Load();

            profile.RecentSources.Sort(StringComparer.InvariantCultureIgnoreCase);

            return profile;
        }

        /// <summary>
        /// Aktualisiert die Daten des Geräteprofils.
        /// </summary>
        /// <param name="newProfile">Das neue Geräteprofil.</param>
        /// <returns>Die aktuellen Daten.</returns>
        [HttpPut]
        public void UpdateProfile([FromBody] UserProfile newProfile) => store.Save(newProfile);

        /// <summary>
        /// Aktualisiert die Suchen der Programmzeitschrift.
        /// </summary>
        [HttpPut("favorites")]
        public async Task UpdateGuideFavorites()
        {
            var profile = store.Load();

            // Just store body as data
            using var reader = new StreamReader(Request.Body);

            profile.GuideSearches = await reader.ReadToEndAsync();

            // And update
            store.Save(profile);
        }
    }
}
