using JMS.DVB.NET.Recording.Actions;
using JMS.DVB.NET.Recording.Services.Planning;
using Microsoft.AspNetCore.Mvc;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Erlaubt den Zugriff auf die Programmzeitschrift.
    /// </summary>
    [ApiController]
    [Route("api/guide")]
    public class GuideController(IVCRServer server, IProgramGuideEntries entries) : ControllerBase
    {
        /// <summary>
        /// Ermittelt einen einzelnen Eintrag der Programmzeitschrift.
        /// </summary>
        /// <param name="profile">Der Name des zu verwendende Geräteprofils.</param>
        /// <param name="source">Die zugehörige Quelle.</param>
        /// <param name="pattern">Informationen zum Abruf des Eintrags.</param>
        /// <returns>Der gewünschte Eintrag.</returns>
        [HttpGet]
        public GuideItem Find(string profile, string source, string pattern)
        {
            // Check mode
            var split = pattern.IndexOf('-');
            if (split < 0)
                return null!;

            // Split pattern
            var start = new DateTime(long.Parse(pattern[..split]) * Tools.UnixTimeFactor + Tools.UnixTimeBias, DateTimeKind.Utc);
            var end = new DateTime(long.Parse(pattern[(split + 1)..]) * Tools.UnixTimeFactor + Tools.UnixTimeBias, DateTimeKind.Utc);

            // Forward
            return server.FindProgramGuideEntry(profile, SourceIdentifier.Parse(source), start, end, GuideItem.Create)!;
        }

        /// <summary>
        /// Meldet alle Einträge der Programmzeitschrift zu einem Geräteprofil.
        /// </summary>
        /// <param name="filter">Die Beschreibung des Filters.</param>
        /// <returns>Die Liste aller passenden Einträge.</returns>
        [HttpPost("query")]
        public GuideItem[] Find([FromBody] GuideFilter filter) => entries.Get(filter, GuideFilter.Translate, GuideItem.Create);

        /// <summary>
        /// Meldet alle Einträge der Programmzeitschrift zu einem Geräteprofil.
        /// </summary>
        /// <param name="filter">Die Beschreibung des Filters.</param>
        /// <returns>Die Anzahl aller passenden Einträge.</returns>
        [HttpPost("count")]
        public int Count([FromBody] GuideFilter filter) => entries.Get(filter, GuideFilter.Translate);

        /// <summary>
        /// Ermittelt Informationen zu den Einträgen in einem Geräteprofil.
        /// </summary>
        /// <param name="profile">Der Name des Profils.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        [HttpGet("info/{profile}")]
        public GuideInfo GetInfo(string profile) => entries.GetInformation(profile, GuideInfo.Create);
    }
}
