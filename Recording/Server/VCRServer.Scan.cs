using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Planning;

namespace JMS.DVB.NET.Recording.Server;

/// <param name="server">Die primäre VCR.NET Instanz.</param>
public partial class VCRServer
{
    /// <inheritdoc/>
    public void ForceSoureListUpdate()
    {
        // Report
        Tools.ExtendedLogging("Full immediate Source List Update requested");

        // Forward
        ForEachProfile(state => state.LastSourceUpdateTime = null);

        // Replan
        BeginNewPlan();
    }

    /// <summary>
    /// Erstellt einen periodischen Auftrag für die Aktualisierung der Liste der Quellen.
    /// </summary>
    /// <param name="resource">Die zu verwendende Ressource.</param>
    /// <param name="profile">Das zugehörige Geräteprofil.</param>
    /// <returns>Der gewünschte Auftrag.</returns>
    PeriodicScheduler IRecordingPlannerSite.CreateSourceScanTask(IScheduleResource resource, Profile profile)
    {
        // Protect against misuse
        if (_stateMap.TryGetValue(profile.Name, out var state))
            return new SourceListTask(resource, state, configuration, jobs);
        else
            return null!;
    }
}
