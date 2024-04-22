using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Services;

namespace JMS.DVB.NET.Recording.Planning
{
    /// <summary>
    /// Berechnete die Aktualisierungszeiten für die Liste der Quellen.
    /// </summary>
    public class SourceListTask : PeriodicScheduler
    {
        /// <summary>
        /// Die Methode zur Ermittelung des letzten Aktualisierungszeitpunktes.
        /// </summary>
        private Func<DateTime?> m_LastUpdate;

        /// <summary>
        /// Das Verzeichnis, in dem temporäre Dateien während der Sammlung abgelegt werden können.
        /// </summary>
        public DirectoryInfo CollectorDirectory { get; private set; } = null!;

        private readonly IVCRConfiguration m_configuration;

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        /// <param name="forResource">Das Geräteprofil, auf dem der Lauf erfolgen soll.</param>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        public SourceListTask(IScheduleResource forResource, ProfileState profile, IVCRConfiguration configuration, IJobManager jobs)
            : this(forResource, profile, () => profile.LastSourceUpdateTime, configuration, jobs)
        {
        }

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        /// <param name="forResource">Das Geräteprofil, auf dem der Lauf erfolgen soll.</param>
        /// <param name="lastUpdate">Methode zur Ermittelung des letzten Aktualisierungszeitpunktes.</param>
        public SourceListTask(IScheduleResource forResource, Func<DateTime?> lastUpdate, IVCRConfiguration configuration, IJobManager jobs)
            : this(forResource, null!, lastUpdate, configuration, jobs)
        {
        }

        /// <summary>
        /// Erzeugt eine neue Verwaltung.
        /// </summary>
        /// <param name="forResource">Das Geräteprofil, auf dem der Lauf erfolgen soll.</param>
        /// <param name="profile">Das zugehörige Geräteprofil.</param>
        /// <param name="lastUpdate">Methode zur Ermittelung des letzten Aktualisierungszeitpunktes.</param>
        /// <exception cref="ArgumentNullException">Der letzte Aktualisierungszeitpunkt ist nicht gesetzt.</exception>
        private SourceListTask(IScheduleResource forResource, ProfileState profile, Func<DateTime?> lastUpdate, IVCRConfiguration configuration, IJobManager jobs)
            : base("Sendersuchlauf", Guid.NewGuid())
        {
            // Validate
            if (forResource == null)
                throw new ArgumentNullException(nameof(forResource));
            if (lastUpdate == null)
                throw new ArgumentNullException(nameof(lastUpdate));

            // Remember
            m_configuration = configuration;
            m_Resources = [forResource];
            m_LastUpdate = lastUpdate;

            // Set the job directory
            if (profile != null)
                CollectorDirectory = jobs.CollectorDirectory;
        }

        /// <summary>
        /// Meldet das Gerät, für das diese Sammlung erfolgt.
        /// </summary>
        private readonly IScheduleResource[] m_Resources;

        /// <summary>
        /// Meldet das Gerät, für das diese Sammlung erfolgt.
        /// </summary>
        public override IScheduleResource[] Resources => m_Resources;

        /// <summary>
        /// Meldet, ob die Ausführung grundsätzlich aktiviert ist.
        /// </summary>
        public override bool IsEnabled
        {
            get
            {
                // Report
                if (m_configuration.SourceListUpdateDuration < 1)
                    return false;
                else if (m_configuration.SourceListUpdateInterval == 0)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Meldet die maximale Dauer einer Ausführung.
        /// </summary>
        public override TimeSpan Duration => TimeSpan.FromMinutes(m_configuration.SourceListUpdateDuration);

        /// <summary>
        /// Meldet wenn möglich den Zeitpunkt, an dem letztmalig ein Durchlauf
        /// stattgefunden hat.
        /// </summary>
        public override DateTime? LastRun => m_LastUpdate();

        /// <summary>
        /// Meldet die Zeitspanne nach der ein neuer Durchlauf gestartet werden darf,
        /// wenn der Computer sowieso gerade aktiv ist.
        /// </summary>
        public override TimeSpan? JoinThreshold => m_configuration.HasRecordedSomething ? m_configuration.SourceListJoinThreshold : null;

        /// <summary>
        /// Meldet die Zeitspanne, die mindestens zwischen zwei Durchläufen liegen soll.
        /// </summary>
        public override TimeSpan DefaultInterval
        {
            get
            {
                // Check for manual update mode
                var days = m_configuration.SourceListUpdateInterval;
                if (days < 1)
                    return TimeSpan.MaxValue;

                // There is exactly one hour we prefer do the best to match it
                if (m_configuration.SourceListUpdateHours.Length == 1)
                    return TimeSpan.FromDays(days - 1) + new TimeSpan(1);

                // There are none or multiple preferred hours so just use the full interval normally skipping the last hour choosen
                return TimeSpan.FromDays(days);
            }
        }

        /// <summary>
        /// Meldet die bevorzugten Uhrzeiten für eine Ausführung. Die verwendeten Zeiten
        /// bezeichnen dabei Stunden in der lokalen Zeitzone.
        /// </summary>
        public override uint[] PreferredHours => m_configuration.SourceListUpdateHours;
    }
}
