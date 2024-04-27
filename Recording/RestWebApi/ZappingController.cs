using JMS.DVB.CardServer;
using JMS.DVB.NET.Recording.Server;
using JMS.DVB.NET.Recording.Services.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Vermittelt den Zugriff auf die im <i>VCR.NET Recording Service</i> verwalteten
    /// Geräte zur Fernnutzung.
    /// </summary>
    [ApiController]
    [Route("api/zapping")]
    public class ZappingController(IVCRServer server, IVCRProfiles profiles) : ControllerBase
    {
        /// <summary>
        /// Steuert den Zapping Modus.
        /// </summary>
        /// <typeparam name="TStatus">Die Art der Zustandsinformation.</typeparam>
        /// <param name="profile">Das betroffene DVB.NET Geräteprofil.</param>
        /// <param name="active">Gesetzt, wenn der Zapping Modus aktiviert werden soll.</param>
        /// <param name="connectTo">Die TCP/IP UDP Adresse, an die alle Daten geschickt werden sollen.</param>
        /// <param name="source">Die zu aktivierende Quelle.</param>
        /// <param name="factory">Methode zum Erstellen einer neuen Zustandsinformation.</param>
        /// <returns>Der aktuelle Zustand des Zapping Modus oder <i>null</i>, wenn dieser nicht ermittelt
        /// werden konnte.</returns>
        private TStatus LiveModeOperation<TStatus>(string profile, bool active, string connectTo, SourceIdentifier source, Func<string, ServerInformation, TStatus> factory)
        {
            // Attach to the profile and process
            var state = server.FindProfile(profile);
            if (state == null)
                return default!;
            else
                return state.LiveModeOperation(active, connectTo, source, factory);
        }

        /// <summary>
        /// Ermittelt den aktuellen Zustand.
        /// </summary>
        /// <param name="profile">Der Name des zu verwendenden Geräteprofils.</param>
        /// <returns>Der Zustand auf dem gewählten Geräteprofil.</returns>
        [HttpGet("status/{profile}")]
        public ZappingStatus GetCurrentStatus(string profile)
            => LiveModeOperation(profile, true, null!, null!, ZappingStatus.Create);

        /// <summary>
        /// Ermittelt alle verfügbaren Sender.
        /// </summary>
        /// <param name="profile">Der Name des zu verwendenden Geräteprofils.</param>
        /// <param name="tv">Gesetzt, wenn Fernsehender berücksichtigt werden sollen.</param>
        /// <param name="radio">Gesetzt, wenn Radiosender berücksichtigt werden sollen.</param>
        /// <returns>Die gewünschte Liste von Sendern.</returns>
        [HttpGet("source/{profile}")]
        public ZappingSource[] FindSources(string profile, bool tv, bool radio)
            => server.GetSources(profile, tv, radio, ZappingSource.Create, profiles);

        /// <summary>
        /// Aktiviert eine neue Sitzung.
        /// </summary>
        /// <param name="profile">Der Name des zu verwendenden Geräteprofils.</param>
        /// <param name="target">Legt fest, wohin die Nutzdaten zu senden sind.</param>
        /// <returns>Der Zustand auf dem gewählten Geräteprofil.</returns>
        [HttpPost("live/{profile}")]
        public ZappingStatus Connect(string profile, string target)
            => LiveModeOperation(profile, true, target, null!, ZappingStatus.Create);

        /// <summary>
        /// Deaktiviert eine Sitzung.
        /// </summary>
        /// <param name="profile">Der Name des zu verwendenden Geräteprofils.</param>
        /// <returns>Der Zustand auf dem gewählten Geräteprofil.</returns>
        [HttpDelete("live/{profile}")]
        public ZappingStatus Disconnect(string profile)
            => LiveModeOperation(profile, false, null!, null!, ZappingStatus.Create);

        /// <summary>
        /// Wählt einen Quelle aus.
        /// </summary>
        /// <param name="profile">Der Name des zu verwendenden Geräteprofils.</param>        
        /// <param name="source">Die gewünschte Quelle als Tripel analog zur Textdarstellung von <see cref="SourceIdentifier"/>.</param>
        /// <returns>Der Zustand auf dem gewählten Geräteprofil.</returns>
        [HttpPut("tune/{profile}")]
        public ZappingStatus Tune(string profile, string source)
            => LiveModeOperation(profile, true, null!, SourceIdentifier.Parse(source), ZappingStatus.Create);
    }
}
