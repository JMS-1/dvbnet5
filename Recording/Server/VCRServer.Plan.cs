using System.Timers;
using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Planning;
using JMS.DVB.NET.Recording.Services.Logging;

namespace JMS.DVB.NET.Recording.Server;

public partial class VCRServer
{
    /// <summary>
    /// Die zugehörige Aufzeichnungsplanung über alle Geräteprofile hinweg.
    /// </summary>
    private IRecordingPlanner m_planner = null!;

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
    private volatile Thread m_planThread = null!;

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
    private readonly object m_planAvailableSync = new();

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
                        // Reset timer
                        using (m_timer)
                            m_timer = null;

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
                m_pendingActions?.Invoke();
            }
            catch (Exception e)
            {
                // Report and ignore - we do not expect any error to occur
                logger.Log(e);
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
                logger.Log(LoggingLevel.Errors, "There is no outstanding asynchronous Recording Request for Schedule '{0}'", scheduleIdentifier);
            }
            else if (m_pendingSchedule.Definition.UniqueIdentifier != scheduleIdentifier)
            {
                // Report
                logger.Log(LoggingLevel.Errors, "Confirmed asynchronous Recording Request for Schedule '{0}' but waiting for '{1}'", scheduleIdentifier, m_pendingSchedule.Definition.UniqueIdentifier);
            }
            else
            {
                // Report
                logger.Log(LoggingLevel.Schedules, "Confirmed asynchronous Recording Request for Schedule '{0}'", scheduleIdentifier);

                // Check mode
                if (isStart != m_pendingStart)
                    logger.Log(LoggingLevel.Errors, "Recording Request confirmed wrong Type of Operation");

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
}
