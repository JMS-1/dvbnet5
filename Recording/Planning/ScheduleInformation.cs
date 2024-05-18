using JMS.DVB.Algorithms.Scheduler;

namespace JMS.DVB.NET.Recording.Planning
{
    /// <summary>
    /// Beschreibt eine aktive Aufzeichnung.
    /// </summary>
    public class ScheduleInformation
    {
        /// <summary>
        /// Die originalen Informationen.
        /// </summary>
        public IScheduleInformation Schedule { get; private set; }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="original">Die originalen Informationen.</param>
        public ScheduleInformation(IScheduleInformation original)
        {
            // Validate
            ArgumentNullException.ThrowIfNull(original);

            // Remember
            RealTime = original.Time;
            Schedule = original;
        }

        /// <summary>
        /// Meldet oder ändert den Zeitraum der Aufzeichnung.
        /// </summary>
        public PlannedTime RealTime { get; set; }
    }
}
