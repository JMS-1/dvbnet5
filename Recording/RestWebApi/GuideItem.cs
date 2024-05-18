using System.Globalization;
using System.Text.Json.Serialization;
using JMS.DVB.NET.Recording.ProgramGuide;
using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt einen einzelnen Eintrag aus der Programmzeitschrift.
    /// </summary>
    public class GuideItem
    {
        /// <summary>
        /// Der Startzeitpunkt der Sendung.
        /// </summary>
        public string StartTimeISO
        {
            get { return StartTime.ToString("o"); }
            set { StartTime = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind); }
        }

        /// <summary>
        /// Die Dauer der Sendung in Sekunden.
        /// </summary>
        public int DurationInSeconds
        {
            get { return (int)Math.Round(Duration.TotalSeconds); }
            set { Duration = TimeSpan.FromSeconds(value); }
        }

        /// <summary>
        /// Der Name der Sendung.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Die Sprache der Sendung.
        /// </summary>
        public string Language { get; set; } = null!;

        /// <summary>
        /// Der Sender, auf dem die Sendung empfangen wird.
        /// </summary>
        public string Station { get; set; } = null!;

        /// <summary>
        /// Der Startzeitpunkt der Sendung.
        /// </summary>
        [JsonIgnore]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Die Dauer der Sendung.
        /// </summary>
        [JsonIgnore]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Die Liste der Freigaben.
        /// </summary>
        public string[] Ratings { get; set; } = null!;

        /// <summary>
        /// Die Liste der Kategorien.
        /// </summary>
        public string[] Categories { get; set; } = null!;

        /// <summary>
        /// Die Langbeschreibung der Sendung.
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Die Kurzbeschreibung der Sendung.
        /// </summary>
        public string Summary { get; set; } = null!;

        /// <summary>
        /// Gesetzt, wenn das Ende in der Zukunft liegt.
        /// </summary>
        public bool IsActive => (StartTime + Duration) > DateTime.UtcNow;

        /// <summary>
        /// Die eindeutige Kennung.
        /// </summary>
        public string Identifier { get; set; } = null!;

        /// <summary>
        /// Erstellt einen neuen Eintrag für die Programmzeitschrift.
        /// </summary>
        /// <param name="entry">Der originale Eintrag aus der Verwaltung.</param>
        /// <param name="profileName">Der Name des zugehörigen Geräteprofils.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static GuideItem Create(ProgramGuideEntry entry, string profileName, IVCRProfiles profiles)
        {
            // Validate
            ArgumentNullException.ThrowIfNull(entry);

            // Default name of the station
            var source = profiles.FindSource(profileName, entry.Source);

            // Create
            return
                new GuideItem
                {
                    Identifier = $"{entry.StartTime.Ticks}:{profileName}:{SourceIdentifier.ToString(entry.Source)!.Replace(" ", "")}",
                    Station = (source == null) ? entry.StationName : profiles.GetUniqueName(source),
                    Duration = TimeSpan.FromSeconds(entry.Duration),
                    Categories = entry.Categories.ToArray(),
                    Ratings = entry.Ratings.ToArray(),
                    Summary = entry.ShortDescription,
                    Description = entry.Description,
                    StartTime = entry.StartTime,
                    Language = entry.Language,
                    Name = entry.Name,
                };
        }
    }
}
