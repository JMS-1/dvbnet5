namespace JMS.DVB.EPG.Tables
{
    /// <summary>
    /// Beschreibt eine <i>Time Offset Table</i>, die neben der Uhrzeit im UTC / GMT
    /// Format auch noch Beschreibungen zur lokalen Zeitzone enth�lt.
    /// </summary>
    public class TOT : Table
    {
        /// <summary>
        /// Die Beschreiber zu dieser Tabelle.
        /// </summary>
        public Descriptor[] Descriptors { get; private set; } = null!;

        /// <summary>
        /// Datum und Uhrzeit zu dieser Tabelle.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Meldet, f�r welche Tabellenkennungen diese Klasse zust�ndig ist.
        /// </summary>
        /// <param name="tableIdentifier">Eine zu pr�fende Tabellenkennung.</param>
        /// <returns>Gesetzt, wenn diese Klasse die Tabelle analysieren kann.</returns>
        public static bool IsHandlerFor(byte tableIdentifier)
        {
            // Check all
            return (0x73 == tableIdentifier);
        }

        /// <summary>
        /// Erzeugt eine neue Tabelle.
        /// </summary>
        /// <param name="section">Der Bereich, in den die Tabelle eingebettet ist.</param>
        public TOT(Section section)
            : base(section)
        {
            // Load length
            int length = section.Length - 7;

            // Check for minimum length required
            if (length < 7)
                return;

            // Load the time
            Time = Tools.DecodeTime(section, 0);

            // Correct
            length -= 7;

            // Load the length of the descriptors
            int deslen = Tools.MergeBytesToWord(section[6], section[5]) & 0x0fff;

            // Validate
            if (deslen > length)
                return;

            // Create my descriptors
            Descriptors = Descriptor.Load(this, 7, deslen);

            // Done
            m_IsValid = (deslen == length);
        }
    }
}
