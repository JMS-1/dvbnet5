using System.Diagnostics;

namespace JMS.DVB.Algorithms
{
    /// <summary>
    /// Bezeichnet die Art eine Protokolleintrags.
    /// </summary>
    public enum ProtocolRecordMode
    {
        /// <summary>
        /// Eine Quellgruppe, auf der keinerlei Quellen verfügbar sind.
        /// </summary>
        EmptyGroup,

        /// <summary>
        /// Eine bekannte Quelle wurde bestätigt.
        /// </summary>
        Found,

        /// <summary>
        /// Eine bekannte Quelle konnte nicht mehr gefunden werden.
        /// </summary>
        NotFound,

        /// <summary>
        /// Eine neue Quelle wurde gefunden.
        /// </summary>
        Added
    }

    /// <summary>
    /// Beschreibt eine gefundene Quelle oder andere Protokollinformationen.
    /// </summary>
    public class ProtocolRecord
    {
        /// <summary>
        /// Beschreibt das Format von <see cref="ToString"/>.
        /// </summary>
        public const string LineFormat = "Mode\tName\tProvider\tNetwork\tTransportstream\tService\tNVOD\tEncrypted\tType\tGroup\tLocation";

        /// <summary>
        /// Die Art des Eintrags.
        /// </summary>
        public ProtocolRecordMode Mode { get; set; }

        /// <summary>
        /// Der Ursprung, der untersucht wurde.
        /// </summary>
        public GroupLocation Location { get; set; } = null!;

        /// <summary>
        /// Die zugehörige Quellgruppe.
        /// </summary>
        public SourceGroup Group { get; set; } = null!;

        /// <summary>
        /// Die zugehörige Quelle.
        /// </summary>
        public SourceIdentifier Source { get; set; } = null!;

        /// <summary>
        /// Erzeugt einen neuen Protokolleintrag.
        /// </summary>
        public ProtocolRecord()
        {
        }

        /// <summary>
        /// Erstellt einen Protokolleintrag.
        /// </summary>
        /// <returns>Der gewünschte Protokolleintrag.</returns>
        public override string ToString()
        {
            // Check type

            // Depends on mode
            if (Source is Station station)
                return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}", Mode, station.Name, station.Provider, station.Network, station.TransportStream, station.Service, station.IsService, station.IsEncrypted, station.SourceType, Group, Location);
            else
                return string.Format("{0}\t\t\t\t\t\t\t\t\t{1}\t{2}", Mode, Group, Location);
        }
    }

    /// <summary>
    /// Mit dieser Klasse wird ein Sendersuchlauf durchgeführt.
    /// </summary>
    public class TransponderScanner : IDisposable
    {
        /// <summary>
        /// Mit dieser Einstellung werden die Vorgänge während einer Aktualisierung der Quellen (Sendersuchlauf) protokolliert.
        /// </summary>
        public static readonly TraceSwitch ScannerTraceSwitch = new("ScannerTrace", "Reports various Steps while Updating the Source List of a Profile");

        /// <summary>
        /// Wird aktiviert, bevor ein neuer Ursprung aktiviert wird. Wird <i>false</i> zurück
        /// geliefert, so bricht der Suchlauf ab.
        /// </summary>
        public event Func<GroupLocation, TransponderScanner, bool>? OnStartLocation;

        /// <summary>
        /// Wird aktiviert, nachdem ein Ursprung vollständig abgesucht wurde. Wird <i>false</i> zurück
        /// geliefert, so bricht der Suchlauf ab.
        /// </summary>
        public event Func<GroupLocation, TransponderScanner, bool>? OnDoneLocation;

        /// <summary>
        /// Wird aktiviert, bevor eine neue Quellgruppe (Transponder) aktiviert wird. Wird <i>false</i> zurück
        /// geliefert, so bricht der Suchlauf ab.
        /// </summary>
        public event Func<GroupLocation, SourceGroup, TransponderScanner, bool>? OnStartGroup;

        /// <summary>
        /// Wird aktiviert, nachdem eine Quellgruppe vollständig abgesucht wurde. Wird <i>false</i> zurück
        /// geliefert, so bricht der Suchlauf ab.
        /// </summary>
        public event Func<GroupLocation, SourceGroup, TransponderScanner, bool>? OnDoneGroup;

        /// <summary>
        /// Wird aktiviert, wenn eine Quelle zur Senderliste hinzugefügt wird.
        /// </summary>
        public event Func<GroupLocation, SourceGroup, Station, TransponderScanner, bool>? OnStationAdded;

        /// <summary>
        /// Meldet das verwendete Geräteprofil.
        /// </summary>
        public Profile Profile { get; private set; }

        /// <summary>
        /// Der <see cref="Thread"/>, auf dem der eigentliche Suchlauf ausgeführt wird.
        /// </summary>
        private volatile Thread m_Worker = null!;

        /// <summary>
        /// Nimmt eine etwaige Fehlermeldung auf, die während des Suchlaufs aufgetreten ist.
        /// </summary>
        private Exception m_ThreadException = null!;

        /// <summary>
        /// Die Urprünge, die abzusuchen sind.
        /// </summary>
        private List<GroupLocation> m_Locations = null!;

        /// <summary>
        /// Gesetzt, wenn der Suchlauf vorzeitig abgebrochen wurde.
        /// </summary>
        private bool m_Aborted;

        /// <summary>
        /// Alle Quellgruppen (Transponder), die explizit vom Sendersuchlauf ausgeschlossen wurden.
        /// </summary>
        private readonly List<SourceGroup> m_GroupsExcluded = [];

        /// <summary>
        /// Alle Quellgruppen (Transponder), die grundsätzlich nich angesteuert werden können.
        /// </summary>
        private readonly List<SourceGroup> m_UnhandledGroups = [];

        /// <summary>
        /// Das Ergebnis einer NIT Analyse.
        /// </summary>
        private readonly Dictionary<GroupLocation, ScanLocation> m_AnalyseResult = [];

        /// <summary>
        /// Das Ergebnis eines Sendersuchlaufs.
        /// </summary>
        private readonly List<GroupLocation> m_ScanResults = [];

        /// <summary>
        /// Detailinformationen zu einerm Sendersuchlauf.
        /// </summary>
        private readonly List<ProtocolRecord> m_Protocol = [];

        /// <summary>
        /// Die gesamte Anzahl der Ursprünge, die abgesucht werden.
        /// </summary>
        public int TotalLocations { get; private set; }

        /// <summary>
        /// Die 1-basierte laufende Nummer des aktuell untersuchten Ursprungs.
        /// </summary>
        public int CurrentLocation { get; private set; }

        /// <summary>
        /// Die Anzahl der Quellgruppen (Transponder) des aktuellen Ursprungs, die bereits untersucht wurden.
        /// </summary>
        public int CurrentLocationGroup { get; private set; }

        /// <summary>
        /// Die Anzahl der Quellgruppen (Transponder) des aktuellen Ursprungs, die noch untersucht werden sollen.
        /// </summary>
        public int CurrentLocationGroupsPending { get; private set; }

        /// <summary>
        /// Die Anzahl der bei einem Sendersuchlauf gefundenden Quellen.
        /// </summary>
        public Dictionary<SourceTypes, int> SourcesFound = [];

        /// <summary>
        /// Erzeugt eine neue Instanz für einen Sendersuchlauf.
        /// </summary>
        /// <param name="profile">Das zu verwendende DVB.NET Geräteprofil.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Geräteprofil angegeben.</exception>
        public TransponderScanner(Profile profile)
        {
            // Validate
            ArgumentNullException.ThrowIfNull(profile);

            // Remember
            Profile = profile;
        }

        /// <summary>
        /// Erzeugt eine neue Instanz für einen Sendersuchlauf.
        /// </summary>
        /// <param name="profile">Das zu verwendende DVB.NET Geräteprofil.</param>
        public TransponderScanner(string profile)
            : this(ProfileManager.FindProfile(profile)!)
        {
        }

        /// <summary>
        /// Bereitet diese Instanz für einen neuen Sendersuchlauf vor.
        /// </summary>
        private void Reset()
        {
            // Attach to thread
            Thread worker = m_Worker;

            // Wipe it out
            m_Worker = null!;

            // Synchronize
            if (null != worker)
                worker.Join();
        }

        /// <summary>
        /// Führt einen Sendersuchlauf aus.
        /// </summary>
        public void Scan()
        {
            // Forward
            Scan(ScannerThread);
        }

        /// <summary>
        /// Prüft die Konfiguration des Sendersuchlaufs.
        /// </summary>
        public void Analyse()
        {
            // Forward
            Scan(AnalyserThread);
        }

        /// <summary>
        /// Führt einen Sendersuchlauf aus.
        /// </summary>
        /// <param name="worker">Der auszuführende Algorithmus.</param>
        private void Scan(ThreadStart worker)
        {
            // Start from scratch
            Reset();

            // Load all locations to process
            m_Locations = Profile.CreateScanLocations().ToList();

            // Clear lists
            m_UnhandledGroups.Clear();
            m_GroupsExcluded.Clear();
            m_AnalyseResult.Clear();
            m_ScanResults.Clear();
            SourcesFound.Clear();
            m_Protocol.Clear();

            // Reset state
            m_ThreadException = null!;
            m_Aborted = false;

            // Start the new scanner thread
            m_Worker = new Thread(worker) { Priority = ThreadPriority.AboveNormal };
            m_Worker.Start();
        }

        /// <summary>
        /// Wählt eine Quellgruppe (Transponder) an und prüft, ob ein Signal anliegt.
        /// </summary>
        /// <param name="device">Das aktuell verwendete DVB.NET Geräte.</param>
        /// <param name="location">Optional der Ursprung der Quellgruppe (Transponder).</param>
        /// <param name="group">Die gewünschte Quellgruppe (Transponder).</param>
        /// <param name="lastInversion">Bei DVB-C die zuletzt erfolgreich verwendete spektrale Inversion.</param>
        /// <returns>Die Informationen zu den Quellen oder <i>null</i>.</returns>
        private static GroupInformation? SelectGroup(Hardware device, GroupLocation location, SourceGroup group, ref SpectrumInversions lastInversion)
        {
            // See if this is a cable group
            var cableGroup = group as CableGroup;
            if (cableGroup != null)
                if (cableGroup.SpectrumInversion == SpectrumInversions.Auto)
                    cableGroup.SpectrumInversion = lastInversion;

            // Choose group
            device.SelectGroup(location, group);

            // Did it
            var info = device.GetGroupInformation(5000);
            if (info != null)
                return info;

            // Not cable
            if (cableGroup == null)
                return null;

            // Swap inversion
            lastInversion = (lastInversion == SpectrumInversions.On) ? SpectrumInversions.Off : SpectrumInversions.On;

            // Put into group
            cableGroup.SpectrumInversion = lastInversion;

            // Choose group
            device.SelectGroup(location, group);

            // Final try
            return device.GetGroupInformation(5000);
        }

        /// <summary>
        /// Führt den eigentlichen Sendersuchlauf aus.
        /// </summary>
        private void ScannerThread()
        {
            // Be safe
            try
            {
                // Configure language
                UserProfile.ApplyLanguage();

                // Uses hardware manager
                using (HardwareManager.Open())
                {
                    // Attach to the hardware itself
                    var device = HardwareManager.OpenHardware(Profile)!;

                    // Tell it that we only do a scan
                    device.PrepareSourceScan();

                    // Start loading location result list
                    foreach (var location in m_Locations)
                        m_ScanResults.Add(location.Clone());

                    // Reset counters
                    TotalLocations = m_Locations.Count;

                    // Last successfull inversion
                    var lastInversion = SpectrumInversions.On;

                    // Process all transponders
                    for (CurrentLocation = 0; CurrentLocation < m_Locations.Count;)
                    {
                        // Get new and old
                        var location = m_Locations[CurrentLocation];
                        var newLocation = m_ScanResults[CurrentLocation];

                        // Count
                        ++CurrentLocation;

                        // Reset
                        CurrentLocationGroupsPending = 0;
                        CurrentLocationGroup = 0;

                        // Check caller
                        if (!LocationStart(location))
                            return;

                        // Allow NIT scan on...
                        var enableNIT = new List<SourceGroup>();
                        var foundInfo = new List<SourceGroup>();

                        // Startup and load the groups from the configuration
                        foreach (SourceGroup group in location.Groups)
                            enableNIT.Add(group);

                        // All we have to process
                        var process = new List<SourceGroup>(enableNIT);

                        // Allowed group type
                        var groupType = (process.Count > 0) ? process[0].GetType() : null;

                        // All we did so far
                        var done = new HashSet<SourceGroup>();

                        // Process all groups                        
                        while (process.Count > 0)
                        {
                            // Take the first one
                            var group = process[0];

                            // Remove it
                            process.RemoveAt(0);

                            // Reset counter
                            CurrentLocationGroupsPending = process.Count;

                            // Count what we did
                            ++CurrentLocationGroup;

                            // Already done
                            if (done.Contains(group))
                                continue;

                            // Found group information on equivalent group
                            if (foundInfo.FirstOrDefault(g => g.CompareTo(group, true)) != null)
                                continue;

                            // Mark as done
                            done.Add(group);

                            // Check for external termination
                            if (m_Worker == null)
                                return;

                            // Not supported
                            if (!Profile.SupportsGroup(group) || !device.CanHandle(group))
                            {
                                // Remember
                                m_UnhandledGroups.Add(group);

                                // Next
                                continue;
                            }

                            // Read the configuration
                            var filter = Profile.GetFilter(group);

                            // See if this should be skiped
                            if (filter != null)
                                if (filter.ExcludeFromScan)
                                {
                                    // Remember
                                    m_GroupsExcluded.Add(group);

                                    // Next
                                    continue;
                                }

                            // Check caller
                            if (!GroupStart(location, group))
                                return;

                            // See if we should process the NIT
                            var checkNIT = enableNIT.Contains(group);

                            // See if we should process
                            if (checkNIT || group.IsComplete)
                            {
                                // Clone the group - may change
                                group = SourceGroup.FromString<SourceGroup>(group.ToString()!);

                                // Attach to the group
                                var info = SelectGroup(device, location, group!, ref lastInversion);

                                // Mark as done again - group may be updated for DVB-C and if unchanged this is simply a no-operation
                                done.Add(group!);

                                // Read the configuration again - actually for DVB-C it may change
                                filter = Profile.GetFilter(group!);

                                // See if this should be skiped
                                if (filter != null)
                                    if (filter.ExcludeFromScan)
                                    {
                                        // Remember
                                        m_GroupsExcluded.Add(group!);

                                        // Next
                                        continue;
                                    }

                                // Remember that we found a group information - transponder is not dead
                                if (info != null)
                                    foundInfo.Add(group!);

                                // See if NIT update is allowed
                                if (checkNIT)
                                    if (info != null)
                                    {
                                        // Try load
                                        var nit = device.GetLocationInformation(15000);
                                        if (nit != null)
                                            foreach (SourceGroup other in nit.Groups)
                                                if (other.GetType() == groupType)
                                                {
                                                    // Disable NIT scan on the group as is
                                                    enableNIT.RemoveAll(g => g.CompareTo(other, true));
                                                    process.RemoveAll(g => g.CompareTo(other, true));

                                                    // Set inversion
                                                    if (other is CableGroup otherCable)
                                                        if (group is CableGroup cableGroup)
                                                        {
                                                            // Use same parameters
                                                            otherCable.SpectrumInversion = cableGroup.SpectrumInversion;
                                                            otherCable.Bandwidth = cableGroup.Bandwidth;

                                                            // Disable NIT scan on the group which may be DVB-C corrected                                        
                                                            enableNIT.RemoveAll(g => g.CompareTo(otherCable, true));
                                                            process.RemoveAll(g => g.CompareTo(otherCable, true));
                                                        }

                                                    // Add for processing
                                                    process.Insert(0, other);
                                                }
                                    }

                                // Reset counter - may be updated if a new NIT was processed
                                CurrentLocationGroupsPending = process.Count;

                                // Skip if a legacy template group
                                if (group!.IsComplete)
                                {
                                    // Merge
                                    newLocation.Groups.Add(group);

                                    // Process this group                            
                                    if (info == null)
                                        m_Protocol.Add(new ProtocolRecord { Mode = ProtocolRecordMode.EmptyGroup, Location = location, Group = group });
                                    else
                                        foreach (var source in info.Sources)
                                        {
                                            // Only stations
                                            if (source is not Station station)
                                                continue;

                                            // Attach to the filter
                                            var modifiers = Profile.GetFilter(source);
                                            if (modifiers != null)
                                                if (modifiers.ExcludeFromScan)
                                                    continue;
                                                else
                                                    modifiers.ApplyTo(station);

                                            // Remember
                                            group.Sources.Add(station);

                                            // Get the count
                                            int foundOfType;
                                            if (!SourcesFound.TryGetValue(station.SourceType, out foundOfType))
                                                foundOfType = 0;

                                            // Update
                                            SourcesFound[station.SourceType] = ++foundOfType;

                                            // Ask user
                                            if (!StationFound(location, group, station))
                                                return;
                                        }
                                }
                            }

                            // Check caller
                            if (!GroupDone(location, group))
                                return;
                        }

                        // Check caller
                        if (!LocationDone(location))
                            return;
                    }
                }
            }
            catch (Exception e)
            {
                // Remember
                m_ThreadException = e;
            }
        }

        /// <summary>
        /// Analysiert die aktuelle Konfiguration des Suchlaufs auf Basis
        /// der DVB <i>Network Information Table (NIT)</i> Informationen.
        /// </summary>
        private void AnalyserThread()
        {
            // Be safe
            try
            {
                // Configure language
                UserProfile.ApplyLanguage();

                // Uses hardware manager
                using (HardwareManager.Open())
                {
                    // Attach to the hardware itself
                    var device = HardwareManager.OpenHardware(Profile)!;

                    // Reset counters
                    TotalLocations = m_Locations.Count;
                    CurrentLocation = 0;

                    // Last inversion used
                    SpectrumInversions lastInversion = SpectrumInversions.On;

                    // Process all transponders
                    foreach (GroupLocation location in m_Locations)
                    {
                        // Update counter
                        ++CurrentLocation;

                        // Reset
                        CurrentLocationGroupsPending = 0;
                        CurrentLocationGroup = 0;

                        // Check caller
                        if (!LocationStart(location))
                            return;

                        // Groups found on this location
                        Dictionary<SourceGroup, bool> found = [];

                        // Groups with valid network information - only those are considered
                        List<SourceGroup> nitAvailable = [];

                        // Load counter
                        CurrentLocationGroupsPending = location.Groups.Count;

                        // Process all groups                        
                        foreach (SourceGroup group in location.Groups)
                        {
                            // Get the expected type
                            Type groupType = group.GetType();

                            // Clone the group
                            var newGroup = SourceGroup.FromString<SourceGroup>(group.ToString()!)!;

                            // Count up
                            --CurrentLocationGroupsPending;
                            ++CurrentLocationGroup;

                            // See if this is already processed
                            if (null != found.Keys.FirstOrDefault(p => p.CompareTo(newGroup, true)))
                                continue;

                            // Check for external termination
                            if (null == m_Worker)
                                return;

                            // Not supported
                            if (!Profile.SupportsGroup(newGroup) || !device.CanHandle(newGroup))
                            {
                                // Remember
                                m_UnhandledGroups.Add(newGroup);

                                // Next
                                continue;
                            }

                            // Check caller
                            if (!GroupStart(location, newGroup))
                                return;

                            // Attach to the group
                            if (null != SelectGroup(device, location, newGroup, ref lastInversion))
                            {
                                // Attach to the NIT
                                var nit = device.GetLocationInformation(15000);
                                if (null != nit)
                                {
                                    // Remember
                                    nitAvailable.Add(newGroup);

                                    // Process
                                    foreach (SourceGroup other in nit.Groups)
                                        if (other.GetType() == groupType)
                                        {
                                            // Update inversion
                                            if (other is CableGroup otherCable)
                                            {
                                                // Other must be cable, too
                                                if (newGroup is not CableGroup cableGroup)
                                                    continue;

                                                // Use same parameters
                                                otherCable.SpectrumInversion = cableGroup.SpectrumInversion;
                                                otherCable.Bandwidth = cableGroup.Bandwidth;
                                            }

                                            // Mark it
                                            found[other] = true;
                                        }
                                }
                            }

                            // Check caller
                            if (!GroupDone(location, newGroup))
                                return;
                        }

                        // Create a brand new scan location
                        ScanLocation scan = location.ToScanLocation();

                        // Try to update
                        foreach (SourceGroup group in nitAvailable)
                        {
                            // Try to find the full qualified name
                            var nitGroup = found.Keys.FirstOrDefault(p => p.CompareTo(group, true));

                            // Update
                            if (null == nitGroup)
                                scan.Groups.Add(group);
                            else
                                scan.Groups.Add(nitGroup);
                        }

                        // Just remember
                        m_AnalyseResult[location] = scan;

                        // Check caller
                        if (!LocationDone(location))
                            return;
                    }
                }
            }
            catch (Exception e)
            {
                // Remember
                m_ThreadException = e;
            }
        }

        /// <summary>
        /// Meldet das Ergebnis einer NIT Analyse.
        /// </summary>
        public ScanLocation[] ScanLocations => m_AnalyseResult.Values.ToArray();

        /// <summary>
        /// Meldet das Ergebnis des Sendersuchlaufs.
        /// </summary>
        public GroupLocation[] ScanResults => m_ScanResults.ToArray();

        /// <summary>
        /// Meldet Detailergebnisse zum Sendersuchlauf.
        /// </summary>
        public ProtocolRecord[] Protocol => m_Protocol.ToArray();

        /// <summary>
        /// Übernimmt das Ergebnis des Sendersuchlaufs in das Geräteprofil.
        /// </summary>
        public void UpdateProfile()
        {
            // Create a map of all current locations
            Dictionary<GroupLocation, GroupLocation> newLocations = [];

            // Fill current locations
            foreach (GroupLocation location in m_ScanResults)
                newLocations[location] = location;

            // All previous locations
            List<GroupLocation> oldLocations = [];

            // Fill all which are still active
            foreach (GroupLocation location in Profile.Locations)
                if (newLocations.ContainsKey(location))
                    oldLocations.Add(location);

            // Wipe out
            Profile.Locations.Clear();

            // Reload
            foreach (GroupLocation location in m_ScanResults)
                Profile.Locations.Add(location);

            // All locations to process
            Dictionary<GroupLocation, bool> unprocessedLocations = newLocations.Keys.ToDictionary(l => l, l => true);

            // Process each location individually for a possible merge
            foreach (GroupLocation location in oldLocations)
            {
                // Find the related new location
                GroupLocation newLocation = newLocations[location];

                // No need to check for protocol
                unprocessedLocations.Remove(newLocation);

                // Create a map of all source groups we found
                Dictionary<SourceIdentifier, SourceIdentifier> found = [];

                // Fill it
                foreach (SourceGroup newGroup in newLocation.Groups)
                    foreach (SourceIdentifier newSource in newGroup.Sources)
                        found[newSource] = newSource;

                // Already processed
                Dictionary<SourceIdentifier, bool> updatedSources = [];

                // Now see which old sources are no longer available
                foreach (SourceGroup oldGroup in location.Groups)
                    foreach (SourceIdentifier oldSource in oldGroup.Sources)
                    {
                        // Load the new source
                        if (found.ContainsKey(oldSource))
                        {
                            // Mark
                            updatedSources[oldSource] = true;

                            // Next
                            continue;
                        }

                        // At least a group must exist
                        SourceGroup? newGroup = null;

                        // Find it
                        foreach (SourceGroup testGroup in newLocation.Groups)
                            if (testGroup.CompareTo(oldGroup, true))
                            {
                                // Nearly the same
                                newGroup = testGroup;

                                // Done
                                break;
                            }

                        // Must create the group
                        if (null == newGroup)
                        {
                            // Clone it
                            newGroup = SourceGroup.FromString<SourceGroup>(oldGroup.ToString()!);

                            // Remember it
                            newLocation.Groups.Add(newGroup);
                        }

                        // Add the missing source
                        newGroup!.Sources.Add(oldSource);

                        // Remember
                        m_Protocol.Add(new ProtocolRecord { Mode = ProtocolRecordMode.NotFound, Location = newLocation, Group = newGroup, Source = oldSource });
                    }

                // Finish
                foreach (SourceGroup newGroup in newLocation.Groups)
                    foreach (SourceIdentifier newSource in newGroup.Sources)
                        if (found.ContainsKey(newSource))
                        {
                            // Check mode
                            ProtocolRecordMode mode;
                            if (updatedSources.ContainsKey(newSource))
                                mode = ProtocolRecordMode.Found;
                            else
                                mode = ProtocolRecordMode.Added;

                            // Add to protocol
                            m_Protocol.Add(new ProtocolRecord { Mode = mode, Location = newLocation, Group = newGroup, Source = newSource });
                        }
            }

            // Add all sources to protocol
            foreach (GroupLocation newLocation in unprocessedLocations.Keys)
                foreach (SourceGroup newGroup in newLocation.Groups)
                    foreach (SourceIdentifier newSource in newGroup.Sources)
                        m_Protocol.Add(new ProtocolRecord { Mode = ProtocolRecordMode.Added, Location = newLocation, Group = newGroup, Source = newSource });
        }

        /// <summary>
        /// Prüft vor der Aktivierung einer Quellgruppe (Transponder), ob der
        /// Suchlauf abgebrochen werden soll.
        /// </summary>
        /// <param name="location">Der Ursprung zur Quellgruppe.</param>
        /// <param name="group">Die zu aktivierende Quellgruppe.</param>
        /// <returns>Gesetzt, wenn der Suchlauf fortgesetzt werden soll.</returns>
        private bool GroupStart(GroupLocation location, SourceGroup group)
        {
            // See if handler is provided
            var onStartGroup = OnStartGroup;
            if (null == onStartGroup)
                return true;

            // Ask caller
            if (onStartGroup(location, group, this))
                return true;

            // Mark stop
            m_Aborted = true;

            // Done
            return false;
        }

        /// <summary>
        /// Prüft nach der Untersuchung einer Quellgruppe (Transponder), ob der
        /// Suchlauf abgebrochen werden soll.
        /// </summary>
        /// <param name="location">Der Ursprung zur Quellgruppe.</param>
        /// <param name="group">Die zu Quellgruppe, die gerade bearbeitet wurde.</param>
        /// <returns>Gesetzt, wenn der Suchlauf fortgesetzt werden soll.</returns>
        private bool GroupDone(GroupLocation location, SourceGroup group)
        {
            // See if handler is provided
            var onDoneGroup = OnDoneGroup;
            if (null == onDoneGroup)
                return true;

            // Ask caller
            if (onDoneGroup(location, group, this))
                return true;

            // Mark stop
            m_Aborted = true;

            // Done
            return false;
        }

        /// <summary>
        /// Prüft auf den Abbruch des Suchlaufs vor der Ausführung eines Ursprungs.
        /// </summary>
        /// <param name="location">Der zu besuchende Ursprung.</param>
        /// <returns>Gesetzt, wenn der Suchlauf fortgesetzt werden soll.</returns>
        private bool LocationStart(GroupLocation location)
        {
            // See if handler is provided
            var onStartLocation = OnStartLocation;
            if (null == onStartLocation)
                return true;

            // Ask caller
            if (onStartLocation(location, this))
                return true;

            // Mark stop
            m_Aborted = true;

            // Done
            return false;
        }

        /// <summary>
        /// Meldet, dass eine Quelle erkennt wurde und zur Senderliste hinzugeführt wurde.
        /// </summary>
        /// <param name="location">Der Ursprung, auf dem der Sender gefunden wurde.</param>
        /// <param name="group">Die Quellgruppe (Transponder), die den Sender anbietet.</param>
        /// <param name="station">Der gefundene Sender.</param>
        /// <returns>Gesetzt, wenn der Suchlauf fortgesetzt werden soll.</returns>
        private bool StationFound(GroupLocation location, SourceGroup group, Station station)
        {
            // See if handler is provided
            var onStationAdded = OnStationAdded;
            if (null == onStationAdded)
                return true;

            // Ask caller
            if (onStationAdded(location, group, station, this))
                return true;

            // Mark stop
            m_Aborted = true;

            // Done
            return false;
        }

        /// <summary>
        /// Prüft auf den Abbruch des Suchlaufs nach der Ausführung eines Ursprungs.
        /// </summary>
        /// <param name="location">Der gerade besuchte Ursprung.</param>
        /// <returns>Gesetzt, wenn der Suchlauf fortgesetzt werden soll.</returns>
        private bool LocationDone(GroupLocation location)
        {
            // See if handler is provided
            var onDoneLocation = OnDoneLocation;
            if (null == onDoneLocation)
                return true;

            // Ask caller
            if (onDoneLocation(location, this))
                return true;

            // Mark stop
            m_Aborted = true;

            // Done
            return false;
        }

        /// <summary>
        /// Meldet, ob der Suchlauf abgeschlossen wurde.
        /// </summary>
        public bool Done
        {
            get
            {
                // Attach to thread
                Thread worker = m_Worker;

                // Test
                return (null == worker) || !worker.IsAlive;
            }
        }

        /// <summary>
        /// Meldet, ob der Suchlauf durch den Aufrufer beendet wurde.
        /// </summary>
        public bool HasBeenAborted
        {
            get
            {
                // Report
                return m_Aborted;
            }
        }

        /// <summary>
        /// Meldet, ob der Suchlauf durch einen Fehler beendet wurde.
        /// </summary>
        public Exception Error
        {
            get
            {
                // Report
                return m_ThreadException;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endgültig und gibt alle noch
        /// verwendeten Ressourcen frei.
        /// </summary>
        public void Dispose()
        {
            // Terminate all
            Reset();
        }

        #endregion
    }
}
