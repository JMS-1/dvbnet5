using System.Globalization;
using JMS.DVB.NET.Recording.Actions;
using JMS.DVB.NET.Recording.Server;
using Microsoft.AspNetCore.Mvc;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Erlaubt den Zugriff auf den Aufzeichnungsplan.
    /// </summary>
    [ApiController]
    [Route("api/plan")]
    public class PlanController(IVCRServer server, IRecordings recordings) : ControllerBase
    {
        /// <summary>
        /// Meldet den aktuellen Aufzeichnungsplan.
        /// </summary>
        /// <param name="limit">Die maximale Anzahl von Einträgen im Ergebnis.</param>
        /// <param name="end">Es werden nur Aufzeichnungen betrachtet, die vor diesem Zeitpunkt beginnen.</param>
        /// <returns>Alle Einträge des Aufzeichnungsplans.</returns>
        [HttpGet]
        public PlanActivity[] GetPlan(string? limit, string? end)
        {
            // Get the limit
            if (!int.TryParse(limit, out int maximum) || (maximum <= 0))
                maximum = 1000;

            // Get the date
            if (!DateTime.TryParse(end, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime endTime))
                endTime = DateTime.MaxValue;

            // Route from Web AppDomain into service AppDomain
            var activities = server.GetPlan(endTime, maximum, PlanActivity.Create);

            // Must resort to correct for running entries
            Array.Sort(activities, PlanActivity.ByStartComparer);

            // Report
            return activities;
        }

        /// <summary>
        /// Meldet den aktuellen Aufzeichnungsplan der nächsten 4 Wochen für mobile Geräte, wobei Aufgaben ausgeschlossen sind.
        /// </summary>
        /// <param name="limit">Die maximale Anzahl von Einträgen im Ergebnis.</param>
        /// <param name="mobile">Schalter zum Umschalten auf die Liste für mobile Geräte.</param>
        /// <returns>Alle Einträge des Aufzeichnungsplans.</returns>
        [HttpGet("mobile")]
        public PlanActivityMobile[] GetPlanMobile(string limit, string mobile)
        {
            // Get the limit
            if (!int.TryParse(limit, out int maximum) || (maximum <= 0))
                maximum = 1000;

            // Use helper
            return
                GetPlan(null!, DateTime.UtcNow.AddDays(28).ToString("o"))
                    .Where(plan => !string.IsNullOrEmpty(plan.Source))
                    .Take(maximum)
                    .Select(PlanActivityMobile.Create)
                    .ToArray();
        }

        /// <summary>
        /// Ermittelt Informationen zu allen Geräteprofilen.
        /// </summary>
        /// <returns>Die gewünschte Liste.</returns>
        [HttpGet("current")]
        public PlanCurrent[] GetCurrent()
        {
            // Forward
            return
                recordings
                    .GetCurrent(PlanCurrent.Create, PlanCurrent.Create, PlanCurrent.Create)
                    .OrderBy(current => current.StartTime)
                    .ThenBy(current => current.Duration)
                    .ToArray();
        }

        /// <summary>
        /// Ermittelt Informationen zu allen Geräteprofilen.
        /// </summary>
        /// <returns>Die gewünschte Liste.</returns>
        [HttpGet("current/mobile")]
        public PlanCurrentMobile[] GetCurrentMobile()
        {
            // Forward
            return
                recordings
                    .GetCurrent(PlanCurrent.Create)
                    .Where(current => !string.IsNullOrEmpty(current.SourceName))
                    .Select(PlanCurrentMobile.Create)
                    .OrderBy(current => current.StartTime)
                    .ThenBy(current => current.Duration)
                    .ToArray();
        }

        /// <summary>
        /// Fordert die Aktualisierung der Quellen an.
        /// </summary>
        [HttpPost("scan")]
        public void StartSourceScan() => server.ForceSoureListUpdate();

        /// <summary>
        /// Fordert die Aktualisierung der Programmzeitschrift an.
        /// </summary>
        [HttpPost("guide")]
        public void StartGuideUpdate() => server.ForceProgramGuideUpdate();

        /// <summary>
        /// Ändert den Netzwerkversand.
        /// </summary>
        /// <param name="profile">Der Name eines Geräteprofils.</param>
        /// <param name="source">Eine Quelle.</param>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung einer Aufzeichnung.</param>
        /// <param name="target">Das neue Ziel des Netzwerkversands.</param>
        [HttpPost("target/{profile}")]
        public void SetStreamTarget(string profile, string source, Guid scheduleIdentifier, string target)
            => server.FindProfile(profile)?.SetStreamTarget(SourceIdentifier.Parse(source), scheduleIdentifier, target);
    }
}
