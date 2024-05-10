using JMS.DVB.NET.Recording.Services.Logging;

namespace JMS.DVB.NET.Recording.Services.Configuration;

/// <summary>
/// Verwaltet die Konfiguration des VCR.NET Recording Service.
/// </summary>
/// <remarks>LEAF SINGLETON SERVICE.</remarks>
public interface IVCRConfiguration
{
    /// <summary>
    /// Meldet alle vollen Stunden, zu denen eine Sammlung stattfinden soll.
    /// </summary>
    uint[] SourceListUpdateHours { get; }

    /// <summary>
    /// Meldet die maximale Laufzeit für die Aktualisierung der Quellen eines Geräteprofils.
    /// </summary>
    uint SourceListUpdateDuration { get; }

    /// <summary>
    /// Meldet die Zeitspanne, nachdem eine Aktualisierung der Quellen vorgezogen
    /// erfolgen darf.
    /// </summary>
    TimeSpan? SourceListJoinThreshold { get; }

    /// <summary>
    /// Meldet die minimale Zeitspanne zwischen zwei Aktualisierungen der Programmzeitschrift.
    /// </summary>
    TimeSpan? ProgramGuideUpdateInterval { get; }

    /// <summary>
    /// Ermittelt das Ersetzungsmuster für Dateinamen.
    /// </summary>
    string FileNamePattern { get; }

    /// <summary>
    /// Meldet, ob nach Abschluss der Aktualisierung die Listen der Quellen zusammengeführt
    /// werden sollen.
    /// </summary>
    bool MergeSourceListUpdateResult { get; }

    /// <summary>
    /// Meldet alle erlaubten Aufzeichnungsverzeichnisse.
    /// </summary>
    string[] TargetDirectoryNames { get; }

    /// <summary>
    /// Meldet die Zeitspanne, nachdem eine Aktualisierung der Programmzeitschrift vorgezogen
    /// erfolgen darf.
    /// </summary>
    TimeSpan? ProgramGuideJoinThreshold { get; }

    /// <summary>
    /// Meldet alle Quellen, für die Daten gesammelt werden sollen.
    /// </summary>
    string[] ProgramGuideSourcesAsArray { get; }

    /// <summary>
    /// Meldet das primäre Aufzeichnungsverzeichnis.
    /// </summary>
    DirectoryInfo PrimaryTargetDirectory { get; }

    /// <summary>
    /// Meldet alle vollen Stunden, zu denen eine Sammlung stattfinden soll.
    /// </summary>
    uint[] ProgramGuideUpdateHours { get; }

    /// <summary>
    /// Meldet die Größe für die Zwischenspeicherung bei Fernsehaufnahmen normaler Qualität.
    /// </summary>
    int? StandardVideoBufferSize { get; }

    /// <summary>
    /// Meldet die maximale Laufzeit einer Aktualisierung gemäß der Konfiguration.
    /// </summary>
    uint ProgramGuideUpdateDuration { get; }

    /// <summary>
    /// Meldet, ob die Programmzeitschrift der englischen FreeSat Sender eingeschlossen 
    /// werden soll.
    /// </summary>
    bool EnableFreeSat { get; }

    /// <summary>
    /// Meldet alle Quellen, für die Daten gesammelt werden sollen.
    /// </summary>
    IEnumerable<string> ProgramGuideSources { get; }

    /// <summary>
    /// Meldet die Namen der DVB.NET Geräteprofile, die der VCR.NET Recording Service
    /// verwenden darf.
    /// </summary>
    string ProfileNames { get; }

    /// <summary>
    /// Gesetzt wenn es nicht gestattet ist, aus einem MPEG2 Bildsignal die Zeitbasis (PCR)
    /// abzuleiten.
    /// </summary>
    bool DisablePCRFromMPEG2Generation { get; }

    /// <summary>
    /// Gesetzt wenn es nicht gestattet ist, aus einem H.264 Bildsignal die Zeitbasis (PCR)
    /// abzuleiten.
    /// </summary>
    bool DisablePCRFromH264Generation { get; }

    /// <summary>
    /// Meldet die Größe für die Zwischenspeicherung bei Fernsehaufnahmen mit hoher Auflösung.
    /// </summary>
    int? HighDefinitionVideoBufferSize { get; }

    /// <summary>
    /// Meldet die maximale Verweildauer eines archivierten Auftrags im Archiv, bevor
    /// er gelöscht wird.
    /// </summary>
    uint ArchiveLifeTime { get; }

    /// <summary>
    /// Meldet den aktuellen Umfang der Protokollierung.
    /// </summary>
    LoggingLevel LoggingLevel { get; }

    /// <summary>
    /// Meldet, ob eine Aktialisierung der Programmzeitschrift überhaupt stattfinden soll.
    /// </summary>
    bool ProgramGuideUpdateEnabled { get; }

    /// <summary>
    /// Meldet, wieviele Tage mindestens zwischen zwei Aktualisierungen der Liste
    /// der Quellen eines Geräteprofils liegen müssen.
    /// </summary>
    int SourceListUpdateInterval { get; }

    /// <summary>
    /// Meldet die Zeit in Wochen, die ein Protokolleintrag vorgehalten wird.
    /// </summary>
    uint LogLifeTime { get; }

    /// <summary>
    /// Meldet die Größe für die Zwischenspeicherung bei Radioaufnahmen.
    /// </summary>
    int? AudioBufferSize { get; }

    /// <summary>
    /// Bereitet eine Aktualisierung vor.
    /// </summary>
    /// <param name="names">Die zu aktualisierenden Einträge.</param>
    /// <returns>Alle gewünschten Einträge.</returns>
    Dictionary<SettingNames, SettingDescription> BeginUpdate(params SettingNames[] names);

    /// <summary>
    /// Führt eine Aktualisierung aus.
    /// </summary>
    /// <param name="settings">Die eventuell veränderten Einstellungen.</param>
    /// <returns>Gesetzt, wenn ein Neustart erforderlich war.</returns>
    bool CommitUpdate(IEnumerable<SettingDescription> settings);

    /// <summary>
    /// Prüft, ob eine Datei in einem zulässigen Aufzeichnungsverzeichnis liegt.
    /// </summary>
    /// <param name="path">Der zu prüfende Dateipfad.</param>
    /// <returns>Gesetzt, wenn der Pfad gültig ist.</returns>
    bool IsValidTarget(string path);

    /// <summary>
    /// Load the configuration from the provided file.
    /// </summary>
    void Reload();

    /// <summary>
    /// Meldet oder legt fest, ob bereits einmal eine Aufzeichnung ausgeführt wurde.
    /// </summary>
    bool HasRecordedSomething { get; set; }
}

