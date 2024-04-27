namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// Eine Liste von Diensten.
    /// </summary>
    public class ServiceList : Descriptor
    {
        /// <summary>
        /// Alle Dienste aufgeschl�sselt nach der Art des Dienstes.
        /// </summary>
        public readonly Dictionary<ushort, ServiceTypes> Services = [];

        /// <summary>
        /// Erzeugt eine neue Liste.
        /// </summary>
        /// <param name="container">Alle Beschreibungen.</param>
        /// <param name="offset">Das erste Byte in den Rohdaten, dass zu dieser Beschreibung geh�rt.</param>
        /// <param name="length">Die Gr��e der Rohdaten zu dieser Beschreibung.</param>
        public ServiceList(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
        {
            // Attach to section
            Section section = container.Section;

            // Process all
            while (length > 0)
            {
                // Not possible
                if (length < 3) return;

                // Load key
                ushort serviceIdentifier = Tools.MergeBytesToWord(section[offset + 1], section[offset + 0]);

                // Remember
                Services[serviceIdentifier] = (ServiceTypes)section[offset + 2];

                // Advance
                offset += 3;
                length -= 3;
            }

            // We are valid
            m_Valid = true;
        }

        /// <summary>
        /// Pr�ft, ob diese Liste zu einer Beschreibungskennung geh�rt.
        /// </summary>
        /// <param name="tag">Die beobachtete Kennung.</param>
        /// <returns>Gesetzt, wenn es sich um eine Liste von Diensten handelt.</returns>
        public static bool IsHandlerFor(byte tag)
        {
            // Check it
            return (DescriptorTags.SeviceList == (DescriptorTags)tag);
        }
    }
}
