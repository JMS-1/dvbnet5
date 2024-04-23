using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Dieser Webdienst erlaubt den Zugriff auf Protokolldaten.
    /// </summary>
    [ApiController]
    [Route("api/protocol")]
    public class ProtocolController(VCRServer server) : ControllerBase
    {
        /// <summary>
        /// Ermittelt einen Auszug aus dem Protokoll eines Gerätes.
        /// </summary>
        /// <param name="detail">Das gewünschte Gerät.</param>
        /// <param name="start">Der Startzeitpunkt in ISO Notation.</param>
        /// <param name="end">Der Endzeitpunkt in ISO Notation.</param>
        /// <returns>Die gewünschte Liste.</returns>
        [HttpGet]
        public ProtocolEntry[] Query(string detail, string start, string end)
        {
            // Decode
            var startTime = DateTime.Parse(start, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var endTime = DateTime.Parse(end, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            // Forward
            return server.QueryLog(detail, startTime.Date, endTime.Date, ProtocolEntry.Create);
        }
    }
}
