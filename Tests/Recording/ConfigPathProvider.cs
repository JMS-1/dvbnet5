using JMS.DVB.NET.Recording.Services.Configuration;

using PathTool = System.IO.Path;

namespace DVBNETTests.Recording;

public class ConfigPathProvider : IVCRConfigurationExePathProvider
{
    public string Path { get; private set; }

    public ConfigPathProvider()
    {
        Path = PathTool.Combine(PathTool.GetTempPath(), Guid.NewGuid().ToString("N"));

        File.Copy("TestData/Service.exe", Path);
        File.Copy("TestData/Service.exe.config", Path + ".config");

        var registry = PathTool.Combine(PathTool.GetDirectoryName(Path)!, "Registry.conf");

        if (File.Exists(registry))
            File.Delete(registry);
    }
}
