using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DVBNETTests.Recording;

[TestFixture]
public class RegistryTests
{
    private ServiceProvider Services;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        var loggingMock = new Mock<ILogger>();

        services.AddSingleton(loggingMock.Object);

        services.AddTransient<IRegistry, Registry>();
        services.AddSingleton<IVCRConfigurationExePathProvider, ConfigPathProvider>();

        Services = services.BuildServiceProvider();
    }

    [TearDown]
    public void Teardown()
    {
        Services?.Dispose();
    }

    [Test]
    public void Can_Write_And_Read_Registry()
    {
        var cut = Services.GetRequiredService<IRegistry>();

        Assert.That(cut.GetTime("dummy"), Is.Null);

        cut.SetTime("1", new DateTime(2024, 3, 27, 15, 39, 28));
        cut.SetTime("dummy", new DateTime(2024, 4, 27, 15, 39, 28));
        cut.SetTime("b", new DateTime(2024, 2, 27, 15, 39, 28));
        cut.SetTime("dummy", new DateTime(2024, 4, 27, 15, 39, 29));

        var dt = cut.GetTime("dummy");

        Assert.That(dt, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(dt.Value.Year, Is.EqualTo(2024));
            Assert.That(dt.Value.Month, Is.EqualTo(4));
            Assert.That(dt.Value.Day, Is.EqualTo(27));
            Assert.That(dt.Value.Hour, Is.EqualTo(15));
            Assert.That(dt.Value.Minute, Is.EqualTo(39));
            Assert.That(dt.Value.Second, Is.EqualTo(29));
        });


        cut.SetTime("dummy", null);

        Assert.That(cut.GetTime("dummy"), Is.Null);
    }

    [Test]
    public void Can_Save_And_Load_Registry()
    {
        var cut1 = Services.GetRequiredService<IRegistry>();

        cut1.SetTime("dummy", new DateTime(2024, 4, 27, 16, 5, 38));

        var cut2 = Services.GetRequiredService<IRegistry>();

        var dt = cut2.GetTime("dummy");

        Assert.That(dt, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(dt.Value.Year, Is.EqualTo(2024));
            Assert.That(dt.Value.Month, Is.EqualTo(4));
            Assert.That(dt.Value.Day, Is.EqualTo(27));
            Assert.That(dt.Value.Hour, Is.EqualTo(16));
            Assert.That(dt.Value.Minute, Is.EqualTo(5));
            Assert.That(dt.Value.Second, Is.EqualTo(38));
        });
    }
}
