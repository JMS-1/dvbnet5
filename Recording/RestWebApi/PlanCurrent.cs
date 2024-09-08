using JMS.DVB.Algorithms.Scheduler;
using System.Globalization;
using JMS.DVB.NET.Recording.Planning;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Status;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;
using JMS.DVB.NET.Recording.Server;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt eine Aufzeichnung, die entweder aktiv ist oder als mächstes auf einem gerade unbenutzen
    /// Gerät ausgeführt wird.
    /// </summary>
    public class PlanCurrentMobile
    {
        /// <summary>
        /// Der Name des Gerätes.
        /// </summary>
        public string ProfileName { get; set; } = null!;

        /// <summary>
        /// Der Name der Aufzeichnung.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Der Startzeitpunkt der Aufzeichnung.
        /// </summary>
        public string StartTimeISO
        {
            get { return StartTime.ToString("o"); }
            set { StartTime = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind); }
        }

        /// <summary>
        /// Der Startzeitpunkt der Aufzeichnung.
        /// </summary>
        [JsonIgnore]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Die Laufzeit der Aufzeichnung.
        /// </summary>
        public int DurationInSeconds
        {
            get { return (int)Math.Round(Duration.TotalSeconds); }
            set { Duration = TimeSpan.FromSeconds(value); }
        }

        /// <summary>
        /// Die Laufzeit der Aufzeichnung.
        /// </summary>
        [JsonIgnore]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gesetzt, wenn es Einträge in der Programmzeitschrift zu dieser Aufzeichnung gibt.
        /// </summary>
        public bool HasGuideEntry { get; set; }

        /// <summary>
        /// Die zugehörige Quelle, sofern bekannt.
        /// </summary>
        public string SourceName { get; set; } = null!;

        /// <summary>
        /// Die zugehörige Quelle, sofern bekannt.
        /// </summary>
        public string Source { get; set; } = null!;

        /// <summary>
        /// Erstellt eine reduzierte Version der Information zu einer Aktivität.
        /// </summary>
        /// <param name="full">Die volle Information.</param>
        /// <returns>Die reduzierte Information.</returns>
        public static PlanCurrentMobile Create(PlanCurrent full)
        {
            // Cut down
            return
                new PlanCurrentMobile
                {
                    HasGuideEntry = full.HasGuideEntry,
                    ProfileName = full.ProfileName,
                    SourceName = full.SourceName,
                    StartTime = full.StartTime,
                    Duration = full.Duration,
                    Source = full.Source,
                    Name = full.Name,
                };
        }
    }

    /// <summary>
    /// Beschreibt eine Aufzeichnung, die entweder aktiv ist oder als mächstes auf einem gerade unbenutzen
    /// Gerät ausgeführt wird.
    /// </summary>
    public class PlanCurrent : PlanCurrentMobile
    {
        /// <summary>
        /// Eine leere Liste von Dateien.
        /// </summary>
        private static readonly string[] _NoFiles = [];

        /// <summary>
        /// Die eindeutige Kennung der Aufzeichnung.
        /// </summary>
        public string Identifier { get; set; } = null!;

        /// <summary>
        /// Gesetzt, wenn die Aufzeichung gerade ausgeführt wird.
        /// </summary>
        public string PlanIdentifier { get; set; } = null!;

        /// <summary>
        /// Gesetzt, wenn die Aufzeichnung verspätet beginnt.
        /// </summary>
        public bool IsLate { get; set; }

        /// <summary>
        /// Gesetzt, wenn es sich hier um einen Platzhalter für ein Gerät handelt, dass nicht in Benutzung ist.
        /// </summary>
        public bool IsIdle { get; set; }

        /// <summary>
        /// Eine Beschreibung der Größe, Anzahl etc.
        /// </summary>
        public string SizeHint { get; set; } = null!;

        /// <summary>
        /// Die zugehörige Quelle.
        /// </summary>
        [JsonIgnore]
        private SourceSelection m_source = null!;

        /// <summary>
        /// Die laufende Nummer des Datenstroms, die zur Anzeige benötigt wird.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Die Netzwerkadresse, an die gerade die Aufzeichnungsdaten versendet werden.
        /// </summary>
        public string StreamTarget { get; set; } = null!;

        /// <summary>
        /// Die verbleibende Restzeit der Aufzeichnung.
        /// </summary>
        public uint RemainingTimeInMinutes
        {
            get { return (PlanIdentifier == null) ? 0 : checked((uint)Math.Max(0, Math.Round((StartTime + Duration - DateTime.UtcNow).TotalMinutes))); }
            set { }
        }

        /// <summary>
        /// Alle zu dieser Aktivität erstellten Dateien.
        /// </summary>
        public required string[] Files { get; set; }

        /// <summary>
        /// Alle zu dieser Aktivität erstellten Dateien als Hashwerte für den FTP Zugriff.
        /// </summary>
        public string[] FileHashes => [.. Files.Select(Tools.GetPathHash)];

        /// <summary>
        /// Rundet einen Datumswert auf die volle Sekunde.
        /// </summary>
        /// <param name="original">Die originale Zeit.</param>
        /// <returns>Die gerundete Zeit.</returns>
        public static DateTime RoundToSecond(DateTime original)
        {
            // Helper
            const long HalfASecond = 5000000;
            const long FullSecond = 2 * HalfASecond;

            // Load as 100ns units
            var ticks = original.Ticks;

            // Get the difference to the previous second
            var mod = ticks % FullSecond;

            // Get the truncated time
            ticks -= mod;

            // Round up
            if (mod >= HalfASecond)
                ticks += FullSecond;

            // Done
            return new DateTime(ticks, DateTimeKind.Utc);
        }

        /// <summary>
        /// Erstellt eine neue Liste von Beschreibungen für eine aktive Aufzeichnung.
        /// </summary>
        /// <param name="active">Die Daten zur aktiven Aufzeichnung.</param>
        /// <param name="server">Der zugehörige Dienst.</param>
        /// <returns>Die gewünschten Beschreibungen.</returns>
        public static PlanCurrent[] Create(FullInfo active, IVCRServer server, IVCRProfiles profiles, IJobManager jobs)
        {
            // Validate
            ArgumentNullException.ThrowIfNull(active);

            // Validate
            var recording = active.Recording ?? throw new ArgumentException(null, "active.Recording");

            // Multiple recordings
            var streams = active.Streams;
            if (streams != null)
                if (streams.Count > 0)
                {
                    var regular = streams.SelectMany((stream, index) => Create(active, stream, index, server, profiles, jobs)).ToList();

                    // Append finished recordings on still active cards
                    foreach (var finished in active.Finished)
                        if (finished.PhysicalStart != null)
                        {
                            // Try to locate the context
                            var job = finished.JobUniqueID == null ? null : jobs[finished.JobUniqueID.Value];
                            var schedule = ((job == null) || finished.ScheduleUniqueID == null) ? null : job[finished.ScheduleUniqueID.Value];

                            regular.Add(new()
                            {
                                Identifier = (schedule == null) ? null! : ServerTools.GetUniqueWebId(job!, schedule),
                                ProfileName = finished.Source.ProfileName,
                                StartTime = finished.PhysicalStart.Value,
                                m_source = finished.Source,
                                Files = [finished.FileName],
                                Name = finished.Name,
                                PlanIdentifier = "",
                                SizeHint = "-"
                            });
                        }

                    return [.. regular];
                }

            // Single recording - typically a task
            var start = RoundToSecond(active.Recording.PhysicalStart.GetValueOrDefault(DateTime.UtcNow));
            var end = RoundToSecond(recording.EndsAt);
            var source = recording.Source;
            var sourceName = source.DisplayName;

            // Create
            var current =
                new PlanCurrent
                {
                    PlanIdentifier = recording.ScheduleUniqueID!.Value.ToString("N"),
                    ProfileName = source.ProfileName,
                    Duration = end - start,
                    Name = recording.Name,
                    m_source = source,
                    StartTime = start,
                    Files = _NoFiles,
                    IsLate = false,
                    Index = -1,
                };

            // Finish            
            if (VCRJob.ProgramGuideName.Equals(sourceName))
                current.SizeHint = $"{recording.TotalSize:N0} Einträge";
            else if (VCRJob.SourceScanName.Equals(sourceName))
                current.SizeHint = $"{recording.TotalSize:N0} Quellen";
            else if (VCRJob.ZappingName.Equals(sourceName))
                current.SizeHint = GetSizeHint(recording.TotalSize);
            else
                current.Complete(server, profiles);

            // Report
            return [current];
        }

        /// <summary>
        /// Schließt die Konfiguration einer Beschreibung ab.
        /// </summary>
        /// <param name="server">Der zugehörige Dienst.</param>
        private void Complete(IVCRServer server, IVCRProfiles profiles)
        {
            // No source
            if (m_source == null)
                return;

            // At least we have this
            Source = SourceIdentifier.ToString(m_source.Source)!.Replace(" ", "");

            // Check profile - should normally be available
            var profile = server[ProfileName];
            if (profile == null)
                return;

            // Load the profile
            HasGuideEntry = profile.ProgramGuide.HasEntry(m_source.Source, StartTime, StartTime + Duration);
            SourceName = profiles.GetUniqueName(m_source);
        }

        /// <summary>
        /// Ermittelt die Größenangabe.
        /// </summary>
        /// <param name="totalSize">Die Anzahl von bisher übertragenen Kilobytes.</param>
        /// <returns>Die Größe in Textform.</returns>
        public static string GetSizeHint(decimal totalSize)
        {
            // Check mode
            if (totalSize < 10000m)
                return $"{totalSize:N0} kBytes";
            else
                return $"{Math.Round(totalSize / 1024m):N0} MBytes";
        }

        /// <summary>
        /// Erstellt eine Beschreibung zu einer einzelnen Aufzeichnung auf einem Gerät.
        /// </summary>
        /// <param name="active">Beschreibt die gesamte Aufzeichnung.</param>
        /// <param name="stream">Die zu verwendende Teilaufzeichnung.</param>
        /// <param name="streamIndex">Die laufende Nummer dieses Datenstroms.</param>
        /// <param name="server">Der zugehörige Dienst.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        private static IEnumerable<PlanCurrent> Create(FullInfo active, StreamInfo stream, int streamIndex, IVCRServer server, IVCRProfiles profiles, IJobManager jobs)
        {
            // Static data
            var recording = active.Recording;
            var profileName = recording.Source.ProfileName;
            var sizeHint = GetSizeHint(recording.TotalSize) + " (Gerät)";

            // Process all - beginning with VCR.NET 4.1 there is only one schedule per stream
            foreach (var scheduleInfo in stream.Schedules)
            {
                // Try to locate the context
                var job = string.IsNullOrEmpty(scheduleInfo.JobUniqueID) ? null : jobs[new Guid(scheduleInfo.JobUniqueID)];
                var schedule = ((job == null) || string.IsNullOrEmpty(scheduleInfo.ScheduleUniqueID)) ? null : job[new Guid(scheduleInfo.ScheduleUniqueID)];

                // Create
                var start = RoundToSecond(scheduleInfo.StartsAt);
                var end = RoundToSecond(scheduleInfo.EndsAt);
                var current =
                    new PlanCurrent
                    {
                        Identifier = (schedule == null) ? null! : ServerTools.GetUniqueWebId(job!, schedule),
                        PlanIdentifier = scheduleInfo.ScheduleUniqueID,
                        Files = scheduleInfo.Files ?? _NoFiles,
                        StreamTarget = stream.StreamsTo,
                        m_source = scheduleInfo.Source,
                        ProfileName = profileName,
                        Name = scheduleInfo.Name,
                        Duration = end - start,
                        Index = streamIndex,
                        SizeHint = sizeHint,
                        StartTime = start,
                    };

                // Finish
                current.Complete(server, profiles);

                // Report
                yield return current;
            }
        }

        /// <summary>
        /// Erstellt eine neue Beschreibung aus dem Aufzeichnungsplan.
        /// </summary>
        /// <param name="plan">Die Planung der Aufzeichnung.</param>
        /// <param name="context">Die aktuelle Analyseumgebung.</param>
        /// <param name="server">Der zugehörige Dienst.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static PlanCurrent Create(IScheduleInformation plan, PlanContext context, IVCRServer server, IVCRProfiles profiles)
        {
            // Attach to the definition
            var definition = (IScheduleDefinition<VCRSchedule>)plan.Definition;
            var job = context.TryFindJob(definition.UniqueIdentifier);
            var schedule = job?[definition.UniqueIdentifier];
            var source = (schedule == null) ? null : (schedule.Source ?? job!.Source);

            // Create
            var planned =
                new PlanCurrent
                {
                    Identifier = (schedule == null) ? null! : ServerTools.GetUniqueWebId(job!, schedule),
                    ProfileName = plan.Resource.Name,
                    Duration = plan.Time.Duration,
                    StartTime = plan.Time.Start,
                    IsLate = plan.StartsLate,
                    SizeHint = string.Empty,
                    Name = definition.Name,
                    m_source = source!,
                    Files = _NoFiles,
                    Index = -1,
                };

            // Finish
            planned.Complete(server, profiles);

            // Report
            return planned;
        }

        /// <summary>
        /// Erstellt einen Eintrag für ein Geräteprofil, das nicht verwendet wird.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <returns>Die zugehörige Beschreibung.</returns>
        public static PlanCurrent Create(string profileName) => new() { ProfileName = profileName, IsIdle = true, Files = [] };
    }
}
