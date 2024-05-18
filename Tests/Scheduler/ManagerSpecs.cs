using JMS.DVB.Algorithms.Scheduler;

namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Prüft den Umgang mit einer Verwaltungsinstanz für Geräte.
    /// </summary>
    [TestFixture]
    public class ManagerSpecs
    {
        /// <summary>
        /// Beim Hinzufügen von Geräten müssen auch tatsächlich Geräte angegeben werden.
        /// </summary>
        [Test]
        public void ResourceAddedToManagerMustNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Create component under test
                using var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase);

                cut.Add(default(IScheduleResource)!);
            });
        }

        /// <summary>
        /// Die Konfiguration der Entschlüsselung muss immer gültig sein.
        /// </summary>
        [Test]
        public void DecryptionGroupsMustBeValid()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Create component under test
                using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
                    cut.Add(new DecryptionGroup { DecryptionGroups = [new DecryptionGroup { ScheduleResources = new IScheduleResource[1] }] });
            });
        }

        /// <summary>
        /// Es ist nicht erlaubt, ein Gerät mehrfach zu verwenden.
        /// </summary>
        [Test]
        public void EachResourceCanOnlyBeUsedOnce()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Create component under test
                using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
                {
                    // Add it
                    cut.Add(ResourceMock.Device1);
                    cut.Add(ResourceMock.Device1);
                }
            });
        }

        /// <summary>
        /// Es können mehrere Geräte verwaltet werden.
        /// </summary>
        [Test]
        public void CanUseMultipleResources()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Add it
                cut.Add(ResourceMock.Device1);
                cut.Add(ResourceMock.Device2);
            }
        }

        /// <summary>
        /// Eine Aufzeichnung muss eine positive Laufzeit haben.
        /// </summary>
        [Test]
        public void RecordingTimeMustBePositive()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
         {
             // Create component under test
             using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
             {
                 // Register device
                 cut.Add(ResourceMock.Device1);

                 // Get some time
                 var time = DateTime.UtcNow;

                 // Try it
                 cut.Start(ResourceMock.Device1, SourceMock.Source1Group1Free, Guid.NewGuid(), "test", time, time);
             }
         });
        }

        /// <summary>
        /// Die Zuordnung einer Aufzeichnung ist nur bei Angabe eines Geräte möglich.
        /// </summary>
        [Test]
        public void StartRequiresResource()
        {
            Assert.Throws<ArgumentNullException>(() =>
         {
             // Create component under test
             using var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase);

             cut.Start(null!, SourceMock.Source1Group1Free, Guid.NewGuid(), "test", DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
         });
        }

        /// <summary>
        /// Eine Aufzeichnung kann nur auf einem vorher definierten Gerät gestartet werden.
        /// </summary>
        [Test]
        public void StartRequiresKnownResource()
        {
            Assert.Throws<ArgumentException>(() =>
         {
             // Create component under test
             using var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase);

             cut.Start(ResourceMock.Device1, SourceMock.Source1Group1Free, Guid.NewGuid(), "test", DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
         });
        }

        /// <summary>
        /// Wenn eine Quelle beim Starten angegeben wird, so muss diese von dem Gerät empfangen werden können.
        /// </summary>
        [Test]
        public void StartSourceMustBeAccessibleByDevice()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Register device
                cut.Add(ResourceMock.Device1);

                // Try it
                Assert.That(cut.Start(ResourceMock.Device1, SourceMock.Source5Group1Pay, Guid.NewGuid(), "test", DateTime.UtcNow, DateTime.UtcNow.AddHours(1)), Is.False);
            }
        }

        /// <summary>
        /// Eine einzelne Aufzeichnung wird korrekt vermerkt.
        /// </summary>
        [Test]
        public void CanAddASingleRecordingAndReportAllocation()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Register device
                cut.Add(ResourceMock.Device1);
                cut.Add(ResourceMock.Device2);

                // Time
                var start = DateTime.UtcNow;
                var end = start.AddMinutes(23);
                var id = Guid.NewGuid();

                // Try it
                Assert.That(cut.Start(ResourceMock.Device1, SourceMock.Source1Group1Free, id, "test1", start, end), Is.True);

                // Read out
                var allocations = cut.CurrentAllocations;
                var resources = cut.Resources;

                // Validate
                Assert.That(resources, Is.EquivalentTo(new[] { ResourceMock.Device1, ResourceMock.Device2 }), "Resources");
                Assert.That(allocations.Length, Is.EqualTo(1), "Allocations");
                Assert.That(allocations[0].Resources.Single(), Is.SameAs(ResourceMock.Device1), "Resource");
                Assert.That(allocations[0].Name, Is.EqualTo("test1"), "Name");
                Assert.That(allocations[0].Source, Is.SameAs(SourceMock.Source1Group1Free), "Source");
                Assert.That(allocations[0].UniqueIdentifier, Is.EqualTo(id), "UniqueIdentifier");
                Assert.That(allocations[0].Time.Start, Is.EqualTo(start), "Start");
                Assert.That(allocations[0].Time.End, Is.EqualTo(end), "End");
            }
        }

        /// <summary>
        /// Es ist möglich, zwei Aufzeichnungen zu starten, die auf verschiedenen Quellen einer Quellgruppe
        /// basieren.
        /// </summary>
        [Test]
        public void CanAddTwoSourcesOnSameGroup()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Register device
                cut.Add(ResourceMock.Device1);
                cut.Add(ResourceMock.Device2);

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes(23);
                var id1 = Guid.NewGuid();
                var start2 = DateTime.UtcNow.AddMinutes(7);
                var end2 = start2.AddMinutes(120);
                var id2 = Guid.NewGuid();

                // Try it
                Assert.That(cut.Start(ResourceMock.Device1, SourceMock.Source1Group1Free, id1, "test1", start1, end1), Is.True);
                Assert.That(cut.Start(ResourceMock.Device1, SourceMock.Source2Group1Free, id2, "test2", start2, end2), Is.True);

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.That(allocations.Length, Is.EqualTo(2), "Allocations");
                Assert.That(allocations[0].Resources.Single(), Is.SameAs(ResourceMock.Device1), "Resource 1");
                Assert.That(allocations[0].Name, Is.EqualTo("test1"), "Name 1");
                Assert.That(allocations[0].Source, Is.SameAs(SourceMock.Source1Group1Free), "Source 1");
                Assert.That(allocations[0].UniqueIdentifier, Is.EqualTo(id1), "UniqueIdentifier 1");
                Assert.That(allocations[0].Time.Start, Is.EqualTo(start1), "Start");
                Assert.That(allocations[0].Time.End, Is.EqualTo(end1), "End");
                Assert.That(allocations[1].Resources.Single(), Is.SameAs(ResourceMock.Device1), "Resource 2");
                Assert.That(allocations[1].Name, Is.EqualTo("test2"), "Name 2");
                Assert.That(allocations[1].Source, Is.SameAs(SourceMock.Source2Group1Free), "Source 2");
                Assert.That(allocations[1].UniqueIdentifier, Is.EqualTo(id2), "UniqueIdentifier 2");
                Assert.That(allocations[1].Time.Start, Is.EqualTo(start2), "Start");
                Assert.That(allocations[1].Time.End, Is.EqualTo(end2), "End");
            }
        }

        /// <summary>
        /// Es ist nicht möglich, zwei Aufzeichnungen auf verschiedenen Quellgruppen zu starten.
        /// </summary>
        [Test]
        public void CanNotStartTwoSourcesOnDifferentGroups()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Register device
                cut.Add(ResourceMock.Device1);
                cut.Add(ResourceMock.Device2);

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes(23);
                var id1 = Guid.NewGuid();
                var start2 = DateTime.UtcNow.AddMinutes(7);
                var end2 = start2.AddMinutes(120);
                var id2 = Guid.NewGuid();

                // Try it
                Assert.That(cut.Start(ResourceMock.Device1, SourceMock.Source1Group1Free, id1, "test1", start1, end1), Is.True);
                Assert.That(cut.Start(ResourceMock.Device1, SourceMock.Source1Group2Free, id2, "test2", start2, end2), Is.False);

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.That(allocations.Length, Is.EqualTo(1), "Allocations");
                Assert.That(allocations[0].Resources.Single(), Is.SameAs(ResourceMock.Device1), "Resource 1");
                Assert.That(allocations[0].Name, Is.EqualTo("test1"), "Name 1");
                Assert.That(allocations[0].Source, Is.SameAs(SourceMock.Source1Group1Free), "Source 1");
                Assert.That(allocations[0].UniqueIdentifier, Is.EqualTo(id1), "UniqueIdentifier 1");
                Assert.That(allocations[0].Time.Start, Is.EqualTo(start1), "Start");
                Assert.That(allocations[0].Time.End, Is.EqualTo(end1), "End");
            }
        }

        /// <summary>
        /// Wenn ein Gerät Aufzeichnungen durchführt ist es nicht möglich, eine Aufgabe zu starten.
        /// </summary>
        [Test]
        public void CanNotStartTaskIfRegularRecordingIsActive()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Register device
                cut.Add(ResourceMock.Device1);
                cut.Add(ResourceMock.Device2);

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes(23);
                var id1 = Guid.NewGuid();
                var start2 = DateTime.UtcNow.AddMinutes(7);
                var end2 = start2.AddMinutes(120);
                var id2 = Guid.NewGuid();

                // Try it
                Assert.That(cut.Start(ResourceMock.Device1, SourceMock.Source1Group1Free, id1, "test1", start1, end1), Is.True);
                Assert.That(cut.Start(ResourceMock.Device1, null, id2, "test2", start2, end2), Is.False);

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.That(allocations.Length, Is.EqualTo(1), "Allocations");
                Assert.That(allocations[0].Resources.Single(), Is.SameAs(ResourceMock.Device1), "Resource 1");
                Assert.That(allocations[0].Name, Is.EqualTo("test1"), "Name 1");
                Assert.That(allocations[0].Source, Is.SameAs(SourceMock.Source1Group1Free), "Source 1");
                Assert.That(allocations[0].UniqueIdentifier, Is.EqualTo(id1), "UniqueIdentifier 1");
                Assert.That(allocations[0].Time.Start, Is.EqualTo(start1), "Start");
                Assert.That(allocations[0].Time.End, Is.EqualTo(end1), "End");
            }
        }

        /// <summary>
        /// Es ist nicht erlaubt, eine Aufzeichnung mehrfach zu starten.
        /// </summary>
        [Test]
        public void EachRecordingCanOnlyBeStartedOnce()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Create component under test
                using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
                {
                    // Register device
                    cut.Add(ResourceMock.Device1);
                    cut.Add(ResourceMock.Device2);

                    // Time
                    var start = DateTime.UtcNow;
                    var end = start.AddMinutes(23);
                    var id = Guid.NewGuid();

                    // Try it
                    Assert.That(cut.Start(ResourceMock.Device1, SourceMock.Source1Group1Free, id, "test", start, end), Is.True);

                    // Fail it
                    cut.Start(ResourceMock.Device1, SourceMock.Source1Group1Free, id, "test", start, end);
                }
            });
        }

        /// <summary>
        /// Führt ein Gerät eine Aufgabe aus, so können keine regulären Aufzeichnungen ergänzt werden.
        /// </summary>
        [Test]
        public void CanNotStartRegularRecordingIfTaskIsActive()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Register device
                cut.Add(ResourceMock.Device1);
                cut.Add(ResourceMock.Device2);

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes(23);
                var id1 = Guid.NewGuid();
                var start2 = DateTime.UtcNow.AddMinutes(7);
                var end2 = start2.AddMinutes(120);
                var id2 = Guid.NewGuid();

                // Try it
                Assert.That(cut.Start(ResourceMock.Device1, null, id2, "test2", start2, end2), Is.True);
                Assert.That(cut.Start(ResourceMock.Device1, SourceMock.Source1Group1Free, id1, "test1", start1, end1), Is.False);

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.That(allocations.Length, Is.EqualTo(1), "Allocations");
                Assert.That(allocations[0].Resources.Single(), Is.SameAs(ResourceMock.Device1), "Resource 1");
                Assert.That(allocations[0].Name, Is.EqualTo("test2"), "Name 1");
                Assert.That(allocations[0].Source, Is.Null, "Source 1");
                Assert.That(allocations[0].UniqueIdentifier, Is.EqualTo(id2), "UniqueIdentifier 1");
                Assert.That(allocations[0].Time.Start, Is.EqualTo(start2), "Start");
                Assert.That(allocations[0].Time.End, Is.EqualTo(end2), "End");
            }
        }

        /// <summary>
        /// Führt ein Gerät eine Aufgabe aus, so können keine regulären Aufzeichnungen ergänzt werden.
        /// </summary>
        [Test]
        public void CanNotStartRegularRecordingIfTaskIsActiveEvenIfOverlapping()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Register device
                cut.Add(ResourceMock.Device1);
                cut.Add(ResourceMock.Device2);

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes(120);
                var id1 = Guid.NewGuid();
                var start2 = DateTime.UtcNow.AddMinutes(7);
                var end2 = start2.AddMinutes(23);
                var id2 = Guid.NewGuid();

                // Try it
                Assert.That(cut.Start(ResourceMock.Device1, null, id2, "test2", start2, end2), Is.True);
                Assert.That(cut.Start(ResourceMock.Device1, SourceMock.Source1Group1Free, id1, "test1", start1, end1), Is.False);

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.That(allocations.Length, Is.EqualTo(1), "Allocations");
                Assert.That(allocations[0].Resources.Single(), Is.SameAs(ResourceMock.Device1), "Resource 1");
                Assert.That(allocations[0].Name, Is.EqualTo("test2"), "Name 1");
                Assert.That(allocations[0].Source, Is.Null, "Source 1");
                Assert.That(allocations[0].UniqueIdentifier, Is.EqualTo(id2), "UniqueIdentifier 1");
                Assert.That(allocations[0].Time.Start, Is.EqualTo(start2), "Start");
                Assert.That(allocations[0].Time.End, Is.EqualTo(end2), "End");
            }
        }

        /// <summary>
        /// Führt ein Gerät eine Aufgabe aus, so kann keine weitere Aufgabe gestartet werden.
        /// </summary>
        [Test]
        public void CanNotStartTwoTasks()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Register device
                cut.Add(ResourceMock.Device1);
                cut.Add(ResourceMock.Device2);

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes(120);
                var id1 = Guid.NewGuid();
                var start2 = DateTime.UtcNow.AddMinutes(7);
                var end2 = start2.AddMinutes(23);
                var id2 = Guid.NewGuid();

                // Try it
                Assert.That(cut.Start(ResourceMock.Device1, null, id2, "test2", start2, end2), Is.True);
                Assert.That(cut.Start(ResourceMock.Device1, null, id1, "test1", start1, end1), Is.False);

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.That(allocations.Length, Is.EqualTo(1), "Allocations");
                Assert.That(allocations[0].Resources.Single(), Is.SameAs(ResourceMock.Device1), "Resource 1");
                Assert.That(allocations[0].Name, Is.EqualTo("test2"), "Name 1");
                Assert.That(allocations[0].Source, Is.Null, "Source 1");
                Assert.That(allocations[0].UniqueIdentifier, Is.EqualTo(id2), "UniqueIdentifier 1");
                Assert.That(allocations[0].Time.Start, Is.EqualTo(start2), "Start");
                Assert.That(allocations[0].Time.End, Is.EqualTo(end2), "End");
            }
        }

        /// <summary>
        /// Eine Aufzeichnung kann beendet werden.
        /// </summary>
        [Test]
        public void CanStopARecording()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Register device
                cut.Add(ResourceMock.Device1);
                cut.Add(ResourceMock.Device2);

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes(23);
                var id1 = Guid.NewGuid();
                var start2 = DateTime.UtcNow.AddMinutes(7);
                var end2 = start2.AddMinutes(120);
                var id2 = Guid.NewGuid();

                // Try it
                Assert.That(cut.Start(ResourceMock.Device1, SourceMock.Source1Group1Free, id1, "test1", start1, end1), Is.True);
                Assert.That(cut.Start(ResourceMock.Device1, SourceMock.Source2Group1Free, id2, "test2", start2, end2), Is.True);

                // Remove one
                cut.Stop(id1);

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.That(allocations.Length, Is.EqualTo(1), "Allocations");
                Assert.That(allocations[0].Resources.Single(), Is.SameAs(ResourceMock.Device1), "Resource");
                Assert.That(allocations[0].Name, Is.EqualTo("test2"), "Name");
                Assert.That(allocations[0].Source, Is.SameAs(SourceMock.Source2Group1Free), "Source");
                Assert.That(allocations[0].UniqueIdentifier, Is.EqualTo(id2), "UniqueIdentifier");
                Assert.That(allocations[0].Time.Start, Is.EqualTo(start2), "Start");
                Assert.That(allocations[0].Time.End, Is.EqualTo(end2), "End");
            }
        }

        /// <summary>
        /// Es können nur bekannte Aufzeichnungen beendet werden.
        /// </summary>
        [Test]
        public void StopRequestWillBeValidated()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Create component under test
                using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
                    cut.Stop(Guid.NewGuid());
            });
        }

        /// <summary>
        /// Es können nur bekannte Aufzeichnungen verändert werden.
        /// </summary>
        [Test]
        public void ModifyRequestWillBeValidated()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                // Create component under test
                using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
                    cut.Modify(Guid.NewGuid(), DateTime.MaxValue);
            });
        }

        /// <summary>
        /// Der Endzeitpunkt einer Aufzeichnung kann verändert werden.
        /// </summary>
        [Test]
        public void CanModifyARecording()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Register device
                cut.Add(ResourceMock.Device1);
                cut.Add(ResourceMock.Device2);

                // Time
                var start = DateTime.UtcNow;
                var end1 = start.AddMinutes(23);
                var end2 = start.AddMinutes(90);
                var id = Guid.NewGuid();

                // Try it
                Assert.That(cut.Start(ResourceMock.Device1, SourceMock.Source1Group1Free, id, "test1", start, end1), Is.True);
                Assert.That(cut.Modify(id, end2), Is.True);

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.That(allocations.Length, Is.EqualTo(1), "Allocations");
                Assert.That(allocations[0].Resources.Single(), Is.SameAs(ResourceMock.Device1), "Resource");
                Assert.That(allocations[0].Name, Is.EqualTo("test1"), "Name");
                Assert.That(allocations[0].Source, Is.SameAs(SourceMock.Source1Group1Free), "Source");
                Assert.That(allocations[0].UniqueIdentifier, Is.EqualTo(id), "UniqueIdentifier");
                Assert.That(allocations[0].Time.Start, Is.EqualTo(start), "Start");
                Assert.That(allocations[0].Time.End, Is.EqualTo(end2), "End");
            }
        }

        /// <summary>
        /// Der Endzeitpunkt einer Aufzeichnung kann niemals vor dem Beginn liegen.
        /// </summary>
        [Test]
        public void CanNotSetEndBeforeStart()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // Create component under test
                using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
                {
                    // Register device
                    cut.Add(ResourceMock.Device1);
                    cut.Add(ResourceMock.Device2);

                    // Time
                    var start = DateTime.UtcNow;
                    var end = start.AddMinutes(23);
                    var id = Guid.NewGuid();

                    // Try it
                    Assert.That(cut.Start(ResourceMock.Device1, SourceMock.Source1Group1Free, id, "test1", start, end), Is.True);

                    // Will fail
                    cut.Modify(id, start);
                }
            });
        }

        /// <summary>
        /// Beim Ändern des Endzeitpunktes einer Aufzeichnung werden die Grenzwerte berücksichtigt.
        /// </summary>
        [Test]
        public void ModifyWillVerifyResourceUsage()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Register device - this selection has a single decryption slot available
                cut.Add(ResourceMock.Device2);

                // Time
                var start1 = DateTime.UtcNow;
                var end1 = start1.AddMinutes(23);
                var id1 = Guid.NewGuid();
                var start2 = end1;
                var end2 = start2.AddMinutes(120);
                var id2 = Guid.NewGuid();

                // Create non overlapping recordings allowing the device to add both
                Assert.That(cut.Start(ResourceMock.Device2, SourceMock.Source1Group1Pay, id1, "test1", start1, end1), Is.True, "Start 1");
                Assert.That(cut.Start(ResourceMock.Device2, SourceMock.Source4Group1Pay, id2, "test2", start2, end2), Is.True, "Start 2");

                // Now extend the first recording just a tiny bit
                Assert.That(cut.Modify(id1, start2.AddTicks(1)), Is.False, "Modify 1");

                // Read out
                var allocations = cut.CurrentAllocations;

                // Validate
                Assert.That(allocations.Length, Is.EqualTo(2), "Allocations");
                Assert.That(allocations[0].Resources.Single(), Is.SameAs(ResourceMock.Device2), "Resource 1");
                Assert.That(allocations[0].Name, Is.EqualTo("test1"), "Name 1");
                Assert.That(allocations[0].Source, Is.SameAs(SourceMock.Source1Group1Pay), "Source 1");
                Assert.That(allocations[0].UniqueIdentifier, Is.EqualTo(id1), "UniqueIdentifier 1");
                Assert.That(allocations[0].Time.Start, Is.EqualTo(start1), "Start");
                Assert.That(allocations[0].Time.End, Is.EqualTo(end1), "End");
                Assert.That(allocations[1].Resources.Single(), Is.SameAs(ResourceMock.Device2), "Resource 2");
                Assert.That(allocations[1].Name, Is.EqualTo("test2"), "Name 2");
                Assert.That(allocations[1].Source, Is.SameAs(SourceMock.Source4Group1Pay), "Source 2");
                Assert.That(allocations[1].UniqueIdentifier, Is.EqualTo(id2), "UniqueIdentifier 2");
                Assert.That(allocations[1].Time.Start, Is.EqualTo(start2), "Start");
                Assert.That(allocations[1].Time.End, Is.EqualTo(end2), "End");
            }
        }

        /// <summary>
        /// Eine Aufzeichnung ohne Wiederholung wird einmal ausgeführt.
        /// </summary>
        [Test]
        public void WillScheduleSingleNonRepeatedTask()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Add a single device
                cut.Add(ResourceMock.Device1);

                // Time to use
                var now = DateTime.UtcNow;

                // Create the recording
                var play1 = RecordingDefinition.Create(this, "test", Guid.NewGuid(), [ResourceMock.Device1], SourceMock.Source1Group1Free, now.AddMinutes(10), TimeSpan.FromMinutes(100));

                // Create initializer
                Func<RecordingScheduler, DateTime, IEnumerable<IScheduleInformation>> callback =
                    (s, t) =>
                    {
                        // Load
                        s.Add(play1);

                        // Report
                        return s.GetSchedules(t);
                    };

                // Check action - should be waiting on the next recording to start
                var next = cut.GetNextActivity(now, callback);

                // Test
                Assert.That(next, Is.InstanceOf<WaitActivity>());

                // Advance
                now = ((WaitActivity)next).RetestTime;

                // Check action - next recording is now ready to start
                next = cut.GetNextActivity(now, callback);

                // Test
                Assert.That(next, Is.InstanceOf<StartActivity>());

                // Get all 
                var plan = ((StartActivity)next).Recording;

                // Validate
                Assert.That(plan.Time.Start, Is.EqualTo(now), "Initial Start");
                Assert.That(cut.GetEndOfAllocation(), Is.Null, "Initial Allocation");

                // Mark as started
                Assert.That(cut.Start(plan), Is.True, "Start");
                Assert.That(cut.GetEndOfAllocation(), Is.EqualTo(plan.Time.End), "End");

                // Check action - should now wait until recording ends
                next = cut.GetNextActivity(now, callback);

                // Test
                Assert.That(next, Is.InstanceOf<WaitActivity>());

                // Advance
                now = ((WaitActivity)next).RetestTime;

                // Test
                Assert.That(now, Is.EqualTo(plan.Time.End));

                // Check action - recording is done and can be terminated
                next = cut.GetNextActivity(now, callback);

                // Test
                Assert.That(next, Is.InstanceOf<StopActivity>());
                Assert.That(((StopActivity)next).UniqueIdentifier, Is.EqualTo(play1.UniqueIdentifier));

                // Do it
                cut.Stop(play1.UniqueIdentifier);

                // Retest - nothing more to do
                Assert.That(cut.GetNextActivity(now, callback), Is.Null);
            }
        }

        /// <summary>
        /// Eine Aufzeichnung mit Wiederholung wird bei der Folgeplanung korrekt ausgeblendet.
        /// </summary>
        [Test]
        public void WillScheduleSingleRepeatedTask()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Add a single device
                cut.Add(ResourceMock.Device1);

                // Time to use
                var now = DateTime.UtcNow;

                // Create the recording
                var play1 = RecordingDefinition.Create(this, "test", Guid.NewGuid(), [ResourceMock.Device1], SourceMock.Source1Group1Free, now.AddMinutes(10), TimeSpan.FromMinutes(100), new DateTime(2100, 12, 31), DayOfWeek.Monday);

                // Get the schedule
                var scheduler = cut.CreateScheduler();

                // Load recordings
                scheduler.Add(play1);

                // Get all 
                var plan = scheduler.GetSchedules(now).Take(100).ToArray();

                // Validate
                Assert.That(plan.Length, Is.EqualTo(100), "Initial Count");
                Assert.That(plan[0].Time.LocalStart.DayOfWeek, Is.EqualTo(DayOfWeek.Monday), "Initial Start");
                Assert.That(cut.GetEndOfAllocation(), Is.Null, "Initial Allocation");

                // Mark as started
                Assert.That(cut.Start(plan[0]), Is.True, "Start");

                // Get the schedule
                scheduler = cut.CreateScheduler();

                // Load recordings
                scheduler.Add(play1);

                // Test
                Assert.That(scheduler.GetSchedules(now).Any(), Is.False, "Ignore Recording");
                Assert.That(cut.GetEndOfAllocation(), Is.EqualTo(plan[0].Time.End), "End");
            }
        }

        /// <summary>
        /// Eine Aufzeichnung mit Wiederholung wird bei der Folgeplanung korrekt ausgeblendet.
        /// </summary>
        [Test]
        public void WillNotShowCurrentRecordingInPlan()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Add a single device
                cut.Add(ResourceMock.Device1);

                // Time to use
                var now = DateTime.UtcNow;

                // Create the recording
                var play1 = RecordingDefinition.Create(this, "test", Guid.NewGuid(), [ResourceMock.Device1], SourceMock.Source1Group1Free, now.AddMinutes(10), TimeSpan.FromMinutes(100), new DateTime(2100, 12, 31), DayOfWeek.Monday);

                // Loading helper
                Func<RecordingScheduler, DateTime, IEnumerable<IScheduleInformation>> loader =
                    (s, d) =>
                    {
                        // Load recording
                        s.Add(play1);

                        // Report
                        return s.GetSchedules(d);
                    };

                // Find empty list
                var initial = cut.GetSchedules(now, loader).Take(2).ToArray();

                // Start the first one
                Assert.That(cut.Start(initial[0]), Is.True);

                // Recheck the list
                var follower = cut.GetSchedules(now, loader).First();

                // Make sure that first is skipped
                Assert.That(follower.Time.Start, Is.EqualTo(initial[1].Time.Start));
            }
        }

        /// <summary>
        /// Eine Aufzeichnung mit Wiederholung wird bei der Folgeplanung nicht eingeblendet, wenn die Laufzeit verlängert wurde.
        /// </summary>
        [Test]
        public void WillNotShowCurrentRecordingInPlanIfExtended()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Add a single device
                cut.Add(ResourceMock.Device1);

                // Time to use
                var now = DateTime.UtcNow;

                // Create the recording
                var play1 = RecordingDefinition.Create(this, "test", Guid.NewGuid(), [ResourceMock.Device1], SourceMock.Source1Group1Free, now.AddMinutes(10), TimeSpan.FromMinutes(100), new DateTime(2100, 12, 31), DayOfWeek.Monday);

                // Loading helper
                Func<RecordingScheduler, DateTime, IEnumerable<IScheduleInformation>> loader =
                    (s, d) =>
                    {
                        // Load recording
                        s.Add(play1);

                        // Report
                        return s.GetSchedules(d);
                    };

                // Find empty list
                var initial = cut.GetSchedules(now, loader).Take(2).ToArray();

                // Start the first one
                Assert.That(cut.Start(initial[0]), Is.True);

                // Move it
                Assert.That(cut.Modify(initial[0].Definition.UniqueIdentifier, initial[0].Time.End.AddMinutes(20)), Is.True);

                // Recheck the list
                var follower = cut.GetSchedules(now, loader).First();

                // Make sure that first is skipped
                Assert.That(follower.Time.Start, Is.EqualTo(initial[1].Time.Start));
            }
        }

        /// <summary>
        /// Eine Aufzeichnung mit Wiederholung wird bei der Folgeplanung eingeblendet, wenn die Laufzeit verkürzt wurde.
        /// </summary>
        [Test]
        public void WillNotShowCurrentRecordingInPlanIfCut()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Add a single device
                cut.Add(ResourceMock.Device1);

                // Time to use
                var now = DateTime.UtcNow;

                // Create the recording
                var play1 = RecordingDefinition.Create(this, "test", Guid.NewGuid(), [ResourceMock.Device1], SourceMock.Source1Group1Free, now.AddMinutes(10), TimeSpan.FromMinutes(100), new DateTime(2100, 12, 31), DayOfWeek.Monday);

                // Loading helper
                Func<RecordingScheduler, DateTime, IEnumerable<IScheduleInformation>> loader =
                    (s, d) =>
                    {
                        // Load recording
                        s.Add(play1);

                        // Report
                        return s.GetSchedules(d);
                    };

                // Find empty list
                var initial = cut.GetSchedules(now, loader).Take(2).ToArray();

                // Start the first one
                Assert.That(cut.Start(initial[0]), Is.True);

                // Move it
                Assert.That(cut.Modify(initial[0].Definition.UniqueIdentifier, initial[0].Time.End.AddMinutes(-20)), Is.True);

                // Recheck the list
                var follower = cut.GetSchedules(now, loader).First();

                // Make sure that first is skipped
                Assert.That(follower.Time.Start, Is.EqualTo(initial[1].Time.Start));
            }
        }


        /// <summary>
        /// Eine laufende Aufgabe wird korrekt berücksichtigt.
        /// </summary>
        [Test]
        public void CanAddRecordingOverlappingRunning()
        {
            // Create component under test
            using (var cut = ResourceManager.Create(StringComparer.InvariantCultureIgnoreCase))
            {
                // Add a single device
                cut.Add(ResourceMock.Device1);

                // Time to use
                var now = new DateTime(2013, 12, 3, 17, 0, 0, DateTimeKind.Utc);

                // Start a task
                Assert.That(cut.Start(ResourceMock.Device1, null, Guid.NewGuid(), "EPG", now, now.AddMinutes(20)), Is.True, "epg");

                // Create the recording
                var plan1 = RecordingDefinition.Create(false, "testA", Guid.NewGuid(), null, SourceMock.Source1Group1Free, now.AddMinutes(1), TimeSpan.FromMinutes(120));
                var plan2 = RecordingDefinition.Create(false, "testB", Guid.NewGuid(), null, SourceMock.Source1Group2Free, now.AddDays(1).AddHours(1), TimeSpan.FromMinutes(30));
                var plan3 = RecordingDefinition.Create(false, "testC", Guid.NewGuid(), null, SourceMock.Source1Group3Free, now.AddDays(1).AddHours(2), TimeSpan.FromMinutes(30));

                // Loading helper
                Func<RecordingScheduler, DateTime, IEnumerable<IScheduleInformation>> loader =
                    (s, d) =>
                    {
                        // Load recording
                        s.Add(plan1);
                        s.Add(plan2);
                        s.Add(plan3);

                        // Report
                        return s.GetSchedules(d);
                    };

                // Find empty list
                var schedules = cut.GetSchedules(now, loader).ToArray();

                // Validate
                Assert.That(schedules.Count(s => s.Resource != null), Is.EqualTo(3), "#");
            }
        }
    }
}
