﻿namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass ein <i>Card Server</i> keine neuen Aufgaben annehmen kann, da er gerade beschäftigt 
    /// ist.
    /// </summary>
    [Serializable]
    public class ServerBusyFault : CardServerFault
    {
        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        public ServerBusyFault()
            : base("Please wait for the previous command to finish")
        {
        }
    }
}
