namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Wird ausgelöst, wenn eine Quelle aktiviert werden soll, die nicht zum aktiven Geräteprofil gehört.
    /// </summary>
    [Serializable]
    public class ProfileMismatchFault : CardServerFault
    {
        /// <summary>
        /// Der Name des bereits zugeordneten Geräteprofils.
        /// </summary>
        public string ProfileInUse { get; set; } = null!;

        /// <summary>
        /// Der Name des angeforderten Geräteprofils.
        /// </summary>
        public string ProfileRequested { get; set; } = null!;

        /// <summary>
        /// Wird für die XML Serialisierung benötigt.
        /// </summary>
        public ProfileMismatchFault()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        /// <param name="inUseName">Der Name des bereits zugeordneten Geräteprofils.</param>
        /// <param name="requestName">Der Name des gewünschten Geräteprofils.</param>
        public ProfileMismatchFault(string inUseName, string requestName)
            : base(string.Format("This instance is bound to the device profile '{0}' and not '{1}'", inUseName, requestName))
        {
            // Remember
            ProfileInUse = inUseName;
            ProfileRequested = requestName;
        }
    }
}
