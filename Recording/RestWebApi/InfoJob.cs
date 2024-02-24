using System.Runtime.Serialization;
using JMS.DVB.NET.Recording.Persistence;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt einen Auftrag.
    /// </summary>
    [Serializable]
    [DataContract]
    public class InfoJob
    {
        /// <summary>
        /// Der Name des Auftrags.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Die eindeutige Kennung des Auftrags.
        /// </summary>
        [DataMember(Name = "id")]
        public string WebId { get; set; } = null!;

        /// <summary>
        /// Die einzelnen Aufzeichnungen des Auftrags.
        /// </summary>
        [DataMember(Name = "schedules")]
        public InfoSchedule[] Schedules { get; set; } = null!;

        /// <summary>
        /// Gesetzt, wenn der Auftrag aktiv ist.
        /// </summary>
        [DataMember(Name = "active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Der Name des Geräteprofils.
        /// </summary>
        [DataMember(Name = "device")]
        public string ProfileName { get; set; } = null!;

        /// <summary>
        /// Der Name des Quelle.
        /// </summary>
        [DataMember(Name = "source")]
        public string SourceName { get; set; } = null!;

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="job">Ein Auftrag.</param>
        /// <param name="active">Gesetzt, wenn es sich um einen aktiven Auftrag handelt.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static InfoJob Create(VCRJob job, bool active)
        {
            // Report
            return
                new InfoJob
                {
                    Schedules = job.Schedules.Select(schedule => InfoSchedule.Create(schedule, job)).OrderBy(schedule => schedule.Name ?? string.Empty, StringComparer.InvariantCultureIgnoreCase).ToArray(),
                    WebId = ServerTools.GetUniqueWebId(job, null!),
                    ProfileName = job.Source.ProfileName,
                    SourceName = job.Source.DisplayName,
                    IsActive = active,
                    Name = job.Name,
                };
        }
    }
}
