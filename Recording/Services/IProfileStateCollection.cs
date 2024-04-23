using JMS.DVB.NET.Recording.Planning;
using JMS.DVB.NET.Recording.Requests;

namespace JMS.DVB.NET.Recording.Services;

/// <summary>
/// Verwaltet den Arbeitszustand aller Geräteprofile.
/// </summary>
public interface IProfileStateCollection : IRecordingPlannerSite
{
    ILogger Logger { get; }

    IJobManager JobManager { get; }

    IVCRProfiles Profiles { get; }

    ServiceFactory ServiceFactory { get; }

    /// <summary>
    /// Ermittelt den Zustand eines einzelnen Geräteprofils.
    /// </summary>
    /// <param name="profileName">Das gewünschte Profil.</param>
    /// <returns>Die Zustandsbeschreibung oder <i>null</i>.</returns>
    IProfileState? this[string profileName] { get; }

    /// <summary>
    /// Meldet die Anzahl der aktiven Aufzeichnungen.
    /// </summary>
    int NumberOfActiveRecordings { get; }

    /// <summary>
    /// Meldet, ob auf irgendeinem Geräteprofil ein Zugriff aktiv ist.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Meldet Informationen zu allen Geräteprofilen.
    /// </summary>
    /// <typeparam name="TInfo">Die Art der gemeldeten Informationen.</typeparam>
    /// <param name="factory">Methode zum Erstellen der Information zu einem einzelnen Geräteprofil.</param>
    /// <returns>Die Informationen zu den Profilen.</returns>
    IEnumerable<TInfo> InspectProfiles<TInfo>(Func<IProfileState, TInfo> factory);

    /// <summary>
    /// Verändert den Endzeitpunkt einer Aufzeichnung.
    /// </summary>
    /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
    /// <param name="newEndTime">Der gewünschte Verschiebung des Endzeitpunkts.</param>
    /// <returns>Gesetzt, wenn die Änderung möglich war.</returns>
    bool ChangeEndTime(Guid scheduleIdentifier, DateTime newEndTime);

    /// <summary>
    /// Aktualisiert den Aufzeichnungsplan.
    /// </summary>
    void BeginNewPlan();

    /// <summary>
    /// Fordert eine Aktualisierung an und berechnet einen neuen Plan.
    /// </summary>
    void EnsureNewPlan();

    /// <summary>
    /// Ermittelt den aktuellen Ablaufplan.
    /// </summary>
    /// <returns>Alle Einträge des aktuellen Plans, maximal aber <i>1000</i>.</returns>
    PlanContext GetPlan();

    /// <summary>
    /// Bestätigt den Abschluss einer Operation und die Bereitschaft, die nächste Operation 
    /// zu starten.
    /// </summary>
    /// <param name="scheduleIdentifier">Die eindeutige Kennung der Operation.</param>
    /// <param name="isStart">Gesetzt, wenn es sich um einen Startbefehl handelt.</param>
    void ConfirmOperation(Guid scheduleIdentifier, bool isStart);

    /// <summary>
    /// Bereitet den Übergang in den Schlafzustand vor.
    /// </summary>
    void PrepareSuspend();

    /// <summary>
    /// Führt den Übergang in den Schlafzustand durch.
    /// </summary>
    void Suspend();

    /// <summary>
    /// Reaktiviert die Planung der Aufzeichnungen nach der Rückkehr aus dem Schlafzustand.
    /// </summary>
    void Resume();

    /// <summary>
    /// Fordert eine baldmögliche Aktualisierung der Programmzeitschrift an.
    /// </summary>
    void ForceProgramGuideUpdate();

    /// <summary>
    /// Erzwingt eine baldige Aktualisierung aller Listen von Quellen in allen
    /// Geräteprofilen.
    /// </summary>
    void ForceSoureListUpdate();
}
