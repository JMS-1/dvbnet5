namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass bereits ein Sendersuchlauf ausgeführt wird.
    /// </summary>
    [Serializable]
    public class SourceUpdateActiveFault : CardServerFault
    {
        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        public SourceUpdateActiveFault()
            : base("A source update (transponder scan) is already running")
        {
        }
    }
}
