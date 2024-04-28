using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Planning;
using JMS.DVB.NET.Recording.Services.Logging;

namespace JMS.DVB.NET.Recording.Server;

public partial class VCRServer
{
    /// <inheritdoc/>
    public int NumberOfActiveRecordings => (_stateMap.Count < 1) ? 0 : _stateMap.Values.Sum(profile => profile.NumberOfActiveRecordings);

    /// <inheritdoc/>
    public bool IsActive => _stateMap.Values.Any(s => s.IsActive);

    /// <summary>
    /// Meldet, dass eine Aufzeichnung nun beendet werden kann.
    /// </summary>
    /// <param name="item">Die betroffene Aufzeichnung.</param>
    /// <param name="planner">Die Planungsinstanz.</param>
    void IRecordingPlannerSite.Stop(IScheduleInformation item, RecordingPlanner planner)
    {
        // Report
        logger.Log(LoggingLevel.Schedules, "Done recording '{0}'", item.Definition.Name);

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
        logger.Log(LoggingLevel.Schedules, "Start recording '{0}'", item.Definition.Name);

        // Locate the profile - if we don't find it we are in big trouble!
        if (!_stateMap.TryGetValue(item.Resource.Name, out var profile))
            return;

        // Mark as pending
        m_pendingSchedule = item;
        m_pendingStart = true;

        // Create the recording
        var recording = recordingFactory.Create(item, context)!;

        // Check for EPG
        if (item.Definition is ProgramGuideTask guideUpdate)
        {
            // Start a new guide collector
            m_pendingActions += guideFactory.Create(profile, recording).Start;
        }
        else
        {
            // Check for PSI
            if (item.Definition is SourceListTask sourceUpdate)
            {
                // Start a new update
                m_pendingActions += scanFactory.Create(profile, recording).Start;
            }
            else
            {
                // Start a regular recording - profile will decide if to join an existing recording
                m_pendingActions += () => profile.StartRecording(recording);
            }
        }
    }
}
