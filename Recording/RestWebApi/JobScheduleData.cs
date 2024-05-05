using System.Runtime.Serialization;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.ProgramGuide;
using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Die Daten zum Anlegen eines neuen Auftrags mit der ersten Aufzeichnung.
    /// </summary>
    public class JobScheduleData
    {
        /// <summary>
        /// Der zugehörige Auftrag.
        /// </summary>
        public EditJob Job { get; set; } = null!;

        /// <summary>
        /// Die zugehörige Aufzeichnung.
        /// </summary>
        public EditSchedule Schedule { get; set; } = null!;
    }

    /// <summary>
    /// Informationen zu einer Aufzeichnung.
    /// </summary>
    public class JobScheduleInfo : JobScheduleData
    {
        /// <summary>
        /// Die eindeutige Kennung der Auftrags.
        /// </summary>
        public string JobIdentifier { get; set; } = null!;

        /// <summary>
        /// Die eindeutige Kennung der Aufzeichnung.
        /// </summary>
        public string ScheduleIdentifier { get; set; } = null!;

        /// <summary>
        /// Legt eine neue Information an.
        /// </summary>
        /// <param name="job">Der Auftrag.</param>
        /// <param name="schedule">Die Aufzeichnung.</param>
        /// <param name="guide">Ein Eintrag der Programmzeitschrift.</param>
        /// <param name="profile">Vorgabe für das Geräteprofil.</param>
        /// <returns>Die Information.</returns>
        public static JobScheduleInfo Create(VCRJob job, VCRSchedule schedule, ProgramGuideEntry guide, string profile, IVCRProfiles profiles)
        {
            // Process
            return
                new JobScheduleInfo
                {
                    ScheduleIdentifier = (schedule == null) ? null! : schedule.UniqueID!.Value.ToString("N"),
                    JobIdentifier = (job == null) ? null! : job.UniqueID!.Value.ToString("N"),
                    Schedule = EditSchedule.Create(schedule!, job!, guide, profiles)!,
                    Job = EditJob.Create(job!, guide, profile, profiles)!,
                };
        }
    }
}

