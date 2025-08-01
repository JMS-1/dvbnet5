using System.Diagnostics;

namespace JMS.DVB.TS;

/// <summary>
/// Provide HLS LIVE information through FFMPEG transcoding.
/// </summary>
public class HLStreaming : IDisposable
{
    /// <summary>
    /// Unique identifier of this stream.
    /// </summary>
    public readonly string StreamIdentifier = Guid.NewGuid().ToString("N").ToUpper();

    /// <summary>
    /// Total number of bytes coming from the DVB device.
    /// </summary>
    private long _BytesReceived;

    /// <summary>
    /// Total number of bytes sent to the transcoder.
    /// </summary>
    private long _BytesProcessed;

    /// <summary>
    /// FFMPEG transcoder instance.
    /// </summary>
    private readonly Process _Transcoder;

    /// <summary>
    /// Initialize a new LIVE stream.
    /// </summary>
    /// <param name="folder">Folder to keep temporary files in.</param>
    public HLStreaming(string folder)
    {
        // Create folder or this LIVE stream.
        var liveFolder = Path.Join(folder, StreamIdentifier);

        Directory.CreateDirectory(liveFolder);

        // Start transcoding.
        var start = new ProcessStartInfo
        {
            CreateNoWindow = true,
            FileName = "ffmpeg",
            RedirectStandardInput = true,
            WorkingDirectory = liveFolder,
            ArgumentList = {
                "-loglevel",        "fatal",
                "-i",               "-",
                "-map",             "0",
                "-map",             "-0:d",
                "-map",             "-0:s",
                "-c:v",             "libx264",
                "-c:a",             "aac",
                "-g",               "50",
                "-keyint_min",      "50",
                "-f",               "hls",
                "-hls_time",        "5",
                "-hls_list_size",   "10",
                "-hls_flags",       "delete_segments",
                "./live.m3u8"
            },
        };

        _Transcoder = Process.Start(start) ?? throw new InvalidOperationException("unable to start FFMPEG");
    }

    /// <summary>
    /// All bytes send to us.
    /// </summary>
    public long BytesReceived => _BytesReceived;

    /// <summary>
    /// All bytes sent to the transcoder.
    /// </summary>
    public long BytesProcessed => _BytesProcessed;

    /// <summary>
    /// Maximum number of pending write operations.
    /// </summary>
    public int MaxPending { get; private set; }

    /// <summary>
    /// Overall synchronizer.
    /// </summary>
    private readonly Lock _Sync = new();

    /// <summary>
    /// Initial buffer is 1 MBytes in size.
    /// </summary>
    private byte[] _Buffer = new byte[1024 * 1024];

    /// <summary>
    /// Current buffer fill.
    /// </summary>
    private int _BufferPos = 0;

    /// <summary>
    /// Current number of pending writes to the transcoder.
    /// </summary>
    private int _Pending = 0;

    /// <summary>
    /// Process a new chunk of the stream.
    /// </summary>
    /// <param name="payload">Chunk to process</param>
    public void AddPayload(byte[] payload)
    {
        // Count incoming.
        Interlocked.Add(ref _BytesReceived, payload.Length);

        // Enqueue outgoing.
        using (_Sync.EnterScope())
        {
            // Finish previous.
            if (_BufferPos + payload.Length > _Buffer.Length)
            {
                // Clear Buffer.
                SendBuffer();

                // Reallocate if too small.
                if (payload.Length > _Buffer.Length) _Buffer = new byte[payload.Length];
            }

            // Just collect.
            payload.CopyTo(_Buffer, _BufferPos);

            _BufferPos += payload.Length;
        }
    }

    /// <summary>
    /// Clear the current buffer and send it to the transcoder.
    /// </summary>
    private void SendBuffer()
    {
        // Nothing in it.
        if (_BufferPos < 1) return;

        // Count.
        Interlocked.Add(ref _BytesProcessed, _BufferPos);

        // Remember - we cache at most 100 MBytes of data.
        var buffer = _Buffer.AsSpan(0, _BufferPos).ToArray();

        // Try to send - clip at 100 MB Buffer count.
        if (_Pending >= 100)
            Console.WriteLine("HLS data overrun");
        else
        {
            MaxPending = Math.Max(MaxPending, Interlocked.Increment(ref _Pending));

            // Decrement counter when done.
            _Transcoder
                .StandardInput
                .BaseStream
                .WriteAsync(buffer)
                .AsTask()
                .ContinueWith(t => Interlocked.Decrement(ref _Pending));
        }

        // Reset.
        _BufferPos = 0;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            // This should close the transcoder process properly.
            _Transcoder.StandardInput.Close();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
        finally
        {
            if (!_Transcoder.WaitForExit(TimeSpan.FromSeconds(10)))
                Debug.WriteLine("FFMPEG will not finish");

            _Transcoder.Kill();
        }
    }
}