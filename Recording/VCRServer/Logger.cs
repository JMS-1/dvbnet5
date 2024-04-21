using Microsoft.Extensions.Logging;

namespace JMS.DVB.NET.Recording
{
    /// <summary>
    /// Global logging helper.
    /// </summary>
    /// <remarks>LEAF SERVICE</remarks>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    public class Logger(VCRConfiguration configuration, ILogger<VCRServer> logger)
    {
        /// <summary>
        /// Trägt eine <see cref="Exception"/> ins Ereignisprotokoll ein, wenn die Konfiguration
        /// des VCR.NET Recording Service die Protokollierung von Fehlern gestattet.
        /// </summary>
        /// <param name="e">Abgefangener Fehler, eingetragen wird 
        /// <see cref="Exception.ToString"/>.</param>
        public void Log(Exception e) => LogError("{0}", e);

        /// <summary>
        /// Trägt eine Fehlermeldung ins Ereignisprotokoll ein, wenn die Konfiguration
        /// des VCR.NET Recording Service die Protokollierung von Fehlern gestattet.
        /// </summary>
        /// <param name="format">Format für den Aufbau der Fehlermeldung.</param>
        /// <param name="args">Parameter für den Aufbau der Fehlermeldung.</param>
        public void LogError(string format, params object[] args) => Log(LoggingLevel.Errors, format, args);

        /// <summary>
        /// Check if an event of the indicated level should be reported to
        /// the event log of Windows.
        /// </summary>
        /// <param name="reportLevel">Some logging level.</param>
        /// <returns>Set, if the logging level configured requires
        /// the event to be logged.</returns>
        public bool ShouldLog(LoggingLevel reportLevel) => reportLevel >= configuration.LoggingLevel;

        /// <summary>
        /// Trägt eine Meldung ins Ereignisprotokoll ein, wenn die Schwere der Meldung
        /// gemäß der Konfiguration des VCR.NET Recording Service eine Protokollierung
        /// gestattet.
        /// </summary>
        /// <param name="level">Schwere der Meldung.<seealso cref="ShouldLog"/></param>
        /// <param name="format">Format für den Aufbau der Meldung.</param>
        /// <param name="args">Parameter für den Aufbau der Meldung.</param>
        public void Log(LoggingLevel level, string format, params object[] args)
        {
            // Nothing more to do
            if (!ShouldLog(level))
                return;

            // Report
            logger.LogInformation(string.Format(format, args));
        }
    }
}
