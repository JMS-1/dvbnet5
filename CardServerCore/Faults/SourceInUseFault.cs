namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Wird ausgelöst, wenn eine Quelle mehrfach aktiviert werden soll.
    /// </summary>
    [Serializable]
    public class SourceInUseFault : CardServerFault
    {
        /// <summary>
        /// Die betroffene Quelle.
        /// </summary>
        public SourceIdentifier Source { get; set; } = null!;

        /// <summary>
        /// Wird für die XML Serialisierung benötigt.
        /// </summary>
        public SourceInUseFault()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        /// <param name="source">Die doppelt verwendete Quelle.</param>
        public SourceInUseFault(SourceIdentifier source)
            : base(string.Format("Unable to activate source '{0}' twice", SourceIdentifier.ToString(source)))
        {
            // Remember
            Source = source;
        }
    }
}
