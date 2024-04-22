using JMS.DVB.NET.Recording.Persistence;

namespace JMS.DVB.NET.Recording.Services;

/// <summary>
/// Verwaltung aller Aufträge für alle DVB.NET Geräteprofile.
/// </summary>
/// <remarks>LEAF SERVICE</remarks>
public interface IJobManager
{
    /// <summary>
    /// Ermittelt das EPG und Sendersuchlaufverzeichnis vom VCR.NET.
    /// </summary>
    DirectoryInfo CollectorDirectory { get; }

    /// <summary>
    /// Aktualisiert einen Auftrag oder legt einen Auftrag neu an.
    /// </summary>
    /// <param name="job">Der neue oder veränderte Auftrag.</param>
    /// <param name="scheduleIdentifier">Die eindeutige Kennung der veränderten Aufzeichnung.</param>
    void Update(VCRJob job, Guid? scheduleIdentifier);

    /// <summary>
    /// Löscht einen aktiven oder archivierten Auftrag.
    /// </summary>
    /// <param name="job">Der zu löschende Auftrag.</param>
    void Delete(VCRJob job);

    /// <summary>
    /// Liefert einen bestimmten aktiven Auftrag.
    /// </summary>
    /// <param name="jobIdentifier">Die eindeutige Kennung des Auftrags.</param>
    /// <returns>Ein aktiver Auftrag oder <i>null</i>.</returns>
    VCRJob? this[Guid jobIdentifier] { get; }

    /// <summary>
    /// Ermittelt alle Aufträge zu einem DVB.NET Geräteprofil.
    /// </summary>
    /// <returns>Alle Aufträge zum Geräteprofil</returns>
    /// <exception cref="ArgumentNullException">Es wurde kein Geräteprofil angegeben.</exception>
    List<VCRJob> GetActiveJobs();

    /// <summary>
    /// Ermittelt alle archivierten Aufträge zu allen DVB.NET Geräteprofilen.
    /// </summary>
    VCRJob[] ArchivedJobs { get; }

    /// <summary>
    /// Entfernt veraltete Aufträge aus dem Archiv.
    /// </summary>
    void CleanupArchivedJobs();

    /// <summary>
    /// Erzeugt einen Protokolleintrag.
    /// </summary>
    /// <param name="logEntry">Der Protokolleintrag.</param>
    void CreateLogEntry(VCRRecordingInfo logEntry);

    /// <summary>
    /// Ermittelt alle Protokolleinträge für einen bestimmten Zeitraum.
    /// </summary>
    /// <param name="firstDate">Erster zu berücksichtigender Tag.</param>
    /// <param name="lastDate">Letzter zu berücksichtigender Tag.</param>
    /// <param name="profile">Profile, dessen Protokolle ausgelesen werden sollen.</param>
    /// <returns>Liste aller Protokolleinträge für den gewünschten Zeitraum.</returns>
    List<VCRRecordingInfo> FindLogEntries(DateTime firstDate, DateTime lastDate, ProfileState profile);

    /// <summary>
    /// Bereinigt alle veralteten Protokolleinträge.
    /// </summary>
    void CleanupLogEntries();

    /// <summary>
    /// Rekonstruiert einen Auftrag und eine Aufzeichnung aus einer Textdarstellung.
    /// </summary>
    /// <param name="id">Die Textdarstellung.</param>
    /// <param name="job">Der ermittelte Auftrag.</param>
    /// <returns>Die zugeh�rige Aufzeichnung im Auftrag.</returns>
    VCRSchedule? ParseUniqueWebId(string id, out VCRJob job);

    /// <summary>
    /// Legt nach einer abgeschlossenen Aufzeichnung fest, wann frühestens eine Wiederholung
    /// stattfinden darf.
    /// </summary>
    /// <param name="recording">Alle Informationen zur ausgeführten Aufzeichnung.</param>
    void SetRestartThreshold(VCRRecordingInfo recording);
}

