﻿namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Beschreibt den Zeitraum einer einzelnen Aufzeichnung.
    /// </summary>
    public struct PlannedTime
    {
        /// <summary>
        /// Der Startzeitpunkt in UTC / GMT Notation.
        /// </summary>
        public DateTime Start;

        /// <summary>
        /// Die Dauer der Aufzeichnung.
        /// </summary>
        public TimeSpan Duration;

        /// <summary>
        /// Meldet den Start der Aufzeichnung in der lokalen Zeitzone.
        /// </summary>
        public readonly DateTime LocalStart => Start.ToLocalTime();

        /// <summary>
        /// Meldet das Ende der Aufzeichnung in UTC / GMT Notation.
        /// </summary>
        public readonly DateTime End => Start + Duration;

        /// <summary>
        /// Meldet das Ende der Aufzeichnung in der lokalen Zeitzone.
        /// </summary>
        public readonly DateTime LocalEnd => End.ToLocalTime();
    }
}
