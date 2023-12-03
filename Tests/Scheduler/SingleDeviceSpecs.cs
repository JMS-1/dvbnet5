using JMS.DVB.Algorithms.Scheduler;

namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Arbeitet mit einem einzelnen Gerät.
    /// </summary>
    [TestFixture]
    public class SingleDeviceSpecs
    {
        /// <summary>
        /// Das in diesem Test verwendete Gerät ohne Entschlüsselungsfunktion.
        /// </summary>
        public static readonly IScheduleResource FreeTVDevice = ResourceMock.Device1;

        /// <summary>
        /// Das in diesem Test verwendete Gerät mit Entschlüsselungsfunktion.
        /// </summary>
        public static readonly IScheduleResource PayTVDevice = ResourceMock.Device2;

        /// <summary>
        /// Der Bezugspunkt für alle Zeitmessungen.
        /// </summary>
        public static readonly DateTime TimeBias = new DateTime(2011, 9, 7, 22, 28, 13, DateTimeKind.Utc);

        /// <summary>
        /// Ein verschlüsselter Sender kann nur auf einem Gerät mit Entschlüsselung ausgeführt werden.
        /// </summary>
        [Test]
        public void Decrypted_Source_Requires_Decryption_Resource()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test1", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours(1), TimeSpan.FromMinutes(90));
            var plan2 = RecordingDefinition.Create(false, "test2", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours(2), TimeSpan.FromMinutes(90));

            // Create component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { FreeTVDevice, plan1, plan2 };

            // Resolve
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(2), "Schedule");
            Assert.That(schedules[0].Resource, Is.Null, "Resource 1");
            Assert.That(schedules[1].Resource, Is.SameAs(FreeTVDevice), "Resource 2");
        }

        /// <summary>
        /// Ein verschlüsselter Sender kann nur auf einem Gerät mit Entschlüsselung ausgeführt werden.
        /// </summary>
        [Test]
        public void Decrypted_Source_Will_Run_On_Decryption_Device()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours(1), TimeSpan.FromMinutes(90));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Pay, TimeBias.AddHours(2), TimeSpan.FromMinutes(90));
            var plan3 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddHours(3), TimeSpan.FromMinutes(90));

            // Create component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { PayTVDevice, plan1, plan2, plan3 };

            // Resolve
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(3), "Schedule");
            Assert.That(schedules[0].Resource, Is.SameAs(PayTVDevice), "Resource 1");
            Assert.That(schedules[1].Resource, Is.SameAs(PayTVDevice), "Resource 2");
            Assert.That(schedules[2].Resource, Is.SameAs(PayTVDevice), "Resource 3");
        }

        /// <summary>
        /// Zwei Aufzeichnungen, die sich auf die selbe Quellgruppe beziehen, können simulan ausgeführt 
        /// werden.
        /// </summary>
        [Test]
        public void Two_Recordings_Of_The_Same_Group_Can_Be_Recorded_Simultanously()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours(1), TimeSpan.FromMinutes(90));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddHours(2), TimeSpan.FromMinutes(60));

            // Create component under test
            var cut =
                new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase)
                    {
                        { FreeTVDevice },
                        { plan1 },
                        { plan2 },
                    };

            // Resolve
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(2), "Schedules");
            Assert.That(schedules[0].Resource, Is.SameAs(FreeTVDevice), "Resource 1");
            Assert.That(schedules[1].Resource, Is.SameAs(FreeTVDevice), "Resource 2");
            Assert.That(schedules[0].Definition, Is.SameAs(plan1), "Plan 1");
            Assert.That(schedules[1].Definition, Is.SameAs(plan2), "Plan 2");
            Assert.That(schedules[0].StartsLate, Is.False, "Late 1");
            Assert.That(schedules[1].StartsLate, Is.False, "Late 2");
        }

        /// <summary>
        /// Drei Aufzeichnungen, die sich auf die selbe Quellgruppe beziehen, können simulan ausgeführt 
        /// werden.
        /// </summary>
        [Test]
        public void Unrestricted_Resource_Can_Serve_Three_Records_At_A_Time()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours(1), TimeSpan.FromMinutes(90));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddHours(2), TimeSpan.FromMinutes(70));
            var plan3 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source3Group1Free, TimeBias.AddHours(3), TimeSpan.FromMinutes(90));

            // Create component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { FreeTVDevice, plan1, plan2, plan3 };

            // Resolve
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(3), "Schedules");
            Assert.That(schedules[0].StartsLate, Is.False, "Late 1");
            Assert.That(schedules[1].StartsLate, Is.False, "Late 2");
            Assert.That(schedules[2].StartsLate, Is.False, "Late 3");
        }

        /// <summary>
        /// Drei Aufzeichnungen, die sich auf die selbe Quellgruppe beziehen, können simulan ausgeführt 
        /// werden.
        /// </summary>
        [Test]
        public void Restricted_Resource_Can_Not_Serve_Three_Records_At_A_Time()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours(1), TimeSpan.FromMinutes(90));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddHours(2), TimeSpan.FromMinutes(70));
            var plan3 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source3Group1Free, TimeBias.AddHours(2.25), TimeSpan.FromMinutes(90));

            // Load current
            var device = (ResourceMock)FreeTVDevice;
            var limit = device.SourceLimit;
            try
            {
                // Create component under test
                var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { device.SetSourceLimit(2), plan1, plan2, plan3 };

                // Resolve
                var schedules = cut.GetSchedules(TimeBias).ToArray();

                // Validate
                Assert.That(schedules.Length, Is.EqualTo(3), "Schedules");
                Assert.That(schedules[0].StartsLate, Is.False, "Late 1");
                Assert.That(schedules[1].StartsLate, Is.False, "Late 2");
                Assert.That(schedules[2].StartsLate, Is.True, "Late 3");
            }
            finally
            {
                // Reset
                device.SetSourceLimit(limit);
            }
        }

        /// <summary>
        /// Zwei Aufzeichnungen, die sich auf die selbe Quellgruppe beziehen, können hintereinander ausgeführt 
        /// werden.
        /// </summary>
        [Test]
        public void Two_Recordings_Of_The_Same_Group_Can_Be_Recorded_After_Each_Other()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours(1), TimeSpan.FromMinutes(90));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddHours(3), TimeSpan.FromMinutes(60));

            // Create component under test
            var cut =
                new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase)
                    {
                        { FreeTVDevice },
                        { plan2 },
                        { plan1 },
                    };

            // Resolve
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(2), "Schedules");
            Assert.That(schedules[0].Resource, Is.SameAs(FreeTVDevice), "Resource 1");
            Assert.That(schedules[1].Resource, Is.SameAs(FreeTVDevice), "Resource 2");
            Assert.That(schedules[0].Definition, Is.SameAs(plan1), "Plan 1");
            Assert.That(schedules[1].Definition, Is.SameAs(plan2), "Plan 2");
            Assert.That(schedules[0].StartsLate, Is.False, "Late 1");
            Assert.That(schedules[1].StartsLate, Is.False, "Late 2");
        }

        /// <summary>
        /// Zwei Aufzeichnungen, die sich auf unterschiedliche Quellgruppen beziehen, können hintereinander ausgeführt 
        /// werden.
        /// </summary>
        [Test]
        public void Two_Recordings_Of_Different_Groups_Can_Be_Recorded_After_Each_Other()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours(1), TimeSpan.FromMinutes(90));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group2Free, TimeBias.AddHours(3), TimeSpan.FromMinutes(60));

            // Create component under test
            var cut =
                new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase)
                    {
                        { FreeTVDevice },
                        { plan2 },
                        { plan1 },
                    };

            // Resolve
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(2), "Schedules");
            Assert.That(schedules[0].Definition, Is.SameAs(plan1), "Plan 1");
            Assert.That(schedules[1].Definition, Is.SameAs(plan2), "Plan 2");
            Assert.That(schedules[0].StartsLate, Is.False, "Late 1");
            Assert.That(schedules[1].StartsLate, Is.False, "Late 2");
        }

        /// <summary>
        /// Aufzeichnungen auf einer Quelle werden kombiniert und zählen nur einmal.
        /// </summary>
        [Test]
        public void Will_Detect_Recordings_On_Same_Source_As_One()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "testA", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddMinutes(60), TimeSpan.FromMinutes(90));
            var plan2 = RecordingDefinition.Create(false, "testB", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddMinutes(90), TimeSpan.FromMinutes(40));
            var plan3 = RecordingDefinition.Create(false, "testC", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddMinutes(100), TimeSpan.FromMinutes(40));

            // Load current
            var device = (ResourceMock)FreeTVDevice;
            var limit = device.SourceLimit;
            try
            {
                // Create component under test
                var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { device.SetSourceLimit(2), plan1, plan2, plan3 };

                // Resolve
                var schedules = cut.GetSchedules(TimeBias).ToArray();

                // Validate
                Assert.That(schedules.Length, Is.EqualTo(3), "Schedules");
                Assert.That(schedules[0].StartsLate, Is.False, "Late 1");
                Assert.That(schedules[1].StartsLate, Is.False, "Late 2");
                Assert.That(schedules[2].StartsLate, Is.False, "Late 3");
            }
            finally
            {
                // Reset
                device.SetSourceLimit(limit);
            }
        }

        /// <summary>
        /// Zwei Aufzeichnungen, die sich auf unterschiedliche Quellgruppen beziehen, können nicht simultan ausgeführt 
        /// werden.
        /// </summary>
        [Test]
        public void A_Recording_May_Start_Late_If_Overlapping_Occurs_On_Different_Groups()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours(1), TimeSpan.FromMinutes(90));
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group2Free, TimeBias.AddHours(2), TimeSpan.FromMinutes(60));

            // Create component under test
            var cut =
                new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase)
                    {
                        { FreeTVDevice },
                        { plan2 },
                        { plan1 },
                    };

            // Resolve
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(2), "Schedules");
            Assert.That(schedules[0].Resource, Is.SameAs(FreeTVDevice), "Resource 1");
            Assert.That(schedules[1].Resource, Is.SameAs(FreeTVDevice), "Resource 2");
            Assert.That(schedules[0].Definition, Is.SameAs(plan1), "Plan 1");
            Assert.That(schedules[1].Definition, Is.SameAs(plan2), "Plan 2");
            Assert.That(schedules[0].StartsLate, Is.False, "Late 1");
            Assert.That(schedules[1].StartsLate, Is.True, "Late 2");
            Assert.That(TimeBias.AddHours(2.5), Is.EqualTo(schedules[1].Time.Start), "Start");
        }

        /// <summary>
        /// Zwei Aufzeichnungen, die sich auf unterschiedliche Quellgruppen beziehen, können nicht simultan ausgeführt 
        /// werden.
        /// </summary>
        [Test]
        public void A_Recording_May_Be_Discarded_If_Overlapping_Occurs_On_Different_Groups()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "test1", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours(1), TimeSpan.FromMinutes(90));
            var plan2 = RecordingDefinition.Create(false, "test2", Guid.NewGuid(), null, SourceMock.Source1Group2Free, TimeBias.AddHours(2), TimeSpan.FromMinutes(10));

            // Create component under test
            var cut =
                new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase)
                    {
                        { FreeTVDevice },
                        { plan2 },
                        { plan1 },
                    };

            // Resolve
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(2), "Schedules");
            Assert.That(schedules[0].Resource, Is.Null, "Resource 1");
            Assert.That(schedules[1].Resource, Is.SameAs(FreeTVDevice), "Resource 2");
            Assert.That(schedules[0].Definition, Is.SameAs(plan2), "Plan 1");
            Assert.That(schedules[1].Definition, Is.SameAs(plan1), "Plan 2");
            Assert.That(schedules[0].StartsLate, Is.False, "Late 1");
            Assert.That(schedules[1].StartsLate, Is.False, "Late 2");
        }

        /// <summary>
        /// Eine Quelle, die dem Gerät nicht bekannt ist, kann nicht aufgezeichnet werden.
        /// </summary>
        [Test]
        public void An_Unknown_Source_Can_Not_Be_Recorded()
        {
            // Create recordings
            var plan = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("unknown"), TimeBias.AddHours(1), TimeSpan.FromMinutes(90));

            // Create component under test
            var cut =
                new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase)
                    {
                        { FreeTVDevice },
                        { plan },
                    };

            // Resolve
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(1), "Schedules");
            Assert.That(schedules[0].Resource, Is.SameAs(null), "Resource");
            Assert.That(schedules[0].Definition, Is.SameAs(plan), "Plan");
        }

        /// <summary>
        /// Die Planung der Aufzeichnungen wird nach einer festen Anzahl von Einträgen
        /// als Sollbruchstelle unterbrochen.
        /// </summary>
        [Test]
        public void The_Number_Of_Recordings_Per_Plan_Is_Limited()
        {
            // Create component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { FreeTVDevice };

            // Add all plans
            for (int i = 0; i <= RecordingScheduler.MaximumRecordingsInPlan; i++)
            {
                // Create recording
                var plan = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddHours(i), TimeSpan.FromMinutes(90));

                // Add it
                cut.Add(plan);
            }

            // Resolve
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That((uint)schedules.Length, Is.EqualTo(RecordingScheduler.MaximumRecordingsInPlan + 1), "Schedules");

            // Check all
            for (int i = 0; i < RecordingScheduler.MaximumRecordingsInPlan; i++)
            {
                // Load
                var schedule = schedules[i];

                // Validate
                Assert.That(schedule.Resource, Is.SameAs(FreeTVDevice), $"Resource {i}");
                Assert.That(schedule.Time.Start, Is.EqualTo(TimeBias.AddHours(i)), $"Start {i}");
                Assert.That(schedule.Time.Duration, Is.EqualTo(TimeSpan.FromMinutes(90)), $"Duration {i}");
                Assert.That(schedule.StartsLate, Is.False, $"Late {i}");
            }

            // Load the last
            var last = schedules[RecordingScheduler.MaximumRecordingsInPlan];

            // Validate - internal planning is not touched
            Assert.That(last.Resource, Is.SameAs(FreeTVDevice), "Resource");
            Assert.That(last.Time.Start, Is.EqualTo(TimeBias.AddHours(RecordingScheduler.MaximumRecordingsInPlan)), "Start");
            Assert.That(last.Time.Duration, Is.EqualTo(TimeSpan.FromMinutes(90)), "Duration");
            Assert.That(last.StartsLate, Is.False, "Late");
        }

        /// <summary>
        /// Sich wiederholende und einzelne Aufzeichnungen können überlappen.
        /// </summary>
        [Test]
        public void A_Repeating_Recording_May_Be_Overlapped_By_A_Single_Recording()
        {
            // Create recordings
            var repeatStart = TimeBias.AddHours(1);
            var repeatStartLocal = repeatStart.ToLocalTime();
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group1Free, repeatStart, TimeSpan.FromMinutes(90), DateTime.MaxValue.Date, repeatStartLocal.DayOfWeek);
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Source1Group2Free, repeatStart.AddDays(7).AddMinutes(-10), TimeSpan.FromMinutes(60));

            // Create component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { FreeTVDevice, plan2, plan1, };

            // Load first repeats
            var schedules = cut.GetSchedules(TimeBias).Take(3).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(3), "Schedules");
            Assert.That(schedules[0].Definition, Is.SameAs(plan1), "Plan 1");
            Assert.That(schedules[0].Resource, Is.SameAs(FreeTVDevice), "Resource 1");
            Assert.That(schedules[1].Definition, Is.SameAs(plan2), "Plan 2");
            Assert.That(schedules[1].Resource, Is.SameAs(FreeTVDevice), "Resource 2");
            Assert.That(schedules[1].Time.Start, Is.EqualTo(repeatStart.AddDays(7).AddMinutes(-10)), "Start 2");
            Assert.That(schedules[1].Time.Duration, Is.EqualTo(TimeSpan.FromMinutes(60)), "Duration 2");
            Assert.That(schedules[2].Definition, Is.SameAs(plan1), "Plan 3");
            Assert.That(schedules[2].Resource, Is.SameAs(FreeTVDevice), "Resource 3");
            Assert.That(schedules[2].StartsLate, Is.True, "Late");
            Assert.That(schedules[2].Time.Start, Is.EqualTo(repeatStart.AddDays(7).AddMinutes(50)), "Start 3");
            Assert.That(schedules[2].Time.Duration, Is.EqualTo(TimeSpan.FromMinutes(40)), "Duration 3");
        }

        /// <summary>
        /// Auf eine sich wiederholende Aufzeichnung können Ausnahmen angewendet werden.
        /// </summary>
        [Test]
        public void A_Repeating_Recording_Can_Have_Exceptions()
        {
            // Create recordings
            var repeatStart = TimeBias.AddHours(1);
            var repeatStartLocal = repeatStart.ToLocalTime();
            var plan =
                RecordingDefinition.Create
                    (
                        false,
                        "test",
                        Guid.NewGuid(),
                        null,
                        SourceMock.Source1Group1Free,
                        repeatStart,
                        TimeSpan.FromMinutes(90),
                        DateTime.MaxValue.Date,
                        Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToArray()
                    );
            var exception1 =
                new PlanException
                {
                    ExceptionDate = repeatStartLocal.AddDays(12).Date,
                    DurationDelta = TimeSpan.FromMinutes(-10),
                    StartDelta = TimeSpan.FromMinutes(10),
                };
            var exception2 =
                new PlanException
                {
                    ExceptionDate = repeatStartLocal.AddDays(22).Date,
                    DurationDelta = TimeSpan.FromMinutes(10),
                };
            var exception3 =
                new PlanException
                {
                    ExceptionDate = repeatStartLocal.AddDays(24).Date,
                    DurationDelta = TimeSpan.FromMinutes(-100),
                };

            // Create component under test
            var cut =
                new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase)
                    {
                        { FreeTVDevice },
                        { plan, exception1, exception2, exception3 },
                    };

            // Load 
            var schedules = cut.GetSchedules(TimeBias).Take(30).ToArray();

            // Check
            for (int i = 30; i-- > 0;)
                if (i == 12)
                {
                    // Validate
                    Assert.That(repeatStart.AddDays(i).AddMinutes(10), Is.EqualTo(schedules[i].Time.Start), $"Start {i}");
                    Assert.That(TimeSpan.FromMinutes(80), Is.EqualTo(schedules[i].Time.Duration), $"Duration {i}");
                }
                else if (i == 22)
                {
                    // Validate
                    Assert.That(repeatStart.AddDays(i), Is.EqualTo(schedules[i].Time.Start), $"Start {i}");
                    Assert.That(TimeSpan.FromMinutes(100), Is.EqualTo(schedules[i].Time.Duration), $"Duration {i}");
                }
                else
                {
                    // Correct
                    var day = (i < 24) ? i : i + 1;

                    // Validate
                    Assert.That(repeatStart.AddDays(day), Is.EqualTo(schedules[i].Time.Start), $"Start {i}");
                    Assert.That(TimeSpan.FromMinutes(90), Is.EqualTo(schedules[i].Time.Duration), $"Duration {i}");
                }
        }

        /// <summary>
        /// Die zeitliche Ordnung der Aufzeichnungen wird bei der Planung berücksichtigt.
        /// </summary>
        [Test]
        public void Will_Keep_Time_Order_When_Planning()
        {
            // Create recordings
            var plan1 = RecordingDefinition.Create(false, "testA", Guid.NewGuid(), null, SourceMock.Source1Group1Free, TimeBias.AddMinutes(60), TimeSpan.FromMinutes(60));
            var plan2 = RecordingDefinition.Create(false, "testB", Guid.NewGuid(), null, SourceMock.Source1Group2Free, TimeBias.AddMinutes(90), TimeSpan.FromMinutes(60));
            var plan3 = RecordingDefinition.Create(false, "testC", Guid.NewGuid(), null, SourceMock.Source2Group1Free, TimeBias.AddMinutes(100), TimeSpan.FromMinutes(100));

            // Create component under test
            var cut = new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase) { FreeTVDevice, plan1, plan2, plan3 };

            // Resolve
            var schedules = cut.GetSchedules(TimeBias).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(3), "Schedules");
            Assert.That(schedules[0].Definition.Name, Is.EqualTo("testA"), "Name 1");
            Assert.That(schedules[1].Definition.Name, Is.EqualTo("testB"), "Name 2");
            Assert.That(schedules[2].Definition.Name, Is.EqualTo("testC"), "Name 3");
            Assert.That(schedules[0].Resource, Is.SameAs(FreeTVDevice), "Resource 1");
            Assert.That(schedules[1].Resource, Is.SameAs(FreeTVDevice), "Resource 2");
            Assert.That(schedules[2].Resource, Is.SameAs(FreeTVDevice), "Resource 3");
            Assert.That(schedules[0].StartsLate, Is.False, "Late 1");
            Assert.That(schedules[1].StartsLate, Is.True, "Late 2");
            Assert.That(schedules[2].StartsLate, Is.True, "Late 3");
        }
    }
}
