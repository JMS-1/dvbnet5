using System.Xml.Serialization;

namespace JMS.DVB.NET.Recording.Persistence
{
    /// <summary>
    /// Beschreibt eine einzelne Aufzeichnung mit einem optionalen Wiederholungsintervall.
    /// </summary>
    [Serializable]
    public class VCRSchedule
    {
        /// <summary>
        /// Das maximal erlaubte Datum - ausreichend in der Zukunft.
        /// </summary>
        public static readonly DateTime MaxMovableDay = new(2999, 12, 31);

        /// <summary>
        /// Abbildung der .NET Wochentage auf die interne Darstellung in <see cref="VCRDay"/>.
        /// </summary>
        static private readonly Dictionary<DayOfWeek, VCRDay> m_DayMapper = new()
            {
                { DayOfWeek.Monday, VCRDay.Monday },
                { DayOfWeek.Tuesday, VCRDay.Tuesday },
                { DayOfWeek.Wednesday, VCRDay.Wednesday },
                { DayOfWeek.Thursday, VCRDay.Thursday },
                { DayOfWeek.Friday, VCRDay.Friday },
                { DayOfWeek.Saturday, VCRDay.Saturday },
                { DayOfWeek.Sunday, VCRDay.Sunday },
            };

        /// <summary>
        /// Für sich wiederholende Aufzeichnungen das früheste Datum, an dem die
        /// nächste Aufzeichnung stattfinden darf.
        /// </summary>
        public DateTime? NoStartBefore { get; set; }

        /// <summary>
        /// Datum und Uhrzeit, wann die Aufzeichnung das erste Mal ausgeführt werden soll.
        /// </summary>
        public DateTime FirstStart { get; set; }

        /// <summary>
        /// Datum, an dem die Aufzeichnung das letzte Mal ausgeführt werden soll.
        /// </summary>
        /// <remarks>
        /// Das Datum wird ignoriert, wenn die Aufzeichnung keine Wiederholungen verwendet.
        /// </remarks>
        public DateTime? LastDay { get; set; }

        /// <summary>
        /// Eindeutige Kennung der Aufzeichnung.
        /// </summary>
        public Guid? UniqueID { get; set; }

        /// <summary>
        /// Die gewünschte Quelle.
        /// </summary>
        public SourceSelection Source { get; set; } = null!;

        /// <summary>
        /// Die Datenströme, die aufgezeichnet werden sollen.
        /// </summary>
        public StreamSelection Streams { get; set; } = null!;

        /// <summary>
        /// Dauer der Aufzeichung in Minuten.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Optionaler Name der Aufzeichnung.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Maske zur Definition des Wiederholungsinterfalls.
        /// </summary>
        /// <remarks>
        /// Jedes Bit entspricht einem Wochentag aus <see cref="VCRDay"/>.
        /// </remarks>
        public VCRDay? Days { get; set; }

        /// <summary>
        /// Alle Ausnahmeregeln für eine sich wiederholende Aufzeichnung.
        /// </summary>
        [XmlElement("Exception")]
        public readonly List<VCRScheduleException> Exceptions = [];

        /// <summary>
        /// Setzt den nächsten möglichen Zeitpunkt für eine Wiederholung der Aufzeichnung.
        /// </summary>
        [XmlIgnore]
        public DateTime DoNotRestartBefore
        {
            set
            {
                // No need if in the past
                if (NoStartBefore.HasValue)
                    if (value <= NoStartBefore.Value)
                        return;

                // Remember
                NoStartBefore = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Prüft, ob diese Aufzeichnung noch einmal verwendet wird.
        /// </summary>
        [XmlIgnore]
        public bool IsActive
        {
            get
            {
                // Not allowed
                if (!UniqueID.HasValue)
                    return false;

                // Read recording parameters
                var now = DateTime.UtcNow;
                var duration = Duration;
                var start = FirstStart;

                // Direct test if not repeating 
                if (IsRepeatable)
                {
                    // Unlimited repetition always is a good candidate
                    if (!LastDay.HasValue)
                        return true;

                    // When will we record for the very last time
                    var lastDate = new DateTime(LastDay.Value.Date.Ticks, DateTimeKind.Local);
                    if (lastDate == MaxMovableDay)
                        return true;

                    // Attach to the start
                    var startLocal = start.ToLocalTime();
                    var startDate = new DateTime(startLocal.Date.Ticks, DateTimeKind.Local);

                    // Find some allowed date - no need to test more than a week
                    for (var day = 7; day-- > 0;)
                    {
                        // Yeah, can to it
                        if (MayRecordOn(lastDate))
                            break;

                        // Previous day
                        lastDate = lastDate.AddDays(-1);

                        // Falled back to a date before the first start
                        if (lastDate < startDate)
                            return false;
                    }

                    // Move time to the very last recording day
                    start = (lastDate + startLocal.TimeOfDay).ToUniversalTime();

                    // May need to correct for exception
                    var exception = GetException(lastDate);
                    if (exception != null)
                    {
                        // Correct
                        start = start.AddMinutes(exception.ShiftTime.GetValueOrDefault(0));
                        duration = exception.Duration.GetValueOrDefault(duration);
                    }
                }

                // Survive if we are not at or beyond the end of the recording
                return (now < (start + TimeSpan.FromMinutes(duration)));
            }
        }

        /// <summary>
        /// Meldet, ob für diese Aufzeichnung eine Wiederholung definiert ist.
        /// </summary>
        [XmlIgnore]
        private bool IsRepeatable => Days.HasValue && ((int)Days.Value != 0);

        /// <summary>
        /// Prüft, ob eine Aufzeichnung an dem angegebenen Wochentag erlaubt ist.
        /// </summary>
        /// <param name="day">Der genaue Aufzeichnungszeitpunkt in der lokalen Zeitzone.</param>
        /// <returns>Gesetzt, wenn eine Aufzeichnung erlaubt ist.</returns>
        private bool MayRecordOn(DateTime day) => Days.HasValue && ((Days.Value & GetDay(day.DayOfWeek)) != 0);

        /// <summary>
        /// Wandelt einen Wochentag in eine VCR.NET Kennzeichnung des Wochentags.
        /// </summary>
        /// <param name="day">Ein beliebiger Wochentag.</param>
        /// <returns>Die zugehörige Kennzeichnung.</returns>
        public static VCRDay GetDay(DayOfWeek day) => m_DayMapper[day];

        /// <summary>
        /// Ermittelt, ob für einen bestimmten Tag eine Ausnahme definiert ist.
        /// </summary>
        /// <param name="day">Der gewünschte Tag.</param>
        /// <returns>Die Ausnahme für diesen Tag oder <i>null</i>, wenn eine reguläre Aufzeichnung ausgeführt
        /// werden soll.</returns>
        public VCRScheduleException? GetException(DateTime day)
        {
            // Forward
            var date = day.Date;
            return Exceptions.Find(e => e.When.Date == date);
        }

        /// <summary>
        /// Ermittelt die Ausnahmeregel für einen bestimmten Tag.
        /// </summary>
        /// <param name="recordingEnd">Der Endzeitpunkt einer Aufzeichnung.</param>
        /// <returns>Die zugehörige Ausnahmeregel.</returns>
        public VCRScheduleException? FindException(DateTime recordingEnd)
        {
            // Not repeating
            if (!Days.HasValue)
                return null;

            // Get the local time we will record
            var time = FirstStart.ToLocalTime().TimeOfDay;

            // Get the day to check for
            var date = recordingEnd.ToLocalTime().Date;

            // Process day around check day
            for (int i = 5; i-- > 0;)
            {
                // Get the current date to test
                var referenceDay = date.AddDays(2 - i);

                // Wrong day of week
                if (!MayRecordOn(referenceDay))
                    continue;

                // Get the exception for the day
                var exception = GetException(referenceDay) ?? new VCRScheduleException { When = referenceDay };

                // Get the start and end
                var start = (referenceDay + time).ToUniversalTime().AddMinutes(exception.ShiftTime.GetValueOrDefault(0));
                var end = start.AddMinutes(exception.Duration.GetValueOrDefault(Duration));

                // Check for match
                if (end == recordingEnd)
                    return exception;
            }

            // Report
            return new VCRScheduleException { When = date };
        }

        /// <summary>
        /// Trägt eine Ausnahmeregelung für einen bestimmten Tag ein.
        /// </summary>
        /// <param name="day">Der gewünschte Tag.</param>
        /// <param name="exception">Die Ausnahmeregel oder <i>null</i>, wenn für den Tag keine Ausnahme
        /// definiert werden soll.</param>
        public void SetException(DateTime day, VCRScheduleException exception)
        {
            // Remove the entry
            var date = day.Date;
            Exceptions.RemoveAll(e => e.When.Date == date);

            // Append
            if (exception != null)
                if (exception.ShiftTime.HasValue || exception.Duration.HasValue)
                    Exceptions.Add(exception);
        }

        /// <summary>
        /// Entfernt alle Ausnahmeregeleungen, die bereits verstrichen sind.
        /// </summary>
        public void CleanupExceptions()
        {
            // Remove the entry
            var date = DateTime.Now.Date.AddDays(-1);
            Exceptions.RemoveAll(e => (e.When.Date < date) || ((e.Duration.HasValue && (e.Duration.Value < 0))));
        }

        /// <summary>
        /// Erstellt die Liste der Tage, an denen eine Aufzeichnung wiederholt werden soll.
        /// </summary>
        /// <returns>Die gewünschte Liste oder <i>null</i>, falls keinerlei Wiederholung erwünscht ist.</returns>
        public DayOfWeek[]? CreateRepeatPattern()
        {
            // Load pattern
            if (!IsRepeatable)
                return null;

            // Create the day pattern
            var repeatPattern = new HashSet<DayOfWeek>();
            for (int i = 7, mask = 0x40, days = (int)Days!.Value; i-- > 0; mask >>= 1)
                if ((days & mask) != 0)
                    repeatPattern.Add((DayOfWeek)((i + 1) % 7));

            // Report
            if (repeatPattern.Count < 1)
                return null;
            else
                return repeatPattern.ToArray();
        }

    }
}