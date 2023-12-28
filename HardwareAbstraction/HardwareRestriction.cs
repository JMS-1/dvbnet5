namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt mögliche Einschränkungen und Eigenschaften eines DVB.NET Gerätes.
    /// </summary>
    public class HardwareRestriction
    {
        /// <summary>
        /// Die maximale Anzahl von Verbrauchern, die dieses Gerät anbieten kann.
        /// </summary>
        public int? ConsumerLimit { get; set; }

        /// <summary>
        /// Meldet oder legt fest, ob das Gerät nutzbare Informationen zum empfangenen
        /// Signal geben kann.
        /// </summary>
        public bool ProvidesSignalInformation { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public HardwareRestriction()
        {
        }
    }
}
