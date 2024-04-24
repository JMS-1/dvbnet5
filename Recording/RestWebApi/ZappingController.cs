using Microsoft.AspNetCore.Mvc;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Vermittelt den Zugriff auf die im <i>VCR.NET Recording Service</i> verwalteten
    /// Geräte zur Fernnutzung.
    /// </summary>
    [ApiController]
    [Route("api/zapping")]
    public class ZappingController(VCRServer server) : ControllerBase
    {
        /// <summary>
        /// Ermittelt den aktuellen Zustand.
        /// </summary>
        /// <param name="detail">Der Name des zu verwendenden Geräteprofils.</param>
        /// <returns>Der Zustand auf dem gewählten Geräteprofil.</returns>
        [HttpGet("status/{detail}")]
        public ZappingStatus GetCurrentStatus(string detail)
            => server.LiveModeOperation(detail, true, null!, null!, ZappingStatus.Create);

        /// <summary>
        /// Ermittelt alle verfügbaren Sender.
        /// </summary>
        /// <param name="detail">Der Name des zu verwendenden Geräteprofils.</param>
        /// <param name="tv">Gesetzt, wenn Fernsehender berücksichtigt werden sollen.</param>
        /// <param name="radio">Gesetzt, wenn Radiosender berücksichtigt werden sollen.</param>
        /// <returns>Die gewünschte Liste von Sendern.</returns>
        [HttpGet("source/{detail}")]
        public ZappingSource[] FindSources(string detail, bool tv, bool radio)
            => server.GetSources(detail, tv, radio, ZappingSource.Create);

        /// <summary>
        /// Aktiviert eine neue Sitzung.
        /// </summary>
        /// <param name="detail">Der Name des zu verwendenden Geräteprofils.</param>
        /// <param name="target">Legt fest, wohin die Nutzdaten zu senden sind.</param>
        /// <returns>Der Zustand auf dem gewählten Geräteprofil.</returns>
        [HttpPost("live/{detail}")]
        public ZappingStatus Connect(string detail, string target)
            => server.LiveModeOperation(detail, true, target, null!, ZappingStatus.Create);

        /// <summary>
        /// Deaktiviert eine Sitzung.
        /// </summary>
        /// <param name="detail">Der Name des zu verwendenden Geräteprofils.</param>
        /// <returns>Der Zustand auf dem gewählten Geräteprofil.</returns>
        [HttpDelete("live/{detail}")]
        public ZappingStatus Disconnect(string detail)
            => server.LiveModeOperation(detail, false, null!, null!, ZappingStatus.Create);

        /// <summary>
        /// Wählt einen Quelle aus.
        /// </summary>
        /// <param name="detail">Der Name des zu verwendenden Geräteprofils.</param>        
        /// <param name="source">Die gewünschte Quelle als Tripel analog zur Textdarstellung von <see cref="SourceIdentifier"/>.</param>
        /// <returns>Der Zustand auf dem gewählten Geräteprofil.</returns>
        [HttpPut("tune/{detail}")]
        public ZappingStatus Tune(string detail, string source)
            => server.LiveModeOperation(detail, true, null!, SourceIdentifier.Parse(source), ZappingStatus.Create);
    }
}
