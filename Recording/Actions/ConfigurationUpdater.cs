using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Actions;

public class ConfigurationUpdater(IVCRConfiguration configuration, IProfileStateCollection states, VCRServer server) : IConfigurationUpdater
{
    /// <inheritdoc/>
    public bool? UpdateConfiguration(IEnumerable<SettingDescription> settings, bool forceRestart = false)
    {
        // Check state
        if (states.IsActive)
            return null;

        // Process
        if (configuration.CommitUpdate(settings) || forceRestart)
        {
            // Do not restart in debug mode
            if (Tools.DebugMode)
                return null;

            // Create new process to restart the service
            server.Restart?.Invoke();

            // Finally back to the administration page
            return true;
        }
        else
        {
            // Check for new tasks
            states.BeginNewPlan();

            // Finally back to the administration page
            return false;
        }
    }
}