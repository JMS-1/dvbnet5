using System.Xml.Serialization;

namespace JMS.DVB.NET.Recording.Persistence
{
    /// <summary>
    /// Beschreibt einen Auftrag.
    /// </summary>
    /// <remarks>
    /// Ein Auftrag enth�lt zumindest eine Aufzeichnung.
    /// </remarks>
    [Serializable]
    public class VCRJob
    {
        /// <summary>
        /// Der spezielle Name f�r die Aktualisierung der Quellen eines Ger�teprofils.
        /// </summary>
        public const string SourceScanName = "PSI";

        /// <summary>
        /// Der spezielle Name f�r die Aktualisierung der Programmzeitschrift.
        /// </summary>
        public const string ProgramGuideName = "EPG";

        /// <summary>
        /// Der spezielle Name f�r den LIVE Modus, der von <i>Zapping Clients</i> wie
        /// dem DVB.NET / VCR.NET Viewer verwendet werden.
        /// </summary>
        public const string ZappingName = "LIVE";

        /// <summary>
        /// Dateiendung f�r Auftr�ge im XML Serialisierungsformat.
        /// </summary>
        public const string FileSuffix = ".j39";

        /// <summary>
        /// Aufzeichnungen zu diesem Auftrag.
        /// </summary>        
        [XmlElement("Schedule")]
        public readonly List<VCRSchedule> Schedules = new List<VCRSchedule>();

        /// <summary>
        /// Verzeichnis, in dem Aufzeichnungsdateien abgelegt werden sollen.
        /// </summary>
        public string Directory { get; set; } = null!;

        /// <summary>
        /// Eindeutige Kennung des Auftrags.
        /// </summary>
        public Guid? UniqueID { get; set; }

        /// <summary>
        /// Die gew�nschte Quelle.
        /// </summary>
        public SourceSelection Source { get; set; } = null!;

        /// <summary>
        /// Die Datenstr�me, die aufgezeichnet werden sollen.
        /// </summary>
        public StreamSelection Streams { get; set; } = null!;

        /// <summary>
        /// Name des Auftrags.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gesetzt, wenn es das Ger�t zur Aufzeichnung automatisch ausgew�hlt werden darf.
        /// </summary>
        public bool AutomaticResourceSelection { get; set; }

        /// <summary>
        /// Speichert diesen Auftrag ab.
        /// </summary>
        /// <param name="target">Der Pfad zu einem Zielverzeichnis.</param>
        /// <returns>Gesetzt, wenn der Speichervorgang erfolgreich war. <i>null</i> wird
        /// gemeldet, wenn diesem Auftrag keine Datei zugeordnet ist.</returns>
        public bool? Save(DirectoryInfo target, VCRServer server)
        {
            // Get the file
            var file = GetFileName(target);
            if (file == null)
                return null;

            // Be safe
            try
            {
                // Process
                SerializationTools.Save(this, file);
            }
            catch (Exception e)
            {
                // Report
                server.Log(e);

                // Done
                return false;
            }

            // Done
            return true;
        }

        /// <summary>
        /// L�schte diesen Auftrag.
        /// </summary>
        /// <param name="target">Der Pfad zu einem Zielverzeichnis.</param>
        /// <returns>Gesetzt, wenn der L�schvorgang erfolgreich war. <i>null</i> wird gemeldet,
        /// wenn die Datei nicht existierte.</returns>
        public bool? Delete(DirectoryInfo target, VCRServer server)
        {
            // Get the file
            var file = GetFileName(target);
            if (file == null)
                return null;
            if (!file.Exists)
                return null;

            // Be safe
            try
            {
                // Process
                file.Delete();
            }
            catch (Exception e)
            {
                // Report error
                server.Log(e);

                // Failed
                return false;
            }

            // Did it
            return true;
        }

        /// <summary>
        /// Ermittelt den Namen dieses Auftrags in einem Zielverzeichnis.
        /// </summary>
        /// <param name="target">Der Pfad zu einem Zielverzeichnis.</param>
        /// <returns>Die zugeh�rige Datei.</returns>
        private FileInfo? GetFileName(DirectoryInfo target) => UniqueID.HasValue ? new FileInfo(Path.Combine(target.FullName, UniqueID.Value.ToString("N").ToUpper() + FileSuffix)) : null;

        /// <summary>
        /// Ermittelt alle Auftr�ge in einem Verzeichnis.
        /// </summary>
        /// <param name="directory">Das zu bearbeitende Verzeichnis.</param>
        /// <returns>Alle Auftr�ge.</returns>
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
        /// Pr�ft, ob dieser Auftrag noch einmal verwendet wird. Das ist der Fall, wenn mindestens
        /// eine Aufzeichnung noch vorhanden ist.
        /// </summary>
        [XmlIgnore]
        public bool IsActive => Schedules.Any(schedule => schedule.IsActive);

        /// <summary>
        /// Pr�ft, ob ein Auftrag zul�ssig ist.
        /// </summary>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der ver�nderten Aufzeichnung.</param>
        /// <exception cref="InvalidJobDataException">Die Konfiguration dieses Auftrags is ung�ltig.</exception>
        public void Validate(Guid? scheduleIdentifier)
        {
            // Identifier
            if (!UniqueID.HasValue)
                throw new InvalidJobDataException("Die eindeutige Kennung ist ungültig");

            // Name
            if (!Name.IsValidName())
                throw new InvalidJobDataException("Der Name enthält ungültige Zeichen");

            // Test the station
            if (HasSource)
            {
                // Source
                if (!Source.Validate())
                    throw new InvalidJobDataException("Eine Quelle ist ungültig");

                // Streams
                if (!Streams.Validate())
                    throw new InvalidJobDataException("Die Aufzeichnungsoptionen sind ungültig");
            }
            else if (Streams != null)
                throw new InvalidJobDataException("Die Aufzeichnungsoptionen sind ungültig");

            // List of schedules
            if (Schedules.Count < 1)
                throw new InvalidJobDataException("Keine Aufzeichnungen vorhanden");

            // Only validate the schedule we updated
            if (scheduleIdentifier.HasValue)
                foreach (var schedule in Schedules)
                    if (!schedule.UniqueID.HasValue || schedule.UniqueID.Value.Equals(scheduleIdentifier))
                        schedule.Validate(this);
        }

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
        /// Stellt sicher, dass f�r diesen Auftrag ein Ger�teprprofil ausgew�hlt ist.
        /// </summary>
        internal void SetProfile()
        {
            // No need
            if (!string.IsNullOrEmpty(Source?.ProfileName))
                return;

            // Attach to the default profile
            var defaultProfile = VCRProfiles.DefaultProfile;
            if (defaultProfile == null)
                return;

            // Process
            if (Source == null)
                Source = new SourceSelection { ProfileName = defaultProfile.Name };
            else
                Source.ProfileName = defaultProfile.Name;
        }

        /// <summary>
        /// Stellt sicher, dass f�r diesen Auftrag ein Ger�teprprofil ausgew�hlt ist.
        /// </summary>
        /// <param name="defaultProfileName">Der Name des bevorzugten Ger�teprofils.</param>
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
    /// Hilfsmethoden zur Validierung von Auftr�gen und Aufzeichnungen.
    /// </summary>
    public static class ValidationExtension
    {
        /// <summary>
        /// Alle in Dateinamen nicht erlaubten Zeichen.
        /// </summary>
        private static readonly char[] m_BadCharacters = Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).Distinct().ToArray();

        /// <summary>
        /// Pr�ft, ob eine Quelle g�ltig ist.
        /// </summary>
        /// <param name="source">Die Auswahl der Quelle oder <i>null</i>.</param>
        /// <returns>Gesetzt, wenn die Auswahl g�ltig ist.</returns>
        public static bool Validate(this SourceSelection source) => (VCRProfiles.FindSource(source) != null);

        /// <summary>
        /// Pr�ft, ob eine Datenstromauswahl zul�ssig ist.
        /// </summary>
        /// <param name="streams">Die Auswahl der Datenstr�me.</param>
        /// <returns>Gesetzt, wenn die Auswahl g�ltig ist - und mindestens eine Tonspur enth�lt.</returns>
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
        /// Pr�ft, ob eine Zeichenkette als Name f�r einen Auftrag oder eine
        /// Aufzeichnung verwendet werden darf.
        /// </summary>
        /// <param name="name">Der zu pr�fenden Name.</param>
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
