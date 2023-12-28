namespace JMS.DVB.TS
{
    /// <summary>
    /// Repr�sentiert einen Rohdatenstrom mit echten DVB Untertiteln.
    /// </summary>
    /// <param name="consumer">Der zugeh�rige Gesamtdatenstrom, im Allgemeinen ein
    /// <i>Transport Stream</i> <see cref="Manager"/>.</param>
    /// <param name="pid">Die Datenstromkennung im Gesamtstrom.</param>
    /// <param name="isPCR">Gesetzt, wenn dieser Datenstrom die Zeitbasis f�r den Gesamtstrom
    /// bereitstellt (sehr un�blich f�r Untertitel).</param>
    public class SubtitleStream(IStreamConsumer consumer, short pid, bool isPCR) : StreamBase(consumer, pid, isPCR)
    {

        /// <summary>
        /// Pr�ft, ob ein Zeichen ein legaler MPEG-2 Startcode f�r DVB Untertitelstr�me
        /// ist.
        /// </summary>
        /// <param name="code">Der zu pr�fende Code.</param>
        /// <returns>Gesetzt, wenn es sich um einen legalen Startcode handelt.</returns>
        protected override bool IsValidStartCode(byte code) => 0xbd == code;

        /// <summary>
        /// Nimmt Daten entgegen aber verz�gert die �bernahme in den Gesamtdatenstrom bis
        /// ein PCR vorliegt.
        /// </summary>
        /// <param name="buffer">Speicher mit den Nutzdaten.</param>
        /// <param name="start">Das erste zu verwendende Byte in den Nutzdaten.</param>
        /// <param name="length">Die Anzahl der Bytes in den Nutzdaten.</param>
        public override void AddPayload(byte[] buffer, int start, int length)
        {
            // Forward to base if PCR is sent to the stream
            if (Consumer.PCRAvailable) base.AddPayload(buffer, start, length);
        }
    }
}
