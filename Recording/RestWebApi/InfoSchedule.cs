using System.Globalization;
using System.Text.Json.Serialization;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Meldet Kerninformationen zu einer Aufzeichnung.
    /// </summary>
    public class InfoSchedule
    {
        /// <summary>
        /// Der optionale Name der Aufzeichnung.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Der Startzeitpunkt der Aufzeichnung.
        /// </summary>
        public string StartTimeISO
        {
            get { return StartTime.ToString("o"); }
            set { StartTime = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind); }
        }

        /// <summary>
        /// Der Startzeitpunkt der Aufzeichnung.
        /// </summary>
        [JsonIgnore]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Die Tage, an denen die Aufzeichnung wiederholt werden soll.
        /// </summary>
        public int RepeatPatternJSON
        {
            get { return RepeatPattern.HasValue ? (int)RepeatPattern.Value : 0; }
            set { RepeatPattern = (value == 0) ? null : (VCRDay?)value; }
        }

        /// <summary>
        /// Die Tage, an denen die Aufzeichnung wiederholt werden soll.
        /// </summary>
        [JsonIgnore]
        public VCRDay? RepeatPattern { get; set; }

        /// <summary>
        /// Der optionale Name der Quelle, von der aufgezeichnet werden soll.
        /// </summary>
        public string Source { get; set; } = null!;

        /// <summary>
        /// Der eindeutige Name der Aufzeichnung.
        /// </summary>
        public string WebId { get; set; } = null!;

        /// <summary>
        /// Die Dauer der Aufzeichnung.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Meldet die Daten zu einer Aufzeichnung.
        /// </summary>
        /// <param name="schedule">Die Aufzeichnung.</param>
        /// <param name="job">Der zugehörige Auftrag.</param>
        /// <returns></returns>
        public static InfoSchedule Create(VCRSchedule schedule, VCRJob job, IVCRProfiles profiles)
        {
            // Create
            return
                new InfoSchedule
                {
                    Source = profiles.GetUniqueName(schedule.Source ?? job.Source),
                    WebId = ServerTools.GetUniqueWebId(job, schedule),
                    StartTime = schedule.FirstStart,
                    RepeatPattern = schedule.Days,
                    Duration = schedule.Duration,
                    Name = schedule.Name,
                };
        }
    }
}
