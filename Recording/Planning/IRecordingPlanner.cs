using JMS.DVB.Algorithms.Scheduler;

namespace JMS.DVB.NET.Recording.Planning;

/// <summary>
/// Die globale Aufzeichnungsplanung.
/// </summary>
/// <threadsafety static="true" instance="false">Diese Klasse kann nicht <see cref="Thread"/> 
/// übergreifend verwendet werden. Der Aufrufer hat für eine entsprechende Synchronisation zu 
/// sorgen.</threadsafety>
public interface IRecordingPlanner : IDisposable
{
    /// <summary>
    /// Ermittelt den aktuellen Aufzeichnungsplan.
    /// </summary>
    /// <param name="referenceTime">Der Bezugspunkt für die Planung.</param>
    /// <returns>Die Liste der nächsten Aufzeichnungen.</returns>
    PlanContext GetPlan(DateTime referenceTime);

    /// <summary>
    /// Startet eine Aufzeichnung oder eine Aufgabe.
    /// </summary>
    /// <param name="item">Die Beschreibung der Aufgabe.</param>
    /// <returns>Gesetzt, wenn der Vorgang erfolgreich war.</returns>
    bool Start(IScheduleInformation item);

    /// <summary>
    /// Beendet eine Aufzeichnung oder eine Aufgabe.
    /// </summary>
    /// <param name="itemIdentifier">Die gewünschte Aufgabe.</param>
    void Stop(Guid itemIdentifier);

    /// <summary>
    /// Ermittelt die nächste Aufgabe.
    /// </summary>
    /// <param name="referenceTime">Der Bezugspunkt für die Analyse.</param>
    void DispatchNextActivity(DateTime referenceTime);

    /// <summary>
    /// Verändert den Endzeitpunkt einer Aufzeichnung.
    /// </summary>
    /// <param name="itemIdentifier">Die zugehörige Aufzeichnung.</param>
    /// <param name="newEndTime">Die gewünschte Verschiebung des Endzeitpunktes.</param>
    /// <returns>Gesetzt, wenn die Änderung ausgeführt werden konnte.</returns>
    bool SetEndTime(Guid itemIdentifier, DateTime newEndTime);

    /// <summary>
    /// Entfernt alle aktiven Aufzeichnungen.
    /// </summary>
    void Reset();
}

