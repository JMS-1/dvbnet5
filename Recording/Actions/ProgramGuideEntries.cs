using JMS.DVB.NET.Recording.ProgramGuide;
using JMS.DVB.NET.Recording.Server;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.ProgramGuide;

namespace JMS.DVB.NET.Recording.Actions;

public class ProgramGuideEntries(IVCRServer server, IVCRProfiles profiles) : IProgramGuideEntries
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
    public TEntry[] Get<TFilter, TEntry>(
        TFilter filter,
        Func<TFilter, IVCRProfiles, GuideEntryFilter?> filterConverter,
        Func<ProgramGuideEntry, string, IVCRProfiles, TEntry> factory
    ) where TFilter : class
    {
        // Validate filter
        if (filter == null)
            return [];

        // Convert filter
        var filterIntern = filterConverter(filter, profiles);
        if (filterIntern == null)
            return [];

        // Locate profile and forward call
        var profileName = filterIntern.ProfileName;
        if (string.IsNullOrEmpty(profileName))
            return [];

        var profile = server.FindProfile(profileName);
        if (profile == null)
            return [];

        return profile.ProgramGuide.GetProgramGuideEntries(filterIntern, factory);
    }

    /// <inheritdoc/>
    public int Get<TFilter>(
        TFilter filter,
        Func<TFilter, IVCRProfiles, GuideEntryFilter?> filterConverter
    ) where TFilter : class
    {
        // Validate filter
        if (filter == null)
            return 0;

        // Convert filter
        var filterIntern = filterConverter(filter, profiles);

        // Locate profile and forward call
        var profileName = filterIntern!.ProfileName;
        if (string.IsNullOrEmpty(profileName))
            return 0;
        var profile = server.FindProfile(profileName);
        if (profile == null)
            return 0;

        return profile.ProgramGuide.GetProgramGuideEntries(filterIntern);
    }

    /// <inheritdoc/>
    public TInfo GetInformation<TInfo>(string profileName, Func<IProgramGuideManager, IVCRProfiles, TInfo> factory)
    {
        // Locate profile and forward call
        if (string.IsNullOrEmpty(profileName))
            return default!;
        var profile = server.FindProfile(profileName);
        if (profile == null)
            return default!;
        else
            return factory(profile.ProgramGuide, profiles);
    }
}