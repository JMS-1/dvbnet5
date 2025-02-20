﻿using JMS.DVB.Algorithms.Scheduler;

namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Prüft den Umgang mit der Entschlüsselung von Quellen.
    /// </summary>
    [TestFixture]
    public class DecryptionSpecs
    {
        /// <summary>
        /// Das in diesem Test verwendete erste Gerät mit Entschlüsselungsfunktion.
        /// </summary>
        public static readonly IScheduleResource PayTVDevice1 = ResourceMock.Device2;

        /// <summary>
        /// Das in diesem Test verwendete zweite Gerät mit Entschlüsselungsfunktion.
        /// </summary>
        public static readonly IScheduleResource PayTVDevice2 = ResourceMock.Device3;

        /// <summary>
        /// Das in diesem Test verwendete erste Gerät ohne Entschlüsselungsfunktion.
        /// </summary>
        public static readonly IScheduleResource FreeTVDevice = ResourceMock.Device1;

        /// <summary>
        /// Der Bezugspunkt für alle Zeitmessungen.
        /// </summary>
        public static readonly DateTime TimeBias = new(2011, 9, 9, 22, 44, 59, DateTimeKind.Utc);

        /// <summary>
        /// Eine verschlüsselte Aufzeichnung wird überprungen, wenn sie nicht entschlüsselt werden kann.
        /// </summary>
        [Test]
        public void Will_Skip_Recording_If_Unable_To_Decrypt()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test1", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours(1), TimeSpan.FromMinutes(100));
            var plan2 = RecordingDefinition.Create(false, "test2", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours(2), TimeSpan.FromMinutes(100));

            // Create component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { FreeTVDevice, plan1, plan2 };

            // Process
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(2), "Schedules");
            Assert.That(schedules[0].Resource, Is.Null, "Resource 1");
            Assert.That(schedules[1].Resource, Is.SameAs(FreeTVDevice), "Resource 2");
            Assert.That(schedules[1].StartsLate, Is.False, "Late 2");
        }

        /// <summary>
        /// Verschlüsselte Aufzeichnungen werden auf zwei Geräte verteilt, selbst wenn
        /// sie von der selben Quellgruppe stammen.
        /// </summary>
        [Test]
        public void Two_Decrypted_Recordings_On_Same_Group_Require_Two_Decryption_Devices()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours(1), TimeSpan.FromMinutes(100));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source4Group1Pay, TimeBias.AddHours(2), TimeSpan.FromMinutes(10));

            // Create component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { PayTVDevice1, PayTVDevice2, plan1, plan2 };

            // Process
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(2), "Schedules");
            Assert.That(schedules[0].Resource, Is.Not.Null, "Resource 1");
            Assert.That(schedules[1].Resource, Is.Not.Null, "Resource 2");
            Assert.That(schedules[1].Resource, Is.Not.SameAs(schedules[0].Resource), "Resources");
        }

        /// <summary>
        /// Ein höher priorisiertes Geräte wird nicht verwendet, wenn es keine Entschlüsselung unterstützt.
        /// </summary>
        [Test]
        public void High_Priority_Device_Will_Be_Ignored_If_Not_Able_To_Decrypt()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours(1), TimeSpan.FromMinutes(100));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source4Group1Pay, TimeBias.AddHours(2), TimeSpan.FromMinutes(10));

            // Attach to the free device
            var device = (ResourceMock)FreeTVDevice;
            var prio = device.AbsolutePriority;

            // With reset
            try
            {
                // Create component under test
                var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { device.SetPriority(100), PayTVDevice1, PayTVDevice2, plan1, plan2 };

                // Process
                var schedules = cut.GetSchedules(TimeBias).ToArray();

                // Validate
                Assert.That(schedules.Length, Is.EqualTo(2), "Schedules");
                Assert.That(schedules[0].Resource, Is.Not.Null, "Resource 1");
                Assert.That(schedules[0].Resource, Is.Not.SameAs(device), "Resource 1 Free");
                Assert.That(schedules[1].Resource, Is.Not.Null, "Resource 2");
                Assert.That(schedules[1].Resource, Is.Not.SameAs(device), "Resource 2 Free");
                Assert.That(schedules[1].Resource, Is.Not.SameAs(schedules[0].Resource), "Resources");
            }
            finally
            {
                // Back to normal
                device.SetPriority(prio);
            }
        }

        /// <summary>
        /// Eine Verschlüsselungsgruppe kann verhindert, dass zwei Entschlüsselungen auf verschiedenen Geräten
        /// gleichzeitig abgearbeitet werden.
        /// </summary>
        [Test]
        public void Decryption_Group_May_Forbid_Using_Two_Resources_At_The_Same_Time()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours(1), TimeSpan.FromMinutes(100));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source4Group1Pay, TimeBias.AddHours(2), TimeSpan.FromMinutes(100));

            // Create group
            var group =
                new DecryptionGroup
                {
                    ScheduleResources = [PayTVDevice1, PayTVDevice2],
                    MaximumParallelSources = 1,
                };

            // Create component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { PayTVDevice1, PayTVDevice2, plan1, plan2, group };

            // Process
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(2), "Schedules");
            Assert.That(schedules[0].StartsLate, Is.False, "Late 1");
            Assert.That(schedules[1].StartsLate, Is.True, "Late 2");
        }

        /// <summary>
        /// Jede entschlüsselte Quelle wird zu jedem Zeitpunkt nur einmal berücksichtigt.
        /// </summary>
        [Test]
        public void Same_Source_Does_Not_Require_Additional_Decyrption_Slot()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours(1), TimeSpan.FromMinutes(100));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours(2), TimeSpan.FromMinutes(100));

            // Create component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { PayTVDevice1, plan1, plan2 };

            // Process
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(2), "Schedules");
            Assert.That(schedules[0].StartsLate, Is.False, "Late 1");
            Assert.That(schedules[1].StartsLate, Is.False, "Late 2");
        }

        /// <summary>
        /// Eine Verschlüsselungsgruppe kann erlauben, dass zwei Entschlüsselungen auf verschiedenen Geräten
        /// gleichzeitig abgearbeitet werden.
        /// </summary>
        [Test]
        public void Decryption_Group_May_Allow_Using_Two_Resources_At_The_Same_Time()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours(1), TimeSpan.FromMinutes(100));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source4Group1Pay, TimeBias.AddHours(2), TimeSpan.FromMinutes(100));

            // Create group
            var group =
                new DecryptionGroup
                {
                    ScheduleResources = [PayTVDevice1, PayTVDevice2],
                    MaximumParallelSources = 2,
                };

            // Create component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { PayTVDevice1, PayTVDevice2, plan1, plan2, group };

            // Process
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(2), "Schedules");
            Assert.That(schedules[0].StartsLate, Is.False, "Late 1");
            Assert.That(schedules[1].StartsLate, Is.False, "Late 2");
        }

        /// <summary>
        /// Bei unglücklicher Wahl der Aufzeichnungen müssen drei Geräte verwendet werden.
        /// </summary>
        [Test]
        public void May_Require_Three_Resources_On_Bad_Mixture_Of_Three_Recordings()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours(1), TimeSpan.FromMinutes(100));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source4Group1Pay, TimeBias.AddHours(2), TimeSpan.FromMinutes(100));
            var plan3 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group2Free, TimeBias.AddHours(2).AddMinutes(10), TimeSpan.FromMinutes(100));

            // Create component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { FreeTVDevice, PayTVDevice1, PayTVDevice2, plan1, plan2, plan3 };

            // Process
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(3), "Schedules");
            Assert.That(schedules[0].Resource, Is.Not.SameAs(FreeTVDevice), "Resource 1");
            Assert.That(schedules[1].Resource, Is.Not.SameAs(FreeTVDevice), "Resource 2");
            Assert.That(schedules[2].Resource, Is.SameAs(FreeTVDevice), "Resource 3");
        }

        /// <summary>
        /// Sind Geräte durch Entschlüsselungen unterschiedlich blockiert, so muss eine folgende freie
        /// Aufzeichnung auf dem Gerät mit dem kleinsten Zeitverlust aufgezeichnet werdenm egal, welche 
        /// Priorität den Geräten zugeordnet ist.
        /// </summary>
        [Test]
        public void Free_Recording_Must_Use_Best_Fit_Resource()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source5Group1Pay, TimeBias.AddHours(1), TimeSpan.FromMinutes(100));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source4Group1Pay, TimeBias.AddHours(2), TimeSpan.FromMinutes(100));
            var plan3 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group2Free, TimeBias.AddHours(2).AddMinutes(10), TimeSpan.FromMinutes(100));

            // Attach to the device
            var device = (ResourceMock)PayTVDevice1;
            var prio = device.AbsolutePriority;

            // Must reset
            try
            {
                // Create component under test but make the device to choose the one with the least priority
                var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { device.SetPriority(-100), PayTVDevice2, plan1, plan2, plan3 };

                // Process
                var schedules = cut.GetSchedules(TimeBias).ToArray();

                // Validate
                Assert.That(schedules.Length, Is.EqualTo(3), "Schedules");
                Assert.That(schedules[2].Resource, Is.SameAs(device), "Resource");
            }
            finally
            {
                // Reset
                device.SetPriority(prio);
            }
        }

        /// <summary>
        /// Eine Aufzeichnungsplanung kommt durcheinander, wenn eine Aufzeichnung gestartet wird.
        /// </summary>
        [Test]
        public void Bad_Planning_After_Recording_Start()
        {
            // Create component under test
            using (var rm = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                var now = DateTime.Now.Date.AddDays(10);

                var source1 = SourceMock.Create("s1");
                var source2 = SourceMock.Create("s2");
                var source3 = SourceMock.Create("s3");

                var dev1 = ResourceMock.Create("dev1", source1, source2, source3);
                var dev2 = ResourceMock.Create("dev2", source1, source2, source3);

                var id1 = Guid.NewGuid();
                var start1 = now.AddHours(11).AddMinutes(40);
                var dur1 = TimeSpan.FromMinutes(15);

                var plan1 = RecordingDefinition.Create(false, "test1", id1, null, source1, start1, dur1);
                var plan2 = RecordingDefinition.Create(false, "test2", Guid.NewGuid(), null, source2, now.AddHours(11).AddMinutes(45), TimeSpan.FromMinutes(15));
                var plan3 = RecordingDefinition.Create(false, "test3", Guid.NewGuid(), null, source3, now.AddHours(11).AddMinutes(50), TimeSpan.FromMinutes(15));

                rm.Add(dev1);
                rm.Add(dev2);

                Assert.That(rm.Start(dev2, source1, id1, "test1", start1, start1 + dur1), Is.True);

                var cut = rm.CreateScheduler(false);
                cut.Add(plan1);
                cut.Add(plan2);
                cut.Add(plan3);

                var schedules = cut.GetSchedules(start1.AddMinutes(5).AddTicks(1)).Where(s => s.Definition.UniqueIdentifier != id1).ToArray();

                Assert.That(schedules.Length, Is.EqualTo(2), "#schedules");
                Assert.That(schedules[0].Definition.Name, Is.EqualTo("test2"), "1");
                Assert.That(schedules[1].Definition.Name, Is.EqualTo("test3"), "2");
            }
        }
    }
}
