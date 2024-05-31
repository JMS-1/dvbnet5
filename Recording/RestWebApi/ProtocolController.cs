using System.Globalization;
using JMS.DVB.NET.Recording.Actions;
using Microsoft.AspNetCore.Mvc;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Dieser Webdienst erlaubt den Zugriff auf Protokolldaten.
    /// </summary>
    [ApiController]
    [Route("api/protocol")]
    public class ProtocolController(ILogQuery logs) : ControllerBase
    {
        /// <summary>
        /// Ermittelt einen Auszug aus dem Protokoll eines Gerätes.
        /// </summary>
        /// <param name="profile">Das gewünschte Gerät.</param>
        /// <param name="start">Der Startzeitpunkt in ISO Notation.</param>
        /// <param name="end">Der Endzeitpunkt in ISO Notation.</param>
        /// <returns>Die gewünschte Liste.</returns>
        [HttpGet("{profile}")]
        public ProtocolEntry[] Query(string profile, string start, string end)
        {
            // Decode
            var startTime = DateTime.Parse(start, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var endTime = DateTime.Parse(end, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            // Forward
            return logs.Query(profile, startTime.Date, endTime.Date);
        }
    }
}
