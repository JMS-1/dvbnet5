using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace JMS.DVB.NET.Recording
{
    partial class VCRServer
    {
        /// <summary>
        /// Ereignisprotokoll f�r alle Meldungen, die mit Aufzeichnungen in Verbindung stehen.
        /// </summary>
        public static readonly ILogger<VCRServer> Logger = new NullLogger<VCRServer>();

        /// <summary>
        /// Tr�gt eine <see cref="Exception"/> ins Ereignisprotokoll ein, wenn die Konfiguration
        /// des VCR.NET Recording Service die Protokollierung von Fehlern gestattet.
        /// </summary>
        /// <param name="e">Abgefangener Fehler, eingetragen wird 
        /// <see cref="Exception.ToString"/>.</param>
        public static void Log(Exception e) => LogError("{0}", e);

        /// <summary>
        /// Tr�gt eine Fehlermeldung ins Ereignisprotokoll ein, wenn die Konfiguration
        /// des VCR.NET Recording Service die Protokollierung von Fehlern gestattet.
        /// </summary>
        /// <param name="format">Format f�r den Aufbau der Fehlermeldung.</param>
        /// <param name="args">Parameter f�r den Aufbau der Fehlermeldung.</param>
        public static void LogError(string format, params object[] args) => Log(LoggingLevel.Errors, format, args);

        /// <summary>
        /// Check if an event of the indicated level should be reported to
        /// the event log of Windows.
        /// </summary>
        /// <param name="reportLevel">Some logging level.</param>
        /// <returns>Set, if the logging level configured requires
        /// the event to be logged.</returns>
        public static bool ShouldLog(LoggingLevel reportLevel) => (reportLevel >= VCRConfiguration.Current.LoggingLevel);

        /// <summary>
        /// Tr�gt eine Meldung ins Ereignisprotokoll ein, wenn die Schwere der Meldung
        /// gem�� der Konfiguration des VCR.NET Recording Service eine Protokollierung
        /// gestattet.
        /// </summary>
        /// <param name="level">Schwere der Meldung.<seealso cref="ShouldLog"/></param>
        /// <param name="format">Format f�r den Aufbau der Meldung.</param>
        /// <param name="args">Parameter f�r den Aufbau der Meldung.</param>
        public static void Log(LoggingLevel level, string format, params object[] args)
        {
            // Nothing more to do
            if (!ShouldLog(level))
                return;

            // Report
            Logger.LogInformation(string.Format(format, args));
        }
    }
}
