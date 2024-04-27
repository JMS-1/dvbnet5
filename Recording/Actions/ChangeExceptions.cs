using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Server;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Actions;

public class ChangeExceptions(IVCRServer server, IJobManager jobs) : IChangeExceptions
{
    /// <summary>
    /// Verändert eine Ausnahme.
    /// </summary>
    /// <param name="jobIdentifier">Die eindeutige Kennung des Auftrags.</param>
    /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
    /// <param name="when">Der betroffene Tag.</param>
    /// <param name="startDelta">Die Verschiebung der Startzeit in Minuten.</param>
    /// <param name="durationDelta">Die Änderung der Aufzeichnungsdauer in Minuten.</param>
    public void Update(Guid jobIdentifier, Guid scheduleIdentifier, DateTime when, int startDelta, int durationDelta)
    {
        // Locate the job
        var job = jobs[jobIdentifier];
        if (job == null)
            return;
        var schedule = job[scheduleIdentifier];
        if (schedule == null)
            return;

        // Validate
        if (durationDelta < -schedule.Duration)
            return;

        // Create description
        var exception = new VCRScheduleException { When = when.Date };

        // Fill all data 
        if (startDelta != 0)
            exception.ShiftTime = startDelta;
        if (durationDelta != 0)
            exception.Duration = schedule.Duration + durationDelta;

        // Process
        schedule.SetException(exception.When, exception);

        // Store
        jobs.Update(job, null);

        // Recalculate plan
        server.BeginNewPlan();
    }
}