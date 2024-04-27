using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Planning;
using JMS.DVB.NET.Recording.Server;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;
using JMS.DVB.NET.Recording.Status;

namespace JMS.DVB.NET.Recording.Actions;

public class Recordings(IVCRServer server, IVCRProfiles profiles, IJobManager jobs) : IRecordings
{
    /// <inheritdoc/>
    public TInfo[] GetCurrent<TInfo>(
        Func<FullInfo, IVCRServer, IVCRProfiles, IJobManager, TInfo[]> fromActive,
        Func<IScheduleInformation, PlanContext, IVCRServer, IVCRProfiles, TInfo> fromPlan = null!,
        Func<string, TInfo> forIdle = null!
    )
    {
        // Validate
        if (fromActive == null)
            throw new ArgumentNullException(nameof(fromActive));

        // All profile we know
        var idleProfiles = new HashSet<string>(server.InspectProfiles(profile => profile.ProfileName), ProfileManager.ProfileNameComparer);

        // Collect per profile
        var perProfile =
            server
                .InspectProfiles(profile => profile.CurrentRecording)
                .Where(current => current != null)
                .ToDictionary(
                    current => current!.Recording.Source.ProfileName,
                    current => fromActive(current!, server, profiles, jobs),
                    idleProfiles.Comparer
                );

        // Check for idle profiles
        idleProfiles.ExceptWith(perProfile.Keys);

        // There are any
        if ((fromPlan != null) || (forIdle != null))
            if (idleProfiles.Count > 0)
            {
                // Attach to plan - will be internally limited to some reasonable count
                var context = server.GetPlan();

                // Parse plan
                foreach (var schedule in context)
                {
                    // Will not be executed
                    var resource = schedule.Resource;
                    if (resource == null)
                        continue;

                    // Check for regular recordings - we will not report tasks
                    if (schedule.Definition is not IScheduleDefinition<VCRSchedule> definition)
                        continue;

                    // See if we are currently recording this thing
                    var runningInfo = context.GetRunState(definition.UniqueIdentifier);
                    if (runningInfo != null)
                        if (schedule.Time.End == runningInfo.Schedule.Time.End)
                            continue;

                    // See if this is one of our outstanding profiles
                    var profileName = resource.Name;
                    if (!idleProfiles.Remove(profileName))
                        continue;

                    // Add entry
                    if (fromPlan != null)
                        perProfile.Add(profileName, [fromPlan(schedule, context, server, profiles)]);

                    // Did it
                    if (idleProfiles.Count < 1)
                        break;
                }

                // Idle stuff
                if (forIdle != null)
                    foreach (var idleProfile in idleProfiles)
                        perProfile.Add(idleProfile, [forIdle(idleProfile)]);
            }

        // Report
        return perProfile.SelectMany(info => info.Value).ToArray();
    }
}