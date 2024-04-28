using JMS.DVB.CardServer;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Logging;
using JMS.DVB.NET.Recording.Services.Planning;
using JMS.DVB.NET.Recording.Status;

namespace JMS.DVB.NET.Recording.Requests;

/// <summary>
/// Diese Klasse beschreibt die Ausführung des <i>Zapping Modes</i>.
/// </summary>
public class ZappingProxy : CardServerProxy
{
    /// <summary>
    /// Die Zieladresse für diesen Zugriff.
    /// </summary>
    private readonly string m_target;

    /// <summary>
    /// Die letzen Zustandsinformationen.
    /// </summary>
    private volatile ServerInformation m_lastState = null!;

    /// <summary>
    /// Erstellt einen neuen Zugriff.
    /// </summary>
    /// <param name="profile">Das zu verwendende Geräteprofil.</param>
    /// <param name="primary">Informationen zur Aufzeichnung als Ganzes.</param>
    /// <param name="target">Die aktuelle Zieladresse für die Nutzdaten.</param>
    public ZappingProxy(
        IProfileState profile,
        VCRRecordingInfo primary,
        string target,
        ILogger<ZappingProxy> logger,
        IJobManager jobManager,
        IVCRConfiguration configuration,
        IVCRProfiles profiles,
        IExtensionManager extensionManager
    ) : base(profile, logger, jobManager, configuration, profiles, extensionManager, primary)
    {
        // Remember
        m_target = target;
    }

    /// <summary>
    /// Ändert den Endzeitpunkt der Benutzung.
    /// </summary>
    /// <param name="streamIdentifier">Die eindeutige Kennung dieser Aufzeichnung.</param>
    /// <param name="newEndTime">Der Zeitversatz, der angewendet werden soll.</param>
    /// <param name="disableHibernation">Gesetzt, wenn der Übergang in den Schlafzustand verhindert werden soll.</param>
    public override void ChangeEndTime(Guid streamIdentifier, DateTime newEndTime, bool disableHibernation)
    {
        // Check for us
        if (streamIdentifier != Representative.ScheduleUniqueID!.Value)
            return;

        // Get the time
        var endTimeLimit = DateTime.UtcNow;
        if (newEndTime < endTimeLimit)
            newEndTime = endTimeLimit;

        // Just update
        Representative.EndsAt = newEndTime;
    }

    /// <summary>
    /// Meldet, ob noch asynchrone Aufrufe ausstehen.
    /// </summary>
    protected override bool HasPendingServerRequest => false;

    /// <summary>
    /// Prüft, ob es sich um eine reguläre Aufzeichnungs handelt.
    /// </summary>
    protected override bool IsRealRecording => false;

    /// <summary>
    /// Ermittelt eine Beschreibung der aktuellen Nutzung.
    /// </summary>
    /// <param name="info">Von der Basisklasse bereits vorbereitete Daten.</param>
    /// <param name="finalCall">Gesetzt, wenn das Ergebnis ein Protokolleintrag werden soll.</param>
    /// <param name="state">Der zuletzt ermittelte Zustand des Aufzeichnungsprozesses.</param>
    protected override void OnFillInformation(FullInfo info, bool finalCall, ServerInformation state)
    {
        // Reset
        info.Recording.TotalSize = 0;

        // Attach to the current state
        if (state == null)
            return;

        // Get the first stream
        if (state.Streams.Count > 0)
            info.Recording.TotalSize = (uint)(state.Streams[0].BytesReceived / 1024);
    }

    /// <summary>
    /// Bearbeitet den aktuellen Zustand.
    /// </summary>
    /// <param name="state">Der aktuelle Zustand.</param>
    protected override void OnNewStateAvailable(ServerInformation state)
    {
        // Remember
        m_lastState = state;

        // Check for end
        if (DateTime.UtcNow >= Representative.EndsAt)
            Stop(Representative.ScheduleUniqueID!.Value);
    }

    /// <summary>
    /// Wird aufgerufen, sobald der Aufzeichnungsprozess aktiv ist.
    /// </summary>
    protected override void OnStart()
    {
    }

    /// <summary>
    /// Wird aufgerufen, bevor der Aufzeichnungsprozess beendet wird.
    /// </summary>
    protected override void OnStop()
    {
    }

    /// <summary>
    /// Meldet einen Anzeigenamen für den <i>LIVE</i> Zugang.
    /// </summary>
    protected override string TypeName => "LIVE";

    /// <summary>
    /// Aktiviert eine neue Quelle.
    /// </summary>
    /// <typeparam name="TStatus">Die Art der Zustandsinformation.</typeparam>
    /// <param name="source">Die gewünschte Quelle.</param>
    /// <param name="factory">Methode zum Erzeugen einer neuen Zustandsinformation.</param>
    /// <returns>Der neue Zustand der Übertragung.</returns>
    public TStatus SetSource<TStatus>(SourceIdentifier source, Func<string, ServerInformation, TStatus> factory)
    {
        // Update end time
        Stamp();

        // Remap to real source
        var selection = Profiles.FindSource(ProfileName, source);
        if (selection == null)
            throw new ArgumentNullException(nameof(source));

        // Report
        Tools.ExtendedLogging("Will now zap to Source {0}", selection.Source);

        // Process and remember
        m_lastState = EnqueueActionAndWait(() => ServerImplementation.EndRequest(CardServer.BeginSetZappingSource(selection.SelectionKey, m_target)))!;

        // Report
        return CreateStatus(factory);
    }

    /// <summary>
    /// Setzt den Endzeitpunkt weiter.
    /// </summary>
    private void Stamp()
    {
        // Update end time
        var endsAt = DateTime.UtcNow.AddMinutes(2);
        if (endsAt > Representative.EndsAt)
            Representative.EndsAt = endsAt;
    }

    /// <summary>
    /// Ermittelt den aktuellen Zustand.
    /// </summary>
    /// <typeparam name="TStatus">Die Art der Zustandsinformation.</typeparam>
    /// <param name="factory">Methode zum Erzeugen einer Zustandsinformation.</param>
    /// <returns>Der gewünschte Zustand.</returns>
    public TStatus CreateStatus<TStatus>(Func<string, ServerInformation, TStatus> factory)
    {
        // Update end time
        Stamp();

        // None
        if (IsShuttingDown)
            return factory(null!, null!);
        else
            return factory(m_target, m_lastState);
    }
}

