namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass ein Geräteprofil keine eigene Liste von Quellen verwaltet.
    /// </summary>
    [Serializable]
    public class NoSourceListFault : CardServerFault
    {
        /// <summary>
        /// Der Name des bereits nich gefundenen Profils.
        /// </summary>
        public string ProfileName { get; set; } = null!;

        /// <summary>
        /// Wird für die XML Serialisierung benötigt.
        /// </summary>
        public NoSourceListFault()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        /// <param name="profileName">Der Name des gewünschten Geräteprofils.</param>
        public NoSourceListFault(string profileName)
            : base(string.Format("Profile '{0}' does not contain a source list", profileName))
        {
            // Remember
            ProfileName = profileName;
        }
    }
}
