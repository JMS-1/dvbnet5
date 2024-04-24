using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.ProgramGuide;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;
using Microsoft.AspNetCore.Mvc;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Der Web Service zur Pflege von Aufzeichnungen und Aufträgen.
    /// </summary>
    [ApiController]
    [Route("api/edit")]
    public class EditController(VCRServer server, IProfileStateCollection states, IVCRProfiles profiles, IJobManager jobs) : ControllerBase
    {
        /// <summary>
        /// Wird zum Anlegen einer neuen Aufzeichnung verwendet.
        /// </summary>
        /// <param name="data">Die Daten zur Aufzeichnung.</param>
        /// <returns>Die Identifikation des neuen Auftrags.</returns>
        [HttpPost("job")]
        public string CreateNewJob([FromBody] JobScheduleData data)
        {
            // Reconstruct
            var job = data.Job.CreateJob(server);
            var schedule = data.Schedule.CreateSchedule(job, server);

            // See if we can use it
            if (!schedule.IsActive)
                throw new ArgumentException("Die Aufzeichnung liegt in der Vergangenheit");

            // Connect
            job.Schedules.Add(schedule);

            // Process
            jobs.Update(job, schedule.UniqueID!.Value);

            states.BeginNewPlan();

            // Update recently used channels
            UserProfileSettings.AddRecentChannel(data.Job.Source);
            UserProfileSettings.AddRecentChannel(data.Schedule.Source);

            // Report
            return ServerTools.GetUniqueWebId(job, schedule);
        }

        /// <summary>
        /// Ermittelt die Daten zu einer einzelnen Aufzeichnung.
        /// </summary>
        /// <param name="detail">Die Referenz auf die Aufzeichnung.</param>
        /// <param name="epg">Informationen zu einem Eintrag aus der Programmzeitschrift.</param>
        /// <returns>Die Daten zur gewünschten Aufzeichnung.</returns>
        [HttpGet("job/{detail}")]
        public JobScheduleInfo FindJob(string detail, string epg = null!)
        {
            // May need to recreate the identifier
            if (detail.StartsWith("*"))
                if (detail.Length == 1)
                    detail = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
                else
                    detail = detail.Substring(1, 32) + Guid.NewGuid().ToString("N");

            // Parameter analysieren
            var schedule = jobs.ParseUniqueWebId(detail, out VCRJob job);

            // See if we have to initialize from program guide
            ProgramGuideEntry? epgEntry = null;
            string profile = null!;
            if (!string.IsNullOrEmpty(epg))
            {
                // Get parts
                var epgInfo = epg.Split(':');

                // Locate
                epgEntry = server.FindProgramGuideEntry(profile = epgInfo[1], SourceIdentifier.Parse(epgInfo[2]), new DateTime(long.Parse(epgInfo[0]), DateTimeKind.Utc));
            }

            // Information erzeugen
            return JobScheduleInfo.Create(job, schedule!, epgEntry!, profile, profiles);
        }

        /// <summary>
        /// Aktualisiert die Daten einer Aufzeichnung.
        /// </summary>
        /// <param name="detail">Die Referenz auf die Aufzeichnung.</param>
        /// <param name="data">Die neuen Daten von Auftrag und Aufzeichnung.</param>
        [HttpPut("recording/{detail}")]
        public void UpdateRecording(string detail, [FromBody] JobScheduleData data)
        {
            // Parameter analysieren
            var schedule = jobs.ParseUniqueWebId(detail, out VCRJob job);

            // Validate
            if (schedule == null)
                throw new ArgumentException("Job or Schedule not found");

            // Take the new job data
            var newJob = data.Job.CreateJob(job.UniqueID!.Value, server);
            var newSchedule = data.Schedule.CreateSchedule(schedule.UniqueID!.Value, newJob, server);

            // All exceptions still active
            var activeExceptions = data.Schedule.Exceptions ?? Enumerable.Empty<PlanException>();
            var activeExceptionDates = new HashSet<DateTime>(activeExceptions.Select(exception => exception.ExceptionDate));

            // Copy over all exceptions
            newSchedule.Exceptions.AddRange(schedule.Exceptions.Where(exception => activeExceptionDates.Contains(exception.When)));

            // See if we can use it
            if (!newSchedule.IsActive)
                throw new ArgumentException("Die Aufzeichnung liegt in der Vergangenheit");

            // Copy all schedules expect the one wie founr
            newJob.Schedules.AddRange(job.Schedules.Where(oldSchedule => !ReferenceEquals(oldSchedule, schedule)));

            // Add the updated variant
            newJob.Schedules.Add(newSchedule);

            // Send to persistence
            jobs.Update(newJob, newSchedule.UniqueID!.Value);

            states.BeginNewPlan();

            // Update recently used channels
            UserProfileSettings.AddRecentChannel(data.Job.Source);
            UserProfileSettings.AddRecentChannel(data.Schedule.Source);
        }

        /// <summary>
        /// Entfernt eine Aufzeichnung.
        /// </summary>
        /// <param name="detail">Die Referenz auf die Aufzeichnung.</param>
        [HttpDelete("recording/{detail}")]
        public void DeleteRecording(string detail)
        {
            // Parameter analysieren
            var schedule = jobs.ParseUniqueWebId(detail, out VCRJob job);

            // Validate
            if (schedule == null)
                throw new ArgumentException("Job or Schedule not found");

            // Remove schedule from job - since we are living in a separate application domain we only have a copy of it
            job.Schedules.Remove(schedule);

            // Send to persistence
            if (job.Schedules.Count < 1)
                jobs.Delete(job);
            else
                jobs.Update(job, null);

            states.BeginNewPlan();
        }

        /// <summary>
        /// Legt eine neue Aufzeichnung zu einem Auftrag an.
        /// </summary>
        /// <param name="detail">Die eindeutige Kennung des Auftrags.</param>
        /// <param name="data">Die Daten zum Auftrag und zur Aufzeichnung.</param>
        /// <returns>Die Identifikation des neuen Auftrags.</returns>
        [HttpPost("recording/{detail}")]
        public string CreateNewRecording(string detail, [FromBody] JobScheduleData data)
        {
            // Parameter analysieren
            jobs.ParseUniqueWebId(detail + Guid.NewGuid().ToString("N"), out VCRJob job);

            // Validate
            if (job == null)
                throw new ArgumentException("Job not found");

            // Take the new job data
            var newJob = data.Job.CreateJob(job.UniqueID!.Value, server);
            var newSchedule = data.Schedule.CreateSchedule(newJob, server);

            // See if we can use it
            if (!newSchedule.IsActive)
                throw new ArgumentException("Die Aufzeichnung liegt in der Vergangenheit");

            // Add all existing
            newJob.Schedules.AddRange(job.Schedules);

            // Add the new one
            newJob.Schedules.Add(newSchedule);

            // Send to persistence
            jobs.Update(newJob, newSchedule.UniqueID!.Value);

            states.BeginNewPlan();

            // Update recently used channels
            UserProfileSettings.AddRecentChannel(data.Job.Source);
            UserProfileSettings.AddRecentChannel(data.Schedule.Source);

            // Report
            return ServerTools.GetUniqueWebId(newJob, newSchedule);
        }
    }
}
