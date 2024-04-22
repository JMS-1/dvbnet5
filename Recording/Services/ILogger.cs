namespace JMS.DVB.NET.Recording.Services;

public interface ILogger
{
    /// <summary>
    /// Trägt eine Meldung ins Ereignisprotokoll ein, wenn die Schwere der Meldung
    /// gemäß der Konfiguration des VCR.NET Recording Service eine Protokollierung
    /// gestattet.
    /// </summary>
    /// <param name="level">Schwere der Meldung.<seealso cref="ShouldLog"/></param>
    /// <param name="format">Format für den Aufbau der Meldung.</param>
    /// <param name="args">Parameter für den Aufbau der Meldung.</param>
    void Log(LoggingLevel level, string format, params object[] args);
}
