using JMS.DVB.CardServer;
using JMS.DVB.EPG.Tables;
using JMS.DVB.TS;

namespace DVBNETTests.Support;

[TestFixture]
public class VideoDetectionTests
{
    private class Consumer(int service) : IStreamConsumer, IDisposable
    {
        private readonly FileStream _Stream = new(Path.Combine(Path.GetTempPath(), $"extract-{service}.mpv"), FileMode.Create, FileAccess.Write);

        public bool PCRAvailable => false;

        public void Dispose() => _Stream.Dispose();

        public void Send(ref int counter, int pid, byte[] buffer, int start, int packs, bool isFirst, int sizeOfLast, long pts)
            => _Stream.Write(buffer, start, (packs - 1) * Manager.PacketSize + sizeOfLast);

        public void SendPCR(int counter, int pid, long pts) { }
    }

    [TestCase("truecrime", 55214)]
    public void Can_Detect_True_Crime_Video_Stream(string name, int service)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", $"{name}.ts");

        if (!File.Exists(path)) return;

        using var output = new Consumer(service);

        using var parser = new TSParser();

        parser.RequestPMT(checked((ushort)service));

        parser.PMTFound += (PMT pmt) =>
        {
            foreach (var entry in pmt.ProgramEntries)
                switch (entry.StreamType)
                {
                    case JMS.DVB.EPG.StreamTypes.Video13818:
                        var video = new VideoStream(output, (short)entry.ElementaryPID, false) { AcceptAnyLength = true };

                        parser.SetFilter(entry.ElementaryPID, false, (byte[] data) => video.AddPayload(data));

                        break;
                }
        };

        using var file = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(file);

        for (var buf = new byte[1000000]; ;)
        {
            var n = reader.Read(buf, 0, buf.Length);

            if (n <= 0) break;

            parser.AddPayload(buf, 0, n);
        }
    }
}