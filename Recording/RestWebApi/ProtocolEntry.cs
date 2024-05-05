using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using JMS.DVB.NET.Recording.Persistence;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt einen einzelnen Protokolleintrag.
    /// </summary>
    public class ProtocolEntry
    {
        /// <summary>
        /// Der Startzeitpunkt der Gerätenutzung.
        /// </summary>
        public string StartTimeISO
        {
            get { return StartTime.HasValue ? StartTime.Value.ToString("o") : null!; }
            set { StartTime = string.IsNullOrEmpty(value) ? default(DateTime?) : DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind); }
        }

        /// <summary>
        /// Der Startzeitpunkt der Gerätenutzung.
        /// </summary>
        [JsonIgnore]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Der Endzeitpunkt der Gerätenutzung.
        /// </summary>
        public string EndTimeISO
        {
            get { return EndTime.ToString("o"); }
            set { EndTime = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind); }
        }

        /// <summary>
        /// Der Endzeitpunkt der Gerätenutzung.
        /// </summary>
        [JsonIgnore]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Der Name der ersten Quelle.
        /// </summary>
        public string Source { get; set; } = null!;

        /// <summary>
        /// Die Liste aller Aufzeichnungsdateien.
        /// </summary>
        public string[] Files { get; set; } = null!;

        /// <summary>
        /// Ein Hinweis auf die Größe der Aufzeichnungen.
        /// </summary>
        public string SizeHint { get; set; } = null!;

        /// <summary>
        /// Der Name der primären Aufzeichnungsdatei.
        /// </summary>
        public string PrimaryFile { get; set; } = null!;

        /// <summary>
        /// Kovertiert einen Protokolleintrag in ein für den Client nützliches Format.
        /// </summary>
        /// <param name="entry">Der originale Eintrag.</param>
        /// <returns>Der zugehörige Protokolleintrag.</returns>
        public static ProtocolEntry Create(VCRRecordingInfo entry)
        {
            // Single recording - typically a task
            var source = entry.Source;
            var sourceName = source.DisplayName;

            // Create
            var protocol =
                new ProtocolEntry
                {
                    PrimaryFile = string.IsNullOrEmpty(entry.FileName) ? null! : Path.GetFileName(entry.FileName),
                    Files = entry.RecordingFiles.Select(file => file.Path).Where(File.Exists).ToArray(),
                    Source = entry.Source.DisplayName,
                    StartTime = entry.PhysicalStart,
                    EndTime = entry.EndsAt,
                };

            // Finish            
            if (VCRJob.ProgramGuideName.Equals(sourceName))
                protocol.SizeHint = $"{entry.TotalSize:N0} Einträge";
            else if (VCRJob.SourceScanName.Equals(sourceName))
                protocol.SizeHint = $"{entry.TotalSize:N0} Quellen";
            else
                protocol.SizeHint = PlanCurrent.GetSizeHint(entry.TotalSize);

            // Report
            return protocol;
        }
    }
}
