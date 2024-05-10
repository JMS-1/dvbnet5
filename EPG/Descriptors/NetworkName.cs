

namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// Der Name eines Empfangsnetzwerks.
    /// </summary>
    public class NetworkName : Descriptor
    {
        /// <summary>
        /// Der tats�chlich Name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Erstellt eine Beschreibung.
        /// </summary>
        /// <param name="container">Alle zusammengh�rigen Beschreibungen.</param>
        /// <param name="offset">Das erste Byte dieser Beschreibung in den Rohdaten.</param>
        /// <param name="length">Die Gr��e dieser Beschreibung im Rohformat.</param>
        public NetworkName(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
        {
            // Load the string
            Name = container.Section.ReadEncodedString(offset, length);

            // We are valid
            m_Valid = true;
        }

        /// <summary>
        /// Pr�ft, ob eine bestimmte Beschreibungskennung einen Netzwerknamen beschreibt.
        /// </summary>
        /// <param name="tag">Die zu untersuchende Kennung.</param>
        /// <returns>Gesetzt, wenn die Kennung zu einem Netzwerknamen geh�rt.</returns>
        public static bool IsHandlerFor(byte tag)
        {
            // Check it
            return DescriptorTags.NetworkName == (DescriptorTags)tag;
        }
    }
}
