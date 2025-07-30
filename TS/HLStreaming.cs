using System.Diagnostics;
using System.Text;

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

    private long _BytesReceived;

    private readonly string _Folder;

    private readonly Process _Transcoder;

    /// <summary>
    /// Initialize a new LIVE stream.
    /// </summary>
    /// <param name="folder">Folder to keep temporary files in.</param>
    public HLStreaming(string folder)
    {
        // Create folder or this LIVE stream.
        _Folder = Path.Join(folder, StreamIdentifier);

        Directory.CreateDirectory(_Folder);

        // Start transcoding.
        var start = new ProcessStartInfo
        {
            CreateNoWindow = true,
            FileName = "ffmpeg",
            RedirectStandardInput = true,
            WorkingDirectory = _Folder,
            ArgumentList = {
                "-i",               "-",
                "-map",             "0",
                "-map",             "-0:d",
                "-map",             "-0:s",
                "-c:v",             "libx264",
                "-c:a",             "copy",
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
    public long BytesProcessed { get; private set; }

    /// <summary>
    /// Process a new chunk of the stream.
    /// </summary>
    /// <param name="payload">Chunk to process</param>
    public void AddPayload(byte[] payload)
    {
        // Count incoming.
        Interlocked.Add(ref _BytesReceived, payload.Length);

        _Transcoder.StandardInput.BaseStream.Write(payload);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
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