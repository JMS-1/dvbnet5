using System.Text;
using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Planning;
using JMS.DVB.NET.Recording.Requests;
using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.Services.Planning;

/// <summary>
/// Verwaltet den Arbeitszustand aller Geräteprofile.
/// </summary>
/// <remarks>
/// Erzeugt eine neue Verwaltungsinstanz.
/// </remarks>
/// <param name="server">Die primäre VCR.NET Instanz.</param>
public class ProfileStateCollection(
    IVCRConfiguration configuration,
    IVCRProfiles profiles,
    ILogger logger,
    IJobManager jobs,
    IProfileStateFactory states,
    IExtensionManager extensions
) : IProfileStateCollection, IDisposable
{
    public IVCRProfiles Profiles { get; private set; } = profiles;

    /// <summary>
    /// Alle von dieser Instanz verwalteten Geräteprofile.
    /// </summary>
    private Dictionary<string, IProfileState> _stateMap;

    public ILogger Logger { get; private set; } = logger;

    public IJobManager JobManager { get; private set; } = jobs;

    public IVCRConfiguration Configuration { get; private set; } = configuration;

    public IExtensionManager ExtensionManager { get; private set; } = extensions;

    private readonly IProfileStateFactory _states = states;

    private Action? _restart;

    /// <inheritdoc/>
    public void Restart() => _restart?.Invoke();

    /// <inheritdoc/>
    public void Startup(Action restart)
    {
        _restart = restart;

        // Profiles to use
        var profileNames = Profiles.ProfileNames.ToArray();
        var nameReport = string.Join(", ", profileNames);

        // Log
        Logger.Log(LoggingLevel.Full, "Die Geräteprofile werden geladen: {0}", nameReport);

        // Report
        Tools.ExtendedLogging("Loading Profile Collection: {0}", nameReport);

        // Load current profiles
        _stateMap = profileNames.ToDictionary(
            profileName => profileName,
            profileName => _states.Create(this, profileName), ProfileManager.ProfileNameComparer
        );

        // Now we can create the planner
        m_planner = RecordingPlanner.Create(this, Configuration, Profiles, Logger, JobManager);
        m_planThread = new Thread(PlanThread) { Name = "Recording Planner", IsBackground = true };

        // Start planner
        m_planThread.Start();

    }

    /// <inheritdoc/>
    public IEnumerable<TInfo> InspectProfiles<TInfo>(Func<IProfileState, TInfo> factory) => _stateMap.Values.Select(factory);

    /// <inheritdoc/>
    public int NumberOfActiveRecordings => (_stateMap.Count < 1) ? 0 : _stateMap.Values.Sum(profile => profile.NumberOfActiveRecordings);

    /// <inheritdoc/>
    public IProfileState? this[string profileName]
    {
        get
        {
            // Validate
            if (string.IsNullOrEmpty(profileName) || profileName.Equals("*"))
            {
                // Attach to the default profile
                var defaultProfile = Profiles.DefaultProfile;
                if (defaultProfile == null)
                    return null;

                // Use it
                profileName = defaultProfile.Name;
            }

            // Load
            if (_stateMap.TryGetValue(profileName, out var profile))
                return profile;
            else
                return null;
        }
    }

    /// <inheritdoc/>
    public IProfileState? FindProfile(string profileName)
    {
        // Forward
        var state = this[profileName];
        if (state == null)
            Logger.LogError("Es gibt kein Geräteprofil '{0}'", profileName);

        // Report
        return state;
    }

    /// <inheritdoc/>
    public void ForceProgramGuideUpdate()
    {
        // Report
        Tools.ExtendedLogging("Full immediate Program Guide Update requested");

        // Forward to all
        ForEachProfile(state => state.ProgramGuide.LastUpdateTime = null);

        // Replan
        BeginNewPlan();
    }

    /// <inheritdoc/>
    public void ForceSoureListUpdate()
    {
        // Report
        Tools.ExtendedLogging("Full immediate Source List Update requested");

        // Forward
        ForEachProfile(state => state.LastSourceUpdateTime = null);

        // Replan
        BeginNewPlan();
    }

    /// <inheritdoc/>
    public bool IsActive => _stateMap.Values.Any(s => s.IsActive);

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
                Logger.Log(e);

                // See if we are allowed to ignore
                if (!ignoreErrors)
                    throw;
            }
    }

    #region IDisposable Members

    /// <summary>
    /// Beendet die Nutzung der zugehörigen Geräteprofile.
    /// </summary>
    public void Dispose()
    {
        // Load plan thread
        var planThread = m_planThread;

        // Full disable planner - make sure that order or lock is compatible with other scenarios
        lock (m_planner)
            lock (m_newPlanSync)
            {
                // Full deactivation
                m_plannerActive = false;
                m_planThread = null!;

                // Wake up call
                Monitor.Pulse(m_newPlanSync);
            }

        // Wait for thread to end
        if (planThread != null)
            planThread.Join();

        // Forget planner
        using (m_planner)
            m_planner = null!;

        // Be safe
        try
        {
            // Forward
            ForEachProfile(state => state.Dispose(), true);
        }
        finally
        {
            // Forget
            _stateMap.Clear();
        }
    }

    #endregion

    #region Die Aufzeichnungsplanung und die zugehörige Infrastruktur

    /// <summary>
    /// Die zugehörige Aufzeichnungsplanung über alle Geräteprofile hinweg.
    /// </summary>
    private RecordingPlanner m_planner = null!;

    /// <summary>
    /// Gesetzt, wenn die Aufzeichnungsplanung aktiv ist.
    /// </summary>
    private bool m_plannerActive = true;

    /// <summary>
    /// Die aktuell ausstehende Operation.
    /// </summary>
    private IScheduleInformation m_pendingSchedule = null!;

    /// <summary>
    /// Gesetzt, wenn auf das Starten einer Aufzeichnung gewartet wird.
    /// </summary>
    private bool m_pendingStart;

    /// <summary>
    /// Aktualisiert ständig die Planung.
    /// </summary>
    private volatile Thread m_planThread;

    /// <summary>
    /// Gesetzt, sobald die Berechnung eines neues Plans erwünscht wird.
    /// </summary>
    private volatile bool m_newPlan = true;

    /// <summary>
    /// Synchronisiert den Zugriff auf die Planungsbefehle.
    /// </summary>
    private readonly object m_newPlanSync = new();

    /// <summary>
    /// Synchronisiert die Freigabe auf Planungsbefehle.
    /// </summary>
    private readonly object m_planAvailableSync = new object();

    /// <summary>
    /// Sammelt Startvorgänge.
    /// </summary>
    private Action m_pendingActions = null!;

    /// <summary>
    /// Aktualisiert ständig die Planung.
    /// </summary>
    private void PlanThread()
    {
        // Always loop
        while (m_planThread != null)
        {
            // See if a new plan is requested
            lock (m_newPlanSync)
                while (!m_newPlan)
                    if (m_planThread == null)
                        return;
                    else
                        Monitor.Wait(m_newPlanSync);

            // At least we accepted the request
            m_newPlan = false;

            // See if we still have a planner
            var planner = m_planner;
            if (planner == null)
                break;

            // Just take a look what to do next
            try
            {
                // Reset start actions
                m_pendingActions = null!;

                // Protect planning.
                lock (planner)
                {
                    // Report
                    Tools.ExtendedLogging("Woke up for Plan Calculation at {0}", DateTime.Now);

                    // Retest
                    if (m_planThread == null)
                        break;

                    // See if we are allowed to take the next step in plan - we schedule only one activity at a time
                    if (m_pendingSchedule == null)
                    {
                        // Analyse plan
                        planner.DispatchNextActivity(DateTime.UtcNow);

                        // If we are shutting down better forget anything we did in the previous step - only timer setting matters!
                        if (!m_plannerActive)
                        {
                            // Reset to initial state
                            m_pendingSchedule = null!;
                            m_pendingActions = null!;
                            m_pendingStart = false;

                            // And forget all allocations
                            planner.Reset();
                        }
                    }
                }

                // Run start and stop actions outside the planning lock (to avoid deadlocks) but inside the hibernation protection (to forbid hibernation)
                var toStart = m_pendingActions;
                if (toStart != null)
                    toStart();
            }
            catch (Exception e)
            {
                // Report and ignore - we do not expect any error to occur
                Logger.Log(e);
            }

            // New plan is now available - beside termination this will do nothing at all but briefly aquiring an idle lock
            lock (m_planAvailableSync)
                Monitor.PulseAll(m_planAvailableSync);
        }
    }

    /// <inheritdoc/>
    public PlanContext GetPlan()
    {
        // See if we are still up and running
        var planner = m_planner;
        if (planner == null)
            return new PlanContext(null!);

        // Ensure proper synchronisation with planning thread
        lock (planner)
            return planner.GetPlan(DateTime.UtcNow);
    }

    /// <inheritdoc/>
    public void BeginNewPlan()
    {
        // Check flag
        if (m_newPlan)
            return;

        // Synchronize
        lock (m_newPlanSync)
        {
            // Test again - this may avoid some wake ups of the planning thread
            if (m_newPlan)
                return;

            // Set it
            m_newPlan = true;

            // Fire
            Monitor.Pulse(m_newPlanSync);
        }
    }

    /// <inheritdoc/>
    public void EnsureNewPlan()
    {
        // Make sure that we use an up do date plan
        lock (m_planAvailableSync)
            for (int i = 2; i-- > 0;)
            {
                // Enforce calculation
                BeginNewPlan();

                // Wait
                Monitor.Wait(m_planAvailableSync);
            }
    }

    /// <inheritdoc/>
    public bool ChangeEndTime(Guid scheduleIdentifier, DateTime newEndTime)
    {
        // See if planner exists
        var planner = m_planner;
        if (planner == null)
            return false;

        // While forwarding make sure that we don't interfere with the planning thread
        lock (planner)
            if (!planner.SetEndTime(scheduleIdentifier, newEndTime))
                return false;

        // Spawn new check on schedules
        BeginNewPlan();

        // Report that we did it
        return true;
    }

    /// <inheritdoc/>
    public void ConfirmOperation(Guid scheduleIdentifier, bool isStart)
    {
        // Protect and check
        var planner = m_planner;
        if (planner == null)
            return;

        // Make sure that we synchronize with the planning thread
        lock (planner)
            if (m_pendingSchedule == null)
            {
                // Report
                Logger.Log(LoggingLevel.Errors, "There is no outstanding asynchronous Recording Request for Schedule '{0}'", scheduleIdentifier);
            }
            else if (m_pendingSchedule.Definition.UniqueIdentifier != scheduleIdentifier)
            {
                // Report
                Logger.Log(LoggingLevel.Errors, "Confirmed asynchronous Recording Request for Schedule '{0}' but waiting for '{1}'", scheduleIdentifier, m_pendingSchedule.Definition.UniqueIdentifier);
            }
            else
            {
                // Report
                Logger.Log(LoggingLevel.Schedules, "Confirmed asynchronous Recording Request for Schedule '{0}'", scheduleIdentifier);

                // Check mode
                if (isStart != m_pendingStart)
                    Logger.Log(LoggingLevel.Errors, "Recording Request confirmed wrong Type of Operation");

                // Finish
                if (m_pendingStart)
                    planner.Start(m_pendingSchedule);
                else
                    planner.Stop(scheduleIdentifier);

                // Reset
                m_pendingSchedule = null!;
            }

        // See what to do next
        BeginNewPlan();
    }

    #endregion

    #region Verarbeitung des nächsten Arbeitsschrittes der Aufzeichnungsplanung

    /// <summary>
    /// Der volle Pfad zu dem Regeldatei der Aufzeichnungsplanung.
    /// </summary>
    public string ScheduleRulesPath { get { return Path.Combine(Tools.ApplicationDirectory.FullName, "SchedulerRules.cmp"); } }

    /// <summary>
    /// Meldet die Namen der verwendeten Geräteprofile.
    /// </summary>
    IEnumerable<string> IRecordingPlannerSite.ProfileNames { get { return _stateMap.Keys; } }

    /// <summary>
    /// Erstellt einen periodischen Auftrag für die Aktualisierung der Programmzeitschrift.
    /// </summary>
    /// <param name="resource">Die zu verwendende Ressource.</param>
    /// <param name="profile">Das zugehörige Geräteprofil.</param>
    /// <returns>Der gewünschte Auftrag.</returns>
    PeriodicScheduler IRecordingPlannerSite.CreateProgramGuideTask(IScheduleResource resource, Profile profile, IVCRConfiguration configuration, IJobManager jobs)
    {
        // Protect against misuse
        if (_stateMap.TryGetValue(profile.Name, out var state))
            return new ProgramGuideTask(resource, state, configuration, jobs);
        else
            return null!;
    }

    /// <summary>
    /// Erstellt einen periodischen Auftrag für die Aktualisierung der Liste der Quellen.
    /// </summary>
    /// <param name="resource">Die zu verwendende Ressource.</param>
    /// <param name="profile">Das zugehörige Geräteprofil.</param>
    /// <returns>Der gewünschte Auftrag.</returns>
    PeriodicScheduler IRecordingPlannerSite.CreateSourceScanTask(
        IScheduleResource resource,
        Profile profile,
        IVCRConfiguration configuration,
        IJobManager jobs
    )
    {
        // Protect against misuse
        if (_stateMap.TryGetValue(profile.Name, out var state))
            return new SourceListTask(resource, state, configuration, jobs);
        else
            return null!;
    }

    /// <summary>
    /// Ergänzt alle bekannten Aufzeichnungen zu einer Planungsinstzanz.
    /// </summary>
    /// <param name="scheduler">Die Verwaltung der Aufzeichnungen.</param>
    /// <param name="disabled">Alle nicht zu verwendenden Aufzeichnungen.</param>
    /// <param name="planner">Die Gesamtplanung.</param>
    /// <param name="context">Zusätzliche Informationen zur aktuellen Planung.</param>
    void IRecordingPlannerSite.AddRegularJobs(RecordingScheduler scheduler, Func<Guid, bool> disabled, RecordingPlanner planner, PlanContext context, IVCRProfiles profiles)
    {
        // Retrieve all jobs related to this profile
        foreach (var job in JobManager.GetActiveJobs())
            foreach (var schedule in job.Schedules)
            {
                // No longer in use
                if (!schedule.IsActive)
                    continue;

                // Resolve source
                var source = schedule.Source ?? job.Source;
                if (source == null)
                    continue;

                // Resolve profile
                var resource = planner.GetResourceForProfile(source.ProfileName);
                if (resource == null)
                    continue;

                // Register single item
                JobManager.AddToScheduler(schedule, scheduler, job, [resource], (s, p) => p.FindSource(s), disabled);

                // Remember - even if we skipped it
                context.RegisterSchedule(schedule, job);
            }
    }

    /// <summary>
    /// Es sind keine weiteren Aktionen notwendig.
    /// </summary>
    /// <param name="until">Zu diesem Zeitpunkt soll die nächste Prüfung stattfinden.</param>
    void IRecordingPlannerSite.Idle(DateTime until)
    {
    }

    /// <summary>
    /// Meldet, dass eine Aufzeichnung nicht ausgeführt werden kann.
    /// </summary>
    /// <param name="item">Die nicht ausgeführte Aufzeichnung.</param>
    void IRecordingPlannerSite.Discard(IScheduleDefinition item) =>
        Logger.Log(LoggingLevel.Schedules, "Could not record '{0}'", item.Name);

    /// <summary>
    /// Meldet, dass eine Aufzeichnung nun beendet werden kann.
    /// </summary>
    /// <param name="item">Die betroffene Aufzeichnung.</param>
    /// <param name="planner">Die Planungsinstanz.</param>
    void IRecordingPlannerSite.Stop(IScheduleInformation item, RecordingPlanner planner)
    {
        // Report
        Logger.Log(LoggingLevel.Schedules, "Done recording '{0}'", item.Definition.Name);

        // Locate the profile - if we don't find it we are in big trouble!
        if (!_stateMap.TryGetValue(item.Resource.Name, out var profile))
            return;

        // Mark as pending
        m_pendingSchedule = item;
        m_pendingStart = false;

        // Forward request to profile manager
        m_pendingActions += () => profile.EndRecording(item.Definition.UniqueIdentifier);
    }

    /// <summary>
    /// Meldet, dass eine Aufzeichnung nun beginnen sollte.
    /// </summary>
    /// <param name="item">Die zu startende Aufzeichnung.</param>
    /// <param name="planner">Die Planungsinstanz.</param>
    /// <param name="context">Zusatzinformationen zur Aufzeichnungsplanung.</param>
    void IRecordingPlannerSite.Start(IScheduleInformation item, RecordingPlanner planner, PlanContext context)
    {
        // We are no longer active - simulate start and do nothing
        if (!m_plannerActive)
        {
            // Make planner believe we did it
            planner.Start(item);

            // Done
            return;
        }

        // Report
        Logger.Log(LoggingLevel.Schedules, "Start recording '{0}'", item.Definition.Name);

        // Locate the profile - if we don't find it we are in big trouble!
        if (!_stateMap.TryGetValue(item.Resource.Name, out var profile))
            return;

        // Mark as pending
        m_pendingSchedule = item;
        m_pendingStart = true;

        // Create the recording
        var recording = VCRRecordingInfo.Create(item, context, Configuration, Profiles)!;

        // Check for EPG
        var guideUpdate = item.Definition as ProgramGuideTask;
        if (guideUpdate != null)
        {
            // Start a new guide collector
            m_pendingActions += ProgramGuideProxy.Create(profile, recording).Start;
        }
        else
        {
            // Check for PSI
            var sourceUpdate = item.Definition as SourceListTask;
            if (sourceUpdate != null)
            {
                // Start a new update
                m_pendingActions += SourceScanProxy.Create(profile, recording).Start;
            }
            else
            {
                // Start a regular recording - profile will decide if to join an existing recording
                m_pendingActions += () => profile.StartRecording(recording);
            }
        }
    }

    #endregion

    /// <inheritdoc/>
    public string SchedulerRules
    {
        get
        {
            // Attach to the path
            var rulePath = ScheduleRulesPath;
            if (File.Exists(rulePath))
                using (var reader = new StreamReader(rulePath, true))
                    return reader.ReadToEnd().Replace("\r\n", "\n");

            // Not set
            return null!;
        }
        set
        {
            // Check mode
            var rulePath = ScheduleRulesPath;
            if (string.IsNullOrWhiteSpace(value))
            {
                // Back to default
                if (File.Exists(rulePath))
                    File.Delete(rulePath);
            }
            else
            {
                // Update line feeds
                var content = value.Replace("\r\n", "\n").Replace("\n", "\r\n");
                var scratchFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

                // Write to scratch file
                File.WriteAllText(scratchFile, content, Encoding.UTF8);

                // With cleanup
                try
                {
                    // See if resource manager could be created
                    ResourceManager.Create(scratchFile, ProfileManager.ProfileNameComparer).Dispose();

                    // Try to overwrite
                    File.Copy(scratchFile, rulePath, true);
                }
                finally
                {
                    // Get rid of scratch file
                    File.Delete(scratchFile);
                }
            }
        }
    }

}
