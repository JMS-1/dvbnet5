using JMS.DVB.NET.Recording.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace JMS.DVB.NET.Recording.Services.Logging;

/// <summary>
/// Global logging helper.
/// </summary>
/// <param name="configuration"></param>
/// <param name="logger"></param>
public class Logger<T>(IVCRConfiguration configuration, Microsoft.Extensions.Logging.ILogger<T> logger) : ILogger<T>
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
        switch (level)
        {
            case LoggingLevel.Full:
                logger.LogDebug(string.Format(format, args));
                break;
            case LoggingLevel.Security:
                logger.LogWarning(string.Format(format, args));
                break;
            case LoggingLevel.Schedules:
                logger.LogInformation(string.Format(format, args));
                break;
            case LoggingLevel.Errors:
                logger.LogError(string.Format(format, args));
                break;
            default:
                logger.LogCritical(string.Format(format, args));
                break;
        }
    }
}
