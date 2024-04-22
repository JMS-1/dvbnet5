using Microsoft.Extensions.Logging;

namespace JMS.DVB.NET.Recording.Services;

/// <summary>
/// Global logging helper.
/// </summary>
/// <remarks>LEAF SERVICE</remarks>
/// <param name="configuration"></param>
/// <param name="logger"></param>
public class Logger(IVCRConfiguration configuration, ILogger<VCRServer> logger) : ILogger
{
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
