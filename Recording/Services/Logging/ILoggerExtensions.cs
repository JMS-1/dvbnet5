namespace JMS.DVB.NET.Recording.Services.Logging;

public static class ILoggerExtensions
{
    /// <summary>
    /// Trägt eine Fehlermeldung ins Ereignisprotokoll ein, wenn die Konfiguration
    /// des VCR.NET Recording Service die Protokollierung von Fehlern gestattet.
    /// </summary>
    /// <param name="format">Format für den Aufbau der Fehlermeldung.</param>
    /// <param name="args">Parameter für den Aufbau der Fehlermeldung.</param>
    public static void LogError<T>(this ILogger<T> logger, string format, params object[] args) => logger.Log(LoggingLevel.Errors, format, args);

    /// <summary>
    /// Trägt eine <see cref="Exception"/> ins Ereignisprotokoll ein, wenn die Konfiguration
    /// des VCR.NET Recording Service die Protokollierung von Fehlern gestattet.
    /// </summary>
    /// <param name="e">Abgefangener Fehler, eingetragen wird 
    /// <see cref="Exception.ToString"/>.</param>
    public static void Log<T>(this ILogger<T> logger, Exception e) => logger.LogError("{0}", e);
}
