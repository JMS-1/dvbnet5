using JMS.DVB.NET.Recording.Persistence;

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
    }
}
