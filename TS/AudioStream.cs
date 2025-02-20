namespace JMS.DVB.TS
{
	/// <summary>
	/// Represents a single MP2 audio stream.
	/// </summary>
	/// <param name="consumer">Related transport stream.</param>
	/// <param name="pid">Transport stream identifier for this audio stream.</param>
	/// <param name="isPCR">Set if this stream supplies the PCR.</param>
	public class AudioStream(IStreamConsumer consumer, short pid, bool isPCR) : StreamBase(consumer, pid, isPCR)
	{

		/// <summary>
		/// Valid PES start codes for audio streams range from <i>0x000001c0</i>
		/// to <i>0x000001df</i>.
		/// </summary>
		/// <param name="code">The last byte of a start code.</param>
		/// <returns>Set if the start code represents an audio stream.</returns>
		protected override bool IsValidStartCode(byte code) => (code >= 0xc0) && (code < 0xe0);
	}
}
