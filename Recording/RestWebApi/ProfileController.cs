using System.Globalization;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;
using Microsoft.AspNetCore.Mvc;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Erlaubt den Zurgiff auf die Geräteprofile, die der <i>VCR.NET Recording Service</i>
    /// verwaltet.
    /// </summary>
    [ApiController]
    [Route("api/profile")]
    public class ProfileController(IVCRServer states, IVCRProfiles profiles, IJobManager jobs) : ControllerBase
    {
        /// <summary>
        /// Meldet alle Geräteprofile, die der <i>VCR.NET Recording Service</i> verwenden darf.
        /// </summary>
        /// <returns>Die Liste der Geräteprofile.</returns>
        [HttpGet("profiles")]
        public ProfileInfo[] ListProfiles()
        {
            // Forward
            return
                [.. states
                    .GetProfiles(ProfileInfo.Create)
                    .OrderBy(profile => profile.Name, ProfileManager.ProfileNameComparer)];
        }

        /// <summary>
        /// Ermittelt alle verfügbaren Sender.
        /// </summary>
        /// <param name="profile">Der Name des zu verwendenden Geräteprofils.</param>
        /// <returns>Die gewünschte Liste von Sendern.</returns>
        [HttpGet("sources/{profile}")]
        public ProfileSource[] FindSources(string profile) => states.GetSources(profile, true, true, ProfileSource.Create, profiles);

        /// <summary>
        /// Verändert den Endzeitpunkt.
        /// </summary>
        /// <param name="profile">Der Name eines Geräteprofils.</param>
        /// <param name="disableHibernate">Gesetzt, wenn der Übergang in den Schlafzustand vermieden werden soll.</param>
        /// <param name="schedule">Die Identifikation einer laufenden Aufzeichnung.</param>
        /// <param name="endTime">Der neue Endzeitpunkt.</param>
        [HttpPut("endtime/{profile}")]
        public void SetNewEndTime(string profile, bool disableHibernate, string schedule, string endTime)
            => states
                .FindProfile(profile)?
                .ChangeStreamEnd(
                    new Guid(schedule),
                    DateTime.Parse(endTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    disableHibernate && (states.NumberOfActiveRecordings == 1)
                );

        /// <summary>
        /// Ermittelt alle aktiven Aufträge eines Geräteprofils.
        /// </summary>
        /// <param name="profile">Der Name des Geräteprofils.</param>
        /// <returns>Die Liste der Aufträge.</returns>
        [HttpGet("jobs/{profile}")]
        public ProfileJobInfo[] GetActiveJobs(string profile)
        {
            // Request jobs
            return
                [.. jobs
                    .GetJobs(ProfileJobInfo.Create, profiles)
                    .Where(job => job != null && ProfileManager.ProfileNameComparer.Equals(job.Profile, profile))
                    .Cast<ProfileJobInfo>()
                    .OrderBy(job => job.Name, StringComparer.InvariantCultureIgnoreCase)];
        }
    }
}
