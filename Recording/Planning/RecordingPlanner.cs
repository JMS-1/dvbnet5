﻿using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Services.Logging;

namespace JMS.DVB.NET.Recording.Planning;

/// <summary>
/// Die globale Aufzeichnungsplanung.
/// </summary>
/// <threadsafety static="true" instance="false">Diese Klasse kann nicht <see cref="Thread"/> 
/// übergreifend verwendet werden. Der Aufrufer hat für eine entsprechende Synchronisation zu 
/// sorgen.</threadsafety>
public class RecordingPlanner : IRecordingPlanner
{
    /// <summary>
    /// Die zugehörige Arbeitsumgebung.
    /// </summary>
    private readonly IRecordingPlannerSite _site;

    /// <summary>
    /// Verwaltet alle verwendeten Geräteprofile.
    /// </summary>
    private readonly Dictionary<string, IScheduleResource> m_resources = new(ProfileManager.ProfileNameComparer);

    /// <summary>
    /// Alle aktuellen periodischen Aufgaben.
    /// </summary>
    private readonly List<PeriodicScheduler> m_tasks = [];

    /// <summary>
    /// Die Verwaltung der Geräteprofile.
    /// </summary>
    private IResourceManager m_manager;

    /// <summary>
    /// Alle laufenden Aufzeichnungen.
    /// </summary>
    private readonly Dictionary<Guid, ScheduleInformation> m_started = [];

    private readonly ILogger<RecordingPlanner> _logger;

    /// <summary>
    /// Erstellt eine neue Planung.
    /// </summary>
    /// <param name="site">Die zugehörige Arbeitsumgebung.</param>
    public RecordingPlanner(IRecordingPlannerSite site, ILogger<RecordingPlanner> logger)
    {
        // Remember
        _logger = logger;
        _site = site;

        // Process all profiles
        foreach (var profileName in site.ProfileNames)
        {
            // Look up the profile
            var profile = ProfileManager.FindProfile(profileName);
            if (profile == null)
                continue;

            // Create the resource for it
            var profileResource = ProfileScheduleResource.Create(profileName);

            // Remember
            m_resources.Add(profileName, profileResource);

            // See if this is a leaf profile
            if (!string.IsNullOrEmpty(profile.UseSourcesFrom))
                continue;

            // See if we should process guide updates
            var guideTask = site.CreateProgramGuideTask(profileResource, profile);
            if (guideTask != null)
                m_tasks.Add(guideTask);

            // See if we should update the source list
            var scanTask = site.CreateSourceScanTask(profileResource, profile);
            if (scanTask != null)
                m_tasks.Add(scanTask);
        }

        // Make sure we report all errors
        try
        {
            // Create the manager
            m_manager = ResourceManager.Create(Tools.ScheduleRulesPath, ProfileManager.ProfileNameComparer);
        }
        catch (Exception e)
        {
            // Report
            _logger.LogError("Fehler beim Einlesen der Regeldatei für die Aufzeichnungsplanung: {0}", e.Message);

            // Use standard rules
            m_manager = ResourceManager.Create(ProfileManager.ProfileNameComparer);
        }

        // Safe configure it
        try
        {
            // All all resources
            foreach (var resource in m_resources.Values)
                m_manager.Add(resource);
        }
        catch (Exception e)
        {
            // Cleanup
            Dispose();

            // Report
            _logger.Log(e);
        }
    }

    /// <summary>
    /// Beendet die Nutzung dieser Instanz endgültig.
    /// </summary>
    public void Dispose() => Interlocked.Exchange(ref m_manager!, null)?.Dispose();

    /// <summary>
    /// Ermittelt zu einem Geräteprofil die zugehörige Ressourcenverwaltung.
    /// </summary>
    /// <param name="profileName">Der Name des Geräteprofils.</param>
    /// <returns>Die zugehörige Ressource, sofern bekannt.</returns>
    public IScheduleResource? GetResourceForProfile(string profileName)
    {
        // None
        if (string.IsNullOrEmpty(profileName))
            return null;

        // Ask map
        if (m_resources.TryGetValue(profileName, out var resource))
            return resource;

        // Not found
        return null;
    }

    /// <inheritdoc/>
    public void DispatchNextActivity(DateTime referenceTime)
    {
        // As long as necessary
        for (var skipped = new HashSet<Guid>(); ;)
        {
            // The plan context we created
            PlanContext? context = null;

            // Request activity - we only look 200 plan items into the future to reduce execution time at least a bit
            var activity = m_manager.GetNextActivity(referenceTime, (scheduler, time) => context = GetPlan(scheduler, time, skipped.Contains, 200));
            if (activity == null)
                return;

            // The easiest case is a wait
            if (activity is WaitActivity wait)
            {
                // Report to site
                _site.Idle(wait.RetestTime);

                // Done
                return;
            }

            // Start processing
            if (activity is StartActivity start)
            {
                // Check mode of operation
                var schedule = start.Recording;
                var definition = schedule.Definition;
                if (schedule.Resource == null)
                {
                    // Report to site
                    _site.Discard(definition);

                    // Add to exclusion list
                    skipped.Add(definition.UniqueIdentifier);

                    // Try again without
                    continue;
                }

                // Forward to site
                _site.Start(schedule, this, context!);

                // Done
                return;
            }

            // Must be some wrong version
            if (activity is not StopActivity stop)
                throw new NotSupportedException(activity.GetType().AssemblyQualifiedName);

            // Lookup the item and report to site
            if (!m_started.TryGetValue(stop.UniqueIdentifier, out var stopSchedule))
                return;

            // Report to site
            _site.Stop(stopSchedule.Schedule, this);

            return;
        }
    }

    /// <inheritdoc/>
    public bool Start(IScheduleInformation item)
    {
        // Validate
        if (item is ScheduleInformation)
            _logger.LogError("Es wird versucht, die Aufzeichnung '{1}' ({0}) mehrfach zu starten", item.Definition.UniqueIdentifier, item.Definition.Name);

        // Try start
        if (!m_manager.Start(item))
            return false;

        // Remember
        m_started.Add(item.Definition.UniqueIdentifier, new ScheduleInformation(item));

        // Did it
        return true;
    }

    /// <inheritdoc/>
    public void Stop(Guid itemIdentifier)
    {
        // Unregister
        if (!m_started.Remove(itemIdentifier))
            return;

        // Forward
        m_manager.Stop(itemIdentifier);
    }

    /// <inheritdoc/>
    public bool SetEndTime(Guid itemIdentifier, DateTime newEndTime)
    {
        // Find the recording
        var recording = m_manager.CurrentAllocations.FirstOrDefault(plan => plan.UniqueIdentifier.Equals(itemIdentifier));
        if (recording == null)
            return true;

        // New end time
        var newEndLimit = DateTime.UtcNow;
        if (newEndTime < newEndLimit)
            newEndTime = newEndLimit;

        // Forward
        if (!m_manager.Modify(itemIdentifier, newEndTime))
            return false;

        // See if we know it
        if (!m_started.TryGetValue(itemIdentifier, out var started))
            return true;

        // Try to get the new schedule data
        recording = m_manager.CurrentAllocations.FirstOrDefault(plan => plan.UniqueIdentifier.Equals(itemIdentifier));

        // Update
        if (recording != null)
            started.RealTime = recording.Time;

        // Did it
        return true;
    }

    /// <summary>
    /// Ermittelt den aktuellen Aufzeichnungsplan.
    /// </summary>
    /// <param name="scheduler">Die zu verwendende Zeitplanungsinstanz.</param>
    /// <param name="referenceTime">Der Bezugspunkt für die Planung.</param>
    /// <param name="disabled">Alle deaktivierte Aufträge und Aufgaben.</param>
    /// <param name="limit">Die Anzahl der zu berücksichtigenden Planungselemente.</param>
    /// <returns>Die Liste der nächsten Aufzeichnungen.</returns>
    private PlanContext GetPlan(RecordingScheduler scheduler, DateTime referenceTime, Func<Guid, bool> disabled, int limit)
    {
        // Create a new plan context
        var context = new PlanContext(m_started.Values);

        // Configure it
        _site.AddRegularJobs(scheduler, disabled, this, context);

        // Enable all
        if (disabled == null)
            disabled = identifier => false;

        // Do the sort
        context.LoadPlan(scheduler.GetSchedules(referenceTime, m_tasks.Where(task => !disabled(task.UniqueIdentifier)).ToArray()).Take(limit));

        // Report
        return context;
    }

    /// <inheritdoc/>
    public PlanContext GetPlan(DateTime referenceTime) => GetPlan(m_manager.CreateScheduler(false), referenceTime, null!, 1000);

    /// <inheritdoc/>
    public void Reset()
    {
        // Remove all
        foreach (var active in m_manager.CurrentAllocations)
            m_manager.Stop(active.UniqueIdentifier);

        // Forget what we did
        m_started.Clear();
    }
}

