namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass die Sammlung der Programmzeitschrift nicht aktiv ist.
    /// </summary>
    [Serializable]
    public class EPGNotActiveFault : CardServerFault
    {
        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        public EPGNotActiveFault()
            : base("The electronic program guide (EPG) update is not active")
        {
        }
    }
}
