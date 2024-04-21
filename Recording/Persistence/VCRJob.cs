using System.Xml.Serialization;

namespace JMS.DVB.NET.Recording.Persistence
{
    /// <summary>
    /// Beschreibt einen Auftrag.
    /// </summary>
    /// <remarks>
    /// Ein Auftrag enthält zumindest eine Aufzeichnung.
    /// </remarks>
    [Serializable]
    public class VCRJob
    {
        /// <summary>
        /// Der spezielle Name für die Aktualisierung der Quellen eines Geräteprofils.
        /// </summary>
        public const string SourceScanName = "PSI";

        /// <summary>
        /// Der spezielle Name für die Aktualisierung der Programmzeitschrift.
        /// </summary>
        public const string ProgramGuideName = "EPG";

        /// <summary>
        /// Der spezielle Name für den LIVE Modus, der von <i>Zapping Clients</i> wie
        /// dem DVB.NET / VCR.NET Viewer verwendet werden.
        /// </summary>
        public const string ZappingName = "LIVE";

        /// <summary>
        /// Dateiendung für Aufträge im XML Serialisierungsformat.
        /// </summary>
        public const string FileSuffix = ".j39";

        /// <summary>
        /// Aufzeichnungen zu diesem Auftrag.
        /// </summary>        
        [XmlElement("Schedule")]
        public readonly List<VCRSchedule> Schedules = [];

        /// <summary>
        /// Verzeichnis, in dem Aufzeichnungsdateien abgelegt werden sollen.
        /// </summary>
        public string Directory { get; set; } = null!;

        /// <summary>
        /// Eindeutige Kennung des Auftrags.
        /// </summary>
        public Guid? UniqueID { get; set; }

        /// <summary>
        /// Die gewünschte Quelle.
        /// </summary>
        public SourceSelection Source { get; set; } = null!;

        /// <summary>
        /// Die Datenströme, die aufgezeichnet werden sollen.
        /// </summary>
        public StreamSelection Streams { get; set; } = null!;

        /// <summary>
        /// Name des Auftrags.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gesetzt, wenn es das Gerät zur Aufzeichnung automatisch ausgewählt werden darf.
        /// </summary>
        public bool AutomaticResourceSelection { get; set; }

        /// <summary>
        /// Ermittelt alle Aufträge in einem Verzeichnis.
        /// </summary>
        /// <param name="directory">Das zu bearbeitende Verzeichnis.</param>
        /// <returns>Alle Aufträge.</returns>
        public static IEnumerable<VCRJob> Load(DirectoryInfo directory)
        {
            // Process
            return
                directory
                    .GetFiles("*" + FileSuffix)
                    .Select(SerializationTools.Load<VCRJob>)
                    .Where(job => job != null);
        }

        /// <summary>
        /// Prüft, ob dieser Auftrag noch einmal verwendet wird. Das ist der Fall, wenn mindestens
        /// eine Aufzeichnung noch vorhanden ist.
        /// </summary>
        [XmlIgnore]
        public bool IsActive => Schedules.Any(schedule => schedule.IsActive);

        /// <summary>
        /// Meldet, ob diesem Auftrag eine Quelle zugeordnet ist.
        /// </summary>
        [XmlIgnore]
        public bool HasSource => (Source != null) && (Source.Source != null);

        /// <summary>
        /// Ermittelt eine Aufzeichnung dieses Auftrags.
        /// </summary>
        /// <param name="uniqueIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <returns>Die Aufzeichnung oder <i>null</i>.</returns>
        public VCRSchedule? this[Guid uniqueIdentifier] => Schedules.Find(s => s.UniqueID.HasValue && (s.UniqueID.Value == uniqueIdentifier));

        /// <summary>
        /// Entfernt alle Ausnahmeregelungen, die bereits verstrichen sind.
        /// </summary>
        public void CleanupExceptions() => Schedules.ForEach(schedule => schedule.CleanupExceptions());

        /// <summary>
        /// Stellt sicher, dass für diesen Auftrag ein Geräteprprofil ausgewählt ist.
        /// </summary>
        /// <param name="defaultProfileName">Der Name des bevorzugten Geräteprofils.</param>
        internal void SetProfile(string defaultProfileName)
        {
            // No source at all
            if (Source == null)
                Source = new SourceSelection { ProfileName = defaultProfileName };
            else if (string.IsNullOrEmpty(Source.ProfileName))
                Source.ProfileName = defaultProfileName;
        }
    }

    /// <summary>
    /// Hilfsmethoden zur Validierung von Aufträgen und Aufzeichnungen.
    /// </summary>
    public static class ValidationExtension
    {
        /// <summary>
        /// Alle in Dateinamen nicht erlaubten Zeichen.
        /// </summary>
        private static readonly char[] m_BadCharacters = Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).Distinct().ToArray();

        /// <summary>
        /// Prüft, ob eine Datenstromauswahl zulässig ist.
        /// </summary>
        /// <param name="streams">Die Auswahl der Datenströme.</param>
        /// <returns>Gesetzt, wenn die Auswahl gültig ist - und mindestens eine Tonspur enthält.</returns>
        public static bool Validate(this StreamSelection streams)
        {
            // Not possible
            if (streams == null)
                return false;

            // Test for wildcards - may fail at runtime!
            if (streams.MP2Tracks.LanguageMode != LanguageModes.Selection)
                return true;
            if (streams.AC3Tracks.LanguageMode != LanguageModes.Selection)
                return true;

            // Test for language selection - may fail at runtime but at least we tried
            if (streams.MP2Tracks.Languages.Count > 0)
                return true;
            if (streams.AC3Tracks.Languages.Count > 0)
                return true;

            // Will definitly fail
            return false;
        }

        /// <summary>
        /// Prüft, ob eine Zeichenkette als Name für einen Auftrag oder eine
        /// Aufzeichnung verwendet werden darf.
        /// </summary>
        /// <param name="name">Der zu prüfenden Name.</param>
        /// <returns>Gesetzt, wenn der Name verwendet werden darf.</returns>
        public static bool IsValidName(this string name) => string.IsNullOrEmpty(name) || (name.IndexOfAny(m_BadCharacters) < 0);

        /// <summary>
        /// Ersetzt alle Zeichen, die nicht in Dateinamen erlaubt sind, durch einen
        /// Unterstrich.
        /// </summary>
        /// <param name="s">Eine Zeichenkette.</param>
        /// <returns>Die korrigierte Zeichenkette.</returns>
        public static string MakeValid(this string s)
        {
            // No at all
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            // Correct all
            if (s.IndexOfAny(m_BadCharacters) >= 0)
                foreach (var ch in m_BadCharacters)
                    s = s.Replace(ch, '_');

            // Report
            return s;
        }
    }
}