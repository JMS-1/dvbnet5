using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Actions;

public interface IRuleUpdater
{  /// <summary>
   /// Aktualisiert die Regeln für die Aufzeichnungsplanung.
   /// </summary>
   /// <param name="newRules">Die ab nun zu verwendenden Regeln.</param>
   /// <returns>Meldet, ob ein Neustart erforderlich ist.</returns>
    bool? UpdateSchedulerRules(string newRules);
}

public class RuleUpdater(IProfileStateCollection states) : IRuleUpdater
{
    /// <inheritdoc/>
    public bool? UpdateSchedulerRules(string newRules)
    {
        // Check state
        if (states.IsActive)
            return null;

        // Process
        states.SchedulerRules = newRules;

        // Do not restart in debug mode
        if (Tools.DebugMode)
            return null;

        // Create new process to restart the service
        states.Restart();

        // Finally back to the administration page
        return true;
    }
}