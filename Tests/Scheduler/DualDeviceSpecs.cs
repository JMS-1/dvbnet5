﻿using JMS.DVB.Algorithms.Scheduler;

namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Arbeitet mit zwei Geräten.
    /// </summary>
    [TestFixture]
    public class DualDeviceSpecs
    {
        /// <summary>
        /// Das erste Gerät.
        /// </summary>
        public static readonly IScheduleResource Device1 = ResourceMock.Device1;

        /// <summary>
        /// Das zweite Gerät.
        /// </summary>
        public static readonly IScheduleResource Device2 = ResourceMock.Device2;

        /// <summary>
        /// Der Bezugspunkt für alle Zeitmessungen.
        /// </summary>
        public static readonly DateTime TimeBias = new(2011, 10, 22, 12, 19, 27, DateTimeKind.Utc);

        /// <summary>
        /// Eine Aufzeichnung kann in einen vorherigen Planungsblock aufgenommen werden.
        /// </summary>
        [Test]
        public void Can_Join_Recordings_Even_If_Planning_Creates_Separate_Blocks()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "A1", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddMinutes(60), TimeSpan.FromMinutes(120));
            var plan2 = RecordingDefinition.Create(false, "A2", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddMinutes(165), TimeSpan.FromMinutes(60));
            var plan3 = RecordingDefinition.Create(false, "B1", Guid.NewGuid(), null, SourceMock.Source1Group2Free, TimeBias.AddMinutes(150), TimeSpan.FromMinutes(120));
            var plan4 = RecordingDefinition.Create(false, "C1", Guid.NewGuid(), null, SourceMock.Source1Group3Free, TimeBias.AddMinutes(120), TimeSpan.FromMinutes(120));

            // Create component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { Device1, Device2, plan1, plan2, plan3, plan4 };

            // Resolve
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(4), "Schedule");

            // Process all
            foreach (var schedule in schedules)
                Assert.That(schedule.StartsLate, Is.EqualTo(ReferenceEquals(schedule.Definition, plan3)), $"Late {schedule.Definition.Name}");
        }
    }
}
