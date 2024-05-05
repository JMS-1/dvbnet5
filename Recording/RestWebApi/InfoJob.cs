using System.Runtime.Serialization;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt einen Auftrag.
    /// </summary>
    public class InfoJob
    {
        /// <summary>
        /// Der Name des Auftrags.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Die eindeutige Kennung des Auftrags.
        /// </summary>
        public string WebId { get; set; } = null!;

        /// <summary>
        /// Die einzelnen Aufzeichnungen des Auftrags.
        /// </summary>
        public InfoSchedule[] Schedules { get; set; } = null!;

        /// <summary>
        /// Gesetzt, wenn der Auftrag aktiv ist.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Der Name des Geräteprofils.
        /// </summary>
        public string ProfileName { get; set; } = null!;

        /// <summary>
        /// Der Name des Quelle.
        /// </summary>
        public string SourceName { get; set; } = null!;

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="job">Ein Auftrag.</param>
        /// <param name="active">Gesetzt, wenn es sich um einen aktiven Auftrag handelt.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static InfoJob Create(VCRJob job, bool active, IVCRProfiles profiles)
        {
            // Report
            return
                new InfoJob
                {
                    Schedules = job.Schedules.Select(schedule => InfoSchedule.Create(schedule, job, profiles)).OrderBy(schedule => schedule.Name ?? string.Empty, StringComparer.InvariantCultureIgnoreCase).ToArray(),
                    WebId = ServerTools.GetUniqueWebId(job, null!),
                    ProfileName = job.Source.ProfileName,
                    SourceName = job.Source.DisplayName,
                    IsActive = active,
                    Name = job.Name,
                };
        }
    }
}
