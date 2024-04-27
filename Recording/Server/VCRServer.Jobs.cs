using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Planning;
using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.Server;

public partial class VCRServer
{
    /// <summary>
    /// Ergänzt alle bekannten Aufzeichnungen zu einer Planungsinstzanz.
    /// </summary>
    /// <param name="scheduler">Die Verwaltung der Aufzeichnungen.</param>
    /// <param name="disabled">Alle nicht zu verwendenden Aufzeichnungen.</param>
    /// <param name="planner">Die Gesamtplanung.</param>
    /// <param name="context">Zusätzliche Informationen zur aktuellen Planung.</param>
    void IRecordingPlannerSite.AddRegularJobs(RecordingScheduler scheduler, Func<Guid, bool> disabled, RecordingPlanner planner, PlanContext context)
    {
        // Retrieve all jobs related to this profile
        foreach (var job in jobs.GetActiveJobs())
            foreach (var schedule in job.Schedules)
            {
                // No longer in use
                if (!schedule.IsActive)
                    continue;

                // Resolve source
                var source = schedule.Source ?? job.Source;
                if (source == null)
                    continue;

                // Resolve profile
                var resource = planner.GetResourceForProfile(source.ProfileName);
                if (resource == null)
                    continue;

                // Register single item
                jobs.AddToScheduler(schedule, scheduler, job, [resource], (s, p) => p.FindSource(s), disabled);

                // Remember - even if we skipped it
                context.RegisterSchedule(schedule, job);
            }
    }
}
