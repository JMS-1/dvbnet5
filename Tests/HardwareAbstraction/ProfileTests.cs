using JMS.DVB;

namespace DVBNETTests.HardwareAbstraction;

[TestFixture]
public class ProfileTests
{
    [Test]
    public void Can_Load_Channel_File()
    {
        var profile = ProfileManager.LoadProfile(new FileInfo("TestData/stations.dnp"));

        Assert.That(profile, Is.Not.Null);
        Assert.That(profile.AllSources.Count, Is.EqualTo(2231));
    }
}
