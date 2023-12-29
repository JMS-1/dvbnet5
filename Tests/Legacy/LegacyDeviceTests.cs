using JMS.DVB.Provider.Legacy;

namespace DVBNETTests.Legacy;

[TestFixture]
public class LegacyDeviceTests
{
    [Test]
    public void Can_Read_Legacy_Device_Configuration()
    {
        Assert.That(LegacyDeviceInformation.Devices, Has.Length.EqualTo(1));
        Assert.That(LegacyDeviceInformation.Devices[0].DriverType, Is.EqualTo("JMS.DVB.Provider.Ubuntu.DeviceProvider, JMS.DVB.NET.Ubuntu"));
    }
}
