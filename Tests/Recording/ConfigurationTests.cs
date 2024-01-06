using JMS.DVB.NET.Recording;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using PathTool = System.IO.Path;

namespace DVBNETTests.Recording;

[TestFixture]
public class ConfigurationTests
{
    class ConfigPathProvider : IVCRConfigurationExePathProvider
    {
        public string Path { get; private set; }

        public ConfigPathProvider()
        {
            Path = PathTool.Combine(PathTool.GetTempPath(), Guid.NewGuid().ToString("N"));

            File.Copy("TestData/Service.exe", Path);
            File.Copy("TestData/Service.exe.config", Path + ".config");
        }
    }

    private ServiceProvider Services;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddTransient<ILogger<VCRConfiguration>, NullLogger<VCRConfiguration>>();
        services.AddSingleton<IVCRConfigurationExePathProvider, ConfigPathProvider>();

        services.AddTransient<VCRConfiguration>();

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

    [Test]
    public void Can_Update_Configuration_File()
    {
        var cut = Services.GetRequiredService<VCRConfiguration>();
        var config = cut.BeginUpdate(SettingNames.Profiles);

        config[SettingNames.Profiles].NewValue = "card1|card3|card9";

        Assert.That(cut.CommitUpdate(config.Values), Is.True);
        Assert.That(cut.ProfileNames, Is.EqualTo("card1|card2"));

        cut = Services.GetRequiredService<VCRConfiguration>();

        Assert.That(cut.ProfileNames, Is.EqualTo("card1|card3|card9"));
    }
}
