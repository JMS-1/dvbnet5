namespace JMS.DVB.TS.TSBuilders
{
    /// <summary>
    /// Basisklasse für Rekonstruktionsalgorithmen auf einem <i>Transport Stream</i>.
    /// </summary>
    /// <param name="parser">Die zugeh�rige Analyseeinheit.</param>
    /// <param name="callback">Optional ein Verbraucher für rekonstruierte Pakete.</param>
    public abstract class TSBuilder(TSParser parser, Action<byte[]> callback) : IDisposable
    {
        /// <summary>
        /// Vermerkt die Gr��e des kleinsten an den Verbraucher weitergereichten Paketes.
        /// </summary>
        private int m_MinPacket = int.MaxValue;

        /// <summary>
        /// Vermerkt die Gr��e des gr��ten an den Verbraucher weitergereichten Paketes.
        /// </summary>
        private int m_MaxPacket = int.MinValue;

        /// <summary>
        /// Ein optionaler Verbraucher für rekonstruierte Pakete.
        /// </summary>
        private readonly Action<byte[]> m_Callback = callback;

        /// <summary>
        /// Meldet die Gr��e des kleinsten an den Verbraucher gemeldeten Paketes.
        /// </summary>
        public int MinimumPacketSize { get { return (m_MinPacket == int.MaxValue) ? 0 : m_MinPacket; } }

        /// <summary>
        /// Meldet die Gr��e des gr��ten an den Verbraucher gemeldeten Paketes.
        /// </summary>
        public int MaximumPacketSize { get { return (m_MaxPacket == int.MinValue) ? 0 : m_MaxPacket; } }

        /// <summary>
        /// Meldet die Anzahl der an den Verbraucher durchgereichten Pakete.
        /// </summary>
        public long PacketCount { get; private set; }

        /// <summary>
        /// Meldet die gesamte Anzahl der an den Verbraucher durchgereichten Bytes.
        /// </summary>
        public long TotalBytes { get; private set; }

        /// <summary>
        /// Meldet die zugeh�rige Analyseeinheit.
        /// </summary>
        protected TSParser Parser { get; private set; } = parser;

        /// <summary>
        /// �bertr�gt ein elementares Paket von der Analyseeinheit zur Rekonstruktion.
        /// </summary>
        /// <param name="packet">Ein Zwischenspeicher mit den Paketdaten.</param>
        /// <param name="offset">Die Position des ersten relevanten Bytes im Zwischenspeicher.</param>
        /// <param name="length">Die Anzahl der relevanten Bytes im Zwischenspeicher.</param>
        /// <param name="noincrement">Gesetzt, wenn der Paketz�hler nicht erh�ht werden darf.</param>
        /// <param name="first">Gesetzt, wenn das elementare Paket als Start einer Sequenz von Paketen gekennzeichnet ist.</param>
        /// <param name="counter">Der aktuelle Paketz�hler.</param>
        public abstract void AddPacket(byte[] packet, int offset, int length, bool noincrement, bool first, byte counter);

        /// <summary>
        /// Fordert zum Zur�cksetzen aller Zwischenergebnisse auf.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Sendet ein rekonstruiertes Paket an den zugeordneten Verbraucher.
        /// </summary>
        /// <param name="buffer">Das rekonstruierte Paket.</param>
        protected void Process(byte[] buffer)
        {
            // Counter
            TotalBytes += buffer.Length;
            PacketCount += 1;

            // Bounds
            if (buffer.Length < m_MinPacket)
                m_MinPacket = buffer.Length;
            if (buffer.Length > m_MaxPacket)
                m_MaxPacket = buffer.Length;

            // Forward
            m_Callback(buffer);
        }

        /// <summary>
        /// Sendet ein rekonstuiertes Paket an den zugeordneten Verbraucher.
        /// </summary>
        /// <param name="buffer">Ein Gesamtspeicherbereich mit den Paketdaten.</param>
        /// <param name="start">Position des ersten Bytes des Paketes im Gesamtspeicher.</param>
        /// <param name="length">Anzahl der Bytes im Paket.</param>
        protected void Process(byte[] buffer, int start, int length)
        {
            // Allocate new
            var data = new byte[length];

            // Copy
            Array.Copy(buffer, start, data, 0, data.Length);

            // Use
            Process(data);
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Rekonstruktionsinstanz endg�ltig.
        /// </summary>
        public virtual void Dispose()
        {
        }

        #endregion
    }
}
