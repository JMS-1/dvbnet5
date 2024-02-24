using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.RestWebApi;

namespace JMS.DVB.NET.Recording
{
    partial class VCRServer
    {
        /// <summary>
        /// Die Verwaltung der Auftr�ge.
        /// </summary>
        internal JobManager JobManager { get; private set; }

        /// <summary>
        /// Ermittelt einen Auftrag.
        /// </summary>
        /// <param name="uniqueIdentifier">Die eindeutige Kennung des Auftrags.</param>
        /// <returns>Der gew�nschte Auftrag oder <i>null</i>.</returns>
        public VCRJob? FindJob(Guid uniqueIdentifier) => JobManager.FindJob(uniqueIdentifier);

        /// <summary>
        /// Aktualisiert einen Auftrag oder legt einen neue an.
        /// </summary>
        /// <param name="job">Der betroffene Auftrag.</param>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der ver�nderten Aufzeichnung.</param>
        public void UpdateJob(VCRJob job, Guid? scheduleIdentifier)
        {
            // Cleanup
            if (scheduleIdentifier.HasValue)
                foreach (var schedule in job.Schedules)
                    if (schedule.UniqueID.HasValue)
                        if (schedule.UniqueID.Value.Equals(scheduleIdentifier.Value))
                            schedule.NoStartBefore = null;

            // Add to job manager
            JobManager.Update(job, scheduleIdentifier);

            // Recalculate
            BeginNewPlan();
        }

        /// <summary>
        /// Entfernt einen Auftrag.
        /// </summary>
        /// <param name="job">Der betroffene Auftrag.</param>
        public void DeleteJob(VCRJob job)
        {
            // Remove from job manager
            JobManager.Delete(job);

            // Recalculate
            BeginNewPlan();
        }

        /// <summary>
        /// Rekonstruiert einen Auftrag und eine Aufzeichnung aus einer Textdarstellung.
        /// </summary>
        /// <param name="id">Die Textdarstellung.</param>
        /// <param name="job">Der ermittelte Auftrag.</param>
        /// <returns>Die zugeh�rige Aufzeichnung im Auftrag.</returns>
        public VCRSchedule? ParseUniqueWebId(string id, out VCRJob job)
        {
            ServerTools.ParseUniqueWebId(id, out Guid jobID, out Guid scheduleID);

            // Find the job
            job = FindJob(jobID)!;

            // Report schedule if job exists
            if (job == null)
                return null;
            else
                return job[scheduleID];
        }

    }
}
