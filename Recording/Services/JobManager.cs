using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.RestWebApi;

namespace JMS.DVB.NET.Recording.Services;

/// <summary>
/// Verwaltung aller Aufträge für alle DVB.NET Geräteprofile.
/// </summary>
/// <remarks>LEAF SERVICE</remarks>
public class JobManager : IJobManager
{
    /// <summary>
    /// Das Format, in dem das reine Datum in den Dateinamen von Protokolleinträgen codiert wird.
    /// </summary>
    private const string LogEntryDateFormat = "yyyyMMdd";

    /// <summary>
    /// Das Format, in dem die Uhrzeit von Protokolleinträgen codiert ist.
    /// </summary>
    private const string LogEntryTimeFormat = "HHmmssfffffff";

    /// <summary>
    /// Ermittelt das Protokollverzeichnis vom VCR.NET.
    /// </summary>
    private DirectoryInfo LogDirectory => new(Path.Combine(RootDirectory.FullName, "Logs"));

    /// <inheritdoc/>
    public DirectoryInfo CollectorDirectory => new(Path.Combine(RootDirectory.FullName, "EPG"));

    /// <summary>
    /// Ermittelt das Verzeichnis aller aktiven Aufträge vom VCR.NET.
    /// </summary>
    private DirectoryInfo JobDirectory => new(Path.Combine(RootDirectory.FullName, "Active"));

    /// <summary>
    /// Meldet das Wurzelverzeichnis, unter dem Aufträge und Protokolle abgelegt werden.
    /// </summary>
    private DirectoryInfo RootDirectory { get; set; }

    /// <summary>
    /// 
    /// </summary>
    private IVCRProfiles Profiles { get; set; }

    /// <summary>
    /// Vorhaltung aller Aufträge.
    /// </summary>
    private readonly Dictionary<Guid, VCRJob> m_Jobs = [];

    private readonly IVCRConfiguration _configuration;

    private readonly ILogger _logger;

    /// <summary>
    /// Erzeugt eine neue Verwaltungsinstanz und lädt die aktuellen Auftragsliste.
    /// </summary>
    /// <param name="rootDirectory">Meldet das Verzeichnis, unterhalb dessen alle
    /// Aufträge und Protokolle angelegt werden.</param>
    public JobManager(IVCRProfiles profiles, IVCRConfiguration configuration, ILogger logger)
    {
        // Remember
        _configuration = configuration;
        _logger = logger;

        Profiles = profiles;

        // Create root directory
        var rootDirectory = RunTimeLoader.GetDirectory("Recording");

        // Report
        Tools.ExtendedLogging("Using Root Directory {0}", rootDirectory.FullName);

        RootDirectory = new DirectoryInfo(Path.Combine(rootDirectory.FullName, "Jobs"));
        RootDirectory.Create();

        // Create working directories
        CollectorDirectory.Create();
        ArchiveDirectory.Create();
        JobDirectory.Create();
        LogDirectory.Create();

        // Load all jobs
        foreach (var job in VCRJob.Load(JobDirectory))
            if (job.UniqueID.HasValue)
                m_Jobs[job.UniqueID.Value] = job;
    }

    /// <inheritdoc/>
    public List<VCRJob> GetActiveJobs()
    {
        // Filter out all jobs for the indicated profile
        lock (m_Jobs)
        {
            // All to delete
            m_Jobs.Values.Where(job => job.UniqueID.HasValue && !job.IsActive).ToList().ForEach(Delete);

            // Report the rest
            return m_Jobs.Values.ToList();
        }
    }

    /// <inheritdoc/>
    public void Delete(VCRJob job)
    {
        // Check unique identifier
        if (!job.UniqueID.HasValue)
            throw new InvalidJobDataException("Die eindeutige Kennung ist ungültig");

        // Report
        Tools.ExtendedLogging("Deleting Job {0}", job.UniqueID);

        // Must synchronize
        lock (m_Jobs)
        {
            // Load from the map
            var internalJob = this[job.UniqueID.Value];

            // See if this is active
            if (internalJob != null)
            {
                // Delete it
                internalJob.Delete(JobDirectory);

                // Remove from map
                m_Jobs.Remove(internalJob.UniqueID!.Value);

                // Save to file
                internalJob.Save(ArchiveDirectory);
            }
            else
            {
                // Report
                Tools.ExtendedLogging("Job not found in Active Directory - trying Archive");

                // Must be archived               
                job.Delete(ArchiveDirectory);
            }
        }
    }

    /// <inheritdoc/>
    public VCRJob? this[Guid jobIdentifier]
    {
        get
        {
            // Cut off
            lock (m_Jobs)
                if (m_Jobs.TryGetValue(jobIdentifier, out var result))
                    return result;

            // Report
            return null;
        }
    }

    /// <inheritdoc/>
    public void Update(VCRJob job, Guid? scheduleIdentifier)
    {
        ArgumentNullException.ThrowIfNull(job, nameof(job));

        // Cleanup
        if (scheduleIdentifier.HasValue)
            foreach (var schedule in job.Schedules)
                if (schedule.UniqueID.HasValue)
                    if (schedule.UniqueID.Value.Equals(scheduleIdentifier.Value))
                        schedule.NoStartBefore = null;

        // Report
        Tools.ExtendedLogging("Updating Job {0}", job.UniqueID!);

        // Load default profile name
        job.SetProfile();

        // Validate
        job.Validate(scheduleIdentifier);

        // Cleanup schedules
        job.CleanupExceptions();

        // Remove from archive - if job has been recovered
        job.Delete(ArchiveDirectory);

        // Try to store to disk - actually this is inside the lock because the directory virtually is part of our map
        lock (m_Jobs)
            if (job.Save(JobDirectory).GetValueOrDefault())
                m_Jobs[job.UniqueID!.Value] = job;
            else
                throw new ArgumentException(string.Format("Die Datei zum Auftrag {0} kann nicht geschrieben werden", job.UniqueID), nameof(job));
    }

    /// <summary>
    /// Ermittelt einen Auftrag nach seiner eindeutigen Kennung.
    /// </summary>
    /// <param name="jobIdentifier">Die Kennung des Auftrags.</param>
    /// <returns>Der gewünschte Auftrag oder <i>null</i>, wenn kein derartiger
    /// Auftrag existiert.</returns>
    private VCRJob? FindJob(Guid jobIdentifier)
    {
        // The result
        VCRJob? result = null;

        // Synchronize
        lock (m_Jobs)
        {
            // Try map
            if (m_Jobs.TryGetValue(jobIdentifier, out result))
                return result;

            // Create file name
            var jobFile = new FileInfo(Path.Combine(ArchiveDirectory.FullName, jobIdentifier.ToString("N").ToUpper() + VCRJob.FileSuffix));
            if (!jobFile.Exists)
                return null;

            // Load
            result = SerializationTools.Load<VCRJob>(jobFile);
            if (result == null)
                return null;
        }

        // Check identifier and finalize settings - for pre-3.0 files
        if (!result.UniqueID.HasValue)
            return null;
        if (!jobIdentifier.Equals(result.UniqueID.Value))
            return null;

        // Finish
        result.SetProfile();

        // Found in archive
        return result;
    }

    /// <inheritdoc/>
    public void SetRestartThreshold(VCRRecordingInfo recording)
    {
        // Forward
        if (recording != null)
            SetRestartThreshold(recording, recording.EndsAt);
    }

    /// <summary>
    /// Legt nach einer abgeschlossenen Aufzeichnung fest, wann frühestens eine Wiederholung
    /// stattfinden darf.
    /// </summary>
    /// <param name="recording">Alle Informationen zur ausgeführten Aufzeichnung.</param>
    /// <param name="endsAt">Der Endzeitpunkt der Aufzeichnung.</param>
    private void SetRestartThreshold(VCRRecordingInfo recording, DateTime endsAt)
    {
        // Forward
        if (recording != null)
            if (recording.JobUniqueID.HasValue)
                if (recording.ScheduleUniqueID.HasValue)
                    SetRestartThreshold(recording.JobUniqueID.Value, recording.ScheduleUniqueID.Value, endsAt);
    }

    /// <summary>
    /// Legt nach einer abgeschlossenen Aufzeichnung fest, wann frühestens eine Wiederholung
    /// stattfinden darf.
    /// </summary>
    /// <param name="jobIdentifier">Die eindeutige Kennung des Auftrags.</param>
    /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung im Auftrag.</param>
    /// <param name="endsAt">Der Endzeitpunkt der Aufzeichnung.</param>
    private void SetRestartThreshold(Guid jobIdentifier, Guid scheduleIdentifier, DateTime endsAt)
    {
        // Report
        Tools.ExtendedLogging("Setting Restart Threshold of {0}/{1} to {2}", jobIdentifier, scheduleIdentifier, endsAt);

        // Synchronize
        lock (m_Jobs)
        {
            // Locate job
            var job = this[jobIdentifier];
            if (job == null)
                return;

            // Locate schedule
            var schedule = job[scheduleIdentifier];
            if (schedule == null)
                return;

            // Make sure that this schedule is not used again
            schedule.DoNotRestartBefore = endsAt;

            // Save to file
            Update(job, null);
        }
    }

    /// <summary>
    /// Ermittelt das Archivverzeichnis vom VCR.NET.
    /// </summary>
    private DirectoryInfo ArchiveDirectory => new(Path.Combine(RootDirectory.FullName, "Archive"));

    /// <inheritdoc/>
    public VCRJob[] ArchivedJobs
    {
        get
        {
            // For legacy updates
            var profile = Profiles.DefaultProfile;

            // Process
            lock (m_Jobs)
                return
                    ArchiveDirectory
                    .GetFiles("*" + VCRJob.FileSuffix)
                    .Select(file =>
                        {
                            // Load
                            var job = SerializationTools.Load<VCRJob>(file);

                            // Enrich legacy entries
                            if (job != null)
                                if (profile != null)
                                    job.SetProfile(profile.Name);

                            // Report
                            return job!;
                        })
                    .Where(job => job != null)
                    .ToArray();
        }
    }

    /// <summary>
    /// Der Zeitpunkt, an dem das nächste Mal das Archiv bereinigt werden soll.
    /// </summary>
    private DateTime m_nextArchiveCleanup = DateTime.MinValue;

    /// <inheritdoc/>
    public void CleanupArchivedJobs()
    {
        // Not yet
        if (DateTime.UtcNow < m_nextArchiveCleanup)
            return;

        // Remember
        m_nextArchiveCleanup = DateTime.UtcNow.AddDays(1);

        // Access limit
        var firstValid = DateTime.UtcNow.AddDays(-7 * _configuration.ArchiveLifeTime);
        var jobDirectory = ArchiveDirectory;

        // Protect to avoid parallel operations on the archive directory
        lock (m_Jobs)
            foreach (var file in jobDirectory.GetFiles("*" + VCRJob.FileSuffix))
                if (file.LastWriteTimeUtc < firstValid)
                    try
                    {
                        // Delete the log entry
                        file.Delete();
                    }
                    catch (Exception e)
                    {
                        // Report error
                        _logger.Log(e);
                    }
    }

    /// <inheritdoc/>
    public void CreateLogEntry(VCRRecordingInfo logEntry)
    {
        // Store
        if (logEntry.Source != null)
            if (!string.IsNullOrEmpty(logEntry.Source.ProfileName))
                SerializationTools.SafeSave(
                    logEntry,
                    Path.Combine(LogDirectory.FullName, DateTime.UtcNow.ToString(LogEntryDateFormat + LogEntryTimeFormat) + logEntry.Source.ProfileName + VCRRecordingInfo.FileSuffix),
                    _logger
                );
    }

    /// <inheritdoc/>
    public List<VCRRecordingInfo> FindLogEntries(DateTime firstDate, DateTime lastDate, IProfileState profile)
    {
        // Create list
        var logs = new List<VCRRecordingInfo>();

        // Create search patterns
        var last = lastDate.AddDays(1).ToString(LogEntryDateFormat);
        var first = firstDate.ToString(LogEntryDateFormat);

        // Load all jobs
        foreach (var file in LogDirectory.GetFiles("*" + VCRRecordingInfo.FileSuffix))
        {
            // Skip
            if (file.Name.CompareTo(first) < 0)
                continue;
            if (file.Name.CompareTo(last) >= 0)
                continue;

            // Load item
            var logEntry = SerializationTools.Load<VCRRecordingInfo>(file);
            if (logEntry == null)
                continue;

            // Check
            if (profile != null)
                if (!profile.IsResponsibleFor(logEntry.Source))
                    continue;

            // Attach the name
            logEntry.LogIdentifier = file.Name.ToLower();

            // Remember
            logs.Add(logEntry);
        }

        // Sort by start time
        logs.Sort(VCRRecordingInfo.ComparerByStarted);

        // Report
        return logs;
    }

    /// <summary>
    /// Der Zeitpunkt, an dem die nächste Bereinigung stattfinden soll.
    /// </summary>
    private DateTime m_nextLogCleanup = DateTime.MinValue;

    /// <inheritdoc/>
    public void CleanupLogEntries()
    {
        // Check time
        if (DateTime.UtcNow < m_nextLogCleanup)
            return;

        // Not again for now
        m_nextLogCleanup = DateTime.UtcNow.AddDays(1);

        // For cleanup
        var firstValid = DateTime.Now.Date.AddDays(-7 * _configuration.LogLifeTime).ToString(LogEntryDateFormat);

        // Load all jobs
        foreach (var file in LogDirectory.GetFiles("*" + VCRRecordingInfo.FileSuffix))
            if (file.Name.CompareTo(firstValid) < 0)
                try
                {
                    // Delete the log entry
                    file.Delete();
                }
                catch (Exception e)
                {
                    // Report error
                    _logger.Log(e);
                }
    }

    /// <inheritdoc/>
    public VCRSchedule? ParseUniqueWebId(string id, out VCRJob job)
    {
        ServerTools.ParseUniqueWebId(id, out Guid jobID, out Guid scheduleID);

        // Find the job
        job = FindJob(jobID)!;

        // Report schedule if job exists
        return job?[scheduleID];
    }
}

