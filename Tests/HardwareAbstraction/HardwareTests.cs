using JMS.DVB;
using JMS.DVB.TS;

namespace DVBNETTests.HardwareAbstraction;

[TestFixture]
public class HardwareTests
{
    [TestCase(false)]
    [TestCase(true)]
    [Ignore("will access hardware")]
    public async Task Can_Create_Ubuntu_Hardware(bool anyLength)
    {
        VideoStream.DefaultAcceptAnyLength = anyLength;

        var profile = ProfileManager.LoadProfile(new FileInfo("TestData/stations.dnp"))!;
        var station = profile.FindSource("TRUE CRIME")[0];

        Assert.That(station, Is.Not.Null);

        using (HardwareManager.Open())
        {
            var device = HardwareManager.OpenHardware(profile);

            Assert.That(device, Is.Not.Null);

            device.SelectGroup(station);

            var info = await device.GetSourceInformationAsync(station.Source);

            Assert.That(info, Is.Not.Null);
            Assert.That(info.VideoStream, Is.EqualTo(2650));

            using var stream = new SourceStreamsManager(device, profile, info.Source, new StreamSelection { MP2Tracks = { LanguageMode = LanguageModes.All } });

            Assert.That(stream.CreateStream(Path.Combine(Path.GetTempPath(), $"test-hardware-{(anyLength ? "any" : "0-only")}.ts")), Is.True);

            await Task.Delay(60000);

            Assert.That(stream.BytesReceived, Is.Not.Zero);
        }
    }

    [TestCase("RTL Television")]
    [TestCase("ZDF HD")]
    public async Task Can_Create_HLS_LIVE_Stream(string name)
    {
        var profile = ProfileManager.LoadProfile(new FileInfo("TestData/stations.dnp"))!;
        var station = profile.FindSource(name)[0];

        Assert.That(station, Is.Not.Null);

        using (HardwareManager.Open())
        {
            var device = HardwareManager.OpenHardware(profile);

            Assert.That(device, Is.Not.Null);

            device.SelectGroup(station);

            var info = await device.GetSourceInformationAsync(station.Source);

            Assert.That(info, Is.Not.Null);

            using var stream = new SourceStreamsManager(device, profile, info.Source, new StreamSelection { MP2Tracks = { LanguageMode = LanguageModes.All } });
            using var live = new HLStreaming($"/tmp/hls-{name}");

            Assert.That(stream.CreateStream(null!), Is.True);

            stream.SetLiveStream(live.AddPayload);

            await Task.Delay(10000);

            Assert.That(stream.BytesReceived, Is.Not.Zero);
            Assert.That(live.BytesReceived, Is.EqualTo(stream.BytesReceived));
        }
    }
}
