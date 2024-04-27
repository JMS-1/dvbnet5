using JMS.DVB.NET.Recording.Server;

namespace JMS.DVB.NET.Recording.Actions;

public class RuleUpdater(IVCRServer server) : IRuleUpdater
{
    /// <inheritdoc/>
    public bool? UpdateSchedulerRules(string newRules)
    {
        // Check state
        if (server.IsActive)
            return null;

        // Process
        server.SchedulerRules = newRules;

        // Create new process to restart the service
        server.Restart();

        // Finally back to the administration page
        return true;
    }
}