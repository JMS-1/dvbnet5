﻿using JMS.DVB.CardServer;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Logging;
using JMS.DVB.NET.Recording.Services.Planning;
using JMS.DVB.NET.Recording.Status;

namespace JMS.DVB.NET.Recording.Requests;

/// <summary>
/// Beschreibt die Ausführung der Aktualisierung der Quellen eines Geräteprofils.
/// </summary>
/// <param name="state">Das zugehörige Geräteprofil.</param>
/// <param name="recording">Die Beschreibung der Aufgabe.</param>
public class SourceScanProxy(
    IProfileState state,
    VCRRecordingInfo recording,
    ILogger<SourceScanProxy> logger,
    IJobManager jobManager,
    IVCRConfiguration configuration,
    IVCRProfiles profiles,
    IExtensionManager extensionManager
    ) : CardServerProxy(state, logger, jobManager, configuration, profiles, extensionManager, recording)
{
    /// <summary>
    /// Beschreibt den Zugriff zum Starten der Aktualisierung.
    /// </summary>
    private IAsyncResult m_startPending = null!;

    /// <summary>
    /// Gesetzt, um die Listen nach Abschluss des Suchlaufs zu kombinieren.
    /// </summary>
    private readonly bool m_mergeSources = configuration.MergeSourceListUpdateResult;

    /// <summary>
    /// Die Art dieser Aufzeichnung.
    /// </summary>
    protected override string TypeName => "Update Source List";

    /// <summary>
    /// Aktiviert die Aktualisierung der Quellen.
    /// </summary>
    protected override void OnStart()
    {
        // Report
        Tools.ExtendedLogging("Starting Source List Update for {0}", ProfileName);

        // Just start
        m_startPending = CardServer.BeginStartScan();
    }

    /// <summary>
    /// Wird aufgerufen, sobald eine neuer Zustand verfügbar ist.
    /// </summary>
    /// <param name="state">Der neue Zustand.</param>
    protected override void OnNewStateAvailable(ServerInformation state)
    {
        // See if we are finished
        if (state.UpdateProgress.GetValueOrDefault(0) >= 1)
            ChangeEndTime(Representative.ScheduleUniqueID!.Value, DateTime.UtcNow.AddDays(-365));
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
        ProfileState.LastSourceUpdateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Beendet den Suchlauf endgültig.
    /// </summary>
    protected override void OnStop()
    {
        // Remember the time of the last scan
        ProfileState.LastSourceUpdateTime = DateTime.UtcNow;

        // Log
        Logger.Log(LoggingLevel.Full, "Die Liste der Quellen wird ersetzt");

        // Finish
        ServerImplementation.EndRequest(CardServer.BeginEndScan(m_mergeSources));

        // Report
        Tools.ExtendedLogging("Card Server has updated Profile {0} - VCR.NET will reload all Profiles now", ProfileName);

        // Time to refresh our lists
        Profiles.Reset();
    }

    /// <summary>
    /// Ergänzt den aktuellen Zustand des Suchlaufs.
    /// </summary>
    /// <param name="info">Die bereits vorbereitete Informationsstruktur.</param>
    /// <param name="finalCall">Gesetzt, wenn dieser Zustand als Protokoll verwendet wird.</param>
    /// <param name="state">Der zuletzt erhaltene Zustand.</param>
    protected override void OnFillInformation(FullInfo info, bool finalCall, ServerInformation state)
    {
        // Copy current result
        if (state == null)
            info.Recording.TotalSize = 0;
        else if (!state.UpdateProgress.HasValue)
            info.Recording.TotalSize = 0;
        else
            info.Recording.TotalSize = (uint)state.UpdateSourceCount;
    }

    /// <summary>
    /// Prüft, ob noch Zugriffe ausstehen.
    /// </summary>
    protected override bool HasPendingServerRequest => !WaitForEnd(ref m_startPending, "Source Scan now active");
}

