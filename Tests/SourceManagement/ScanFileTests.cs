using JMS.DVB;

namespace DVBNETTests.SourceManagement;

[TestFixture]
public class ScanFileTests
{
    [Test]
    public void Can_Load_Default_Scanfile()
    {
        var locations = ScanLocations.Default;

        Assert.That(locations, Is.Not.Null);
        Assert.That(locations.Locations.Count, Is.EqualTo(285));
    }
}
