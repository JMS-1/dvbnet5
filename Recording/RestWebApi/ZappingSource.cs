﻿namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt eine mögliche Datenquelle.
    /// </summary>
    public class ZappingSource : SourceInformation<ZappingSource>
    {
        /// <summary>
        /// Die eindeutige Kennung der Quelle.
        /// </summary>
        public string Source { get; set; } = null!;

        /// <summary>
        /// Führt individuelle Initialisierungen aus.
        /// </summary>
        /// <param name="station">Die Informationen zur Quelle.</param>
        protected override void OnCreate(Station station) => Source = SourceIdentifier.ToString(station)!.Replace(" ", "");
    }
}
