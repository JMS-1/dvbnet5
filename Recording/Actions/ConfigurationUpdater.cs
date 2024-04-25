using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Actions;

public class ConfigurationUpdater(IVCRConfiguration configuration, IVCRServer server) : IConfigurationUpdater
{
    /// <inheritdoc/>
    public bool? UpdateConfiguration(IEnumerable<SettingDescription> settings, bool forceRestart = false)
    {
        // Check state
        if (server.IsActive)
            return null;

        // Process
        if (configuration.CommitUpdate(settings) || forceRestart)
        {
            // Do not restart in debug mode
            if (Tools.DebugMode)
                return null;

            // Create new process to restart the service
            server.Restart();

            // Finally back to the administration page
            return true;
        }
        else
        {
            // Check for new tasks
            server.BeginNewPlan();

            // Finally back to the administration page
            return false;
        }
    }
}