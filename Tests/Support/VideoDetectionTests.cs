using JMS.DVB.EPG.Tables;
using JMS.DVB.TS;
using JMS.DVB.TS.TSBuilders;

namespace DVBNETTests.Support;

[TestFixture]
public class VideoDetectionTests
{
    [TestCase("truecrime", 55214)]
    [TestCase("truecrime", 55268)]
    public void Can_Detect_True_Crime_Video_Stream(string name, int service)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", $"{name}.ts");

        if (!File.Exists(path)) return;

        using var extract = new FileStream(Path.Combine(Path.GetTempPath(), $"extract-{service}.mpv"), FileMode.Create, FileAccess.Write);

        using var parser = new TSParser();

        parser.RequestPMT(checked((ushort)service));

        parser.PMTFound += (PMT pmt) =>
        {
            foreach (var entry in pmt.ProgramEntries)
                switch (entry.StreamType)
                {
                    case JMS.DVB.EPG.StreamTypes.Video13818:
                    case JMS.DVB.EPG.StreamTypes.H264:
                        parser.SetFilter(entry.ElementaryPID, false, (byte[] data) => extract.Write(data));
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