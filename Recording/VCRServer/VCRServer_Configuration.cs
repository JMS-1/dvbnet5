using JMS.DVB.NET.Recording.Services;

namespace JMS.DVB.NET.Recording;

partial class VCRServer
{
    /// <summary>
    /// Aktualisiert die Regeln für die Aufzeichnungsplanung.
    /// </summary>
    /// <param name="newRules">Die ab nun zu verwendenden Regeln.</param>
    /// <returns>Meldet, ob ein Neustart erforderlich ist.</returns>
    public bool? UpdateSchedulerRules(string newRules)
    {
        // Check state
        if (Profiles.IsActive)
            return null;

        // Process
        SchedulerRules = newRules;

        // Do not restart in debug mode
        if (InDebugMode)
            return null;

        // Create new process to restart the service
        Restart?.Invoke();

        // Finally back to the administration page
        return true;
    }
}

