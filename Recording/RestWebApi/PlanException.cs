using JMS.DVB.NET.Recording.Persistence;
using System.Globalization;
using System.Text.Json.Serialization;


namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt eine Ausnahmeregel.
    /// </summary>
    public class PlanException
    {
        /// <summary>
        /// Der Referenzzeitpunkt für die Ausnahmeregel.
        /// </summary>
        public string ExceptionDateTicks
        {
            get { return ExceptionDate.Ticks.ToString(CultureInfo.InvariantCulture); }
            set { ExceptionDate = new DateTime(long.Parse(value), DateTimeKind.Utc); }
        }

        /// <summary>
        /// Meldet den Referenzzeitpunkt in einer für den Client geeigneten Notation. Dieser
        /// Eigenschaft kann nur ausgelesen werden.
        /// </summary>
        public string ExceptionDateUnix
        {
            get { return ((ExceptionDate.Ticks - Tools.UnixTimeBias) / Tools.UnixTimeFactor).ToString(CultureInfo.InvariantCulture); }
            set { }
        }

        /// <summary>
        /// Der Referenzzeitpunkt für die Ausnahmeregel.
        /// </summary>
        [JsonIgnore]
        public DateTime ExceptionDate { get; set; }

        /// <summary>
        /// Die Verschiebung der Aufzeichnung an dem Referenztag.
        /// </summary>
        public int ExceptionStartShift { get; set; }

        /// <summary>
        /// Die Änderung der Aufzeichnungsdauer an dem Referenztag.
        /// </summary>
        public int ExceptionDurationDelta { get; set; }

        /// <summary>
        /// Der geplante Startzeitpunkt.
        /// </summary>
        public string PlannedStartISO
        {
            get { return PlannedStart.ToString("o"); }
            set { PlannedStart = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind); }
        }

        /// <summary>
        /// Der geplante Startzeitpunkt.
        /// </summary>
        [JsonIgnore]
        public DateTime PlannedStart { get; set; }

        /// <summary>
        /// Die Laufzeit der Aufzeichnung in Minuten.
        /// </summary>
        public int PlannedDuration { get; set; }

        /// <summary>
        /// Wird für die Deserialisierung benötigt.
        /// </summary>
        public PlanException()
        {
        }

        /// <summary>
        /// Erstellt eine neue Ausnahmebeschreibung.
        /// </summary>
        /// <param name="exception">Die Ausnahmedaten.</param>
        /// <param name="schedule">Die zugehörige Aufzeichnung.</param>
        private PlanException(VCRScheduleException exception, VCRSchedule schedule)
        {
            // Remember
            ExceptionDurationDelta = exception.Duration.GetValueOrDefault(schedule.Duration) - schedule.Duration;
            ExceptionDate = new DateTime(exception.When.Date.Ticks, DateTimeKind.Utc);
            ExceptionStartShift = exception.ShiftTime.GetValueOrDefault(0);

            // Start berechnen
            PlannedStart = (new DateTime(exception.When.Date.Ticks, DateTimeKind.Local) + schedule.FirstStart.ToLocalTime().TimeOfDay).ToUniversalTime();
            PlannedDuration = schedule.Duration;
        }

        /// <summary>
        /// Erstellt eine neue Ausnahmebeschreibung.
        /// </summary>
        /// <param name="exception">Die Ausnahmedaten.</param>
        /// <param name="schedule">Die zugehörige Aufzeichnung.</param>
        /// <returns>Die neue Ausnahmebeschreibung.</returns>
        public static PlanException Create(VCRScheduleException exception, VCRSchedule schedule)
        {
            // Validate
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(schedule);

            // Forward
            return new PlanException(exception, schedule);
        }
    }
}
