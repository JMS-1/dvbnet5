using System.Xml.Serialization;
using JMS.DVB.NET.Recording.Persistence;

namespace JMS.DVB.NET.Recording.Status
{
    /// <summary>
    /// Beschreibt die Aufzeichnung einer Quelle.
    /// </summary>
    [Serializable]
    [XmlType("VCRStreamInfo")]
    public class StreamInfo
    {
        /// <summary>
        /// Alle Teilaufzeichnungen, die zu dieser Quelle gleichzeitig ausgef�hrt werden.
        /// </summary>
        [XmlElement("Schedule")]
        public readonly List<ScheduleInfo> Schedules = new List<ScheduleInfo>();

        /// <summary>
        /// Gesetzt, wenn keine Aufzeichnungdatei angelegt wird.
        /// </summary>
        public bool LiveStream { get; set; }

        /// <summary>
        /// Das optionale Ziel bei einem Netzwerkversand.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public string StreamsTo { get; set; }

        /// <summary>
        /// Die Datei, in der die Daten der Aufzeichnung abgelegt werden.
        /// </summary>
        [XmlElement(IsNullable = true)]
        public string TargetFile { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="primary">Die prim�re Aufzeichnung f�r die Quelle, in der auch der Dateiname
        /// festgelegt ist.</param>
        /// <param name="target">Das aktuelle Ziel eines Netzwerkversands.</param>
        /// <param name="fileMap">Die Liste aller Dateien zur Gesamtaktivit�t.</param>
        /// <returns>Die neue Beschreibung.</returns>
        public static StreamInfo Create(VCRRecordingInfo primary, string target, Dictionary<Guid, string[]> fileMap)
        {
            // Create new
            return
                new StreamInfo
                {
                    Schedules = { ScheduleInfo.Create(primary, fileMap) },
                    LiveStream = string.IsNullOrEmpty(primary.FileName),
                    TargetFile = primary.FileName,
                    StreamsTo = target,
                };
        }
    }
}
