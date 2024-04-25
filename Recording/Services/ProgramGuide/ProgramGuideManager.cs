using System.Text;
using JMS.DVB.NET.Recording.ProgramGuide;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Services.ProgramGuide;

/// <summary>
/// Verwaltet die elektronischen Programmzeitschrift von VCR.NET.
/// </summary>
public class ProgramGuideManager : IProgramGuideManager
{
    /// <summary>
    /// Die zugehörige Auftragsverwaltung.
    /// </summary>
    private readonly IJobManager _jobs;

    /// <inheritdoc/>
    public string ProfileName { get; }

    /// <summary>
    /// Meldet die zugehörige Datei mit den Daten der Programmzeitschrift.
    /// </summary>
    public FileInfo ProgramGuideFile { get; }

    /// <summary>
    /// Die Daten der Programmzeitschrift.
    /// </summary>
    private volatile ProgramGuideEntries m_Events = new();

    /// <summary>
    /// Die Daten der Programmzeitschrift.
    /// </summary>
    public ProgramGuideEntries Events => m_Events;

    private readonly IProfileStateCollection _collection;

    private readonly ILogger _logger;

    private readonly IVCRProfiles _profiles;

    /// <summary>
    /// Erzeugt eine neue Verwaltungsinstanz.
    /// </summary>
    /// <param name="jobs">Die zugehörige Auftragsverwaltung.</param>
    /// <param name="profileName">Der Name des verwalteten DVB.NET Geräteprofils.</param>
    public ProgramGuideManager(
        IProfileStateCollection collection,
        string profileName,
        IVCRProfiles profiles,
        IJobManager jobs,
        ILogger logger
    )
    {
        // Remember
        _collection = collection;
        _jobs = jobs;
        _logger = logger;
        _profiles = profiles;
        ProfileName = profileName;

        // Calculate file
        ProgramGuideFile = new FileInfo(Path.Combine(_jobs.CollectorDirectory.FullName, $"EPGData for {ProfileName}.xml"));

        // See if profile has it's own program guide
        if (!HasProgramGuide)
            return;

        // Report
        Tools.ExtendedLogging("Looking for Program Guide of {0}", ProfileName);

        // No such file - start empty
        if (!ProgramGuideFile.Exists)
            return;

        // Process
        var events = SerializationTools.Load<ProgramGuideEntries>(ProgramGuideFile);
        if (events != null)
        {
            // Use it
            m_Events = events;

            // Report
            Tools.ExtendedLogging("Found valid Program Guide and using it");

            // Done
            return;
        }

        // Report
        _logger.Log(LoggingLevel.Errors, "Die Datei '{0}' zur Programmzeitschrift ist ungültig", ProgramGuideFile.FullName);

        // Save delete
        try
        {
            // Process
            ProgramGuideFile.Delete();
        }
        catch
        {
            // Discard any error
            _logger.Log(LoggingLevel.Errors, "Die Datei '{0}' zur Programmzeitschrift kann nicht gelöscht werden", ProgramGuideFile.FullName);
        }
    }

    /// <summary>
    /// Ermittelt das Geräteprofil, das die elektronische Programmzeitschrift für dieses
    /// Profil zur Verfügung stellt.
    /// </summary>
    public Profile? LeafGuideProfile
    {
        get
        {
            // Attach to us
            var me = Profile;

            // Cycle protection
            var done = new Dictionary<string, Profile>(ProfileManager.ProfileNameComparer);

            // Process using full profile chain
            for (var test = me; ; test = ProfileManager.FindProfile(test.UseSourcesFrom))
            {
                // None existing
                if (null == test)
                    return me;

                // Cycle
                if (done.ContainsKey(test.Name))
                    return me;

                // See if this is active
                if (_profiles.FindProfile(test.Name) != null)
                    if (string.IsNullOrEmpty(test.UseSourcesFrom))
                        return test;

                // Remember
                done[test.Name] = test;
            }
        }
    }

    /// <summary>
    /// Meldet, ob zu diesem Geräteprofil eine Programmzeitschrift geführt wird.
    /// </summary>
    public bool HasProgramGuide
    {
        get
        {
            // Attach to the profile
            var profile = LeafGuideProfile;

            return profile != null && string.Compare(profile.Name, ProfileName, true) == 0;
        }
    }

    /// <summary>
    /// Meldet das zugehörige Geräteprofil.
    /// </summary>
    public Profile? Profile => _profiles.FindProfile(ProfileName);

    /// <summary>
    /// Meldet den Namen des Wertes in der Registrierung von Windows, wo der Zeitpunkt
    /// der letzten Ausführung gespeichert wird.
    /// </summary>
    private string UpdateRegistryName => $"LastEPGRun {ProfileName}";

    /// <inheritdoc/>
    public DateTime? LastUpdateTime
    {
        get { return Tools.GetRegistryTime(UpdateRegistryName, _logger); }
        set { Tools.SetRegistryTime(UpdateRegistryName, value, _logger); }
    }

    /// <inheritdoc/>
    public void UpdateGuide(ProgramGuideEntries entries)
    {
        // Report
        Tools.ExtendedLogging("Program Guide of {0} will be updated", ProfileName);

        // Did collection
        LastUpdateTime = DateTime.UtcNow;

        // Try to load resulting file
        try
        {
            // Create brand new
            var newData = m_Events.Clone();

            // Merge new
            newData.Merge(entries);

            // Cleanup
            newData.DiscardOld();

            // Save
            SerializationTools.Save(newData, ProgramGuideFile, Encoding.UTF8);

            // Use it
            m_Events = newData;

            // Report
            Tools.ExtendedLogging("Now using new Program Guide");
        }
        catch (Exception e)
        {
            // Report
            _logger.Log(LoggingLevel.Errors, "Fehler beim Aktualisieren der Programmzeitschrift: {0}", e);
        }
    }

    /// <inheritdoc/>
    public bool HasEntry(SourceIdentifier source, DateTime start, DateTime end)
    {
        // Forward
        var entries = LeafEntries;
        if (entries == null)
            return false;
        else
            return entries.HasEntry(source, start, end);
    }

    /// <inheritdoc/>
    public TTarget FindBestEntry<TTarget>(
        SourceIdentifier source,
        DateTime start,
        DateTime end,
        Func<ProgramGuideEntry, string, IVCRProfiles, TTarget> factory
    )
    {
        // Forward
        var entries = LeafEntries;
        if (entries == null)
            return default!;
        else
            return entries.FindBestEntry(source, start, end, entry => factory(entry, ProfileName, _profiles));
    }

    /// <inheritdoc/>
    public ProgramGuideEntry? FindEntry(SourceIdentifier source, DateTime start) => LeafEntries?.FindEntry(source, start);

    /// <inheritdoc/>
    public TEntry[] GetProgramGuideEntries<TEntry>(GuideEntryFilter filter, Func<ProgramGuideEntry, string, IVCRProfiles, TEntry> factory)
    {
        // See if there is a guide
        var entries = LeafEntries;
        if (entries == null)
            return [];
        else
            return filter.Filter(entries.Events, _profiles).Select(entry => factory(entry, ProfileName, _profiles)).ToArray();
    }

    /// <inheritdoc/>
    public int GetProgramGuideEntries(GuideEntryFilter filter)
    {
        // See if there is a guide
        var entries = LeafEntries;
        if (entries == null)
            return 0;

        return filter.Filter(entries.Events, _profiles).Count();
    }

    /// <inheritdoc/>
    public ProgramGuideEntries? LeafEntries
    {
        get
        {
            // See if we are already done
            var profiles = _collection;
            if (profiles == null)
                return null;

            // Get the profile holding the data
            var profile = LeafGuideProfile;
            if (profile == null)
                return null;

            // Find it
            var state = profiles[profile.Name];
            if (state == null)
                return null;

            // Forward
            return state.ProgramGuide.Events;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<ProgramGuideEntry> GetEntries(SourceIdentifier source)
    {
        // Find the real holder
        var entries = LeafEntries;
        if (null == entries)
            entries = new ProgramGuideEntries();

        // Report
        return entries.GetEntries(source);
    }
}

