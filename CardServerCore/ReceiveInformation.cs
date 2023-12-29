extern alias oldVersion;
using System.Xml.Serialization;

using Legacy = oldVersion.JMS.DVB.EPG;


namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Enthält die Daten zu einer Quelle, die empfangen werden soll.
    /// </summary>
    [Serializable]
    public class ReceiveInformation : ICloneable
    {
        /// <summary>
        /// Liest oder setzt die Auswahl der Quelle.
        /// </summary>
        public string SelectionKey { get; set; } = null!;

        /// <summary>
        /// Optional eine eindeutige Kennung für diese Quelle - damit ist es dann möglich, eine einzelne
        /// Quelle mehrfach zu verwenden.
        /// </summary>
        public Guid UniqueIdentifier { get; set; }

        /// <summary>
        /// Liest oder setzt die Teildatenströme (PID), die im Empfang eingeschlossen werden sollen.
        /// </summary>
        public StreamSelection Streams { get; set; } = null!;

        /// <summary>
        /// Liest oder setzt den optionalen Dateinamen für eine Aufzeichnung.
        /// </summary>
        public string RecordingPath { get; set; } = null!;

        /// <summary>
        /// Die Größe des Zwischenspeichers für die Dateien zu Radiosendungen.
        /// </summary>
        public int? AudioFileBufferSize { get; set; }

        /// <summary>
        /// Die Größe des Zwischenspeichers für die Dateien zu Fernsehsendungen in normaler Auflösung.
        /// </summary>
        public int? SDTVFileBufferSize { get; set; }

        /// <summary>
        /// Die Größe des Zwischenspeichers für die Dateien zu Fernsehsendungen in hoher Auflösung.
        /// </summary>
        public int? HDTVFileBufferSize { get; set; }

        /// <summary>
        /// Die vollständige Beschreibung der zugehörigen Quelle.
        /// </summary>
        [NonSerialized]
        private SourceSelection m_Selection = null!;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public ReceiveInformation()
        {
        }

        /// <summary>
        /// Ermittelt die zu verwendende Größe für die Zwischenspeicherung beim Schreiben.
        /// </summary>
        /// <param name="type">Die Art des Bildsignals.</param>
        /// <returns>Die zu verwendende Speichergröße.</returns>
        public int? GetFileBufferSize(Legacy.StreamTypes? type)
        {
            // Want audio
            if (!type.HasValue)
                return AudioFileBufferSize;

            // Check for supported types
            switch (type.Value)
            {
                case Legacy.StreamTypes.Video13818: return SDTVFileBufferSize;
                case Legacy.StreamTypes.H264: return HDTVFileBufferSize;
            }

            // We don't known
            return null;
        }

        /// <summary>
        /// Erzeugt eine Kopie dieser Beschreibung.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        public ReceiveInformation Clone()
        {
            // Create
            return
                new ReceiveInformation
                {
                    AudioFileBufferSize = AudioFileBufferSize,
                    SDTVFileBufferSize = SDTVFileBufferSize,
                    HDTVFileBufferSize = HDTVFileBufferSize,
                    UniqueIdentifier = UniqueIdentifier,
                    RecordingPath = RecordingPath,
                    SelectionKey = SelectionKey,
                    Streams = Streams.Clone(),
                };
        }

        /// <summary>
        /// Meldet die Informationen zur Quelle.
        /// </summary>
        [XmlIgnore]
        public SourceSelection Selection
        {
            get
            {
                // Create once
                if (null == m_Selection)
                    m_Selection = new SourceSelection { SelectionKey = SelectionKey };

                // Report
                return m_Selection;
            }
        }

        #region ICloneable Members

        /// <summary>
        /// Erzeugt eine Kopie dieser Beschreibung.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        object ICloneable.Clone() => Clone();

        #endregion
    }
}
