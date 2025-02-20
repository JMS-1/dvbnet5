using System.Xml.Serialization;

namespace JMS.DVB.NET.Recording.Persistence;

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
