namespace JMS.DVB.NET.Recording.Services;

/// <summary>
/// Verwaltet Erweiterungen und deren Instanzen.
/// </summary>
public interface IExtensionManager
{
    /// <summary>
    /// Prüft, ob sich irgendwelchen aktiven Prozesse in der Verwaltung befinden.
    /// </summary>
    bool HasActiveProcesses { get; }

    /// <summary>
    /// Startet Erweiterungen und ergänzt die zugehörigen Prozesse in der Verwaltung.
    /// </summary>
    /// <param name="extensionName">Der Name der Erweiterung.</param>
    /// <param name="environment">Die für die Erweiterung zu verwendenden Umgebungsvariablen.</param>
    void AddWithCleanup(string extensionName, Dictionary<string, string> environment);
}
