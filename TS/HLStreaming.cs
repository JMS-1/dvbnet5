namespace JMS.DVB.TS;

/// <summary>
/// Provide HLS LIVE information through FFMPEG transcoding.
/// </summary>
/// <param name="folder">Folder to keep temporary files in.</param>
public class HLStreaming(string folder) : IDisposable
{
    private long _BytesReceived;

    /// <summary>
    /// All bytes send to us.
    /// </summary>
    public long BytesReceived => _BytesReceived;

    /// <summary>
    /// All bytes sent to the transcoder.
    /// </summary>
    public long BytesProcessed { get; private set; }

    /// <summary>
    /// Process a new chunk of the stream.
    /// </summary>
    /// <param name="payload">Chunk to process</param>
    public void AddPayload(byte[] payload)
    {
        // Count incoming.
        Interlocked.Add(ref _BytesReceived, payload.Length);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}