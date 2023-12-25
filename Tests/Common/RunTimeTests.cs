using JMS.DVB;

namespace DVBNETTests.Common;

[TestFixture]
public class RunTimeTests
{
    [Test]
    public void Can_Retrieve_Configuration_Root_Folder()
    {
        Assert.That(RunTimeLoader.ConfigurationDirectory.FullName, Is.EqualTo("/usr/share/jmsdvbnet"));
    }

    [TestCase("DVBNETProfiles")]
    [TestCase("Huffman Tables")]
    [TestCase("Scan Locations")]
    public void Can_Retrieve_Configuration_Folder(string folder)
    {
        Assert.That(RunTimeLoader.GetDirectory(folder).FullName, Is.EqualTo($"/usr/share/jmsdvbnet/{folder}"));
    }
}
