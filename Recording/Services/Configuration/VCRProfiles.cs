using JMS.DVB.NET.Recording.Services.Logging;

namespace JMS.DVB.NET.Recording.Services.Configuration;

/// <summary>
/// Verwaltet die Geräteprofile des VCR.NET Recording Service.
/// </summary>
public class VCRProfiles : IVCRProfiles
{
    private readonly ILogger<VCRProfiles> _logger;

    private readonly IVCRConfiguration _configuration;

    /// <summary>
    /// Der aktuelle Konfigurationsbestand.
    /// </summary>
    private class _State
    {
        /// <summary>
        /// Alle Geräteprofile, die der VCR.NET Recording Service verwenden darf.
        /// </summary>
        public Profile[] Profiles = [];

        /// <summary>
        /// Die Geräteprofile zum schnellen Zugriff über den Namen.
        /// </summary>
        public Dictionary<string, Profile> ProfileMap = new(ProfileManager.ProfileNameComparer);

        /// <summary>
        /// Verwaltet alle Quellen aller zur Verfügung stehenden Geräteprofile nach dem eindeutigen
        /// Bezeichner der Quelle.
        /// </summary>
        public Dictionary<string, Dictionary<string, SourceSelection>> SourceBySelectionMap = new(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Ermittelt zu einer Quelle den aktuellen eindeutigen Namen.
        /// </summary>
        public Dictionary<string, string> UniqueNameBySelectionMap = new(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Verwaltet alle Quellen aller zur Verfügung stehenden Geräteprofile nach der DVB
        /// Kennung.
        /// </summary>
        public Dictionary<string, Dictionary<SourceIdentifier, SourceSelection>> SourceByIdentifierMap = new(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Erzeugt einen neuen Bestand.
        /// </summary>
        public _State()
        {
        }
    }

    /// <summary>
    /// Beschreibt die aktuell gültige Konfiguration.
    /// </summary>
    private volatile _State CurrentState = new();

    public VCRProfiles(ILogger<VCRProfiles> logger, IVCRConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;

        Reset();
    }

    /// <inheritdoc/>
    public void Reset()
    {
        // Report
        Tools.ExtendedLogging("Reloading Profile List");

        // Forward to profile manager
        ProfileManager.Refresh();

        // Create a new state
        var newState = new _State();

        // List of profiles
        var profiles = new List<Profile>();

        // Load the setting
        var profileNames = _configuration.ProfileNames;
        if (!string.IsNullOrEmpty(profileNames))
            foreach (var profileName in profileNames.Split('|'))
            {
                // Try to locate
                var profile = ProfileManager.FindProfile(profileName.Trim());
                if (profile == null)
                {
                    // This is not goot
                    _logger.LogError("DVB.NET Geräteprofil '{0}' nicht gefunden", profileName.Trim());

                    // Next
                    continue;
                }

                // Report
                Tools.ExtendedLogging("Using Profile {0}", profile.Name);

                // Remember
                profiles.Add(profile);

                // Create the dictionary of sources
                var sourcesByIdentifier = new Dictionary<SourceIdentifier, SourceSelection>();
                var sourcesByKey = new Dictionary<string, SourceSelection>();

                // Load by name
                foreach (var byDisplayName in profile.AllSourcesByDisplayName)
                    sourcesByKey[byDisplayName.DisplayName] = byDisplayName;

                // Remember it
                newState.SourceByIdentifierMap[profile.Name] = sourcesByIdentifier;
                newState.SourceBySelectionMap[profile.Name] = sourcesByKey;

                // Load list
                foreach (var source in sourcesByKey.Values)
                {
                    // Just remember by identifier
                    sourcesByIdentifier[source.Source] = source;

                    // Correct back the name
                    source.DisplayName = ((Station)source.Source).FullName;
                }
            }

        // Fill it
        foreach (var profileMap in newState.SourceBySelectionMap.Values)
            foreach (var mapEntry in profileMap)
                newState.UniqueNameBySelectionMap[mapEntry.Value.SelectionKey] = mapEntry.Key;

        // Add all qualified names to allow semi-legacy clients to do a unique lookup
        foreach (var profileMap in newState.SourceBySelectionMap.Values)
            foreach (var source in profileMap.ToList())
                if (!source.Key.Equals(source.Value.DisplayName))
                {
                    // Unmap the station
                    var station = (Station)source.Value.Source;

                    // Enter special notation
                    profileMap[$"{station.Name} {station.ToStringKey()} [{station.Provider}]"] = source.Value;
                }

        // Use all
        newState.Profiles = [.. profiles];
        newState.ProfileMap = profiles.ToDictionary(profile => profile.Name, newState.ProfileMap.Comparer);

        // Report
        Tools.ExtendedLogging("Activating new Profile Set");

        // Use the new state
        CurrentState = newState;
    }

    /// <inheritdoc/>
    public Profile? DefaultProfile => CurrentState.Profiles.FirstOrDefault();

    /// <inheritdoc/>
    public SourceSelection? FindSource(string profileName, string name)
    {
        // No source
        if (string.IsNullOrEmpty(name))
            return null;

        // No profile
        var state = CurrentState;

        if (string.IsNullOrEmpty(profileName))
            if (state.Profiles.Length < 1)
                return null;
            else
                profileName = state.Profiles[0].Name;

        // Map to use
        if (!state.SourceBySelectionMap.TryGetValue(profileName, out var sources))
            return null;

        // Ask map
        if (!sources.TryGetValue(name, out var source))
            return null;

        // Report
        return source;
    }

    /// <inheritdoc/>
    public SourceSelection? FindSource(string profileName, SourceIdentifier source)
    {
        // No source
        if (source == null)
            return null;

        // No profile
        var state = CurrentState;

        if (string.IsNullOrEmpty(profileName))
            if (state.Profiles.Length < 1)
                return null;
            else
                profileName = state.Profiles[0].Name;

        // Find the map
        if (!state.SourceByIdentifierMap.TryGetValue(profileName, out var map))
            return null;

        // Find the source
        return map.TryGetValue(source, out var found) ? found : null;
    }

    /// <inheritdoc/>
    public IEnumerable<SourceSelection> GetSources(Profile profile, Func<SourceSelection, bool> predicate)
    {
        // Resolve
        if (profile == null)
            yield break;

        // Load the map
        if (!CurrentState.SourceByIdentifierMap.TryGetValue(profile.Name, out var map))
            yield break;

        // Use state
        foreach (SourceSelection source in map.Values)
            if (predicate?.Invoke(source) != false)
                yield return source;
    }

    /// <inheritdoc/>
    public Profile? FindProfile(string name)
    {
        // Use default
        if (string.IsNullOrEmpty(name))
            return DefaultProfile;

        // Read out
        return CurrentState.ProfileMap.TryGetValue(name, out var profile) ? profile : null;
    }

    /// <inheritdoc/>
    public IEnumerable<string> ProfileNames => CurrentState.Profiles.Select(p => p.Name);

    /// <inheritdoc/>
    public string GetUniqueName(SourceSelection source)
    {
        // Map to current
        var active = this.FindSource(source);
        if (active == null)
            return null!;

        // Find the name
        if (!CurrentState.UniqueNameBySelectionMap.TryGetValue(active.SelectionKey, out var name))
            return null!;

        // Report it
        return name;
    }
}
