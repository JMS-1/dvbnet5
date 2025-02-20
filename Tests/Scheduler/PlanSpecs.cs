﻿using JMS.DVB.Algorithms.Scheduler;


namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Prüft das Erzeugen von Aufzeichnungsinformationen.
    /// </summary>
    [TestFixture]
    public class PlanSpecs
    {
        /// <summary>
        /// Es ist nicht möglich, eine Aufzeichnung ohne Quelle anzulegen. 
        /// </summary>
        [Test]
        public void A_Plan_Item_Must_Have_A_Source()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Process
                RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, null, DateTime.UtcNow, TimeSpan.FromMinutes(10));
            });
        }

        /// <summary>
        /// Es kann keine Aufzeichnung vor dem Jahr 2011 geplant werden.
        /// </summary>
        [Test]
        public void A_Plan_Item_Must_Not_Be_Older_Than_2000()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // Process
                RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("s1"), new DateTime(1999, 2, 13, 0, 0, 0, DateTimeKind.Utc), TimeSpan.FromMinutes(10));
            });
        }

        /// <summary>
        /// Die Dauer einer Aufzeichnung darf nicht <i>0</i> sein.
        /// </summary>
        [Test]
        public void Plan_Item_Duration_Must_Not_Be_Zero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // Process
                RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("s1"), DateTime.UtcNow, TimeSpan.Zero);
            });
        }

        /// <summary>
        /// Die Dauer einer Aufzeichnung darf nicht negativ sein.
        /// </summary>
        [Test]
        public void Plan_Item_Duration_Must_Not_Be_Negative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // Process
                RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("s1"), DateTime.UtcNow, new TimeSpan(-1));
            });
        }

        /// <summary>
        /// Die Daten einer Aufzeichnung können wieder abgefragt werden.
        /// </summary>
        [Test]
        public void Plan_Item_Remembers_Settings()
        {
            // Data under test
            var duration = TimeSpan.FromMinutes(45);
            var source = SourceMock.Create("s1");
            var start = DateTime.UtcNow;

            // Process
            var componentUnderTest = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, source, start, duration);

            // Validate
            Assert.That(componentUnderTest, Is.Not.Null, "Create");
            Assert.That(componentUnderTest.Source, Is.SameAs(source), "Source");

            // Load plan
            var times = componentUnderTest.GetTimes(DateTime.MinValue).Select(s => s.Planned).ToArray();

            // Validate
            Assert.That(times.Length, Is.EqualTo(1), "Times");
            Assert.That(times[0].Start, Is.EqualTo(start), "Start");
            Assert.That(times[0].Duration, Is.EqualTo(duration), "Duration");
        }

        /// <summary>
        /// Es ist nicht möglich, eine <i>null</i> Aufzeichnung anzumelden.
        /// </summary>
        [Test]
        public void Can_Not_Add_A_Null_Plan_Item()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {

                // Create component under test
                var componentUnderTest = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase)
                    { default(IRecordingDefinition)! };
            });
        }

        /// <summary>
        /// Die Quelle einer Aufzeichnung muss mindestens von einem Gerät unterstützt
        /// werden.
        /// </summary>
        [Test]
        public void Encrypted_Source_Of_A_Plan_Item_Is_Supported_By_Resource()
        {
            // Create component under test
            var componentUnderTest = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase)
            {
                // Add
                ResourceMock.Create("r1", SourceMock.Create("s1")).SetEncryptionLimit(1),
                RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("s1", true), DateTime.UtcNow, TimeSpan.FromMinutes(12))
            };
        }

        /// <summary>
        /// Die Zeitangaben von Ausnahmeregeln dürfen keine Uhrzeit enthalten.
        /// </summary>
        [Test]
        public void Exceptions_Must_Be_Defined_On_A_Full_Date()
        {
            Assert.Throws<ArgumentException>(() =>
            {

                // Create bad exception
                var exception = new PlanException { ExceptionDate = DateTime.Now.Date.AddMinutes(12) };

                // Create component under test
                var componentUnderTest = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase)
                {
                    // Add
                    ResourceMock.Create("r1", SourceMock.Create("s1")),
                    { RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("s1"), DateTime.UtcNow, TimeSpan.FromMinutes(12)), exception }
                };
            });
        }

        /// <summary>
        /// Ausnahmeregeln dürfen pro Tag nur einmal definiert werden.
        /// </summary>
        [Test]
        public void There_Can_Be_Only_One_Exception_Per_Date()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Create bad exception
                var exception1 = new PlanException { ExceptionDate = DateTime.Now.Date };
                var exception2 = new PlanException { ExceptionDate = exception1.ExceptionDate };

                // Create component under test
                var componentUnderTest = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase)
                {
                    // Add
                    ResourceMock.Create("r1", SourceMock.Create("s1")),
                    { RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("s1"), DateTime.UtcNow, TimeSpan.FromMinutes(12)), exception1, exception2 }
                };
            });
        }

        /// <summary>
        /// Das Ende einer sich wiederholenden Aufzeichnung muss als Datum angegeben werden.
        /// </summary>
        [Test]
        public void End_Of_Repeating_Recording_Must_Be_A_Date()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Process
                RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("a"), DateTime.UtcNow, TimeSpan.FromMinutes(10), DateTime.Now.Date.AddMinutes(1), DayOfWeek.Monday);
            });
        }

        /// <summary>
        /// Das Ende einer sich wiederholenden Aufzeichnung muss nach dem Beginn liegen.
        /// </summary>
        [Test]
        public void End_Of_Repeating_Recording_Must_Be_In_The_Future()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Process
                RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("a"), DateTime.UtcNow, TimeSpan.FromMinutes(10), DateTime.Now.Date.AddYears(-1), DayOfWeek.Monday);
            });
        }

        /// <summary>
        /// Das Wiederholdungsmuster muss mindestens einen Wochentag enthalten.
        /// </summary>
        [Test]
        public void Repeat_Must_Have_At_Least_One_Day()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Process
                RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("a"), DateTime.UtcNow, TimeSpan.FromMinutes(10), DateTime.Now.Date.AddYears(1));
            });
        }

        /// <summary>
        /// Das Wiederholdungsmuster darf jeden Wochentag nur einmal enthalten.
        /// </summary>
        [Test]
        public void Repeat_Must_Not_Use_A_Day_Twice()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Process
                RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("a"), DateTime.UtcNow, TimeSpan.FromMinutes(10), DateTime.Now.Date.AddYears(1), DayOfWeek.Monday, DayOfWeek.Saturday, DayOfWeek.Monday);
            });
        }

        /// <summary>
        /// Eine Aufzeichnung kann sich wiederholen.
        /// </summary>
        [Test]
        public void A_Plan_Can_Have_A_Repeating_Pattern()
        {
            // Create the plan
            var localStart = new DateTime(2011, 9, 8, 21, 35, 0, DateTimeKind.Local);
            var localEnd = new DateTime(2012, 3, 31, 0, 0, 0, DateTimeKind.Local);
            var gmtStart = localStart.ToUniversalTime();
            var plan = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("s1"), gmtStart, TimeSpan.FromMinutes(10), localEnd.Date, DayOfWeek.Monday, DayOfWeek.Friday, DayOfWeek.Tuesday);

            // Cross check map
            var allowed = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Friday, DayOfWeek.Tuesday };

            // Scan all
            foreach (var time in plan.GetTimes(gmtStart.AddYears(-1)).Select(s => s.Planned))
            {
                // Get next good time
                while (!allowed.Contains(localStart.DayOfWeek))
                    localStart = localStart.AddDays(1);

                // Test it
                Assert.That(time.LocalStart, Is.EqualTo(localStart));
                Assert.That(time.Duration, Is.EqualTo(TimeSpan.FromMinutes(10)));

                // Next day
                localStart = localStart.AddDays(1);
            }

            // Check end
            Assert.That(localStart, Is.EqualTo(localEnd + localStart.TimeOfDay));
        }

        /// <summary>
        /// Für eine Aufzeichnung kann festgelegt werden, dass sie nur auf bestimmten Geräten ausgeführt werden kann.
        /// </summary>
        [Test]
        public void A_Plan_Can_Have_A_List_Of_Preferred_Resources()
        {
            // Create environment
            var source = SourceMock.Create("A");
            var device1 = ResourceMock.Create("D1", source);
            var device2 = ResourceMock.Create("D2", source);
            var device3 = ResourceMock.Create("D3", source);

            // Create the plan
            var plan = RecordingDefinition.Create(false, "test", Guid.NewGuid(), new[] { device2 }, source, DateTime.UtcNow, TimeSpan.FromMinutes(10));

            // Create the component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { device1, device3, plan };

            // Get the schedule
            var schedule = cut.GetSchedules(DateTime.UtcNow.AddYears(-1)).Single();

            // Validate
            Assert.That(schedule.Resource, Is.Null, "Resource");
        }

        /// <summary>
        /// Auch sehr lange Aufzeichnungen mit sehr vielen Geräten können bearbeitet werden.
        /// </summary>
        [Test]
        public void Can_Handle_Very_Long_Recordings_With_Multiple_Devices()
        {
            // All sources
            var sources =
                Enumerable
                    .Range(0, 100)
                    .Select(i => SourceMock.Create("S" + i.ToString("00")))
                    .ToArray();

            // Create environment
            var device1 = ResourceMock.Create("D1", sources);
            var device2 = ResourceMock.Create("D2", sources);
            var device3 = ResourceMock.Create("D3", sources);
            var device4 = ResourceMock.Create("D4", sources);

            // Create the plan
            var allDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };
            var refTime = new DateTime(2013, 12, 1);
            var plan1 = RecordingDefinition.Create(false, "test1", Guid.NewGuid(), null, sources[0], refTime, TimeSpan.FromHours(23), refTime.AddYears(100), allDays);
            var plan2 = RecordingDefinition.Create(false, "test2", Guid.NewGuid(), null, sources[1], refTime.AddHours(22), TimeSpan.FromHours(4), refTime.AddYears(100), allDays);

            // Create the component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { device1, device2, device3, device4, plan1, plan2 };

            // Other
            foreach (var plan in
                Enumerable
                    .Range(2, 10)
                    .Select(i => RecordingDefinition.Create(false, "test" + i.ToString("00"), Guid.NewGuid(), null, sources[i], refTime.AddMinutes(5 * i), TimeSpan.FromMinutes(6), refTime.AddYears(100), allDays)))
                cut.Add(plan);

            // Get the schedule
            var schedules = cut.GetSchedules(refTime.AddHours(-1)).Take(500).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(500), "#count");
            Assert.That(schedules.Any(schedule => schedule.StartsLate), Is.False, "late!");

            // Dump plan
            foreach (var schedule in schedules)
                Console.WriteLine(schedule);
        }
    }
}
