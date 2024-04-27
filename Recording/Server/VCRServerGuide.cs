using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Planning;

namespace JMS.DVB.NET.Recording.Server;

public partial class VCRServer
{
    /// <inheritdoc/>
    public void ForceProgramGuideUpdate()
    {
        // Report
        Tools.ExtendedLogging("Full immediate Program Guide Update requested");

        // Forward to all
        ForEachProfile(state => state.ProgramGuide.LastUpdateTime = null);

        // Replan
        BeginNewPlan();
    }

    /// <summary>
    /// Erstellt einen periodischen Auftrag für die Aktualisierung der Programmzeitschrift.
    /// </summary>
    /// <param name="resource">Die zu verwendende Ressource.</param>
    /// <param name="profile">Das zugehörige Geräteprofil.</param>
    /// <returns>Der gewünschte Auftrag.</returns>
    PeriodicScheduler IRecordingPlannerSite.CreateProgramGuideTask(IScheduleResource resource, Profile profile)
    {
        // Protect against misuse
        if (_stateMap.TryGetValue(profile.Name, out var state))
            return new ProgramGuideTask(resource, state, configuration, jobs);
        else
            return null!;
    }
}
