using JMS.DVB.NET.Recording.ProgramGuide;

namespace JMS.DVB.NET.Recording.Services.ProgramGuide;

/// <summary>
/// Verwaltet die elektronischen Programmzeitschrift von VCR.NET.
/// </summary>
public interface IProgramGuideManager
{
    /// <summary>
    /// Der Name des zugehörigen Geräteprofils.
    /// </summary>
    string ProfileName { get; }

    /// <summary>
    /// Meldet die tatsächliche Verwaltung der Programmzeitschrift.
    /// </summary>
    ProgramGuideEntries? LeafEntries { get; }

    /// <summary>
    /// Liest oder setzt den Zeitpunkt der letzen Aktualisierung der Programmzeitschrift.
    /// </summary>
    DateTime? LastUpdateTime { get; set; }

    /// <summary>
    /// Die Daten der Programmzeitschrift.
    /// </summary>
    ProgramGuideEntries Events { get; }

    /// <summary>
    /// Aktualisiert die Programmzeitschrift mit neuen Daten.
    /// </summary>
    /// <param name="entries">Die neuen Daten.</param>
    void UpdateGuide(ProgramGuideEntries entries);

    /// <summary>
    /// Prüft, ob für den gewählten Zeitraum ein Eintrag existiert.
    /// </summary>
    /// <param name="source">Die Quelle, deren Einträge untersucht werden sollen.</param>
    /// <param name="start">Der Beginn des Zeitraums (einschließlich).</param>
    /// <param name="end">Das Ende des Zeitraums (ausschließlich).</param>
    /// <returns>Gesetzt, wenn ein Eintrag existiert.</returns>
    bool HasEntry(SourceIdentifier source, DateTime start, DateTime end);

    /// <summary>
    /// Ermittelt einen bestimmten Eintrag.
    /// </summary>
    /// <param name="source">Die Quelle, deren Eintrag ermittelt werden soll.</param>
    /// <param name="start">Der exakte Startzeitpunkt.</param>
    /// <returns>Der gewünschte Eintrag.</returns>
    ProgramGuideEntry? FindEntry(SourceIdentifier source, DateTime start) => LeafEntries?.FindEntry(source, start);

    /// <summary>
    /// Ermittelt den am besten passenden Eintrag aus einem Zeitraum.
    /// </summary>
    /// <typeparam name="TTarget">Die Art der Rückgabewerte.</typeparam>
    /// <param name="source">Die gewünschte Quelle.</param>
    /// <param name="start">Der Beginn des Zeitraums.</param>
    /// <param name="end">Das Ende des Zeitraums.</param>
    /// <param name="factory">Methode zum Erzeugen eines Rückgabewertes.</param>
    /// <returns>Der am besten passende Eintrag.</returns>
    TTarget FindBestEntry<TTarget>(SourceIdentifier source, DateTime start, DateTime end, Func<ProgramGuideEntry, string, IVCRProfiles, TTarget> factory);

    /// <summary>
    /// Meldet alle Einträge der Programmzeitschrift zu einer Quelle.
    /// </summary>
    /// <param name="source">Die gewünschte Quelle.</param>
    /// <returns>Die gewünschte Liste.</returns>
    IEnumerable<ProgramGuideEntry> GetEntries(SourceIdentifier source);

    /// <summary>
    /// Ermittelt einen Auszug aus der Programmzeitschrift.
    /// </summary>
    /// <typeparam name="TEntry">Die Art der externen Darstellung von Einträgen.</typeparam>
    /// <param name="filter">Der Filter in der internen Darstellung.</param>
    /// <param name="factory">Erstellt die externe Repräsentation eines Eintrags.</param>
    /// <returns>Die Liste aller passenden Einträge.</returns>
    TEntry[] GetProgramGuideEntries<TEntry>(GuideEntryFilter filter, Func<ProgramGuideEntry, string, IVCRProfiles, TEntry> factory);

    /// <summary>
    /// Ermittelt einen Auszug aus der Programmzeitschrift.
    /// </summary>
    /// <param name="filter">Der Filter in der internen Darstellung.</param>
    /// <returns>Die Anzahl der passenden Einträge.</returns>
    int GetProgramGuideEntries(GuideEntryFilter filter);
}

