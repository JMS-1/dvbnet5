using JMS.DVB;

namespace DVBNETTests.HardwareAbstraction;

[TestFixture]
public class HardwareTests
{
    [Test]
    [Ignore("will access hardware")]
    public async Task Can_Create_Ubuntu_Hardware()
    {
        var profile = ProfileManager.LoadProfile(new FileInfo("TestData/stations.dnp"))!;
        var station = profile.FindSource("ZDF")[0];

        Assert.That(station, Is.Not.Null);

        using (HardwareManager.Open())
        {
            var device = HardwareManager.OpenHardware(profile);

            Assert.That(device, Is.Not.Null);

            device.SelectGroup(station);

            var info = await device.GetSourceInformationAsync(station.Source);

            Assert.That(info?.VideoStream, Is.EqualTo(110));
        }
    }
}
