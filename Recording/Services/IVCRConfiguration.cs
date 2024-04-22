namespace JMS.DVB.NET.Recording.Services;

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
    /// Meldet, ob der <i>Card Server</i> als eigenständiger Prozess gestartet werden soll.
    /// </summary>
    bool UseExternalCardServer { get; }

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
    /// Meldet den Namen der Kontogruppe der Anwender, die Zugriff auf den
    /// VCR.NET Recording Service haben.
    /// </summary>
    string UserRole { get; }

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
    /// Meldet den Namen der Kontogruppe der Anwender, die administrativen Zugriff auf den
    /// VCR.NET Recording Service haben.
    /// </summary>
    string AdminRole { get; }

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
    /// Gesetzt, wenn die Verbindung zu den Web Diensten verschlüsselt werden soll.
    /// </summary>
    bool EncryptWebCommunication { get; }

    /// <summary>
    /// Meldet die Zeit die nach einem erzwungenen Schlafzustand verstreichen muss, bevor der
    /// Rechner für eine Aufzeichnung aufgweckt wird.
    /// </summary>
    TimeSpan DelayAfterForcedHibernation { get; }

    /// <summary>
    /// Gesetzt, wenn beim Schlafzustand keine Sonderbehandlung erwünscht ist.
    /// </summary>
    bool SuppressDelayAfterForcedHibernation { get; }

    /// <summary>
    /// Gesetzt wenn es nicht gestattet ist, aus einem H.264 Bildsignal die Zeitbasis (PCR)
    /// abzuleiten.
    /// </summary>
    bool DisablePCRFromH264Generation { get; }

    /// <summary>
    /// Meldet, ob der VCR.NET Recording Service den Rechner in einen Schlafzustand
    /// versetzten darf.
    /// </summary>
    bool MayHibernateSystem { get; }

    /// <summary>
    /// Gesetzt, wenn die Anwender sich auch über das Basic Prototokoll
    /// autorisieren dürfen.
    /// </summary>
    bool EnableBasicAuthentication { get; }

    /// <summary>
    /// Meldet die Größe für die Zwischenspeicherung bei Fernsehaufnahmen mit hoher Auflösung.
    /// </summary>
    int? HighDefinitionVideoBufferSize { get; }

    /// <summary>
    /// Meldet den TCP/IP Port, an den der Web Server bei einer sichen Verbindung gebunden werden soll.
    /// </summary>
    ushort WebServerSecureTcpPort { get; }

    /// <summary>
    /// Meldet die maximale Verweildauer eines archivierten Auftrags im Archiv, bevor
    /// er gelöscht wird.
    /// </summary>
    uint ArchiveLifeTime { get; }

    /// <summary>
    /// Meldet die geschätzte Zeit, die dieses System maximal braucht, um aus dem
    /// Schlafzustand zu erwachen.
    /// </summary>
    uint HibernationDelay { get; }

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
    /// Meldet den TCP/IP Port, an den der Web Server gebunden werden soll.
    /// </summary>
    ushort WebServerTcpPort { get; }

    /// <summary>
    /// Meldet die Zeit in Wochen, die ein Protokolleintrag vorgehalten wird.
    /// </summary>
    uint LogLifeTime { get; }

    /// <summary>
    /// Meldet die Größe für die Zwischenspeicherung bei Radioaufnahmen.
    /// </summary>
    int? AudioBufferSize { get; }

    /// <summary>
    /// Meldet, ob der Schlafzustand S3 (Standby) anstelle von S4 (Hibernate)
    /// verwenden soll.
    /// </summary>
    bool UseS3ForHibernate { get; }

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
    /// Meldet oder legt fest, ob bereits einmal eine Aufzeichnung ausgeführt wurde.
    /// </summary>
    bool HasRecordedSomething { get; set; }
}

