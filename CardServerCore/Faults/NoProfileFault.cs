namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass ein Geräteprofil nicht gefunden wurde.
    /// </summary>
    [Serializable]
    public class NoProfileFault : CardServerFault
    {
        /// <summary>
        /// Der Name des bereits nich gefundenen Profils.
        /// </summary>
        public string ProfileName { get; set; } = null!;

        /// <summary>
        /// Wird für die XML Serialisierung benötigt.
        /// </summary>
        public NoProfileFault()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        /// <param name="profileName">Der Name des gewünschten Geräteprofils.</param>
        public NoProfileFault(string profileName)
            : base(string.Format("There is no device profile '{0}'", profileName))
        {
            // Remember
            ProfileName = profileName;
        }
    }
}
