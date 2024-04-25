using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.Services.Planning;

public static class IVCRServerExtensions
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
    public static TTarget[] GetSources<TTarget>(this IVCRServer server, string profileName, bool withTV, bool withRadio, Func<SourceSelection, IVCRProfiles, TTarget> factory, IVCRProfiles profiles)
    {
        // Find the profile
        var profile = server.FindProfile(profileName);
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
            profiles
                .GetSources(profile.ProfileName, matchStation)
                .Select(s => factory(s, profiles))
                .ToArray();
    }

    /// <summary>
    /// Meldet Informationen zu allen Geräteprofilen.
    /// </summary>
    /// <typeparam name="TInfo">Die Art der gemeldeten Information.</typeparam>
    /// <param name="factory">Methode zum Erzeugen der Informationen zu einem einzelnen Geräteprofil.</param>
    /// <returns>Die Informationen zu den Profilen.</returns>
    public static TInfo[] GetProfiles<TInfo>(this IVCRServer server, Func<IProfileState, TInfo> factory)
        => server.InspectProfiles(factory).ToArray();

    /// <summary>
    /// Meldet alle Aufträge.
    /// </summary>
    /// <typeparam name="TJob">Die Art der externen Darstellung.</typeparam>
    /// <param name="factory">Methode zum Erstellen der externen Darstellung.</param>
    /// <returns>Die Liste der Aufträge.</returns>
    public static TJob[] GetJobs<TJob>(this IJobManager jobManager, Func<VCRJob, bool, IVCRProfiles, TJob> factory, IVCRProfiles profiles)
        => jobManager
            .GetActiveJobs()
            .Select(job => factory(job, true, profiles))
            .Concat(jobManager.ArchivedJobs.Select(job => factory(job, false, profiles)))
            .ToArray();
}