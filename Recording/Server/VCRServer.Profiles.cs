using JMS.DVB.NET.Recording.Planning;
using JMS.DVB.NET.Recording.Services.Logging;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Server;

public partial class VCRServer
{
    /// <summary>
    /// Alle von dieser Instanz verwalteten Geräteprofile.
    /// </summary>
    private Dictionary<string, IProfileState> _stateMap = [];

    /// <inheritdoc/>
    public IEnumerable<TInfo> InspectProfiles<TInfo>(Func<IProfileState, TInfo> factory) => _stateMap.Values.Select(factory);

    /// <inheritdoc/>
    /// <inheritdoc/>
    public IProfileState? this[string profileName]
    {
        get
        {
            // Validate
            if (string.IsNullOrEmpty(profileName) || profileName.Equals("*"))
            {
                // Attach to the default profile
                var defaultProfile = profiles.DefaultProfile;
                if (defaultProfile == null)
                    return null;

                // Use it
                profileName = defaultProfile.Name;
            }

            // Load
            return _stateMap.TryGetValue(profileName, out var profile) ? profile : null;
        }
    }

    /// <inheritdoc/>
    public IProfileState? FindProfile(string profileName)
    {
        // Forward
        var state = this[profileName];
        if (state == null)
            logger.LogError("Es gibt kein Geräteprofil '{0}'", profileName);

        // Report
        return state;
    }

    /// <summary>
    /// Wendet eine Methode auf alle verwalteten Profile an.
    /// </summary>
    /// <param name="method">Die gewünschte Methode.</param>
    /// <param name="ignoreErrors">Gesetzt, wenn Fehler ignoriert werden sollen.</param>
    private void ForEachProfile(Action<IProfileState> method, bool ignoreErrors = false)
    {
        // Forward to all
        foreach (var state in _stateMap.Values)
            try
            {
                // Forward
                method(state);
            }
            catch (Exception e)
            {
                // Report
                logger.Log(e);

                // See if we are allowed to ignore
                if (!ignoreErrors)
                    throw;
            }
    }

    /// <summary>
    /// Meldet die Namen der verwendeten Geräteprofile.
    /// </summary>
    IEnumerable<string> IRecordingPlannerSite.ProfileNames => _stateMap.Keys;
}
