using System.Globalization;
using System.Runtime.Serialization;
using JMS.DVB.NET.Recording.Services;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt die Programmzeitschrift eines Geräteprofils.
    /// </summary>
    [DataContract]
    [Serializable]
    public class GuideInfo
    {
        /// <summary>
        /// Alle Quellen, für die Eintragungen existieren.
        /// </summary>
        [DataMember(Name = "stations")]
        public string[] SourceNames { get; set; } = null!;

        /// <summary>
        /// Der Zeitpunkt, an dem der früheste Eintrag startet.
        /// </summary>
        [DataMember(Name = "first")]
        public string FirstStartISO
        {
            get { return FirstStart.HasValue ? FirstStart.Value.ToString("o") : null!; }
            set { FirstStart = string.IsNullOrEmpty(value) ? default(DateTime?) : DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind); }
        }

        /// <summary>
        /// Der Zeitpunkt, an dem der früheste Eintrag startet.
        /// </summary>
        public DateTime? FirstStart { get; set; }

        /// <summary>
        /// Der Zeitpunkt, an dem der späteste Eintrag startet.
        /// </summary>
        [DataMember(Name = "last")]
        public string LastStartISO
        {
            get { return LastStart.HasValue ? LastStart.Value.ToString("o") : null!; }
            set { LastStart = string.IsNullOrEmpty(value) ? default(DateTime?) : DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind); }
        }

        /// <summary>
        /// Der Zeitpunkt, an dem der späteste Eintrag startet.
        /// </summary>
        public DateTime? LastStart { get; set; }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="guide">Die zugehörige Programmzeitschrift.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static GuideInfo Create(IProgramGuideManager guide, IVCRProfiles profiles)
        {
            // Collectors
            var sources = new HashSet<SourceIdentifier>();
            var stations = new HashSet<string>();
            var first = default(DateTime?);
            var last = default(DateTime?);

            // Process
            foreach (var entry in guide.LeafEntries!.Events)
            {
                // Start time
                var start = entry.StartTime;
                if (!first.HasValue)
                    first = start;
                else if (start < first.Value)
                    first = start;
                if (!last.HasValue)
                    last = start;
                else if (start > last.Value)
                    last = start;

                // Add the source name
                var source = entry.Source;
                var sourceInfo = profiles.FindSource(guide.ProfileName, source);
                if (sourceInfo != null)
                    if (sources.Add(source))
                        stations.Add(profiles.GetUniqueName(sourceInfo));
            }

            // Report
            return
                new GuideInfo
                {
                    SourceNames = stations.OrderBy(name => name).ToArray(),
                    FirstStart = first,
                    LastStart = last,
                };
        }
    }
}
