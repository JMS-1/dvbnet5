using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Actions;
using JMS.DVB.NET.Recording.Planning;
using JMS.DVB.NET.Recording.Requests;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Logging;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Server;

/// <summary>
/// Verwaltet den Arbeitszustand aller Geräteprofile.
/// </summary>
/// <remarks>
/// Erzeugt eine neue Verwaltungsinstanz.
/// </remarks>
/// <param name="server">Die primäre VCR.NET Instanz.</param>
public partial class VCRServer(
    IVCRConfiguration configuration,
    IVCRProfiles profiles,
    ILogger<VCRServer> logger,
    IJobManager jobs,
    IProfileStateFactory states,
    IRecordingPlannerFactory plannerFactory,
    IRecordingInfoFactory recordingFactory,
    IProgramGuideProxyFactory guideFactory,
    ISourceScanProxyFactory scanFactory
) : IVCRServer, IDisposable
{
    private Action? _restart;

    /// <inheritdoc/>
    public void Restart() => _restart?.Invoke();

    /// <inheritdoc/>
    public void Startup(Action restart)
    {
        _restart = restart;

        // Profiles to use
        var profileNames = profiles.ProfileNames.ToArray();
        var nameReport = string.Join(", ", profileNames);

        // Log
        logger.Log(LoggingLevel.Full, "Die Geräteprofile werden geladen: {0}", nameReport);

        // Report
        Tools.ExtendedLogging("Loading Profile Collection: {0}", nameReport);

        // Load current profiles
        _stateMap = profileNames.ToDictionary(
            profileName => profileName,
            profileName => states.Create(this, profileName), ProfileManager.ProfileNameComparer
        );

        // Now we can create the planner
        m_planner = plannerFactory.Create(this);
        m_planThread = new Thread(PlanThread) { Name = "Recording Planner", IsBackground = true };

        // Start planner
        m_planThread.Start();

    }

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
        logger.Log(LoggingLevel.Schedules, "Could not record '{0}'", item.Name);
}
