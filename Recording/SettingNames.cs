
namespace JMS.DVB.NET.Recording
{
    /// <summary>
    /// Enthält die Namen aller Konfigurationsparameter.
    /// </summary>
    public enum SettingNames
    {
        /// <summary>
        /// Das Muster, nach dem die Namen der Aufzeichnungsdateien erzeugt werden.
        /// </summary>
        FileNamePattern,

        /// <summary>
        /// Das primäre Aufzeichnungsverzeichnis. Nur dieses und untergeordnete Verzeichnisse können
        /// zur Ablage von Aufzeichnungsdateien verwerden werden.
        /// </summary>
        VideoRecorderDirectory,

        /// <summary>
        /// Eine Liste von zusätzlichen Verzeichnissen, in denen Aufzeichnungen abgelegt werden dürfen.
        /// </summary>
        AdditionalRecorderPaths,

        /// <summary>
        /// Alle zu verwendenden DVB.NET Geräteprofile. Die einzelnen Namen sind durch einen 
        /// senkrechten Strich getrennt.
        /// </summary>
        Profiles,

        /// <summary>
        /// Die Zeitspanne, nach der archivierte Aufträge automatisch entfernt werden (in Wochen).
        /// </summary>
        ArchiveLifeTime,

        /// <summary>
        /// Die Zeitspanne, nach der Protokolleinträge automatisch entfernt werden (in Wochen).
        /// </summary>
        LogLifeTime,

        /// <summary>
        /// Beschreibt den Umfang der Protokollierung.
        /// </summary>
        LoggingLevel,

        /// <summary>
        /// Die durch Kommas getrennte Liste der Sender, die in der Programmzeitschrift berücksichtigt werden sollen.
        /// </summary>
        EPGStations,

        /// <summary>
        /// Die maximale Laufzeit für die Aktualisierung der Programmzeitschrift.
        /// </summary>
        EPGDuration,

        /// <summary>
        /// Die Programmzeitschrift berücksichtigt auch die englischen Sender.
        /// </summary>
        EPGIncludeFreeSat,

        /// <summary>
        /// Die Liste aller vollen Stunden, zu denen die Programmzeitschrift aktualisiert
        /// werden soll.
        /// </summary>
        EPGHours,

        /// <summary>
        /// Die maximale Laufzeit für die Aktualisierung der Quellen eines Geräteprofils.
        /// </summary>
        ScanDuration,

        /// <summary>
        /// Der Abstand in Tagen zwischen der Aktualisierung der Quellen eines Geräteprofils.
        /// </summary>
        ScanInterval,

        /// <summary>
        /// Die Liste aller vollen Stunden, zu denen die Programmzeitschrift aktualisiert werden soll.
        /// </summary>
        ScanHours,

        /// <summary>
        /// Wird gesetzt, um nach einer Aktualisierung der Quellen das Ergebnis zusammen
        /// zu führen.
        /// </summary>
        MergeScanResult,

        /// <summary>
        /// Optional die Größe für das Zwischenspeichern bei Schreiben in die Datei einer 
        /// Radioaufzeichnung.
        /// </summary>
        TSAudioBufferSize,

        /// <summary>
        /// Optional die Größe für das Zwischenspeichern bei Schreiben in die Datei einer 
        /// Fernsehaufzeichnung geringer Qualität.
        /// </summary>
        TSSDTVBufferSize,

        /// <summary>
        /// Optional die Größe für das Zwischenspeichern bei Schreiben in die Datei einer 
        /// Fernsehaufzeichnung hoher Qualität.
        /// </summary>
        TSHDTVBufferSize,

        /// <summary>
        /// Der minimale Abstand zwischen zwei Aktualisierungen der Programmzeitschrift.
        /// </summary>
        EPGInterval,

        /// <summary>
        /// Der zeitliche Schwellwert für eine vorzeitige Aktualisierung der Programmzeitschrift.
        /// </summary>
        EPGJoinThreshold,

        /// <summary>
        /// Der zeitliche Schwellwert für eine vorzeitige Aktualisierung der Liste der Quellen
        /// (Sendersuchlauf).
        /// </summary>
        ScanJoinThreshold,

        /// <summary>
        /// Verbietet es, aus einem H.264 Bildsignal die Zeitbasis (PCR) abzuleiten.
        /// </summary>
        DisablePCRFromH264Generation,

        /// <summary>
        /// Verbietet es, aus einem MPEG2 Bildsignal die Zeitbasis (PCR) abzuleiten.
        /// </summary>
        DisablePCRFromMPEG2Generation,

        /// <summary>
        /// Optional der SMTP Server über den Nachrichten versendet werden.
        /// </summary>
        SmtpRelay,

        /// <summary>
        /// Der Benutzername zur Autorisierung beim SMTP Server.
        /// </summary>
        SmtpUsername,

        /// <summary>
        /// Das Kennwort zum SMTP Benutzernamen.
        /// </summary>
        SmtpPassword,

        /// <summary>
        /// Der Empfänger von Nachrichten.
        /// </summary>
        SmtpRecipient,
    }
}
