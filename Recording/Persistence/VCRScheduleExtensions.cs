using JMS.DVB.Algorithms.Scheduler;

namespace JMS.DVB.NET.Recording.Persistence
{
    public static class VCRScheduleExtensions
    {
        private static VCRProfiles _profiles = null!;

        public class Initializer
        {
            public Initializer(VCRProfiles profiles)
            {
                _profiles = profiles;
            }
        }

        /// <summary>
        /// Prüft, ob die Daten zur Aufzeichnung zulässig sind.
        /// </summary>
        /// <param name="job">Der zugehörige Auftrag.</param>
        /// <exception cref="InvalidJobDataException">Es wurde keine eindeutige Kennung angegeben.</exception>
        /// <exception cref="InvalidJobDataException">Die Daten der Aufzeichnung sind fehlerhaft.</exception>
        public static void Validate(this VCRSchedule schedule, VCRJob job)
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
                if (!schedule.Source.Validate())
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

        /// <summary>
        /// Registriert diese Aufzeichnung in einer Planungsinstanz.
        /// </summary>
        /// <param name="scheduler">Die zu verwendende Planungsinstanz.</param>
        /// <param name="job">Der zugehörige Auftrag.</param>
        /// <param name="devices">Die Liste der Geräte, auf denen die Aufzeichnung ausgeführt werden darf.</param>
        /// <param name="findSource">Dient zum Prüfen einer Quelle.</param>
        /// <param name="disabled">Alle deaktivierten Aufträge.</param>
        /// <param name="context">Die aktuelle Planungsumgebung.</param>
        /// <exception cref="ArgumentNullException">Es wurden nicht alle Parameter angegeben.</exception>
        public static void AddToScheduler(
            this VCRSchedule schedule,
            RecordingScheduler scheduler,
            VCRJob job,
            IScheduleResource[] devices,
            Func<SourceSelection, VCRProfiles, SourceSelection?> findSource,
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
}