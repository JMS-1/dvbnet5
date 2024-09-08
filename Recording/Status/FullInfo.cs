using System.Xml.Serialization;
using JMS.DVB.NET.Recording.Persistence;

namespace JMS.DVB.NET.Recording.Status
{
    /// <summary>
    /// Beschreibt einen Gesamtauftrag.
    /// </summary>
    [Serializable]
    public class FullInfo
    {
        /// <summary>
        /// Alle Quellen zu dieser Aufzeichnung.
        /// </summary>
        [XmlElement("Stream")]
        public readonly List<StreamInfo> Streams = [];

        /// <summary>
        /// Alle abgeschlossenen Aufzeichnungsdateien.
        /// </summary>
        [XmlElement("Finished")]
        public readonly List<VCRRecordingInfo> Finished = [];

        /// <summary>
        /// Die Daten der primären Aufzeichnung.
        /// </summary>
        public VCRRecordingInfo Recording { get; set; } = null!;

        /// <summary>
        /// Meldet oder legt fest, on ein Netzwerkversand unterstützt wird.
        /// </summary>
        public bool CanStream { get; set; }

        /// <summary>
        /// Gesetzt, wenn es sich um einen Auftrag handelt, bei dem Aufzeichnungen dynamisch
        /// ergänzt und entfernt werden können.
        /// </summary>
        public bool IsDynamic { get; set; }

        /// <summary>
        /// Der Zeitpunkt, zu dem der Gesamtauftrag enden soll.
        /// </summary>
        public DateTime EndsAt { get; set; }
    }
}
