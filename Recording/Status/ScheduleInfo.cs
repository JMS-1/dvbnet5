using System.Xml.Serialization;
using JMS.DVB.NET.Recording.Persistence;

namespace JMS.DVB.NET.Recording.Status
{
    /// <summary>
    /// Enth�lt die Eckdaten einer einzelnen Aufzeichnung.
    /// </summary>
    [Serializable]
    [XmlType("VCRScheduleInfo")]
    public class ScheduleInfo
    {
        /// <summary>
        /// Eine leere Liste von Dateinamen.
        /// </summary>
        private static readonly string[] _NoFiles = [];

        /// <summary>
        /// Die eindeutige Kennung der zugehörigen Aufzeichnungsdefinition oder <i>null</i>.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public string ScheduleUniqueID { get; set; } = null!;

        /// <summary>
        /// Die eindeutige Kennung des zugehörigen Auftrags oder <i>null</i>.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public string JobUniqueID { get; set; } = null!;

        /// <summary>
        /// Alle zugehörigen Aufzeichnungsdateien.
        /// </summary>
        public string[] Files { get; set; } = null!;

        /// <summary>
        /// Meldet oder setzt den Startzeitpunkt dieser Aufzeichnung.
        /// </summary>
        public DateTime StartsAt { get; set; }

        /// <summary>
        /// Meldet oder setzt den geplanten Endzeitpunkt der Aufzeichnung.
        /// </summary>
        public DateTime EndsAt { get; set; }

        /// <summary>
        /// Der Name der Aufzeichnung.
        /// </summary>
        [XmlIgnore]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Die bisher aufgezeichnete Datenmenge in kBytes.
        /// </summary>
        [XmlIgnore]
        public uint TotalSize { get; set; }

        /// <summary>
        /// Die aktuelle Quelle.
        /// </summary>
        [XmlIgnore]
        public SourceSelection Source { get; set; } = null!;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="recording">Die Aufzeichnung, deren Daten �bernommen werden sollen.</param>
        /// <param name="fileMap">Alle Dateien zur Gesamtaktivit�t.</param>
        /// <returns>Die neue Beschreibung.</returns>
        public static ScheduleInfo Create(VCRRecordingInfo recording, Dictionary<Guid, string[]> fileMap)
        {
            // Find files
            string[]? files = null;
            if (recording.ScheduleUniqueID.HasValue)
                fileMap?.TryGetValue(recording.ScheduleUniqueID.Value, out files);

            // Create new
            return
                new ScheduleInfo
                {
                    ScheduleUniqueID = recording.ScheduleUniqueID.HasValue ? recording.ScheduleUniqueID.Value.ToString("N").ToUpper() : null!,
                    JobUniqueID = recording.JobUniqueID.HasValue ? recording.JobUniqueID.Value.ToString("N").ToUpper() : null!,
                    StartsAt = recording.StartsAt.GetValueOrDefault(DateTime.UtcNow),
                    TotalSize = recording.TotalSize,
                    Source = recording.Source,
                    EndsAt = recording.EndsAt,
                    Files = files ?? _NoFiles,
                    Name = recording.Name,
                };
        }
    }
}
