namespace JMS.DVB.NET.Recording
{
    /// <summary>
    /// Verwaltet die Geräteprofile des VCR.NET Recording Service.
    /// </summary>
    /// <remarks>LEAF SERVICE</remarks>
    public class VCRProfiles(Logger logger)
    {
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

        /// <summary>
        /// Lädt alle Profile erneut.
        /// </summary>
        internal void Reset(VCRServer server)
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
            var profileNames = server.Configuration.ProfileNames;
            if (!string.IsNullOrEmpty(profileNames))
                foreach (var profileName in profileNames.Split('|'))
                {
                    // Try to locate
                    var profile = ProfileManager.FindProfile(profileName.Trim());
                    if (profile == null)
                    {
                        // This is not goot
                        logger.LogError("DVB.NET Geräteprofil '{0}' nicht gefunden", profileName.Trim());

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
            newState.Profiles = profiles.ToArray();
            newState.ProfileMap = profiles.ToDictionary(profile => profile.Name, newState.ProfileMap.Comparer);

            // Report
            Tools.ExtendedLogging("Activating new Profile Set");

            // Use the new state
            CurrentState = newState;
        }

        /// <summary>
        /// Meldet das erste zu verwendende Geräteprofil.
        /// </summary>
        public Profile? DefaultProfile
        {
            get
            {
                // Attach to array
                var profiles = CurrentState.Profiles;
                if (profiles.Length > 0)
                    return profiles[0];
                else
                    return null;
            }
        }

        /// <summary>
        /// Ermittelt eine Quelle nach ihrem Namen.
        /// </summary>
        /// <param name="profileName">Das zu verwendende Geräteprofil.</param>
        /// <param name="name">Der Anzeigename der Quelle.</param>
        /// <returns>Die eidneutige Auswahl der Quelle oder <i>null</i>.</returns>
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

        /// <summary>
        /// Ermittelt eine Quelle nach ihrer eindeutigen Kennung.
        /// </summary>
        /// <param name="profileName">Das zu verwendende Geräteprofil.</param>
        /// <param name="source">Die gewünschte Kennung.</param>
        /// <returns>Die eidneutige Auswahl der Quelle oder <i>null</i>.</returns>
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
            if (!map.TryGetValue(source, out var found))
                return null;
            else
                return found;
        }

        /// <summary>
        /// Ermittelt die aktuelle Konfiguration einer Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Die aktuelle Auswahl oder <i>null</i>.</returns>
        public SourceSelection? FindSource(SourceSelection source)
        {
            // Never
            if (source == null)
                return null;
            if (source.Source == null)
                return null;

            // Find the source
            return FindSource(source.ProfileName, source.Source);
        }

        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
        public IEnumerable<SourceSelection> GetSources(string profileName) => GetSources(profileName, (Func<SourceSelection, bool>)null!);

        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profile">Der Name des Geräteprofils.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
        public IEnumerable<SourceSelection> GetSources(Profile profile) => GetSources(profile, (Func<SourceSelection, bool>)null!);

        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="predicate">Methode, die prüft, ob eine Quelle gemeldet werden soll.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
        public IEnumerable<SourceSelection> GetSources(string profileName, Func<SourceSelection, bool> predicate) => GetSources(FindProfile(profileName)!, predicate);

        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="predicate">Methode, die prüft, ob eine Quelle gemeldet werden soll.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
        public IEnumerable<SourceSelection> GetSources(string profileName, Func<Station, bool> predicate)
        {
            // Forward
            if (predicate == null)
                return GetSources(FindProfile(profileName)!);
            else
                return GetSources(FindProfile(profileName)!, s => predicate((Station)s.Source));
        }

        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profile">Das zu verwendende Geräteprofil.</param>
        /// <param name="predicate">Methode, die prüft, ob eine Quelle gemeldet werden soll.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
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
                if ((null == predicate) || predicate(source))
                    yield return source;
        }

        /// <summary>
        /// Ermittelt ein Geräteprofil.
        /// </summary>
        /// <param name="name">Der Name des Geräteprofils oder <i>null</i> für das
        /// bevorzugte Profil.</param>
        /// <returns>Das gewünschte Geräteprofil.</returns>
        public Profile? FindProfile(string name)
        {
            // Use default
            if (string.IsNullOrEmpty(name))
                return DefaultProfile;

            // Read out
            return CurrentState.ProfileMap.TryGetValue(name, out var profile) ? profile : null;
        }

        /// <summary>
        /// Meldet die Namen alle aktivierten Geräteprofile, das bevorzugte Profil immer zuerst.
        /// </summary>
        public IEnumerable<string> ProfileNames { get { return CurrentState.Profiles.Select(p => p.Name); } }

        /// <summary>
        /// Ermittelt den eindeutigen Namen einer Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Der eindeutige Name oder <i>null</i>, wenn die Quelle nicht
        /// bekannt ist..</returns>
        public string GetUniqueName(SourceSelection source)
        {
            // Map to current
            var active = FindSource(source);
            if (active == null)
                return null!;

            // Find the name
            if (!CurrentState.UniqueNameBySelectionMap.TryGetValue(active.SelectionKey, out var name))
                return null!;

            // Report it
            return name;
        }
    }
}
