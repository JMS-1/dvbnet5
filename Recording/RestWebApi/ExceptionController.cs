﻿using JMS.DVB.NET.Recording.Actions;
using Microsoft.AspNetCore.Mvc;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Ändert Ausnahmeregelungen.
    /// </summary>
    [ApiController]
    [Route("api/exception")]
    public class ExceptionController(IChangeExceptions changeExceptions) : ControllerBase
    {
        /// <summary>
        /// Verändert eine Ausnahme.
        /// </summary>
        /// <param name="detail">Die betroffene Aufzeichnung.</param>
        /// <param name="when">Der Referenztag.</param>
        /// <param name="startDelta">Die Verschiebung des Aufzeichnungsbeginns in Minuten.</param>
        /// <param name="durationDelta">Die Verschiebung der Aufzeichnungsdauer in Minuten.</param>
        [HttpPut("{detail}")]
        public void ChangeException(string detail, string when, int startDelta, int durationDelta)
        {
            // Parse the date
            var date = new DateTime(long.Parse(when), DateTimeKind.Utc);
            ServerTools.ParseUniqueWebId(detail, out Guid jobIdentifier, out Guid scheduleIdentifier);

            // Forward
            changeExceptions.Update(jobIdentifier, scheduleIdentifier, date, startDelta, durationDelta);
        }
    }
}
