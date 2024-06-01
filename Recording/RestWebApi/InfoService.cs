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
        public required string Version { get; set; }

        /// <summary>
        /// Die Zeichenkette mit den Versionsdaten.
        /// </summary>
        public required string InstalledVersion { get; set; }

        /// <summary>
        /// Gesetzt, wenn mindestens ein Gerät in Verwendung ist.
        /// </summary>
        public required bool IsRunning { get; set; }

        /// <summary>
        /// Gesetzt, wenn es möglich ist, die Liste der Quellen zu aktualisieren.
        /// </summary>
        public required bool SourceScanEnabled { get; set; }

        /// <summary>
        /// Gesetzt, wenn es möglich ist, die Programmzeitschriften zu aktualisieren.
        /// </summary>
        public required bool GuideUpdateEnabled { get; set; }

        /// <summary>
        /// Gesetzt, wenn noch Erweiterungen aktiv sind.
        /// </summary>
        public required bool HasPendingExtensions { get; set; }

        /// <summary>
        /// Die Namen aller Geräteprofile.
        /// </summary>
        public required string[] ProfilesNames { get; set; }

        /// <summary>
        /// Der FTP Port des Servers.
        /// </summary>
        public required ushort FTPPort { get; set; }
    }
}
