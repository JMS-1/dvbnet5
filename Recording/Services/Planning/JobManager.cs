using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.RestWebApi;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Logging;

namespace JMS.DVB.NET.Recording.Services.Planning;

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
    /// Vorhaltung aller Aufträge.
    /// </summary>
    private readonly Dictionary<Guid, VCRJob> m_Jobs = [];

    private readonly IVCRConfiguration _configuration;

    private readonly ILogger<JobManager> _logger;

    private readonly IVCRProfiles _profiles;

    /// <summary>
    /// Erzeugt eine neue Verwaltungsinstanz und lädt die aktuellen Auftragsliste.
    /// </summary>
    /// <param name="rootDirectory">Meldet das Verzeichnis, unterhalb dessen alle
    /// Aufträge und Protokolle angelegt werden.</param>
    public JobManager(IVCRProfiles profiles, IVCRConfiguration configuration, ILogger<JobManager> logger)
    {
        // Remember
        _configuration = configuration;
        _logger = logger;
        _profiles = profiles;

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
            return [.. m_Jobs.Values];
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
                DeleteJob(internalJob, JobDirectory);

                // Remove from map
                m_Jobs.Remove(internalJob.UniqueID!.Value);

                // Save to file
                SaveJob(internalJob, ArchiveDirectory);
            }
            else
            {
                // Report
                Tools.ExtendedLogging("Job not found in Active Directory - trying Archive");

                // Must be archived               
                DeleteJob(job, ArchiveDirectory);
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
        SetJobProfile(job);

        // Validate
        ValidateJob(job, scheduleIdentifier);

        // Cleanup schedules
        job.CleanupExceptions();

        // Remove from archive - if job has been recovered
        DeleteJob(job, ArchiveDirectory);

        // Try to store to disk - actually this is inside the lock because the directory virtually is part of our map
        lock (m_Jobs)
            if (SaveJob(job, JobDirectory).GetValueOrDefault())
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
        SetJobProfile(result);

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
            var profile = _profiles.DefaultProfile;

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

    /// <summary>
    /// Ermittelt den Namen dieses Auftrags in einem Zielverzeichnis.
    /// </summary>
    /// <param name="target">Der Pfad zu einem Zielverzeichnis.</param>
    /// <returns>Die zugehörige Datei.</returns>
    private FileInfo? GetJobFileName(VCRJob job, DirectoryInfo target)
        => job.UniqueID.HasValue
            ? new FileInfo(Path.Combine(target.FullName, job.UniqueID.Value.ToString("N").ToUpper() + VCRJob.FileSuffix))
            : null;

    /// <summary>
    /// Speichert diesen Auftrag ab.
    /// </summary>
    /// <param name="target">Der Pfad zu einem Zielverzeichnis.</param>
    /// <returns>Gesetzt, wenn der Speichervorgang erfolgreich war. <i>null</i> wird
    /// gemeldet, wenn diesem Auftrag keine Datei zugeordnet ist.</returns>
    private bool? SaveJob(VCRJob job, DirectoryInfo target)
    {
        // Get the file
        var file = GetJobFileName(job, target);
        if (file == null)
            return null;

        // Be safe
        try
        {
            // Process
            SerializationTools.Save(job, file);
        }
        catch (Exception e)
        {
            // Report
            _logger.Log(e);

            // Done
            return false;
        }

        // Done
        return true;
    }

    /// <summary>
    /// Löschte diesen Auftrag.
    /// </summary>
    /// <param name="target">Der Pfad zu einem Zielverzeichnis.</param>
    /// <returns>Gesetzt, wenn der Löschvorgang erfolgreich war. <i>null</i> wird gemeldet,
    /// wenn die Datei nicht existierte.</returns>
    private bool? DeleteJob(VCRJob job, DirectoryInfo target)
    {
        // Get the file
        var file = GetJobFileName(job, target);
        if (file == null)
            return null;
        if (!file.Exists)
            return null;

        // Be safe
        try
        {
            // Process
            file.Delete();
        }
        catch (Exception e)
        {
            // Report error
            _logger.Log(e);

            // Failed
            return false;
        }

        // Did it
        return true;
    }

    /// <summary>
    /// Stellt sicher, dass für diesen Auftrag ein Geräteprprofil ausgewählt ist.
    /// </summary>
    private void SetJobProfile(VCRJob job)
    {
        // No need
        if (!string.IsNullOrEmpty(job.Source?.ProfileName))
            return;

        // Attach to the default profile
        var defaultProfile = _profiles.DefaultProfile;
        if (defaultProfile == null)
            return;

        // Process
        if (job.Source == null)
            job.Source = new SourceSelection { ProfileName = defaultProfile.Name };
        else
            job.Source.ProfileName = defaultProfile.Name;
    }

    /// <summary>
    /// Prüft, ob ein Auftrag zulässig ist.
    /// </summary>
    /// <param name="scheduleIdentifier">Die eindeutige Kennung der veränderten Aufzeichnung.</param>
    /// <exception cref="InvalidJobDataException">Die Konfiguration dieses Auftrags is ungültig.</exception>
    private void ValidateJob(VCRJob job, Guid? scheduleIdentifier)
    {
        // Identifier
        if (!job.UniqueID.HasValue)
            throw new InvalidJobDataException("Die eindeutige Kennung ist ungültig");

        // Name
        if (!job.Name.IsValidName())
            throw new InvalidJobDataException("Der Name enthält ungültige Zeichen");

        // Test the station
        if (job.HasSource)
        {
            // Source
            if (!ValidateSource(job.Source))
                throw new InvalidJobDataException("Eine Quelle ist ungültig");

            // Streams
            if (!job.Streams.Validate())
                throw new InvalidJobDataException("Die Aufzeichnungsoptionen sind ungültig");
        }
        else if (job.Streams != null)
            throw new InvalidJobDataException("Die Aufzeichnungsoptionen sind ungültig");

        // List of schedules
        if (job.Schedules.Count < 1)
            throw new InvalidJobDataException("Keine Aufzeichnungen vorhanden");

        // Only validate the schedule we updated
        if (scheduleIdentifier.HasValue)
            foreach (var schedule in job.Schedules)
                if (!schedule.UniqueID.HasValue || schedule.UniqueID.Value.Equals(scheduleIdentifier))
                    ValidateSchedule(schedule, job);
    }

    /// <summary>
    /// Prüft, ob eine Quelle gültig ist.
    /// </summary>
    /// <param name="source">Die Auswahl der Quelle oder <i>null</i>.</param>
    /// <returns>Gesetzt, wenn die Auswahl gültig ist.</returns>
    private bool ValidateSource(SourceSelection source) => _profiles.FindSource(source) != null;

    /// <summary>
    /// Prüft, ob die Daten zur Aufzeichnung zulässig sind.
    /// </summary>
    /// <param name="job">Der zugehörige Auftrag.</param>
    /// <exception cref="InvalidJobDataException">Es wurde keine eindeutige Kennung angegeben.</exception>
    /// <exception cref="InvalidJobDataException">Die Daten der Aufzeichnung sind fehlerhaft.</exception>
    private void ValidateSchedule(VCRSchedule schedule, VCRJob job)
    {
        // Identifier
        if (!schedule.UniqueID.HasValue)
            throw new InvalidJobDataException("Die eindeutige Kennung ist ungültig");

        // Check for termination date
        if (schedule.LastDay.HasValue)
        {
            // Must be a date
            if (schedule.LastDay.Value != schedule.LastDay.Value.Date)
                throw new InvalidJobDataException("Das Enddatum darf keine Uhrzeit enthalten");
            if (schedule.FirstStart.Date > schedule.LastDay.Value.Date)
                throw new InvalidJobDataException("Der Endzeitpunkt darf nicht vor dem Startzeitpunkt liegen");
        }

        // Duration
        if ((schedule.Duration < 1) || (schedule.Duration > 9999))
            throw new InvalidJobDataException("Ungültige Dauer");

        // Repetition
        if (schedule.Days.HasValue)
            if (0 != (~0x7f & (int)schedule.Days.Value))
                throw new InvalidJobDataException("Die Aufzeichnungstage sind ungültig");

        // Test the station
        if (schedule.Source != null)
        {
            // Match profile
            if (job != null)
                if (job.Source != null)
                    if (!string.Equals(job.Source.ProfileName, schedule.Source.ProfileName, StringComparison.InvariantCultureIgnoreCase))
                        throw new InvalidJobDataException("Die Aufzeichnungstage sind ungültig");

            // Source
            if (!ValidateSource(schedule.Source))
                throw new InvalidJobDataException("Eine Quelle ist ungültig");

            // Streams
            if (!schedule.Streams.Validate())
                throw new InvalidJobDataException("Die Aufzeichnungsoptionen sind ungültig");
        }
        else if (schedule.Streams != null)
            throw new InvalidJobDataException("Die Aufzeichnungsoptionen sind ungültig");

        // Station
        if (!job!.HasSource)
            if (schedule.Source == null)
                throw new InvalidJobDataException("Wenn einem Auftrag keine Quelle zugeordnet ist, so müssen alle Aufzeichnungen eine solche festlegen");

        // Name
        if (!schedule.Name.IsValidName())
            throw new InvalidJobDataException("Der Name enthält ungültige Zeichen");
    }

    /// <inheritdoc/>
    public void AddToScheduler(
        VCRSchedule schedule,
        RecordingScheduler scheduler,
        VCRJob job,
        IScheduleResource[] devices,
        Func<SourceSelection, IVCRProfiles, SourceSelection?> findSource,
        Func<Guid, bool> disabled
    )
    {
        // Validate
        if (scheduler == null)
            throw new ArgumentNullException(nameof(scheduler));
        if (job == null)
            throw new ArgumentNullException(nameof(job));
        if (findSource == null)
            throw new ArgumentNullException(nameof(findSource));

        // Let VCR.NET choose a profile to do the work
        if (job.AutomaticResourceSelection)
            devices = null!;

        // Create the source selection
        var persistedSource = schedule.Source ?? job.Source;
        var selection = findSource(persistedSource, _profiles);

        // Station no longer available
        if (selection == null)
            if (persistedSource != null)
                selection =
                    new SourceSelection
                    {
                        DisplayName = persistedSource.DisplayName,
                        ProfileName = persistedSource.ProfileName,
                        Location = persistedSource.Location,
                        Group = persistedSource.Group,
                        Source =
                            new Station
                            {
                                TransportStream = persistedSource.Source?.TransportStream ?? 0,
                                Network = persistedSource.Source?.Network ?? 0,
                                Service = persistedSource.Source?.Service ?? 0,
                                Name = persistedSource.DisplayName,
                            },
                    };

        // See if we are allowed to process
        var identifier = schedule.UniqueID!.Value;
        if (disabled != null)
            if (disabled(identifier))
                return;

        // Load all
        var name = string.IsNullOrEmpty(schedule.Name) ? job.Name : $"{job.Name} ({schedule.Name})";
        var source = ProfileScheduleResource.CreateSource(selection!);
        var duration = TimeSpan.FromMinutes(schedule.Duration);
        var noStartBefore = schedule.NoStartBefore;
        var start = schedule.FirstStart;

        // Check repetition
        var repeat = schedule.CreateRepeatPattern();
        if (repeat == null)
        {
            // Only if not being recorded
            if (!noStartBefore.HasValue)
                scheduler.Add(RecordingDefinition.Create(schedule, name, identifier, devices, source, start, duration));
        }
        else
        {
            // See if we have to adjust the start day
            if (noStartBefore.HasValue)
            {
                // Attach to the limit - actually we shift it a bit further assuming that we did have no large exception towards the past and the duration is moderate
                var startAfter = noStartBefore.Value.AddHours(12);
                var startAfterDay = startAfter.ToLocalTime().Date;

                // Localize the start time
                var startTime = start.ToLocalTime().TimeOfDay;

                // First adjust
                start = (startAfterDay + startTime).ToUniversalTime();

                // One more day
                if (start < startAfter)
                    start = (startAfterDay.AddDays(1) + startTime).ToUniversalTime();
            }

            // Read the rest
            var exceptions = schedule.Exceptions.Select(e => e.ToPlanException(duration)).ToArray();
            var endDay = schedule.LastDay.GetValueOrDefault(VCRSchedule.MaxMovableDay);

            // A bit more complex
            if (start.Date <= endDay.Date)
                scheduler.Add(RecordingDefinition.Create(schedule, name, identifier, devices, source, start, duration, endDay, repeat), exceptions);
        }
    }
}

