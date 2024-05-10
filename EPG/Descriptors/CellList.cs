namespace JMS.DVB.EPG.Descriptors
{
    /// <summary>
    /// Enth�lt eine Liste von DVB-T Zelleninformationen.
    /// </summary>
    public class CellList : Descriptor
    {
        /// <summary>
        /// Eine Liste mit DVB-T Zellinformationen.
        /// </summary>
        public readonly List<CellInformation> Cells = [];

        /// <summary>
        /// Erzeugt eine neue Liste.
        /// </summary>
        /// <param name="container">Der SI Bereich, in dem diese Liste gefunden wurde.</param>
        /// <param name="offset">Der Index des ersten Bytes dieser Liste in den Rohdaten des SI Bereichs.</param>
        /// <param name="length">Die Anzahl der Bytes für diese Liste.</param>
        public CellList(IDescriptorContainer container, int offset, int length)
            : base(container, offset, length)
        {
            // Check minimum length
            if (length < 0)
                return;

            // Attach to data
            Section section = container.Section;

            // Helper
            List<CellInformation> cells = [];

            // Load
            while (length > 0)
            {
                // Create
                var cell = CellInformation.Create(section, offset, length);

                // Done
                if (null == cell)
                    break;

                // Remember
                cells.Add(cell);

                // Correct 
                offset += cell.Length;
                length -= cell.Length;
            }

            // Test
            m_Valid = 0 == length;

            // Load
            if (m_Valid)
                Cells = cells;
        }

        /// <summary>
        /// Pr�ft, ob diese Klasse für eine bestimmte Art von SI Beschreibungen zust�ndig ist.
        /// </summary>
        /// <param name="tag">Die eindeutige Kennung einer SI Beschreibung.</param>
        /// <returns>Gesetzt, wenn diese Klasse für die angegebene Art von Beschreibung zurst�ndig ist.</returns>
        public static bool IsHandlerFor(byte tag)
        {
            // Check it
            return DescriptorTags.CellList == (DescriptorTags)tag;
        }
    }
}
