namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Beschreibt einen NVOD Dienst.
    /// </summary>
    [Serializable]
    public class ServiceInformation
    {
        /// <summary>
        /// Die eindeutige DVB Kennung des Dienstes.
        /// </summary>
        public SourceIdentifier Service { get; set; } = null!;

        /// <summary>
        /// Der Name des Dienstes mit weiteren Ordnungsinformationen.
        /// </summary>
        public string UniqueName { get; set; } = null!;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public ServiceInformation()
        {
        }
    }

}
