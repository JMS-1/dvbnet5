using JMS.DVB.NET.Recording.ProgramGuide;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.ProgramGuide;

namespace JMS.DVB.NET.Recording.Actions;

public interface IProgramGuideEntries
{
    /// <summary>
    /// Ermittelt einen Auszug aus der Programmzeitschrift.
    /// </summary>
    /// <typeparam name="TFilter">Die Art des Filters.</typeparam>
    /// <typeparam name="TEntry">Die Art der externen Darstellung von Einträgen.</typeparam>
    /// <param name="filter">Der Filter in der externen Darstellung.</param>
    /// <param name="filterConverter">Methode zur Wandlung des Filters in die interne Darstellung.</param>
    /// <param name="factory">Erstellt die externe Repr�sentation eines Eintrags.</param>
    /// <returns>Die Liste aller passenden Einträge.</returns>
    TEntry[] Get<TFilter, TEntry>(
        TFilter filter,
        Func<TFilter, IVCRProfiles, GuideEntryFilter?> filterConverter,
        Func<ProgramGuideEntry, string, IVCRProfiles, TEntry> factory
    ) where TFilter : class;

    /// <summary>
    /// Ermittelt einen Auszug aus der Programmzeitschrift.
    /// </summary>
    /// <typeparam name="TFilter">Die Art des Filters.</typeparam>
    /// <param name="filter">Der Filter in der externen Darstellung.</param>
    /// <param name="filterConverter">Methode zur Wandlung des Filters in die interne Darstellung.</param>
    /// <returns>Die Anzahl der passenden Einträge.</returns>
    int Get<TFilter>(
        TFilter filter,
        Func<TFilter, IVCRProfiles, GuideEntryFilter?> filterConverter
    ) where TFilter : class;

    /// <summary>
    /// Ermittelt die Eckdaten zu den Eintragungen eines Geräteprofils.
    /// </summary>
    /// <typeparam name="TInfo">Die Art der Informationen.</typeparam>
    /// <param name="profileName">Der Name des Geräteprofils.</param>
    /// <param name="factory">Methode zur Erstellung der Informationen.</param>
    /// <returns>Die gewünschten Informationen.</returns>
    TInfo GetInformation<TInfo>(string profileName, Func<IProgramGuideManager, IVCRProfiles, TInfo> factory);
}
