namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass kein Sendersuchlauf ausgeführt wird.
    /// </summary>
    [Serializable]
    public class SourceUpdateNotActiveFault : CardServerFault
    {
        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        public SourceUpdateNotActiveFault()
            : base("There is no source update (transponder scan) active")
        {
        }
    }
}
