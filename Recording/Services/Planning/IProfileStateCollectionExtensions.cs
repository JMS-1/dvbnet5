using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.Services.Planning;

public static class IProfileStateCollectionExtensions
{
    /// <summary>
    /// Ermittelt alle Quellen eines Ger?teprofils für die Nutzung durch den <i>LIVE</i> Zugang.
    /// </summary>
    /// <typeparam name="TTarget">Die Art der Zielklasse.</typeparam>
    /// <param name="profileName">Der Name des Ger?teprofils.</param>
    /// <param name="withTV">Gesetzt, wenn Fernsehsender zu ber?cksichtigen sind.</param>
    /// <param name="withRadio">Gesetzt, wenn Radiosender zu ber?cksichtigen sind.</param>
    /// <param name="factory">Eine Methode zum Erzeugen der Zielelemente aus den Daten einer einzelnen Quelle.</param>
    /// <returns></returns>
    public static TTarget[] GetSources<TTarget>(this IProfileStateCollection states, string profileName, bool withTV, bool withRadio, Func<SourceSelection, IVCRProfiles, TTarget> factory)
    {
        // Find the profile
        var profile = states.FindProfile(profileName);
        if (profile == null)
            return [];

        // Create matcher
        Func<Station, bool> matchStation;
        if (withTV)
            if (withRadio)
                matchStation = station => true;
            else
                matchStation = station => station.SourceType == SourceTypes.TV;
        else
            if (withRadio)
            matchStation = station => station.SourceType == SourceTypes.Radio;
        else
            return [];

        // Filter all we want
        return
            states
                .Profiles
                .GetSources(profile.ProfileName, matchStation)
                .Select(s => factory(s, states.Profiles))
                .ToArray();
    }

    /// <summary>
    /// Meldet Informationen zu allen Geräteprofilen.
    /// </summary>
    /// <typeparam name="TInfo">Die Art der gemeldeten Information.</typeparam>
    /// <param name="factory">Methode zum Erzeugen der Informationen zu einem einzelnen Geräteprofil.</param>
    /// <returns>Die Informationen zu den Profilen.</returns>
    public static TInfo[] GetProfiles<TInfo>(this IProfileStateCollection states, Func<IProfileState, TInfo> factory)
        => states.InspectProfiles(factory).ToArray();

    /// <summary>
    /// Meldet alle Aufträge.
    /// </summary>
    /// <typeparam name="TJob">Die Art der externen Darstellung.</typeparam>
    /// <param name="factory">Methode zum Erstellen der externen Darstellung.</param>
    /// <returns>Die Liste der Aufträge.</returns>
    public static TJob[] GetJobs<TJob>(this IProfileStateCollection states, Func<VCRJob, bool, IVCRProfiles, TJob> factory)
        => states
            .JobManager
            .GetActiveJobs()
            .Select(job => factory(job, true, states.Profiles))
            .Concat(states.JobManager.ArchivedJobs.Select(job => factory(job, false, states.Profiles)))
            .ToArray();
}