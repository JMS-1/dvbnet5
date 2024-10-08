﻿namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass die Sammlung der Programmzeitschrift aktiv ist.
    /// </summary>
    [Serializable]
    public class EPGActiveFault : CardServerFault
    {
        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        public EPGActiveFault()
            : base("The electronic program guide (EPG) update is active")
        {
        }
    }
}
