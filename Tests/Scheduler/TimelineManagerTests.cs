using JMS.DVB.Algorithms.Scheduler;

namespace JMS.DVB.SchedulerTests
{
    /// <summary>
    /// Prüft die Manipulationen einer Zeitlinienverwaltung.
    /// </summary>
    [TestFixture]
    public class TimelineManagerTests
    {
        /// <summary>
        /// Unsere eigene Implementierung einer Zeitlinie.
        /// </summary>
        private class Timeline : TimelineManager<int>
        {
            /// <summary>
            /// Führt die Daten zweier Bereiche zusammen.
            /// </summary>
            /// <param name="existing">Die Daten des existierenden Bereiches.</param>
            /// <param name="added">Die Daten des neuen Bereichs.</param>
            /// <returns>Die Zusammenfassung der Daten.</returns>
            protected override int Merge(int existing, int added)
            {
                // Simply add
                return existing + added;
            }
        }

        /// <summary>
        /// Es ist möglich, einen einzelnen Bereich anzulegen.
        /// </summary>
        [Test]
        public void CanAddASingleRangeByTimeSpan()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays(5);
            var range = Timeline.Range.Create(now, TimeSpan.FromMinutes(120), 0);

            // Create component under test
            var cut =
                new Timeline
                {
                    range,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.That(ranges.Length, Is.EqualTo(1), "#ranges");
            Assert.That(ranges[0].Start, Is.EqualTo(now), "start");
            Assert.That(ranges[0].End, Is.EqualTo(now.AddMinutes(120)), "end");
            Assert.That(ranges[0].Data, Is.EqualTo(0), "data");
        }

        /// <summary>
        /// Es ist möglich, einen einzelnen Bereich anzulegen.
        /// </summary>
        [Test]
        public void CanAddASingleRangeByEndTime()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays(5);
            var range = Timeline.Range.Create(now, now.AddHours(2), 0);

            // Create component under test
            var cut =
                new Timeline
                {
                    range,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.That(ranges.Length, Is.EqualTo(1), "#ranges");
            Assert.That(ranges[0].Start, Is.EqualTo(now), "start");
            Assert.That(ranges[0].End, Is.EqualTo(now.AddMinutes(120)), "end");
            Assert.That(ranges[0].Data, Is.EqualTo(0), "data");
        }

        /// <summary>
        /// Es ist möglich, einen neuen Bereich ganz am Anfang der Liste anzulegen.
        /// </summary>
        [Test]
        public void CanAddOneTimeBeforeTheFirst()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays(5);
            var range1 = Timeline.Range.Create(now, now.AddHours(2), 1);
            var range2 = Timeline.Range.Create(now.AddHours(-2), now, 2);

            // Create component under test
            var cut =
                new Timeline
                {
                    range1,
                    range2,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.That(ranges.Length, Is.EqualTo(2), "#ranges");
            Assert.That(ranges[0].Start, Is.EqualTo(now.AddMinutes(-120)), "start 0");
            Assert.That(ranges[0].End, Is.EqualTo(now), "end 0");
            Assert.That(ranges[0].Data, Is.EqualTo(2), "data 0");
            Assert.That(ranges[1].Start, Is.EqualTo(now), "start 1");
            Assert.That(ranges[1].End, Is.EqualTo(now.AddMinutes(120)), "end 1");
            Assert.That(ranges[1].Data, Is.EqualTo(1), "data 1");
        }

        /// <summary>
        /// Es ist möglich, einen neuen Bereich in die Mitte der Liste zu legen.
        /// </summary>
        [Test]
        public void CanAddOneTimeInBetween()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays(5);
            var range1 = Timeline.Range.Create(now, now.AddHours(2), 1);
            var range2 = Timeline.Range.Create(now.AddHours(-4), now.AddHours(-2), 2);
            var range3 = Timeline.Range.Create(now.AddHours(-2), now, 4);

            // Create component under test
            var cut =
                new Timeline
                {
                    range1,
                    range2,
                    range3,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.That(ranges.Length, Is.EqualTo(3), "#ranges");
            Assert.That(ranges[0].Start, Is.EqualTo(now.AddMinutes(-240)), "start 0");
            Assert.That(ranges[0].End, Is.EqualTo(now.AddMinutes(-120)), "end 0");
            Assert.That(ranges[0].Data, Is.EqualTo(2), "data 0");
            Assert.That(ranges[1].Start, Is.EqualTo(now.AddMinutes(-120)), "start 1");
            Assert.That(ranges[1].End, Is.EqualTo(now), "end 1");
            Assert.That(ranges[1].Data, Is.EqualTo(4), "data 1");
            Assert.That(ranges[2].Start, Is.EqualTo(now), "start 2");
            Assert.That(ranges[2].End, Is.EqualTo(now.AddMinutes(120)), "end 2");
            Assert.That(ranges[2].Data, Is.EqualTo(1), "data 2");
        }

        /// <summary>
        /// Es ist möglich, einen Bereicht zu teilen.
        /// </summary>
        [Test]
        public void CanSplitAndMerge()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays(5);
            var range1 = Timeline.Range.Create(now, now.AddHours(2), 1);
            var range2 = Timeline.Range.Create(now.AddHours(-1), now.AddHours(1), 2);

            // Create component under test
            var cut =
                new Timeline
                {
                    range1,
                    range2,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.That(ranges.Length, Is.EqualTo(3), "#ranges");
            Assert.That(ranges[0].Start, Is.EqualTo(now.AddMinutes(-60)), "start 0");
            Assert.That(ranges[0].End, Is.EqualTo(now), "end 0");
            Assert.That(ranges[0].Data, Is.EqualTo(2), "data 0");
            Assert.That(ranges[1].Start, Is.EqualTo(now), "start 1");
            Assert.That(ranges[1].End, Is.EqualTo(now.AddMinutes(60)), "end 1");
            Assert.That(ranges[1].Data, Is.EqualTo(3), "data 1");
            Assert.That(ranges[2].Start, Is.EqualTo(now.AddMinutes(60)), "start 2");
            Assert.That(ranges[2].End, Is.EqualTo(now.AddMinutes(120)), "end 2");
            Assert.That(ranges[2].Data, Is.EqualTo(1), "data 2");
        }

        /// <summary>
        /// Es ist möglich, eine Lücke zwischen zwei Bereichen zu füllen.
        /// </summary>
        [Test]
        public void CanMergeGap()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays(5);
            var range1 = Timeline.Range.Create(now.AddHours(-1), now.AddHours(1), 1);
            var range2 = Timeline.Range.Create(now.AddHours(2), now.AddHours(4), 2);
            var range3 = Timeline.Range.Create(now, now.AddHours(3), 4);

            // Create component under test
            var cut =
                new Timeline
                {
                    range1,
                    range2,
                    range3,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.That(ranges.Length, Is.EqualTo(5), "#ranges");
            Assert.That(ranges[0].Start, Is.EqualTo(now.AddMinutes(-60)), "start 0");
            Assert.That(ranges[0].End, Is.EqualTo(now), "end 0");
            Assert.That(ranges[0].Data, Is.EqualTo(1), "data 0");
            Assert.That(ranges[1].Start, Is.EqualTo(now), "start 1");
            Assert.That(ranges[1].End, Is.EqualTo(now.AddMinutes(60)), "end 1");
            Assert.That(ranges[1].Data, Is.EqualTo(5), "data 1");
            Assert.That(ranges[2].Start, Is.EqualTo(now.AddMinutes(60)), "start 2");
            Assert.That(ranges[2].End, Is.EqualTo(now.AddMinutes(120)), "end 2");
            Assert.That(ranges[2].Data, Is.EqualTo(4), "data 2");
            Assert.That(ranges[3].Start, Is.EqualTo(now.AddMinutes(120)), "start 3");
            Assert.That(ranges[3].End, Is.EqualTo(now.AddMinutes(180)), "end 3");
            Assert.That(ranges[3].Data, Is.EqualTo(6), "data 3");
            Assert.That(ranges[4].Start, Is.EqualTo(now.AddMinutes(180)), "start 4");
            Assert.That(ranges[4].End, Is.EqualTo(now.AddMinutes(240)), "end 4");
            Assert.That(ranges[4].Data, Is.EqualTo(2), "data 4");
        }

        /// <summary>
        /// Zwei Bereiche mit gleicher Startzeit können überlagert werden.
        /// </summary>
        [Test]
        public void CanAlignAtStart()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays(5);
            var range1 = Timeline.Range.Create(now, now.AddHours(1), 1);
            var range2 = Timeline.Range.Create(now, now.AddHours(2), 2);

            // Create component under test
            var cut =
                new Timeline
                {
                    range1,
                    range2,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.That(ranges.Length, Is.EqualTo(2), "#ranges");
            Assert.That(ranges[0].Start, Is.EqualTo(now), "start 0");
            Assert.That(ranges[0].End, Is.EqualTo(now.AddMinutes(60)), "end 0");
            Assert.That(ranges[0].Data, Is.EqualTo(3), "data 0");
            Assert.That(ranges[1].Start, Is.EqualTo(now.AddMinutes(60)), "start 1");
            Assert.That(ranges[1].End, Is.EqualTo(now.AddMinutes(120)), "end 1");
            Assert.That(ranges[1].Data, Is.EqualTo(2), "data 1");
        }

        /// <summary>
        /// Zwei Bereiche mit gleicher Endzeit können überlagert werden.
        /// </summary>
        [Test]
        public void CanAlignAtEnd()
        {
            // Create test data
            var now = DateTime.UtcNow.Date.AddDays(5);
            var range1 = Timeline.Range.Create(now, now.AddHours(1), 1);
            var range2 = Timeline.Range.Create(now.AddHours(-1), now.AddHours(1), 2);

            // Create component under test
            var cut =
                new Timeline
                {
                    range1,
                    range2,
                };

            // Read-out
            var ranges = cut.ToArray();

            // Validate
            Assert.That(ranges.Length, Is.EqualTo(2), "#ranges");
            Assert.That(ranges[0].Start, Is.EqualTo(now.AddMinutes(-60)), "start 0");
            Assert.That(ranges[0].End, Is.EqualTo(now), "end 0");
            Assert.That(ranges[0].Data, Is.EqualTo(2), "data 0");
            Assert.That(ranges[1].Start, Is.EqualTo(now), "start 1");
            Assert.That(ranges[1].End, Is.EqualTo(now.AddMinutes(60)), "end 1");
            Assert.That(ranges[1].Data, Is.EqualTo(3), "data 1");
        }

        /// <summary>
        /// Füllt die Zeitschiene mit Zufallswerten.
        /// </summary>
        [Test]
        public void RandomFillTimeline()
        {
            // Create random generator
            var rnd = new Random(Environment.TickCount);

            // Create scratch area
            var cnt = new int[rnd.Next(2500, 7500)];
            var cmp = new int[cnt.Length];

            // Create component under test
            var now = DateTime.UtcNow.Date;
            var cut = new Timeline();

            // Process
            for (var n = rnd.Next(1000, 5000); n-- > 0;)
            {
                // Choose range
                var startIndex = rnd.Next(cnt.Length - 10);
                var endIndex = startIndex + rnd.Next(10);

                // Register
                cut.Add(Timeline.Range.Create(now.AddMinutes(startIndex), now.AddMinutes(endIndex + 1), 1));

                // Count self
                while (startIndex <= endIndex)
                    cnt[startIndex++]++;
            }

            // Helper
            var lastEnd = now;

            // Resolve
            foreach (var range in cut)
            {
                // Validate
                Assert.That(range.Start >= lastEnd, Is.True, "start {0}", range);
                Assert.That(range.End > range.Start, Is.True, "duration {0}", range);

                // Get index
                var startIndex = (int)(range.Start - now).TotalMinutes;
                var endIndex = (int)(range.End - now).TotalMinutes;

                // Fill
                while (startIndex < endIndex)
                    cmp[startIndex++] += range.Data;

                // Update
                lastEnd = range.End;
            }

            // Validate
            CollectionAssert.AreEqual(cnt, cmp);

            // See if we checked the good stuff
            Assert.That(cnt.Min() == 0, Is.True, "min");
            Assert.That(cnt.Max() >= 4, Is.True, "max");
        }
    }
}
