using System.Globalization;
using JMS.DVB.NET.Recording.Services.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Erlaubt den Zurgiff auf die Geräteprofile, die der <i>VCR.NET Recording Service</i>
    /// verwaltet.
    /// </summary>
    [ApiController]
    [Route("api/profile")]
    public class ProfileController(VCRServer server, IVCRProfiles profiles) : ControllerBase
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
                server
                    .GetProfiles(ProfileInfo.Create)
                    .OrderBy(profile => profile.Name, ProfileManager.ProfileNameComparer)
                    .ToArray();
        }

        /// <summary>
        /// Ermittelt alle verfügbaren Sender.
        /// </summary>
        /// <param name="profile">Der Name des zu verwendenden Geräteprofils.</param>
        /// <returns>Die gewünschte Liste von Sendern.</returns>
        [HttpGet("sources/{profile}")]
        public ProfileSource[] FindSources(string profile) => server.GetSources(profile, true, true, ProfileSource.Create);

        /// <summary>
        /// Verändert den Endzeitpunkt.
        /// </summary>
        /// <param name="profile">Der Name eines Geräteprofils.</param>
        /// <param name="disableHibernate">Gesetzt, wenn der Übergang in den Schlafzustand vermieden werden soll.</param>
        /// <param name="schedule">Die Identifikation einer laufenden Aufzeichnung.</param>
        /// <param name="endTime">Der neue Endzeitpunkt.</param>
        [HttpPut("endtime/{profile}")]
        public void SetNewEndTime(string profile, bool disableHibernate, string schedule, string endTime)
        {
            // Map parameters to native types
            var scheduleIdentifier = new Guid(schedule);
            var end = DateTime.Parse(endTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            // Forward
            server.ChangeRecordingStreamEndTime(profile, scheduleIdentifier, end, disableHibernate);
        }

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
                server
                    .GetJobs(ProfileJobInfo.Create, profiles)
                    .Where(job => job != null && ProfileManager.ProfileNameComparer.Equals(job.Profile, profile))
                    .Cast<ProfileJobInfo>()
                    .OrderBy(job => job.Name, StringComparer.InvariantCultureIgnoreCase)
                    .ToArray();
        }
    }
}
