using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Planning;
using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.Actions;

public class RecordingInfoFactory(IVCRConfiguration configuration, IVCRProfiles profiles) : IRecordingInfoFactory
{
    /// <summary>
    /// Befüllt vor allem den Dateinamen mit Vorgabewerten.
    /// </summary>
    private void LoadDefaults(VCRRecordingInfo recording)
    {
        // Construct file name
        var pattern = configuration.FileNamePattern;
        var file = recording.FileName;

        // Check for test recording
        if ((recording.RelatedJob != null) && (recording.RelatedSchedule != null))
        {
            // Enter placeholders
            pattern = recording
                .GetReplacementPatterns(profiles)
                .Aggregate(pattern, (current, rule) => current.Replace(rule.Key, rule.Value.MakeValid()));
        }
        else if (!string.IsNullOrEmpty(file))
        {
            // Reconstruct name
            pattern = Path.GetFileNameWithoutExtension(file);

            // Cut off directory
            file = Path.GetDirectoryName(file);
        }
        else
        {
            // Set dummy name
            pattern = $"Test {DateTime.Now:dd-MM-yyyy HH-mm-ss}";
        }

        // Default directory
        if (string.IsNullOrEmpty(file))
            file = configuration.PrimaryTargetDirectory.FullName;

        // Append pattern
        recording.FileName = Path.Combine(file, pattern + ".ts");

        // Check for valid path - user can try to jump out of the allowed area but we will move back
        if (!configuration.IsValidTarget(recording.FileName))
            recording.FileName = Path.Combine(configuration.PrimaryTargetDirectory.FullName, pattern + ".ts");
    }

    /// <inheritdoc/>
    public VCRRecordingInfo? Create(IScheduleInformation planItem, PlanContext context)
    {
        // Validate
        ArgumentNullException.ThrowIfNull(planItem, nameof(planItem));
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        // Check type
        if (planItem.Definition is not IScheduleDefinition<VCRSchedule> definition)
        {
            // Check for program guide collector
            if (planItem.Definition is ProgramGuideTask guideCollection)
                return
                    new VCRRecordingInfo
                    {
                        Source = new SourceSelection { ProfileName = planItem.Resource.Name, DisplayName = VCRJob.ProgramGuideName },
                        FileName = Path.Combine(guideCollection.CollectorDirectory.FullName, Guid.NewGuid().ToString("N") + ".epg"),
                        ScheduleUniqueID = guideCollection.UniqueIdentifier,
                        IsHidden = planItem.Resource == null,
                        StartsLate = planItem.StartsLate,
                        StartsAt = planItem.Time.Start,
                        Name = guideCollection.Name,
                        EndsAt = planItem.Time.End,
                    };

            // Check for source list update
            if (planItem.Definition is SourceListTask sourceUpdater)
                return
                    new VCRRecordingInfo
                    {
                        Source = new SourceSelection { ProfileName = planItem.Resource.Name, DisplayName = VCRJob.SourceScanName },
                        FileName = Path.Combine(sourceUpdater.CollectorDirectory.FullName, Guid.NewGuid().ToString("N") + ".psi"),
                        ScheduleUniqueID = sourceUpdater.UniqueIdentifier,
                        IsHidden = planItem.Resource == null,
                        StartsLate = planItem.StartsLate,
                        StartsAt = planItem.Time.Start,
                        EndsAt = planItem.Time.End,
                        Name = sourceUpdater.Name,
                    };

            // None
            return null;
        }

        // Attach to the schedule and its job - using the context and the map is the easiest way although there may be better alternatives
        var job = context.TryFindJob(definition.UniqueIdentifier);
        var schedule = definition.Context;

        // Find the source
        var source = schedule.Source ?? job!.Source;
        if (source != null)
        {
            // Create a clone
            source = new SourceSelection { DisplayName = source.DisplayName, SelectionKey = source.SelectionKey };

            // Update the name of the profile
            var resource = planItem.Resource;
            if (resource != null)
                source.ProfileName = resource.Name;
        }

        // Create the description of this recording
        var recording =
            new VCRRecordingInfo
            {
                Streams = (schedule.Source == null) ? job!.Streams : schedule.Streams,
                ScheduleUniqueID = schedule.UniqueID,
                IsHidden = planItem.Resource == null,
                StartsLate = planItem.StartsLate,
                StartsAt = planItem.Time.Start,
                EndsAt = planItem.Time.End,
                JobUniqueID = job!.UniqueID,
                RelatedSchedule = schedule,
                FileName = job.Directory,
                Name = definition.Name,
                RelatedJob = job,
                Source = source!,
            };

        // May want to adjust start time if job is active
        var runningInfo = context.GetRunState(definition.UniqueIdentifier);
        if (runningInfo != null)
            if (runningInfo.Schedule.Time.End == recording.EndsAt)
            {
                // Assume we never start late - we are running
                recording.StartsLate = false;

                // If we started prior to this plan report the time we really started
                if (planItem.Time.Start > runningInfo.Schedule.Time.Start)
                    recording.StartsAt = runningInfo.Schedule.Time.Start;
            }

        // Finish
        LoadDefaults(recording);

        // Report
        return recording;
    }
}