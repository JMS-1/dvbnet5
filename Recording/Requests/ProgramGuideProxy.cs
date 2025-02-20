﻿using JMS.DVB.CardServer;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.ProgramGuide;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Planning;
using JMS.DVB.NET.Recording.Status;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Logging;

namespace JMS.DVB.NET.Recording.Requests;

/// <summary>
/// Beschreibt die Ausführung der Aktualisierung der Programmzeitschrift.
/// </summary>
public class ProgramGuideProxy : CardServerProxy
{
    /// <summary>
    /// Beschreibt den Zugriff zum Starten der Sammlung der Programmzeitschrift.
    /// </summary>
    private IAsyncResult m_startPending = null!;

    /// <summary>
    /// Beschreibt, welche Erweiterungen der Programmzeitschrift auch ausgewertet werden sollen.
    /// </summary>
    private readonly EPGExtensions m_extensions;

    /// <summary>
    /// Alle Quellen, die bei der Aktualisierung zu berücksichtigen sind.
    /// </summary>
    private readonly HashSet<SourceIdentifier> m_selected = [];

    /// <summary>
    /// Erstellt eine neue Aktualisierung.
    /// </summary>
    /// <param name="state">Das zugehörige Geräteprofil.</param>
    /// <param name="recording">Daten der primären Aufzeichnung.</param>
    public ProgramGuideProxy(
        IProfileState state,
        VCRRecordingInfo recording,
        ILogger<ProgramGuideProxy> logger,
        IJobManager jobManager,
        IVCRConfiguration configuration,
        IVCRProfiles profiles,
        IExtensionManager extensionManager
    ) : base(state, logger, jobManager, configuration, profiles, extensionManager, recording)
    {
        // Reset fields
        if (Configuration.EnableFreeSat)
            m_extensions = EPGExtensions.FreeSatUK;
        else
            m_extensions = EPGExtensions.None;

        // All sources we know about
        var allSources = new Dictionary<string, SourceSelection>(StringComparer.InvariantCultureIgnoreCase);

        // Load all sources of this profile
        foreach (var source in Profiles.GetSources(ProfileName))
        {
            // Remember by direct name
            allSources[source.DisplayName] = source;

            // allSources by unique name
            allSources[source.QualifiedName] = source;
        }

        // Fill in all
        foreach (var legacyName in Configuration.ProgramGuideSources)
        {
            // Skip if empty
            if (string.IsNullOrEmpty(legacyName))
                continue;

            // Locate
            if (allSources.TryGetValue(legacyName, out var realSource))
                m_selected.Add(realSource.Source);
            else
                Logger.Log(LoggingLevel.Full, "Quelle '{0}' unbekannt: es wird keine Programmzeitschrift ermittelt", legacyName);
        }
    }

    /// <summary>
    /// Die Art dieser Aufzeichnung.
    /// </summary>
    protected override string TypeName => "Update Program Guide";

    /// <summary>
    /// Beginnt mit der Sammlung.
    /// </summary>
    protected override void OnStart()
    {
        // Collect sources
        var sources = m_selected.ToArray();

        // Report
        Tools.ExtendedLogging("Start Program Guide Update for {0} with {1} Source(s) and Mode {2}", ProfileName, sources.Length, m_extensions);

        // Start
        m_startPending = CardServer.BeginStartEPGCollection(sources, m_extensions);
    }

    /// <summary>
    /// Prüft, ob noch Zugriffe ausstehen.
    /// </summary>
    protected override bool HasPendingServerRequest => !WaitForEnd(ref m_startPending, "Program Guide Collection now active");

    /// <summary>
    /// Wird aufgerufen, wenn ein neuer Zustand verfügbar ist.
    /// </summary>
    /// <param name="state">Der neue Zustand.</param>
    protected override void OnNewStateAvailable(ServerInformation state)
    {
        // See if we are finished
        if (state.ProgramGuideProgress.GetValueOrDefault(0) >= 1)
            ChangeEndTime(Representative.ScheduleUniqueID!.Value, DateTime.UtcNow.AddDays(-1));
    }

    /// <summary>
    /// Beendet die Aufzeichnung vorzeitig.
    /// </summary>
    /// <param name="scheduleIdentifier">Die eindeutige Kennung der gewünschten Aufzeichnung.</param>
    protected override void OnEndRecording(Guid scheduleIdentifier)
    {
        // Must be us
        if (scheduleIdentifier != Representative.ScheduleUniqueID!.Value)
            return;

        // Set early to make sure that planner will not re-run immediately
        ProfileState.ProgramGuide.LastUpdateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Beendet die Sammlung endgültig.
    /// </summary>
    protected override void OnStop()
    {
        // At least we tried
        ProfileState.ProgramGuide.LastUpdateTime = DateTime.UtcNow;

        // Report
        Tools.ExtendedLogging("Converting Program Guide Entries from Card Server to VCR.NET Format");

        // Create result
        var result = new ProgramGuideEntries();

        // Fill it
        foreach (var item in CardServer.BeginEndEPGCollection().Result)
        {
            // Create event
            var epg =
                new ProgramGuideEntry
                {
                    TransportIdentifier = item.Source.TransportStream,
                    ShortDescription = item.ShortDescription,
                    NetworkIdentifier = item.Source.Network,
                    ServiceIdentifier = item.Source.Service,
                    Description = item.Description,
                    Duration = item.Duration,
                    Language = item.Language,
                    StartTime = item.Start,
                    Name = item.Name
                };

            // Finish
            if (item.Content != null)
                epg.Categories.AddRange(item.Content.Select(c => c.ToString()));
            if (item.Ratings != null)
                epg.Ratings.AddRange(item.Ratings);

            // Resolve
            var source = Profiles.FindSource(ProfileName, item.Source);
            if (source == null)
            {
                // Load default
                epg.StationName = item.Source.ToString()!;
            }
            else
            {
                // Attach to the station
                var station = (Station)source.Source;

                // Load names
                epg.StationName = station.Name;
            }

            // Add it
            result.Add(epg);
        }

        // Report
        ProfileState.ProgramGuide.UpdateGuide(result);
    }

    /// <summary>
    /// Ermittelt eine Beschreibung der aktuellen Aufzeichnung.
    /// </summary>
    /// <param name="info">Vorabinformation der Basisklasse.</param>
    /// <param name="finalCall">Gesetzt, wenn das Ergebnis zum Protokolleintrag wird.</param>
    /// <param name="state">Die zuletzt erhaltenen Zustandsinformationen.</param>
    protected override void OnFillInformation(FullInfo info, bool finalCall, ServerInformation state)
    {
        // Just copy the progress
        if (state == null)
            info.Recording.TotalSize = 0;
        else if (!state.ProgramGuideProgress.HasValue)
            info.Recording.TotalSize = 0;
        else
            info.Recording.TotalSize = (uint)state.CurrentProgramGuideItems;
    }
}
