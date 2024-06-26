﻿using System.Globalization;
using System.Text.Json.Serialization;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.ProgramGuide;
using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt eine einzelne Aufzeichnung eines Auftrags.
    /// </summary>
    public class EditSchedule
    {
        /// <summary>
        /// Der optionale Name der Aufzeichnung.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Der optionale Name der Quelle, von der aufgezeichnet werden soll.
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Falls <see cref="Source"/> definiert ist wird diese Eigenschaft gesetzt
        /// um alle Tonspuren aufzuzeichnen.
        /// </summary>
        public bool AllLanguages { get; set; }

        /// <summary>
        /// Falls <see cref="Source"/> definiert ist wird diese Eigenschaft gesetzt
        /// um auch die <i>Dolby Digital</i> Tonspur aufzuzeichnen.
        /// </summary>
        public bool DolbyDigital { get; set; }

        /// <summary>
        /// Falls <see cref="Source"/> definiert ist wird diese Eigenschaft gesetzt
        /// um auch die <i>Dolby Digital</i> Tonspur aufzuzeichnen.
        /// </summary>
        public bool Videotext { get; set; }

        /// <summary>
        /// Falls <see cref="Source"/> definiert ist wird diese Eigenschaft gesetzt
        /// um auch alle DVB Untertitelspuren aufzuzeichnen.
        /// </summary>
        public bool DVBSubtitles { get; set; }

        /// <summary>
        /// Der Zeitpunkt, an dem die erste Aufzeichnung stattfinden soll.
        /// </summary>
        public string FirstStartISO
        {
            get { return FirstStart.ToString("o"); }
            set { FirstStart = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind); }
        }

        /// <summary>
        /// Der Zeitpunkt, an dem die erste Aufzeichnung stattfinden soll.
        /// </summary>
        [JsonIgnore]
        public DateTime FirstStart { get; set; }

        /// <summary>
        /// Die Tage, an denen die Aufzeichnung wiederholt werden soll.
        /// </summary>
        public int RepeatPatternJSON
        {
            get { return RepeatPattern.HasValue ? (int)RepeatPattern.Value : 0; }
            set { RepeatPattern = (value == 0) ? null : (VCRDay?)value; }
        }

        /// <summary>
        /// Die Tage, an denen die Aufzeichnung wiederholt werden soll.
        /// </summary>
        [JsonIgnore]
        public VCRDay? RepeatPattern { get; set; }

        /// <summary>
        /// Falls <see cref="RepeatPattern"/> gesetzt ist der Tag der letzten Aufzeichnung.
        /// </summary>
        public string? LastDayISO
        {
            get { return RepeatPattern.HasValue ? LastDay.Date.ToString("o") : null; }
            set { LastDay = string.IsNullOrEmpty(value) ? DateTime.MinValue : DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).Date; }
        }

        /// <summary>
        /// Falls <see cref="RepeatPattern"/> gesetzt ist der Tag der letzten Aufzeichnung.
        /// </summary>
        [JsonIgnore]
        public DateTime LastDay { get; set; }

        /// <summary>
        /// Die Dauer der Aufzeichnung.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Alle Ausnahmeregelungen, die aktiv sind.
        /// </summary>
        public PlanException[] Exceptions { get; set; }

        /// <summary>
        /// Erstellt einen neue Aufzeichnung.
        /// </summary>
        public EditSchedule()
        {
            // Finish
            Exceptions = [];
        }

        /// <summary>
        /// Erstellt eine Beschreibung zu dieser Aufzeichnung.
        /// </summary>
        /// <param name="schedule">Eine Aufzeichnung.</param>
        /// <param name="job">Der bereits vorhandene Auftrag.</param>
        /// <param name="guide">Ein Eintrag aus der Programmzeitschrift.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static EditSchedule? Create(VCRSchedule schedule, VCRJob job, ProgramGuideEntry guide, IVCRProfiles profiles, UserProfile profile)
        {
            // None
            if (schedule == null)
            {
                // No hope
                if (guide == null)
                    return null;

                // Calculate
                var start = guide.StartTime - TimeSpan.FromMinutes(profile.GuideAheadStart);
                var duration = checked((int)(profile.GuideAheadStart + (guide.Duration / 60) + profile.GuideBeyondEnd));

                // Partial - we have a brand new job which is pre-initialized with the source
                if (job == null)
                    return new EditSchedule { FirstStart = start, Duration = duration };

                // Full monty - we have to overwrite the jobs settings since we are not the first schedule
                return
                    new EditSchedule
                    {
                        Source = profiles.GetUniqueName(new SourceSelection { ProfileName = job.Source.ProfileName, Source = guide.Source }),
                        DVBSubtitles = profile.Subtitles,
                        DolbyDigital = profile.Dolby,
                        AllLanguages = profile.Languages,
                        Videotext = profile.Videotext,
                        Name = guide.Name.MakeValid(),
                        Duration = duration,
                        FirstStart = start,
                    };
            }

            // Consolidate exceptions
            schedule.CleanupExceptions();

            // Optionen ermitteln
            var streams = schedule.Streams;
            var sourceName = profiles.GetUniqueName(schedule.Source);

            // Create
            return
                new EditSchedule
                {
                    Exceptions = schedule.Exceptions.Select(exception => PlanException.Create(exception, schedule)).ToArray(),
                    LastDay = schedule.LastDay.GetValueOrDefault(VCRSchedule.MaxMovableDay),
                    DolbyDigital = streams.GetUsesDolbyAudio(),
                    DVBSubtitles = streams.GetUsesSubtitles(),
                    AllLanguages = streams.GetUsesAllAudio(),
                    Videotext = streams.GetUsesVideotext(),
                    FirstStart = schedule.FirstStart,
                    RepeatPattern = schedule.Days,
                    Duration = schedule.Duration,
                    Name = schedule.Name,
                    Source = sourceName,
                };
        }

        /// <summary>
        /// Erstellt die Beschreibung der Aufzeichnung für die persistente Ablage.
        /// </summary>
        /// <param name="job">Der zugehörige Auftrag.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public VCRSchedule CreateSchedule(VCRJob job, IVCRProfiles profiles) => CreateSchedule(Guid.NewGuid(), job, profiles);

        /// <summary>
        /// Erstellt die Beschreibung der Aufzeichnung für die persistente Ablage.
        /// </summary>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
        /// <param name="job">Der zugehörige Auftrag.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public VCRSchedule CreateSchedule(Guid scheduleIdentifier, VCRJob job, IVCRProfiles profiles)
        {
            // Create
            var schedule =
                new VCRSchedule
                {
                    UniqueID = scheduleIdentifier,
                    FirstStart = FirstStart,
                    Days = RepeatPattern,
                    Duration = Duration,
                    LastDay = LastDay,
                    Name = Name ?? "",
                };

            // See if we have a source
            var sourceName = Source;
            if (string.IsNullOrEmpty(sourceName))
                return schedule;

            // See if there is a profile
            var jobSource = job.Source;
            if (jobSource == null)
                return schedule;

            // Locate the source
            schedule.Source = profiles.FindSource(jobSource.ProfileName, sourceName)!;
            if (schedule.Source == null)
                return schedule;

            // Configure streams
            schedule.Streams = new StreamSelection();

            // Set all - oder of audio settings is relevant, dolby MUST come last
            schedule.Streams.SetUsesAllAudio(AllLanguages);
            schedule.Streams.SetUsesDolbyAudio(DolbyDigital);
            schedule.Streams.SetUsesSubtitles(DVBSubtitles);
            schedule.Streams.SetUsesVideotext(Videotext);
            schedule.Streams.ProgramGuide = true;

            // Report
            return schedule;
        }
    }
}
