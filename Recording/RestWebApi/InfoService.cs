namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt die aktuelle Version des VCR.NET Recording Service.
    /// </summary>
    public class InfoService
    {
        /// <summary>
        /// Die Zeichenkette mit den Versionsdaten.
        /// </summary>
        public string Version { get; set; } = null!;

        /// <summary>
        /// Die Zeichenkette mit den Versionsdaten.
        /// </summary>
        public string InstalledVersion { get; set; } = null!;

        /// <summary>
        /// Gesetzt, wenn mindestens ein Gerät in Verwendung ist.
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Gesetzt, wenn der aktuelle Anwender ein Administrator ist.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Gesetzt, wenn es möglich ist, die Liste der Quellen zu aktualisieren.
        /// </summary>
        public bool SourceScanEnabled { get; set; }

        /// <summary>
        /// Gesetzt, wenn es möglich ist, die Programmzeitschriften zu aktualisieren.
        /// </summary>
        public bool GuideUpdateEnabled { get; set; }

        /// <summary>
        /// Gesetzt, wenn noch Erweiterungen aktiv sind.
        /// </summary>
        public bool HasPendingExtensions { get; set; }

        /// <summary>
        /// Die Namen aller Geräteprofile.
        /// </summary>
        public string[] ProfilesNames { get; set; } = null!;
    }
}
