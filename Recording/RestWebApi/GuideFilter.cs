using System.Globalization;
using System.Text.Json.Serialization;
using JMS.DVB.NET.Recording.ProgramGuide;
using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt einen Filter auf Einträge der Programmzeitschrift.
    /// </summary>
    public class GuideFilter
    {
        /// <summary>
        /// Der Name des zu verwendenden Geräteprofils.
        /// </summary>
        public string ProfileName { get; set; } = null!;

        /// <summary>
        /// Optional die Quelle.
        /// </summary>
        public string Source { get; set; } = null!;

        /// <summary>
        /// Optional der Startzeitpunkt.
        /// </summary>
        public string? StartISO
        {
            get { return Start.HasValue ? Start.Value.ToString("o") : null!; }
            set { Start = string.IsNullOrEmpty(value) ? default(DateTime?) : DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind); }
        }

        /// <summary>
        /// Optional der Startzeitpunkt.
        /// </summary>
        [JsonIgnore]
        public DateTime? Start { get; set; }

        /// <summary>
        /// Das Suchmuster für den Titel, das erste Zeichen bestimmt den Suchmodus.
        /// </summary>
        public string TitlePattern { get; set; } = null!;

        /// <summary>
        /// Das Suchmuster für den Inhalt, das erste Zeichen bestimmt den Suchmodus.
        /// </summary>
        public string ContentPattern { get; set; } = null!;

        /// <summary>
        /// Die gewünschte Seitengröße.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Die aktuell gewünschte Seite.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Die Art der Quelle.
        /// </summary>
        public GuideSourceFilter SourceType { get; set; }

        /// <summary>
        /// Die Art der Verschlüsselung.
        /// </summary>
        public GuideEncryptionFilter SourceEncryption { get; set; }

        /// <summary>
        /// Erstellt die interne Repräsentation eines Filters.
        /// </summary>
        /// <param name="filter">Die externe Darstellung des Filters.</param>
        /// <returns>Die gewünschte Repräsentation.</returns>
        public static GuideEntryFilter? Translate(GuideFilter filter, IVCRProfiles profiles)
        {
            // None
            if (filter == null)
                return null;

            // Lookup source by unique name
            var source = (filter.Source == null) ? null : profiles.FindSource(filter.ProfileName, filter.Source);

            // Process
            return
                new GuideEntryFilter
                {
                    Source = source?.Source!,
                    SourceEncryption = filter.SourceEncryption,
                    ContentPattern = filter.ContentPattern,
                    TitlePattern = filter.TitlePattern,
                    ProfileName = filter.ProfileName,
                    SourceType = filter.SourceType,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize,
                    Start = filter.Start,
                };
        }
    }
}
