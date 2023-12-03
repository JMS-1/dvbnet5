using JMS.DVB.Algorithms.Scheduler;

namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Prüft die Priorisierung von Geräten.
    /// </summary>
    [TestFixture]
    public class PrioritySpecs
    {
        /// <summary>
        /// Gibt es zwei Geräte, die eine Quelle unterstützen, so wird die mit der höheren Priorität
        /// gewählt, wenn nur eine Aufzeichnugen ansteht.
        /// </summary>
        [Test]
        public void Will_Choose_Highest_Priority_Source_For_Single_Plan_Item()
        {
            // Create plan
            var plan = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("s1"), DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(20));
            var best = ResourceMock.Create("r3", SourceMock.Create("s1")).SetPriority(6);
            var times = plan.GetTimes(DateTime.UtcNow).Select(s => s.Planned).ToArray();

            // Create component under test
            var componentUnderTest =
                new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase)
                    {
                        { ResourceMock.Create( "r1", SourceMock.Create( "s1" ) ).SetPriority( 1 ) },
                        { best },
                        { ResourceMock.Create( "r2", SourceMock.Create( "s1" ) ).SetPriority( -1 ) },
                        { plan },
                    };

            // Load
            var schedules = componentUnderTest.GetSchedules(DateTime.UtcNow).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(1), "Schedules");
            Assert.That(schedules[0].Resource, Is.SameAs(best), "Resource");
            Assert.That(schedules[0].Definition, Is.SameAs(plan), "Plan");
            Assert.That(schedules[0].Time, Is.EqualTo(times.Single()), "Time");
            Assert.That(schedules[0].StartsLate, Is.False, "Late");
        }

        /// <summary>
        /// Auch bei expliziter Bindung an ein Gerät werden direkt nachfolgende Aufzeichnungen auf dem Gerät mit der
        /// höchsten Priorität ausgeführt.
        /// </summary>
        [Test]
        public void Will_Use_Resource_With_Highest_Priority_When_Explicit_Binding_Is_Used()
        {
            // Create plan
            var source1 = SourceMock.Create("s1");
            var source2 = SourceMock.Create("s2");
            var res1 = ResourceMock.Create("r1", source1, source2).SetPriority(5);
            var res2 = ResourceMock.Create("r2", source1, source2).SetPriority(6);
            var res3 = ResourceMock.Create("r3", source1, source2).SetPriority(0);
            var refTime = DateTime.UtcNow;
            var plan1 = RecordingDefinition.Create(false, "A1", Guid.NewGuid(), new[] { res1 }, source1, refTime.AddMinutes(100), TimeSpan.FromMinutes(20));
            var plan2 = RecordingDefinition.Create(false, "A2", Guid.NewGuid(), new[] { res1 }, source1, refTime.AddMinutes(110), TimeSpan.FromMinutes(20));
            var plan3 = RecordingDefinition.Create(false, "A3", Guid.NewGuid(), null, source2, refTime.AddMinutes(130), TimeSpan.FromMinutes(20));

            // Create component under test
            var componentUnderTest =
                new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase)
                    {
                        { res1 },
                        { res2 },
                        { res3 },
                        { plan1 },
                        { plan2 },
                        { plan3 },
                    };

            // Load
            var schedules = componentUnderTest.GetSchedules(refTime).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(3), "#schedule");
            Assert.That(schedules[0].Resource, Is.SameAs(res1), "resource 0");
            Assert.That(schedules[1].Resource, Is.SameAs(res1), "resource 1");
            Assert.That(schedules[2].Resource, Is.SameAs(res2), "resource 2");
        }

        /// <summary>
        /// Gibt es zwei Geräte, die eine Quelle unterstützen, so wird die mit der höheren Priorität
        /// gewählt, wenn zwei überlappende Aufzeichnugen anstehen.
        /// </summary>
        [Test]
        public void Will_Choose_Highest_Priority_Source_For_Two_Overlapping_Plan_Items()
        {
            // Create plan
            var group = Guid.NewGuid();
            var plan1 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("s1", group), DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(20));
            var times1 = plan1.GetTimes(DateTime.UtcNow).Select(s => s.Planned).Single();
            var plan2 = RecordingDefinition.Create(false, "test", Guid.NewGuid(), null, SourceMock.Create("s1", group), times1.End - TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(20));
            var times2 = plan2.GetTimes(DateTime.UtcNow).Select(s => s.Planned).Single();
            var best = ResourceMock.Create("r3", SourceMock.Create("s1", group)).SetPriority(6);

            // Create component under test
            var componentUnderTest =
                new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase)
                    {
                        { ResourceMock.Create( "r1", SourceMock.Create( "s1", group ) ).SetPriority( 1 ) },
                        { best },
                        { ResourceMock.Create( "r2", SourceMock.Create( "s1", group ) ).SetPriority( -1 ) },
                        { plan2 },
                        { plan1 },
                };

            // Load
            var schedules = componentUnderTest.GetSchedules(DateTime.UtcNow).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(2), "Schedules");
            Assert.That(schedules[0].Resource, Is.SameAs(best), "Resource 0");
            Assert.That(schedules[1].Resource, Is.SameAs(best), "Resource 1");
            Assert.That(schedules[0].Definition, Is.SameAs(plan1), "Plan 0");
            Assert.That(schedules[1].Definition, Is.SameAs(plan2), "Plan 1");
            Assert.That(schedules[0].Time, Is.EqualTo(times1), "Time 0");
            Assert.That(schedules[1].Time, Is.EqualTo(times2), "Time 1");
            Assert.That(schedules[0].StartsLate, Is.False, "Late 0");
            Assert.That(schedules[1].StartsLate, Is.False, "Late 1");
        }

        /// <summary>
        /// Es wird versucht den Zeitraum, bei dem eine Quelle auf mehreren Resourcen gleichzeitig
        /// aufgeszeichnet wird, zu minimieren.
        /// </summary>
        [Test]
        public void Will_Avoid_Source_On_Multiple_Resources()
        {
            // Create plan
            var source1 = SourceMock.Create("s1");
            var source2 = SourceMock.Create("s2");
            var res1 = ResourceMock.Create("r1", source1, source2).SetPriority(1);
            var res2 = ResourceMock.Create("r2", source1, source2).SetPriority(2);
            var refTime = DateTime.UtcNow.Date.AddDays(10);
            var plan1 = RecordingDefinition.Create(false, "A1", Guid.NewGuid(), null, source1, refTime.AddHours(18), TimeSpan.FromHours(2));
            var plan2 = RecordingDefinition.Create(false, "A2", Guid.NewGuid(), null, source2, refTime.AddHours(19), TimeSpan.FromHours(4));
            var plan3 = RecordingDefinition.Create(false, "A3", Guid.NewGuid(), null, source2, refTime.AddHours(21), TimeSpan.FromHours(2));

            // Create component under test
            var componentUnderTest =
                new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase, Utils.LoadTestFile("AvoidSourcesOnMultipledResources.cmp"))
                    {
                        { res1 },
                        { res2 },
                        { plan1 },
                        { plan2 },
                        { plan3 },
                    };

            // Load
            var schedules = componentUnderTest.GetSchedules(refTime).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(3), "#schedules");
            Assert.That(schedules[0].Definition, Is.SameAs(plan1), "plan 1");
            Assert.That(schedules[1].Definition, Is.SameAs(plan2), "plan 2");
            Assert.That(schedules[2].Definition, Is.SameAs(plan3), "plan 3");
            Assert.That(schedules[0].Resource, Is.SameAs(res1), "resource 1");
            Assert.That(schedules[1].Resource, Is.SameAs(res2), "resource 2");
            Assert.That(schedules[2].Resource, Is.SameAs(res2), "resource 3");
        }

        /// <summary>
        /// Es ist möglich, die Priorität durch ein Startverbot zu überschreiben.
        /// </summary>
        [Test]
        public void Will_Enforce_Start_Order()
        {
            // Create plan
            var source1 = SourceMock.Create("s1");
            var source2 = SourceMock.Create("s2");
            var res1 = ResourceMock.Create("r1", source1, source2).SetPriority(1);
            var res2 = ResourceMock.Create("r2", source1, source2).SetPriority(2);
            var refTime = DateTime.UtcNow.Date.AddDays(10);
            var plan1 = RecordingDefinition.Create(false, "A1", Guid.NewGuid(), null, source1, refTime.AddHours(18), TimeSpan.FromHours(2));
            var plan2 = RecordingDefinition.Create(false, "A2", Guid.NewGuid(), null, source2, refTime.AddHours(19), TimeSpan.FromHours(2));

            // Create component under test
            var componentUnderTest =
                new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase, Utils.LoadTestFile("EnforceResourceStartOrder.cmp"))
                    {
                        { res1 },
                        { res2 },
                        { plan1 },
                        { plan2 },
                    };

            // Load
            var schedules = componentUnderTest.GetSchedules(refTime).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(2), "#schedules");
            Assert.That(schedules[0].Definition, Is.SameAs(plan1), "plan 1");
            Assert.That(schedules[1].Definition, Is.SameAs(plan2), "plan 2");
            Assert.That(schedules[0].Resource, Is.SameAs(res1), "resource 1");
            Assert.That(schedules[1].Resource, Is.SameAs(res2), "resource 2");
        }

        /// <summary>
        /// Es wird versucht die Anzahl unterschiedlicher Quellen auf einer Ressource zu minimieren.
        /// </summary>
        [Test]
        public void Will_Minimize_Sources_Per_Resource()
        {
            // Create plan
            var source1 = SourceMock.Create("s1");
            var source2 = SourceMock.Create("s2");
            var source3 = SourceMock.Create("s3");
            var res1 = ResourceMock.Create("r1", source1, source2, source3).SetPriority(1);
            var res2 = ResourceMock.Create("r2", source1, source2, source3).SetPriority(1);
            var res3 = ResourceMock.Create("r3", source1, source2, source3).SetPriority(0);
            var refTime = DateTime.UtcNow.Date.AddDays(10);
            var plan1 = RecordingDefinition.Create(false, "A1", Guid.NewGuid(), null, source1, refTime.AddHours(19), TimeSpan.FromHours(2));
            var plan2 = RecordingDefinition.Create(false, "A2", Guid.NewGuid(), null, source2, refTime.AddHours(20), TimeSpan.FromHours(3));
            var plan3 = RecordingDefinition.Create(false, "A3", Guid.NewGuid(), null, source3, refTime.AddHours(20), TimeSpan.FromHours(2));
            var plan4 = RecordingDefinition.Create(false, "A4", Guid.NewGuid(), null, source2, refTime.AddHours(22), TimeSpan.FromHours(2));

            // Create component under test
            var componentUnderTest =
                new RecordingScheduler(StringComparer.InvariantCultureIgnoreCase, Utils.LoadTestFile("MinimizeSourcesPerResource.cmp"))
                    {
                        { res1 },
                        { res2 },
                        { res3 },
                        { plan1 },
                        { plan2 },
                        { plan3 },
                        { plan4 },
                    };

            // Load
            var schedules = componentUnderTest.GetSchedules(refTime).ToArray();

            // Validate
            Assert.That(schedules.Length, Is.EqualTo(4), "#schedule");
            var exec1 = schedules.Single(s => s.Definition.UniqueIdentifier == plan1.UniqueIdentifier);
            var exec2 = schedules.Single(s => s.Definition.UniqueIdentifier == plan2.UniqueIdentifier);
            var exec3 = schedules.Single(s => s.Definition.UniqueIdentifier == plan3.UniqueIdentifier);
            var exec4 = schedules.Single(s => s.Definition.UniqueIdentifier == plan4.UniqueIdentifier);
            Assert.That(exec1.Resource, Is.SameAs(res1), "A1");
            Assert.That(exec2.Resource, Is.SameAs(res2), "A2");
            Assert.That(exec3.Resource, Is.SameAs(res3), "A3");
            Assert.That(exec4.Resource, Is.SameAs(res2), "A4");
        }
    }
}
