using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Planning;
using JMS.DVB.NET.Recording.Requests;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Server;

/// <param name="server">Die primäre VCR.NET Instanz.</param>
public partial class VCRServer
{
    private readonly ISourceScanProxyFactory _scanFactory = scanFactory;

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
    PeriodicScheduler IRecordingPlannerSite.CreateSourceScanTask(
        IScheduleResource resource,
        Profile profile,
        IVCRConfiguration configuration,
        IJobManager jobs
    )
    {
        // Protect against misuse
        if (_stateMap.TryGetValue(profile.Name, out var state))
            return new SourceListTask(resource, state, configuration, jobs);
        else
            return null!;
    }
}
