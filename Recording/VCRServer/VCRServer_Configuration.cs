using System.Diagnostics;

namespace JMS.DVB.NET.Recording;

partial class VCRServer
{
    /// <summary>
    /// Führt eine Aktualisierung von Konfigurationswerten durch.
    /// </summary>
    /// <param name="settings">Die zu aktualisierenden Konfigurationswerte.</param>
    /// <param name="forceRestart">Erzwingt einen Neustart des Dienstes.</param>
    /// <returns>Gesetzt, wenn ein Neustart erforderlich ist.</returns>
    public bool? UpdateConfiguration(IEnumerable<VCRConfiguration.SettingDescription> settings, bool forceRestart = false)
    {
        // Check state
        if (IsActive)
            return null;

        // Process
        if (Configuration.CommitUpdate(settings) || forceRestart)
        {
            // Do not restart in debug mode
            if (InDebugMode)
                return null;

            // Create new process to restart the service
            Process.Start(Tools.ExecutablePath, "Restart").Dispose();

            // Finally back to the administration page
            return true;
        }
        else
        {
            // Check for new tasks
            BeginNewPlan();

            // Finally back to the administration page
            return false;
        }
    }

    /// <summary>
    /// Aktualisiert die Regeln f�r die Aufzeichnungsplanung.
    /// </summary>
    /// <param name="newRules">Die ab nun zu verwendenden Regeln.</param>
    /// <returns>Meldet, ob ein Neustart erforderlich ist.</returns>
    public bool? UpdateSchedulerRules(string newRules)
    {
        // Check state
        if (IsActive)
            return null;

        // Process
        SchedulerRules = newRules;

        // Do not restart in debug mode
        if (InDebugMode)
            return null;

        // Create new process to restart the service
        Process.Start(Tools.ExecutablePath, "Restart").Dispose();

        // Finally back to the administration page
        return true;
    }
}
