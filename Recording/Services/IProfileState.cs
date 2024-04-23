using JMS.DVB.CardServer;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Requests;
using JMS.DVB.NET.Recording.Status;

namespace JMS.DVB.NET.Recording.Services;

/// <summary>
/// Beschreibt den Arbeitszustand eines einzelnen aktiven Geräteprofils.
/// </summary>
/// <param name="collection">Die zugehörige Verwaltung der aktiven Geräteprofile.</param>
/// <param name="profileName">Der Name des zugehörigen Geräteprofils.</param>
public interface IProfileState : IDisposable
{
    /// <summary>
    /// Die zugehörige Verwaltungsinstanz.
    /// </summary>
    IProfileStateCollection Collection { get; }

    /// <summary>
    /// Der Name des Geräteprofils.
    /// </summary>
    string ProfileName { get; }

    /// <summary>
    /// Meldet die zugehörige Verwaltung der elektronischen Programmzeitschrift (EPG).
    /// </summary>
    IProgramGuideManager ProgramGuide { get; }

    /// <summary>
    /// Meldet die aktuelle Aufzeichnung oder <i>null</i>.
    /// </summary>
    FullInfo? CurrentRecording { get; }

    /// <summary>
    /// Meldet die Anzahl der gerade aktiven Aufzeichnungen.
    /// </summary>
    int NumberOfActiveRecordings { get; }

    /// <summary>
    /// Meldet, ob gerade ein Zugriff auf diesem Geräteprofil stattfindet.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Liest oder setzt den Zeitpunkt der letzen Aktualisierung der Liste der Quellen
    /// dieses Geräteprofils durch den VCR.NET Recording Service.
    /// </summary>
    DateTime? LastSourceUpdateTime { get; set; }

    /// <summary>
    /// Prüft, ob eine bestimmte Quelle zu diesem Geräteprofil gehört.
    /// </summary>
    /// <param name="source">Die zu prüfende Quelle.</param>
    /// <returns>Gesetzt, wenn es sich um eine Quelle dieses Geräteprofil handelt.</returns>
    bool IsResponsibleFor(SourceSelection source);

    /// <summary>
    /// Aktiviert oder deaktiviert den Netzwerkversand einer aktiven Quelle.
    /// </summary>
    /// <param name="source">Die gewünschte Quelle.</param>
    /// <param name="uniqueIdentifier">Die eindeutige Kennung der Teilaufzeichnung.</param>
    /// <param name="target">Das neue Ziel des Netzwerkversands.</param>
    /// <returns>Gesetzt, wenn die Operation ausgeführt wurde.</returns>
    bool SetStreamTarget(SourceIdentifier source, Guid uniqueIdentifier, string target);

    /// <summary>
    /// Verändert die Endzeit der aktuellen Aufzeichnung.
    /// </summary>
    /// <param name="streamIdentifier">Die eindeutige Kennung des zu verwendenden Datenstroms.</param>
    /// <param name="newEndTime">Der neue Endzeitpunkt.</param>
    /// <param name="disableHibernation">Gesetzt, wenn der Übergang in den Schlafzustand deaktiviert werden soll.</param>
    /// <returns>Die Aktivität auf dem Geräteprofil, die verändert wurde.</returns>
    CardServerProxy? ChangeStreamEnd(Guid streamIdentifier, DateTime newEndTime, bool disableHibernation);

    /// <summary>
    /// Beginnt eine Aufzeichnung auf diesem Geräteprofil. Eventuell wird diese mit 
    /// einer anderen zusammen geführt.
    /// </summary>
    /// <param name="recording">Die gewünschte Aufzeichnung.</param>
    void StartRecording(VCRRecordingInfo recording);

    /// <summary>
    /// Beendet eine Aufzeichnung.
    /// </summary>
    /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
    void EndRecording(Guid scheduleIdentifier);

    /// <summary>
    /// Steuert den Zappping Modus.
    /// </summary>
    /// <typeparam name="TStatus">Die Art des Zustands.</typeparam>
    /// <param name="active">Gesetzt, wenn das Zapping aktiviert werden soll.</param>
    /// <param name="connectTo">Die TCP/IP UDP Adresse, an die alle Daten geschickt werden sollen.</param>
    /// <param name="source">Die zu aktivierende Quelle.</param>
    /// <param name="factory">Methode zum Erstellen einer neuen Zustandsinformation.</param>
    /// <returns>Der aktuelle Zustand des Zapping Modus oder <i>null</i>, wenn dieser nicht ermittelt
    /// werden kann.</returns>
    TStatus LiveModeOperation<TStatus>(bool active, string connectTo, SourceIdentifier source, Func<string, ServerInformation, TStatus> factory, ServiceFactory services);

    /// <summary>
    /// Beginnt eine Operation auf diesem Geräteprofil.
    /// </summary>
    /// <param name="request">Der zu aktivierende Zugriff.</param>
    /// <param name="throwOnBusy">Gesetzt um einen Fehler auszulösen, wenn bereits ein Zugriff aktiv ist.</param>
    /// <exception cref="ArgumentNullException">Es wurde kein Zugriff angegeben.</exception>
    /// <exception cref="ArgumentException">Der Zugriff gehört nicht zu diesem Geräteprofil.</exception>
    /// <exception cref="InvalidOperationException">Es ist bereits ein Zugriff aktiv.</exception>
    /// <returns>Gesetzt, wenn der neue Zugriff erfolgreich gestartet wurde.</returns>
    bool BeginRequest(CardServerProxy request, bool throwOnBusy = true);

    /// <summary>
    /// Beendet eine Operation auf diesem Geräteprofil.
    /// </summary>
    /// <param name="request">Der zu beendende Zugriff.</param>
    /// <exception cref="ArgumentNullException">Es wurde kein Zugriff angegeben.</exception>
    /// <exception cref="InvalidOperationException">Dieser Zugriff ist nicht der aktive Zugriff dieses
    /// Geräteprofils.</exception>
    void EndRequest(CardServerProxy request);
}

