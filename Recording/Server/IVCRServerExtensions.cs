using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Planning;
using JMS.DVB.NET.Recording.ProgramGuide;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Server;

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

    /// <summary>
    /// Ermittelt den passendsten Eintrag aus der Programmzeitschrift.
    /// </summary>
    /// <typeparam name="TTarget">Die Art der R�ckgabewerte.</typeparam>
    /// <param name="profileName">Das zu betrachtende Geräteprofil.</param>
    /// <param name="source">Die zugeh�rige Quelle.</param>
    /// <param name="from">Der Beginn eines Zeitraums.</param>
    /// <param name="to">Das Ende eines Zeitraums.</param>
    /// <param name="factory">Name der Metjode zum Erzeugen eines externen Eintrags.</param>
    /// <returns>Der am besten passende Eintrag.</returns>
    public static TTarget? FindProgramGuideEntry<TTarget>(
        this IVCRServer server,
        string profileName,
        SourceIdentifier source,
        DateTime from,
        DateTime to,
        Func<ProgramGuideEntry, string, IVCRProfiles, TTarget> factory)
    {
        // See if profile exists
        var profile = server[profileName];

        return (profile == null) ? default : profile.ProgramGuide.FindBestEntry(source, from, to, factory);
    }

    /// <summary>
    /// Ermittelt den aktuellen Aufzeichnungsplan.
    /// </summary>
    /// <typeparam name="TActivity">Die Art der Information für einen Eintrag der Planung.</typeparam>
    /// <param name="end">Es werden nur Aufzeichnungen betrachtet, die nicht nach diesem Zeitpunkt starten.</param>
    /// <param name="limit">Die maximale Anzahl von Ergebniszeilen.</param>
    /// <param name="factory">Methode zum Erstellen einer einzelnen Planungsinformation.</param>
    /// <returns>Der gewünschte Aufzeichnungsplan.</returns>
    public static TActivity[] GetPlan<TActivity>(this IVCRServer server, DateTime end, int limit, Func<IScheduleInformation, PlanContext, IVCRServer, TActivity> factory)
    {
        // Result
        var activities = new List<TActivity>();

        // Resulting mapping
        var context = server.GetPlan();

        // Load list
        foreach (var schedule in context)
        {
            // See if we reached the very end
            if (schedule.Time.Start >= end)
                break;

            // Create
            var activity = factory(schedule, context, server);
            if (activity == null)
                continue;

            // Register
            activities.Add(activity);

            // Check limit
            if (activities.Count >= limit)
                break;
        }

        // Report
        return [.. activities];
    }
}