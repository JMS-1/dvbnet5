using JMS.DVB.Algorithms.Scheduler;

namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Prüft das Einmischen periodischer Aufgaben.
    /// </summary>
    [TestFixture]
    public class TaskSpecs
    {
        /// <summary>
        /// Simuliert eine periodische Aufgabe.
        /// </summary>
        private class _TaskMock : PeriodicScheduler
        {
            /// <summary>
            /// Alle Geräte, die diese Aufgabe ausführen können.
            /// </summary>
            private IScheduleResource[] m_Resources;

            /// <summary>
            /// Erzeugt eine neue Simulation.
            /// </summary>
            /// <param name="resource">Das zu verwendende Gerät.</param>
            public _TaskMock(IScheduleResource resource)
                : base("task", Guid.NewGuid())
            {
                // Remember
                m_Resources = [resource];
            }

            /// <summary>
            /// Meldet alle Geräte, die verwendet werden dürfen, um die Aufgabe zu erledigen.
            /// </summary>
            public override IScheduleResource[] Resources =>
                    // Report
                    m_Resources;
            /// <summary>
            /// Meldet die Zeitspanne zwischen zwei Läufen.
            /// </summary>
            public override TimeSpan DefaultInterval =>
                    // Not set
                    new TimeSpan(1);

            /// <summary>
            /// Meldet die maximale Dauer eines Laufs.
            /// </summary>
            public override TimeSpan Duration =>
                    // Report
                    TimeSpan.FromMinutes(20);

            /// <summary>
            /// Meldet, ob eine Bearbeitung erwünscht ist.
            /// </summary>
            public override bool IsEnabled =>
                    // Make no sense if not
                    true;

            /// <summary>
            /// Meldet, ob eine vorzeigtig Aktualisierung unterstützt wird.
            /// </summary>
            public override TimeSpan? JoinThreshold =>
                    // Nope
                    null;

            /// <summary>
            /// Meldet den Zeitpunkt der letzten Ausführung.
            /// </summary>
            public override DateTime? LastRun =>
                    // Not needed
                    null;

            /// <summary>
            /// Meldet die Stunden, an denen eine Aktualisierung erlaubt ist. Die Angabe bezieht
            /// sich auf Zeiten in der lokalen Zeitzone.
            /// </summary>
            public override uint[] PreferredHours => [10, 20];
        }

        /// <summary>
        /// Das Gerät, an das die Aufgabe gebunden wird.
        /// </summary>
        private readonly IScheduleResource TaskDevice = ResourceMock.Device1;

        /// <summary>
        /// Ein anderes Gerät.
        /// </summary>
        private readonly IScheduleResource OtherDevice = ResourceMock.Device2;

        /// <summary>
        /// Die vordefinierte Aufgabe, die jeden Tag bevorzug um 10 Uhr Morgens und 8 Uhr Abends für
        /// 20 Minuten ausgeführt wird.
        /// </summary>
        private readonly IScheduleDefinition Task;

        /// <summary>
        /// Die für diesen Test verwendete Uhrzeit.
        /// </summary>
        private static readonly DateTime TimeBias = new(2011, 9, 11, 15, 51, 13, DateTimeKind.Utc);

        /// <summary>
        /// Erzeugt eine neue Testumgebung.
        /// </summary>
        public TaskSpecs()
        {
            // Finish
            Task = new _TaskMock(TaskDevice);
        }

        /// <summary>
        /// Solange keine Aufzeichnungen aktiv sind meldet die Planung nur die Zeiten 
        /// der periodischen Aufgaben.
        /// </summary>
        [Test]
        public void Scheduler_Reports_Task_Times_If_No_Recording_Is_Available()
        {
            // Create the component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { TaskDevice, OtherDevice };

            // Load some
            var schedules = cut.GetSchedules(TimeBias, Task).Take(100).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(100), "Schedules");
            Assert.That(schedules[0].Time.Start, Is.EqualTo(TimeBias), "Start 0");
            Assert.That(schedules[99].Time.Start, Is.EqualTo(TimeBias.ToLocalTime().Date.AddDays(49).AddHours(20).ToUniversalTime()), "Start 99");
        }

        /// <summary>
        /// Eine Aufzeichnung wird immer höher bewertet als eine Aufgabe.
        /// </summary>
        [Test]
        public void Recording_Has_Priority_Over_Task()
        {
            // Create the recording
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddMinutes(15), TimeSpan.FromMinutes(80));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours(3), TimeSpan.FromMinutes(100));

            // Create the component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { TaskDevice, plan1, plan2 };

            // Load some
            var schedules = cut.GetSchedules(TimeBias, Task).Take(5).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(5), "Schedules");
            Assert.That(schedules[0].Definition, Is.SameAs(plan1), "Definition 1");
            Assert.That(schedules[0].StartsLate, Is.False, "Late 1");
            Assert.That(schedules[1].Definition, Is.SameAs(Task), "Definition 2");
            Assert.That(schedules[1].StartsLate, Is.True, "Late 2");
            Assert.That(schedules[2].Definition, Is.SameAs(Task), "Definition 3");
            Assert.That(schedules[2].StartsLate, Is.False, "Late 3");
            Assert.That(schedules[2].Time.Start, Is.EqualTo(TimeBias.ToLocalTime().Date.AddHours(20).ToUniversalTime()), "Start 3");
            Assert.That(schedules[3].Definition, Is.SameAs(plan2), "Definition 4");
            Assert.That(schedules[3].StartsLate, Is.False, "Late 4");
            Assert.That(schedules[4].Definition, Is.SameAs(Task), "Definition 5");
            Assert.That(schedules[4].StartsLate, Is.False, "Late 5");
            Assert.That(schedules[4].Time.Start, Is.EqualTo(TimeBias.ToLocalTime().Date.AddDays(1).AddHours(10).ToUniversalTime()), "Start 5");
        }
    }
}
