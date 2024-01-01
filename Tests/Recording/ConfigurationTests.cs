using JMS.DVB.NET.Recording;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DVBNETTests.Recording;

[TestFixture]
public class ConfigurationTests
{
    class ConfigPathProvider : IVCRConfigurationExePathProvider
    {
        public string Path => "TestData/Service.exe";
    }

    private ServiceProvider Services;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddTransient<ILogger<VCRConfiguration>, NullLogger<VCRConfiguration>>();
        services.AddSingleton<IVCRConfigurationExePathProvider, ConfigPathProvider>();

        services.AddSingleton<VCRConfiguration>();

        Services = services.BuildServiceProvider();
    }

    [TearDown]
    public void Teardown()
    {
        Services?.Dispose();
    }

    [Test]
    public void Can_Read_Configuration_File()
    {
        var cut = Services.GetRequiredService<VCRConfiguration>();

        Assert.That(cut.ProfileNames, Is.EqualTo("card1|card2"));
    }
}
