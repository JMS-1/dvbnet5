using System.Text;


namespace JMS.DVB.EPG
{
    /// <summary>
    /// Erlaubt die Rekonstruktion von <i>SI</i> Tabellen.
    /// </summary>
    public class TableConstructor
    {
        /// <summary>
        /// Die Standard-Codierung für Zeichenketten.
        /// </summary>
        private static readonly Encoding ANSI = Encoding.GetEncoding(1252);

        /// <summary>
        /// Ein Zwischenspeicher.
        /// </summary>
        private List<byte> m_Buffer = new List<byte>(184);

        /// <summary>
        /// Erzeugt einen Eintrag für die L�nge.
        /// </summary>
        /// <returns>Die Position des Eintrags.</returns>
        public int CreateDynamicLength()
        {
            // Create
            m_Buffer.Add(0);

            // Report
            return m_Buffer.Count - 1;
        }

        /// <summary>
        /// Legt die L�nge fest.
        /// </summary>
        /// <param name="dynamicLengthPosition">Der Anfang eines Rohdatenblocks.</param>
        public void SetDynamicLength(int dynamicLengthPosition)
        {
            // Store
            m_Buffer[dynamicLengthPosition] = (byte)(m_Buffer.Count - dynamicLengthPosition - 1);
        }

        /// <summary>
        /// Erg�nzt eine Sprache.
        /// </summary>
        /// <param name="isoLanguage">Der <i>ISO</i> Name der Sprache.</param>
        public void AddLanguage(string isoLanguage)
        {
            // Correct
            if (string.IsNullOrEmpty(isoLanguage))
                isoLanguage = "deu";
            else if (isoLanguage.Length < 3)
                isoLanguage = isoLanguage + new String(' ', 3 - isoLanguage.Length);
            else if (isoLanguage.Length > 3)
                isoLanguage = isoLanguage.Substring(0, 3);

            // Forward
            Add(ANSI.GetBytes(isoLanguage));
        }

        /// <summary>
        /// Erg�nzt ein Byte.
        /// </summary>
        /// <param name="value">Das gewünschte Byte.</param>
        public void Add(byte value) => m_Buffer.Add(value);

        /// <summary>
        /// Erg�nzt eine Zahl.
        /// </summary>
        /// <param name="value">Die gewünschte Zahl.</param>
        public void Add(ushort value) =>
            Add((byte)((value >> 8) & 0xff), (byte)(value & 0xff));

        /// <summary>
        /// Erg�nzt Rohdaten.
        /// </summary>
        /// <param name="bytes">Die Liste der Daten.</param>
        public void Add(params byte[] bytes)
        {
            // Remember
            if (bytes != null)
                m_Buffer.AddRange(bytes);
        }

        /// <summary>
        /// Liefert die gesammelten Rohdaten.
        /// </summary>
        /// <returns>Die rekonstruierte Tabelle.</returns>
        public byte[] ToArray() => m_Buffer.ToArray();

        /// <summary>
        /// Erg�nzt eine Detailbeschreibung.
        /// </summary>
        /// <param name="descriptor">Die zu erg�nzende Beschreibung.</param>
        public void Add(Descriptor descriptor) => descriptor.CreateDescriptor(this);
    }
}
