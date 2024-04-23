using JMS.DVB.CardServer;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.ProgramGuide;
using JMS.DVB.NET.Recording.Requests;
using JMS.DVB.NET.Recording.Status;

namespace JMS.DVB.NET.Recording.Services;

/// <summary>
/// Beschreibt den Arbeitszustand eines einzelnen aktiven Geräteprofils.
/// </summary>
/// <param name="collection">Die zugehörige Verwaltung der aktiven Geräteprofile.</param>
/// <param name="profileName">Der Name des zugehörigen Geräteprofils.</param>
public class ProfileState(IProfileStateCollection collection, string profileName) : IProfileState
{
    /// <inheritdoc/>
    public string ProfileName => profileName;

    /// <inheritdoc/>
    public IProfileStateCollection Collection => collection;

    /// <inheritdoc/>
    public ProgramGuideManager ProgramGuide { get; } = new ProgramGuideManager(collection, profileName);

    /// <summary>
    /// Meldet das zugehörige Geräteprofil.
    /// </summary>
    public Profile? Profile => collection.Profiles.FindProfile(profileName);

    /// <inheritdoc/>
    public bool IsResponsibleFor(SourceSelection source)
        => source == null ? false : ProfileManager.ProfileNameComparer.Equals(ProfileName, source.ProfileName);

    /// <inheritdoc/>
    public TStatus LiveModeOperation<TStatus>(bool active, string connectTo, SourceIdentifier source, Func<string, ServerInformation, TStatus> factory, ServiceFactory services)
    {
        // Check mode of operation
        if (!active)
        {
            // Deactivate
            var activeRequest = m_CurrentRequest as ZappingProxy;
            if (activeRequest != null)
                activeRequest.Stop();
        }
        else if (!string.IsNullOrEmpty(connectTo))
        {
            // Activate 
            ZappingProxy.Create(this, connectTo, services).Start();
        }
        else if (source != null)
        {
            // Switch source
            var request = m_CurrentRequest as ZappingProxy;
            if (request != null)
                return request.SetSource(source, factory);
        }

        // See if we have a current request
        var statusRequest = m_CurrentRequest as ZappingProxy;
        if (statusRequest == null)
            return factory(null!, null!);
        else
            return statusRequest.CreateStatus(factory);
    }

    /// <summary>
    /// Beendet die Nutzung dieses Geräteprofils.
    /// </summary>
    public void Dispose()
    {
        // Stop request
        Stop();
    }

    #region Aktuelle Aufzeichnung

    /// <summary>
    /// Der aktuelle Zugriff auf die zugehörige DVB.NET Hardwareabstraktion.
    /// </summary>
    private volatile CardServerProxy m_CurrentRequest = null!;

    /// <summary>
    /// Synchronisiert den Zugriff auf die aktuelle Operation.
    /// </summary>
    private object m_RequestLock = new object();

    /// <summary>
    /// Der aktuelle Zugriff auf die zugehörige DVB.NET Hardwareabstraktion. Der Aufrufer hält alle
    /// notwendigen Sperren.
    /// </summary>
    /// <param name="newRequest">Der neue Auftrag, der ab sofort ausgeführt wird.</param>
    private void ChangeCurrentRequest(CardServerProxy newRequest)
    {
        // Detach from previous
        var request = m_CurrentRequest;
        if (!ReferenceEquals(request, null))
            request.Deactivate();

        // Update
        m_CurrentRequest = newRequest;

        // Attach to this one
        if (!ReferenceEquals(newRequest, null))
            newRequest.Activate();
    }

    /// <inheritdoc/>
    public bool BeginRequest(CardServerProxy request, bool throwOnBusy = true)
    {
        // Validate
        if (ReferenceEquals(request, null))
            throw new ArgumentNullException(nameof(request));
        if (!ReferenceEquals(request.ProfileState, this))
            throw new ArgumentException(request.ProfileName, "request.ProfileState");

        // Synchronize
        lock (m_RequestLock)
        {
            // Wait for current request to end
            for (; ; )
            {
                // Load
                var current = m_CurrentRequest;
                if (ReferenceEquals(current, null))
                    break;

                // Failed
                if (!current.IsShuttingDown)
                    if (current is ZappingProxy)
                        current.Stop(false);
                    else if (throwOnBusy)
                        throw new InvalidOperationException("Profile is already running a Request");
                    else
                        return false;

                // Wait for it to end
                Monitor.Wait(m_RequestLock);
            }

            // Take new
            ChangeCurrentRequest(request);
        }

        // Did it
        return true;
    }

    /// <inheritdoc/>
    public void EndRequest(CardServerProxy request)
    {
        // Validate
        if (ReferenceEquals(request, null))
            throw new ArgumentNullException(nameof(request));

        // Synchronize
        lock (m_RequestLock)
        {
            // Process
            if (ReferenceEquals(m_CurrentRequest, request))
                ChangeCurrentRequest(null!);
            else
                throw new InvalidOperationException("Wrong Request to End");

            // Fire notification
            Monitor.PulseAll(m_RequestLock);
        }
    }

    /// <summary>
    /// Beendet die aktuelle Aktivität.
    /// </summary>
    private void Stop()
    {
        // Request the stop
        var request = m_CurrentRequest;
        if (request != null)
            request.Stop();
    }

    /// <inheritdoc/>
    public bool IsActive { get { return !ReferenceEquals(m_CurrentRequest, null); } }

    /// <inheritdoc/>
    public FullInfo? CurrentRecording
    {
        get
        {
            // Report
            var current = m_CurrentRequest;
            if (current is null)
                return null;
            else
                return current.CreateFullInformation();
        }
    }

    #endregion

    #region Reguläre Aufzeichnungen

    /// <inheritdoc/>
    public void StartRecording(VCRRecordingInfo recording)
    {
        // Protect current request against transistions
        lock (m_RequestLock)
            for (; ; )
            {
                // Attach to the current request
                var current = m_CurrentRequest;

                // In best case we are just doing nothing
                if (ReferenceEquals(current, null))
                {
                    // Create a brand new regular recording request
                    var request = new RecordingProxy(this, recording, collection.ServiceFactory);

                    // Activate the request
                    request.Start();

                    // We dit it
                    break;
                }
                else if (current is ZappingProxy)
                {
                    // Regular recordings have priority over LIVE mode so request stop and try again later
                    current.Stop(false);
                }
                else if (!current.IsShuttingDown)
                {
                    // There is a current recording  which is not terminating so just join it
                    current.Start(recording);

                    // We dit it
                    break;
                }

                // Wait for transition notification
                Monitor.Wait(m_RequestLock);
            }
    }

    /// <inheritdoc/>
    public void EndRecording(Guid scheduleIdentifier)
    {
        // Protect current request against transistions
        lock (m_RequestLock)
        {
            // Attach to the current request
            var current = m_CurrentRequest;

            // See if it exists
            if (ReferenceEquals(current, null))
            {
                // Report
                collection.Logger.Log(LoggingLevel.Errors, "Request to end Recording '{0}' but there is no active Recording Process", scheduleIdentifier);

                // Better do it ourselves
                Collection.ConfirmOperation(scheduleIdentifier, false);
            }
            else
            {
                // Just stop
                current.Stop(scheduleIdentifier);
            }
        }
    }

    /// <inheritdoc/>
    public CardServerProxy? ChangeStreamEnd(Guid streamIdentifier, DateTime newEndTime, bool disableHibernation)
    {
        // Be safe
        lock (m_RequestLock)
        {
            // Attach to current request
            var current = m_CurrentRequest;
            if (current is null)
                return null;

            // Make sure that job is not restarted
            current.SetRestartThreshold(streamIdentifier);

            // Modify
            current.ChangeEndTime(streamIdentifier, newEndTime, disableHibernation);

            // Report success
            return current;
        }
    }

    /// <inheritdoc/>
    public int NumberOfActiveRecordings => m_CurrentRequest?.NumberOfActiveRecordings ?? 0;

    /// <inheritdoc/>
    public bool SetStreamTarget(SourceIdentifier source, Guid uniqueIdentifier, string target)
    {
        // Get the current request
        var request = m_CurrentRequest as RecordingProxy;
        if (ReferenceEquals(request, null))
            return false;

        // Process
        request.SetStreamTarget(source, uniqueIdentifier, target);

        // Done
        return true;
    }

    #endregion

    #region Quellenlisten verwalten

    /// <summary>
    /// Meldet den Namen des Wertes in der Registrierung von Windows, wo der Zeitpunkt
    /// der letzten Aktualisierung der Liste der Quellen gespeichert wird.
    /// </summary>
    private string SourceUpdateRegistryName => $"LastPSIRun {ProfileName}";

    /// <inheritdoc/>
    public DateTime? LastSourceUpdateTime
    {
        get { return Tools.GetRegistryTime(SourceUpdateRegistryName, collection.Logger); }
        set { Tools.SetRegistryTime(SourceUpdateRegistryName, value, collection.Logger); }
    }

    #endregion
}

