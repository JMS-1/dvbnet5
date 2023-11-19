using JMS.DVB.Algorithms.Scheduler;

namespace JMS.DVB.NET.Recording.Persistence
{
    /// <summary>
    /// Beschreibt eine Ausnahme f�r eine sich wiederholende Aufzeichnung.
    /// </summary>
    [Serializable]
    public class VCRScheduleException
    {
        /// <summary>
        /// Liest oder setzt, an welchem Tag die Ausnahme ber�cksichtigt werden soll.
        /// </summary>
        public DateTime When { get; set; }

        /// <summary>
        /// Liest oder setzt die ver�nderte Dauer der Aufzeichnung. Ist dieser Wert <i>0</i>,
        /// so wird keine Aufzeichnung ausgef�hrt. Ist kein Wert angegeben, so wird die urspr�ngliche
        /// Dauer verwendet.
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// Liest oder setzt, um wieviele Minuten die Aufzeinung verschoben werden soll.
        /// </summary>
        public int? ShiftTime { get; set; }

        /// <summary>
        /// Meldet, ob Ausnahmewerte gesetzt sind.
        /// </summary>
        public bool IsEmpty => !Duration.HasValue && !ShiftTime.HasValue;

        /// <summary>
        /// Wandelt diese Ausnahmebeschreibung in ein entsprechendes �auivalent f�r die Planung.
        /// </summary>
        /// <param name="duration">Die Dauer der zugeh�rigen Aufzeichnung.</param>
        /// <returns>Die gew�nschte �quivalente Repr�sentation der Ausnahmeregel.</returns>
        public PlanException ToPlanException(TimeSpan duration)
        {
            // Create new
            var exception = new PlanException { ExceptionDate = When.Date };

            // Set data
            if (Duration.HasValue)
                exception.DurationDelta = TimeSpan.FromMinutes(Duration.Value) - duration;
            if (ShiftTime.HasValue)
                exception.StartDelta = TimeSpan.FromMinutes(ShiftTime.Value);

            // Report
            return exception;
        }
    }
}
