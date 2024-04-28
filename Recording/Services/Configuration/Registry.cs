using System.Globalization;
using System.Xml.Serialization;
using JMS.DVB.NET.Recording.Services.Logging;

namespace JMS.DVB.NET.Recording.Services.Configuration;

[Serializable]
public class RegistrySetting
{
    public required string Key { get; set; }

    public required string Value { get; set; }
}

[Serializable]
public class RegistryFile
{
    public readonly List<RegistrySetting> Values = [];

    [XmlIgnore]
    public string? this[string key]
    {
        get
        {
            lock (Values)
                return Values.FirstOrDefault(v => v.Key == key)?.Value;
        }
        set
        {
            lock (Values)
            {
                var index = Values.FindIndex(v => v.Key == key);

                if (value != null)
                    if (index < 0)
                        Values.Add(new() { Key = key, Value = value });
                    else Values[index].Value = value;
                else if (index >= 0)
                    Values.RemoveAt(index);
            }
        }
    }

    public void SaveTo(FileInfo path)
    {
        lock (Values)
            SerializationTools.Save(this, path);
    }

    public static RegistryFile LoadFrom(FileInfo path) => SerializationTools.Load<RegistryFile>(path) ?? new();
}

public class Registry : IRegistry
{
    private readonly ILogger<Registry> _logger;

    private readonly FileInfo _path;

    public Registry(ILogger<Registry> logger, IVCRConfigurationExePathProvider configurationExePath)
    {
        _logger = logger;

        _path = new FileInfo(Path.Combine(Path.GetDirectoryName(configurationExePath.Path)!, "Registry.conf"));

        try
        {
            _file = RegistryFile.LoadFrom(_path);
        }
        catch (Exception e)
        {
            _file = new();

            // Report
            _logger.Log(e);
        }
    }

    private RegistryFile _file;

    /// <inheritdoc/>
    public DateTime? GetTime(string name)
    {
        // Try to load
        try
        {
            // Read it
            var value = _file[name];
            if (string.IsNullOrEmpty(value))
                return null;

            // To to convert
            if (DateTime.TryParseExact(value, "u", null, DateTimeStyles.None, out DateTime result))
                return DateTime.SpecifyKind(result, DateTimeKind.Utc);
        }
        catch
        {
            // Ignore any error
        }

        // Discard
        SetTime(name, null);

        // Not known
        return null;
    }

    /// <inheritdoc/>
    public void SetTime(string name, DateTime? value)
    {
        // Always be safe
        try
        {
            _file[name] = value.HasValue ? value.Value.ToString("u") : null!;

            _file.SaveTo(_path);
        }
        catch (Exception e)
        {
            // Report
            _logger.Log(e);
        }
    }
}