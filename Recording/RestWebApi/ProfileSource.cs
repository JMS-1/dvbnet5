namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt eine mögliche Datenquelle.
    /// </summary>
    public class ProfileSource : SourceInformation<ProfileSource>
    {
        /// <summary>
        /// Gesetzt, wenn es sich um einen Fernsehsender handelt.
        /// </summary>
        public bool IsTVStation { get; set; }

        /// <summary>
        /// Führt individuelle Initialisierungen aus.
        /// </summary>
        /// <param name="station">Die Informationen zur Quelle.</param>
        protected override void OnCreate(Station station) => IsTVStation = station.SourceType != SourceTypes.Radio;
    }
}
