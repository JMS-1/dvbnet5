﻿using System.Net;
using System.Net.Mail;
using JMS.DVB.CardServer;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.ProgramGuide;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Logging;
using JMS.DVB.NET.Recording.Services.Planning;
using JMS.DVB.NET.Recording.Status;

namespace JMS.DVB.NET.Recording.Requests;

/// <summary>
/// Beschreibt einen Aufzeichnungsauftrag, der sich aus mehreren Einzelaufzeichnungen
/// auch auf mehreren Quellen zusammensetzen kann.
/// </summary>
/// <param name="state">Der Zustands des zugehörigen Geräteprofils.</param>
/// <param name="firstRecording">Die erste Aufzeichnung, auf Grund derer dieser Zugriff angelegt wurde.</param>
public class RecordingProxy(
    IProfileState state,
    VCRRecordingInfo firstRecording,
    ILogger<RecordingProxy> logger,
    IJobManager jobManager,
    IVCRConfiguration configuration,
    IVCRProfiles profiles,
    IExtensionManager extensionManager
) : CardServerProxy(state, logger, jobManager, configuration, profiles, extensionManager, firstRecording)
{
    #region Felder zur Steuerung der asynchronen Aufrufe an den Aufzeichnungsprozess

    /// <summary>
    /// Gesetzt, solange auf das Auswählen der Quellgruppe gewartet wird.
    /// </summary>
    private IAsyncResult m_groupPending = null!;

    /// <summary>
    /// Gesetzt, solange auf die Aktivierung einer neuen Quelle gewartet wird. Es wird immer nur
    /// eine Quelle zu jedem Zeitpunkt aktiviert.
    /// </summary>
    private IAsyncResult<StreamInformation[]> m_startPending = null!;

    /// <summary>
    /// Gesetzt, solange auf die Deaktivierung einer Quelle gewartet wird.
    /// </summary>
    private IAsyncResult m_stopPending = null!;

    #endregion

    #region Verwaltung der Aufzeichnungen und den zugehörigen Aufzeichnungsdateien

    /// <summary>
    /// Alle gerade aktive Aufzeichnungen.
    /// </summary>
    private readonly List<VCRRecordingInfo> m_recordings = [];

    /// <summary>
    /// Alle bereis abgeschlossenen Aufzeichnungen.
    /// </summary>
    private readonly List<VCRRecordingInfo> m_done = [];

    /// <summary>
    /// Alle bisher angemeldeten Aufzeichnungen - die vollständige Liste wird ausschließlich
    /// für Protokolleinträge verwendet.
    /// </summary>
    private readonly List<VCRRecordingInfo> m_allRecordings = [];

    /// <summary>
    /// Alle Dateien, die jemals erzeugt wurden.
    /// </summary>
    private readonly HashSet<FileInformation> m_files = [];

    #endregion

    #region Implementierung der abstrakten Basisklasse

    /// <summary>
    /// Die Art dieser Aufzeichnung.
    /// </summary>
    protected override string TypeName => "Regular Recording";

    /// <summary>
    /// Prüft, ob asynchrone Aufrufe ausstehen. Es werden niemals neue Anfragen
    /// an den Aufzeichnungsprozeß gesendet, wenn noch eine Anfrage aussteht.
    /// </summary>
    protected override bool HasPendingServerRequest
    {
        get
        {
            // Just test all our flags
            if (!WaitForEnd(ref m_groupPending, "Source Group is now activated for {0}", ProfileName))
                return true;
            if (!WaitForEnd(ref m_startPending, "New Recording started on {0}", ProfileName))
                return true;
            if (!WaitForEnd(ref m_stopPending, "Recording ended on {0}", ProfileName))
                return true;

            // We are idle
            return false;
        }
    }

    /// <summary>
    /// Wird einmalig nach dem Starten des Aufzeichnungsprozesses aufgerufen. Dieser
    /// ist zu diesem Zeitpunkt im Leerlauf.
    /// </summary>
    protected override void OnStart() => m_groupPending = CardServer.BeginSelect(Representative.Source.SelectionKey);

    /// <summary>
    /// Bearbeitet neue Daten vom Aufzeichnungsprozess. Dieser Aufruf erfolgt immer,
    /// wenn neue Informationen vorliegen und zusätzlich einmal unmittelbar nach
    /// dem Beenden durch <see cref="OnStop"/>.
    /// </summary>
    /// <param name="state">Die neuen Daten.</param>
    protected override void OnNewStateAvailable(ServerInformation state)
    {
        // Scan results
        var newFiles = new List<FileInformation>();
        var totalSize = default(long);

        // Process all files
        lock (m_recordings)
        {
            // Collect files
            foreach (var stream in state.Streams ?? Enumerable.Empty<StreamInformation>())
                foreach (var file in stream.AllFiles ?? Enumerable.Empty<FileStreamInformation>())
                {
                    // If we do not know this file remember for extension processing
                    var info = FileInformation.Create(file, stream.UniqueIdentifier);
                    if (m_files.Add(info))
                        newFiles.Add(info);
                }

            // Count up the total sum
            if (m_files.Count > 0)
                totalSize = m_files.Sum(file => file.FileSize.GetValueOrDefault(0));
        }

        // Total file size
        Representative.TotalSize = (uint)(totalSize / 1024);

        // No new files
        if (newFiles.Count < 1)
            return;

        // Report
        Tools.ExtendedLogging("Found {0} new Recording File(s) on {1}", newFiles.Count, ProfileName);

        // Time to clone the environment
        var environment = new Dictionary<string, string>(ExtensionEnvironment);

        // Fill environment
        AddFilesToEnvironment(environment, "Planned", newFiles);

        // Fire up extensions
        FireRecordingStartedExtensions(environment);
    }

    /// <summary>
    /// Ergänzt eine weitere Aufzeichnung. Ein Aufruf dieser Methode erfolgt nur, wenn
    /// keine Antworten vom Aufzeichnunsprozeß ausstehen.
    /// </summary>
    /// <param name="recording">Die Detaildaten der Aufzeichnung.</param>
    protected override void OnStartRecording(VCRRecordingInfo recording)
    {
        // Be safe
        lock (m_recordings)
        {
            // Always stamp time to recording
            recording.PhysicalStart = DateTime.UtcNow;

            // Remember for further processing
            m_allRecordings.Add(recording);
            m_recordings.Add(recording);

            // Make sure that all paths are unique
            EnforceUniqueFileNames();

            // Update end time
            CalculateNewEndOfRecording();
        }

        // Enqueue
        m_startPending = CardServer.BeginAddSources([recording.ToReceiveInformation(Configuration, Profiles)]);
    }

    /// <summary>
    /// Beendet eine Aufzeichnung. Diese Methode wird nur aufgerufen wenn keine Antwort
    /// des Aufzeichnungsprozesses aussteht.
    /// </summary>
    /// <param name="scheduleIdentifier">Die zu beendende Aufzeichung.</param>
    protected override void OnEndRecording(Guid scheduleIdentifier)
    {
        // Cleanup from list
        lock (m_recordings)
            for (int i = m_recordings.Count; i-- > 0;)
            {
                // Load recording
                var recording = m_recordings[i];
                if (!scheduleIdentifier.Equals(recording.ScheduleUniqueID!.Value))
                    continue;

                // Remember
                m_done.Add(recording);

                // Remove
                m_recordings.RemoveAt(i);

                // Enqueue
                m_stopPending = CardServer.BeginRemoveSource(recording.Source.Source, scheduleIdentifier);

                // Update guide extraction
                RecordingPostProcessing(recording);

                // Update end time
                CalculateNewEndOfRecording();

                // Send E-Mail if enabled
                SendEMail(recording);

                // Done
                break;
            }
    }

    /// <summary>
    /// Beendet die Nutzung des Aufzeichnungsprozesses endgültig. Alle ausstehenden Anfragen
    /// an diesen müssen nun beendet werden.
    /// </summary>
    protected override void OnStop()
    {
        // Terminate and wait
        WaitForEnd(ref m_groupPending);
        WaitForEnd(ref m_startPending);
        WaitForEnd(ref m_stopPending);

        // Detach sources
        var request = CardServer.BeginRemoveAllSources();

        // Use wait method to end request - will never throw an exception
        WaitForEnd(ref request);

        // Process all guide extracts
        lock (m_recordings)
            RecordingPostProcessing([.. m_recordings]);
    }

    /// <summary>
    /// Ergänzt die Beschreibung der aktuellen Aufzeichnungen.
    /// </summary>
    /// <param name="info">Die Beschreibung der Gesamtaufgabe des aktuellen Aufzeichnungsprozesses.</param>
    /// <param name="finalCall">Gesetzt, wenn es sich um den abschließenden Aufruf handelt. Dieser
    /// wird ausschließlich für die Erstellung des Protokolleintrags verwendet.</param>
    /// <param name="state">Der aktuelle Zustand.</param>
    protected override void OnFillInformation(FullInfo info, bool finalCall, ServerInformation state)
    {
        // Set static data
        info.IsDynamic = true;
        info.CanStream = true;

        // Synchronize access
        lock (m_recordings)
        {
            // Load all files we've seen so far
            info.Recording.RecordingFiles.Clear();
            info.Recording.RecordingFiles.AddRange(m_files);

            info.Finished.AddRange(m_done);

            // Create the file mapping
            var files =
                m_files
                    .Where(file => !string.IsNullOrEmpty(file.ScheduleIdentifier))
                    .GroupBy(file => new Guid(file.ScheduleIdentifier), file => file.Path)
                    .ToDictionary(group => group.Key, group => group.ToArray());

            // Process all recordings
            info
                .Streams
                .AddRange((finalCall ? m_allRecordings : m_recordings)
                    .Select(recording =>
                        {
                            // Collect the data
                            var streamInfo = state?.Streams.Find(recording.Match);
                            var target = streamInfo?.StreamTarget;

                            // Create record
                            return StreamInfo.Create(recording, target!, files);
                        }));
        }
    }

    /// <summary>
    /// Markiert eine Aufzeichnung so, dass sie für weitere Planungen nicht mehr vor 
    /// einem bestimmten Zeitpunkt berücksichtigt wird.
    /// </summary>
    /// <param name="scheduleIdentifier">Die eindeutige Identifikation des betroffenen Datenstroms.</param>
    public override void SetRestartThreshold(Guid? scheduleIdentifier)
    {
        // Attach to job manager
        var jobs = JobManager;

        // All active recordings
        lock (m_recordings)
            m_recordings
                .ForEach(recording =>
                    {
                        // Check filter and forward
                        if (recording.MatchesScheduleFilter(scheduleIdentifier))
                            jobs.SetRestartThreshold(recording);
                    });
    }

    /// <summary>
    /// Meldet die anzahl der gerade aktiven Aufzeichnungen.
    /// </summary>
    public override int NumberOfActiveRecordings
    {
        get
        {
            // In case there are no recordings (during startup or shutdown) make sure we block out caller
            lock (m_recordings)
                return Math.Max(1, m_recordings.Count);
        }
    }

    /// <summary>
    /// Verändert den Endzeitpunkt einer Aufzeichnung.
    /// </summary>
    /// <param name="streamIdentifier">Die eindeutige Kennung des betroffenen Datenstroms.</param>
    /// <param name="newEndTime">Der neue Endzeitpunkt.</param>
    public override void ChangeEndTime(Guid streamIdentifier, DateTime newEndTime)
    {
        // Report
        Tools.ExtendedLogging("Setting End for {0} to {1}", ProfileName, newEndTime);

        // Protected update
        lock (m_recordings)
        {
            // Update all recordings and use the same loop to find the overall end time
            var recording = FindRecording(streamIdentifier);
            if (recording == null)
                return;

            // Send to planner
            if (!ProfileState.Collection.ChangeEndTime(streamIdentifier, newEndTime))
                return;

            // Update recording
            recording.EndsAt = newEndTime;

            // Update end time
            CalculateNewEndOfRecording();
        }
    }

    #endregion

    #region Spezielle Methoden einer echten Aufzeichnung

    /// <summary>
    /// Aktiviert den Netzwerkversand für eine aktive Quelle.
    /// </summary>
    /// <param name="source">Die betroffene Quelle.</param>
    /// <param name="uniqueIdentifier">Die eindeutige Kennung der Teilaufzeichnung.</param>
    /// <param name="streamingTarget">Das neue Ziel für den Netzwerkversand.</param>
    public void SetStreamTarget(SourceIdentifier source, Guid uniqueIdentifier, string streamingTarget)
    {
        // Validate
        ArgumentNullException.ThrowIfNull(source);

        // Report
        Tools.ExtendedLogging("Changing Streaming for {0} [{1}] to {2}", source, uniqueIdentifier, streamingTarget);

        // Process
        EnqueueActionAndWait(() => ServerImplementation.EndRequest(CardServer.BeginSetStreamTarget(source, uniqueIdentifier, streamingTarget)));
    }

    #endregion

    #region Interne Hilfsmethoden

    /// <summary>
    /// Stellt sicher, dass alle Dateinamen eindeutig sind. Überlappungen können bei einer unglücklichen Wahl
    /// der Dateimuster auftreten.
    /// </summary>
    private void EnforceUniqueFileNames()
    {
        // Name groupings
        var names = new Dictionary<string, List<VCRRecordingInfo>>(StringComparer.InvariantCultureIgnoreCase);
        var orderedNames = new List<List<VCRRecordingInfo>>();

        // Collect by names strictly keeping the order of recordings - this will avoid renaming recordings which are already writing to the files
        foreach (var recording in m_recordings)
        {
            // Load list
            if (!names.TryGetValue(recording.FileName, out var recordings))
            {
                // Create new
                recordings = [];

                // To map and ordered list
                names.Add(recording.FileName, recordings);
                orderedNames.Add(recordings);
            }

            // Remember
            recordings.Add(recording);
        }

        // Make names unique
        foreach (var recordings in orderedNames)
        {
            // We are fine
            if (recordings.Count < 2)
                continue;

            // Get the name of interest and create the start index
            var file = new FileInfo(recordings[0].FileName);
            var name = Path.GetFileNameWithoutExtension(file.Name);
            var dir = file.DirectoryName!;
            var ext = file.Extension;
            var nextIndex = 1;

            // Try to make all names unique - considering any other names
            foreach (var recording in recordings.Skip(1))
                do
                {
                    // Create the new name
                    recording.FileName = Path.Combine(dir, $"{name} ({nextIndex++}){ext}");
                }
                while (names.ContainsKey(recording.FileName));
        }
    }

    /// <summary>
    /// Erstellt für alle noch nicht berücksichtigten Aufzeichnungen Auszüge aus der Programmzeitschrift.
    /// </summary>
    /// <param name="recordings">Die Liste der Aufzeichnungen, die bearbeitet werden sollen.</param>
    private void RecordingPostProcessing(params VCRRecordingInfo[] recordings)
    {
        // Process all
        foreach (var recording in recordings)
        {
            // Report
            Tools.ExtendedLogging("Extracting EPGINFO for {0}", recording.Name);

            // Load bounds
            var from = recording.PhysicalStart!.Value;
            var to = DateTime.UtcNow;

            // Create guide
            var entries = new ProgramGuideEntries();

            // Fill it
            entries.AddRange(
                ProfileState
                    .ProgramGuide
                    .GetEntries(recording.Source.Source)
                    .TakeWhile(e => e.StartTime < to)
                    .Where(e => e.EndTime > from));

            // Write it out
            SerializationTools.SafeSave(entries, Path.ChangeExtension(recording.FileName, "epginfo"), Logger);
        }

        // Detect all files related to the recordings
        var scheduleIdentifiers = new HashSet<Guid>(recordings.Select(recording => recording.ScheduleUniqueID!.Value));
        var files = m_files.Where(file => scheduleIdentifiers.Contains(new Guid(file.ScheduleIdentifier)));

        // Clone current environment
        var environment = new Dictionary<string, string>(ExtensionEnvironment);

        // Fill
        AddFilesToEnvironment(environment, "Recorded", files);

        // Process extensions
        FireRecordingFinishedExtensions(environment);
    }

    /// <summary>
    /// Sendet eine Benachrichtiung über den Abschluss der Aufzeichnung.
    /// </summary>
    /// <param name="recording">Die gerade beendete Aufzeichnung.</param>
    private void SendEMail(VCRRecordingInfo recording)
    {
        // See if sending an E-Mail is enabled
        if (string.IsNullOrEmpty(Configuration.SmtpRelay) || string.IsNullOrEmpty(Configuration.SmtpRecipient)) return;

        try
        {
            // Configure client
            using var smtp = new SmtpClient(Configuration.SmtpRelay);

            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(Configuration.SmtpUsername, Configuration.SmtpPassword);

            // Send the E-Mail
            smtp.Send(new()
            {
                Body = $"Aufzeichnung von {recording.PhysicalStart?.ToLocalTime():dd.MM.yyyy HH:mm:ss} abgeschlossen",
                From = new(Configuration.SmtpUsername, "VCR.NET Recording Service"),
                Subject = recording.Name,
                To = { Configuration.SmtpRecipient }
            });
        }
        catch (Exception e)
        {
            // Just report
            Logger.LogError("Es konnte keine E-Mail versendet werden: {0}", e.Message);
        }
    }

    /// <summary>
    /// Ermittelt eine bestimmte Aufzeichnung.
    /// </summary>
    /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
    /// <returns>Die Aufzeichnung, sofern bekannt.</returns>
    private VCRRecordingInfo? FindRecording(Guid scheduleIdentifier) => m_recordings.FirstOrDefault(recording => scheduleIdentifier.Equals(recording.ScheduleUniqueID!.Value));

    /// <summary>
    /// Berechnet das aktuelle Ende der Aufzeichnung.
    /// </summary>
    private void CalculateNewEndOfRecording()
    {
        // Update if there is at least one recording
        if (m_recordings.Count > 0)
            Representative.EndsAt = new DateTime(m_recordings.Select(recording => recording.EndsAt.Ticks).Max());
    }

    #endregion
}

