#pragma warning disable CA1416 // Validate platform compatibility

using System.Diagnostics;
using System.Text;
using JMS.DVB.NET.Recording.Services.Logging;

namespace JMS.DVB.NET.Recording;

/// <summary>
/// Allgemeine Hilfsfunktionen.
/// </summary>
public static class Tools
{
    /// <summary>
    /// Der volle Pfad zu dem Regeldatei der Aufzeichnungsplanung.
    /// </summary>
    public static string ScheduleRulesPath => Path.Combine(ApplicationDirectory.FullName, "SchedulerRules.cmp");

    /// <summary>
    /// Die Zeitbasis für alle Zeitangaben.
    /// </summary>
    public static long UnixTimeBias = new DateTime(1970, 1, 1).Ticks;

    /// <summary>
    /// Umrechnungsfaktor für Zeitangaben.
    /// </summary>
    public static int UnixTimeFactor = 10000;

    /// <summary>
    /// Der Pfad zur Anwendung.
    /// </summary>
    /// <remarks>
    /// Dieser Pfad wird verwendet, um abh�ngige Dateien relativ zum
    /// VCR.NET Recording Service zu finden. Er kann in Testprogrammen
    /// Verändert werden.
    /// </remarks>
    public static string ExecutablePath => RunTimeLoader.GetDirectory("Recording").FullName;

    /// <summary>
    /// Synchronisiert Einträge in das spezielle Protokoll.
    /// </summary>
    public static object m_LoggingLock = new();

    /// <summary>
    /// Wird aktiviert, wenn Protokollinformationen in das Fenster des Debuggers geschrieben werden sollen.
    /// </summary>
    public static bool EnableTracing = false;

    /// <summary>
    /// Eine Kurzbezeichnung für die aktuelle Laufzeitumgebung für ProtokollEinträge.
    /// </summary>
    public static string DomainName = null!;

    /// <summary>
    /// Gibt eine Fehlermeldung in eine Datei aus.
    /// </summary>
    /// <param name="context">Ursache des Fehlers.</param>
    /// <param name="e">Abgefangene <see cref="Exception"/>.</param>
    static public void LogException(string context, Exception e)
    {
        // Be safe
        try
        {
            // Special - may generate stack overflow!
            //Tools.ExtendedLogging( "LogException({0}) {1}", context, e );

            // Create path name
            var temp = Path.Combine(Path.GetTempPath(), "VCR.NET Recording Service.jlg");

            // Open stream
            using var writer = new StreamWriter(temp, true, Encoding.UTF8);

            // Report
            writer.WriteLine("{0} Fatal Abort at {2}: {1}", DateTime.Now, e, context);
        }
        catch
        {
        }
    }

    /// <summary>
    /// Protokolliert besondere Ereignisse.
    /// </summary>
    /// <param name="format">Format für eine Meldung.</param>
    /// <param name="args">Parameter zum Format für die Meldung.</param>
    public static void ExtendedLogging(string format, params object[] args)
    {
        // Log all errors a special way
        try
        {
            // Get the path
            var path = ExtendedLogPath;
            if (path == null)
                if (!EnableTracing)
                    return;

            // Forward
            InternalLog(path!, string.Format(format, args));
        }
        catch (Exception e)
        {
            // Report
            LogException("ExtendedLogging", e);
        }
    }

    /// <summary>
    /// Meldet den aktuellen Pfad zur erweiterten Protokollierung.
    /// </summary>
    private static string ExtendedLogPath => null!;

    /// <summary>
    /// Protokolliert besondere Ereignisse.
    /// </summary>
    /// <param name="message">Die Meldung.</param>
    public static void ExtendedLogging(string message)
    {
        // Log all errors a special way
        try
        {
            // Get the path
            var path = ExtendedLogPath;
            if (path == null)
                if (!EnableTracing)
                    return;

            // Forward
            InternalLog(path!, message);
        }
        catch (Exception e)
        {
            // Report
            LogException("ExtendedLogging", e);
        }
    }

    /// <summary>
    /// Protokolliert besondere Ereignisse.
    /// </summary>
    /// <param name="path">Der volle Pfad zur Protokolldatei oder <i>null</i>.</param>
    /// <param name="message">Die Meldung.</param>
    private static void InternalLog(string path, string message)
    {
        // Log all errors a special way
        try
        {
            // Create the message
            message = $"{DateTime.Now} on {Thread.CurrentThread.ManagedThreadId}@{DomainName}: {message}";

            // Report to trace
            if (EnableTracing)
                Debug.WriteLine(message);

            // Fully synchonized
            if (path != null)
                lock (m_LoggingLock)
                    using (var writer = new StreamWriter(path, true, Encoding.UTF8))
                        writer.WriteLine(message);
        }
        catch (Exception e)
        {
            // Report
            LogException("ExtendedLogging", e);
        }
    }

    /// <summary>
    /// Erzeugt das Verzeichnis zu einer Datei.
    /// </summary>
    /// <param name="fullPath">Voller Pfad, kann auch leer oder <i>null</i> sein.</param>
    static public void CreateDir(string fullPath)
    {
        // Create it
        if (!string.IsNullOrEmpty(fullPath))
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
    }

    /// <summary>
    /// Liefert das Verzeichnis, in dem die Anwendungsdatei des VCR.NET
    /// Recording Service liegt.
    /// <seealso cref="ExecutablePath"/>
    /// </summary>
    public static DirectoryInfo ApplicationDirectory => RunTimeLoader.GetDirectory("Recording");

    /// <summary>
    /// Startet eine einzelne Erweiterung.
    /// </summary>
    /// <param name="extension">Die gewünschte Erweiterung.</param>
    /// <param name="environment">Parameter für die Erweiterung.</param>
    /// <returns>Der gestartete Prozess oder <i>null</i>, wenn ein Start nicht m�glich war.</returns>
    public static Process? RunExtension<T>(FileInfo extension, Dictionary<string, string> environment, ILogger<T> logger)
    {
        // Be safe
        try
        {
            // Create the start record
            var info =
                new ProcessStartInfo
                {
                    WorkingDirectory = extension.DirectoryName,
                    FileName = extension.FullName,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    ErrorDialog = false,
                };

            // Overwrite as wanted
            if (environment != null)
                foreach (var env in environment)
                    info.EnvironmentVariables.Add(env.Key.Substring(1, env.Key.Length - 2), env.Value);

            // Log
            logger.Log(LoggingLevel.Full, "Start der Erweiterung {0}", extension.FullName);

            // Start the process
            return Process.Start(info);
        }
        catch (Exception e)
        {
            // Report
            logger.Log(e);
        }

        // Report
        return null;
    }

    /// <summary>
    /// Ermittelt eine Liste von vollen Stunden eines Tages aus der Konfiguration.
    /// </summary>
    /// <param name="hours">Die durch Komma getrennte Liste von vollen Stunden.</param>
    /// <returns>Die Liste der Stunden.</returns>
    public static IEnumerable<uint> GetHourList(string hours)
    {
        // None at all
        if (string.IsNullOrEmpty(hours))
            yield break;

        // Process all
        foreach (var hourAsString in hours.Split(','))
            if (uint.TryParse(hourAsString.Trim(), out uint hour))
                if ((hour >= 0) && (hour <= 23))
                    yield return hour;
    }

    /// <summary>
    /// Ermittelt zu einem letzten Ausf�hrungszeitpunkt und einer Liste von erlaubten Stunden
    /// den n�chsten ausf�hrungszeitpunkt für eine periodische Aktualisierung.
    /// </summary>
    /// <param name="lastTime">Der Zeitpunkt, an dem letztmalig eine Aktualisierung ausgef�hrt wurde.</param>
    /// <param name="hourSkip">Die Anzahl von Stunden, die auf jeden Fall �bersprungen werden sollen.</param>
    /// <param name="hourList">Die Liste der erlaubten vollen Stunden.</param>
    /// <returns>Der gewünschte Zeitpunkt oder <i>null</i>.</returns>
    public static DateTime? GetNextTime(DateTime lastTime, uint hourSkip, IEnumerable<uint> hourList)
    {
        // Create dictionary from hours
        var hours = hourList.ToDictionary(h => (int)h);
        if (hours.Count < 1)
            return null;

        // Advance to the next hour
        var nextRun = lastTime.Date.AddHours(lastTime.Hour + hourSkip);

        // Test all
        for (int it = 48; it-- > 0; nextRun = nextRun.AddHours(1))
            if (hours.ContainsKey(nextRun.ToLocalTime().Hour))
                return nextRun;

        // Must be a configuration error
        return null;
    }

    /// <summary>
    /// Ermittelte alle Erweiterungen eine bestimmten Art.
    /// </summary>
    /// <param name="extensionType">Die gewünschte Art von Erweiterungen.</param>
    /// <returns>Alle Erweiterungen der gewünschten Art.</returns>
    public static IEnumerable<FileInfo> GetExtensions(string extensionType)
    {
        // Get the path
        var root = new DirectoryInfo(Path.Combine(ApplicationDirectory.Parent!.FullName, "Server Extensions"));
        if (!root.Exists)
            yield break;

        // Get the path
        var extensionDir = new DirectoryInfo(Path.Combine(root.FullName, extensionType));
        if (!extensionDir.Exists)
            yield break;

        // Get all files
        foreach (var file in extensionDir.GetFiles("*.bat"))
            if (StringComparer.InvariantCultureIgnoreCase.Equals(file.Extension, ".bat"))
                yield return file;
    }

    /// <summary>
    /// Startet aller Erweiterung einer bestimmten Art.
    /// </summary>
    /// <param name="extensionType">Die gewünschte Art von Erweiterungen.</param>
    /// <param name="environment">Aller Parameter für die Erweiterungen.</param>
    /// <returns>Die Prozessinformationen zu allen gestarten Erweiterungen.</returns>
    public static IEnumerable<Process> RunExtensions<T>(string extensionType, Dictionary<string, string> environment, ILogger<T> logger)
    {
        // Forward
        return RunExtensions(GetExtensions(extensionType), environment, logger);
    }

    /// <summary>
    /// Startet aller Erweiterung einer bestimmten Art.
    /// </summary>
    /// <param name="extensionType">Die gewünschte Art von Erweiterungen.</param>
    /// <param name="environment">Aller Parameter für die Erweiterungen.</param>
    /// <returns>Die Prozessinformationen zu allen gestarten Erweiterungen.</returns>
    public static void RunSynchronousExtensions<T>(string extensionType, Dictionary<string, string> environment, ILogger<T> logger)
    {
        // Forward
        foreach (var process in RunExtensions(GetExtensions(extensionType), environment, logger))
            if (process != null)
                try
                {
                    // Wait on done
                    try
                    {
                        // Do it
                        process.WaitForExit();
                    }
                    finally
                    {
                        // Get rid
                        process.Dispose();
                    }
                }
                catch (Exception e)
                {
                    // Report and catch all!
                    logger.Log(e);
                }
    }

    /// <summary>
    /// Startet eine Liste von Erweiterungen.
    /// </summary>
    /// <param name="extensions">Die zu startenden Erweiterungen.</param>
    /// <param name="environment">Aller Parameter für die Erweiterungen.</param>
    /// <returns>Die Prozessinformationen zu allen gestarten Erweiterungen.</returns>
    public static IEnumerable<Process> RunExtensions<T>(IEnumerable<FileInfo> extensions, Dictionary<string, string> environment, ILogger<T> logger)
    {
        // Process all
        foreach (var running in extensions.Select(extension => RunExtension(extension, environment, logger)).Where(process => process != null))
        {
            // Report
            Tools.ExtendedLogging("Started Extension Process {0}", running!.Id);

            // Remember if sucessfully started
            yield return running;
        }
    }
}

