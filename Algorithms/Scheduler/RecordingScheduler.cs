using System.Collections;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Diese Klasse übernimmt die Planung von Aufzeichnungen.
    /// </summary>
    public partial class RecordingScheduler : IEnumerable
    {
        /// <summary>
        /// Die maximale Anzahl von Aufzeichnungen, die in einem Rutsch geplant werden.
        /// </summary>
        public static uint MaximumRecordingsInPlan = 1000;

        /// <summary>
        /// Die maximale Anzahl gleichzeitig untersucher Alternativlösungen.
        /// </summary>
        public static uint MaximumAlternativesInPlan = 1000;

        /// <summary>
        /// Alle Geräte, die bei der Planung zu berücksichtigen sind.
        /// </summary>
        internal ResourceCollection Resources { get; private set; }

        /// <summary>
        /// Alle Aufzeichungen und Aufgaben, die bei der Planung nicht berücksichtigt werden sollen.
        /// </summary>
        private HashSet<Guid> m_ForbiddenDefinitions;

        /// <summary>
        /// Methode zur Erzeugung des initialen Ablaufplans.
        /// </summary>
        private Func<SchedulePlan> m_PlanCreator;

        /// <summary>
        /// Erzeugt eine neue Planungsinstanz.
        /// </summary>
        /// <param name="resources">Die zu verwendenden Geräte.</param>
        /// <param name="forbiddenDefinitions">Alle Aufzeichnungen und Aufgaben, die nicht untersucht werden sollen.</param>
        /// <param name="planCreator">Optional eine Methode zur Erzeugung des initialen Ablaufplans.</param>
        /// <param name="comparer">Der volle Pfad zur Regeldatei.</param>
        /// <exception cref="ArgumentNullException">Ein Parameter wurde nicht angegeben.</exception>
        internal RecordingScheduler(ResourceCollection resources, HashSet<Guid> forbiddenDefinitions, Func<SchedulePlan> planCreator, IComparer<SchedulePlan> comparer)
        {
            // Validate
            ArgumentNullException.ThrowIfNull(resources);
            ArgumentNullException.ThrowIfNull(forbiddenDefinitions);
            ArgumentNullException.ThrowIfNull(comparer);

            // Finish
            m_PlanCreator = planCreator ?? (() => new SchedulePlan(Resources!));
            m_ForbiddenDefinitions = forbiddenDefinitions;
            m_comparer = comparer;
            Resources = resources;
        }

        /// <summary>
        /// Erzeugt eine neue Planungsinstanz.
        /// </summary>
        /// <param name="nameComparer">Der Algorithmus zum Vergleich von Gerätenamen.</param>
        /// <param name="rulePath">Der volle Pfad zur Regeldatei.</param>
        public RecordingScheduler(IEqualityComparer<string> nameComparer, string rulePath = null!)
            : this(nameComparer, string.IsNullOrEmpty(rulePath) ? null! : File.ReadAllBytes(rulePath))
        {
        }

        /// <summary>
        /// Erzeugt eine neue Planungsinstanz.
        /// </summary>
        /// <param name="nameComparer">Der Algorithmus zum Vergleich von Gerätenamen.</param>
        /// <param name="rules">Der Inhalt der Regeldatei.</param>
        public RecordingScheduler(IEqualityComparer<string> nameComparer, byte[] rules)
            : this([], [], null!, (rules == null) ? CustomComparer.Default(nameComparer) : CustomComparer.Create(rules, nameComparer))
        {
        }

        /// <summary>
        /// Meldet ein Gerät zur Verwendung an.
        /// </summary>
        /// <param name="resource">Das gewünschte Gerät.</param>
        public void Add(IScheduleResource resource) => Resources.Add(resource);

        /// <summary>
        /// Meldet komplete Entschlüsselungsregeln an.
        /// </summary>
        /// <param name="group">Eine neue Regel.</param>
        /// <exception cref="ArgumentNullException">Die Regel ist ungültig.</exception>
        public void Add(DecryptionGroup group) => Resources.Add(group);

        #region IEnumerable Members

        /// <summary>
        /// Simuliert eine Auflistung.
        /// </summary>
        /// <returns>Die gewünschte Simulation.</returns>
        IEnumerator IEnumerable.GetEnumerator() => Enumerable.Empty<object>().GetEnumerator();

        #endregion
    }
}
