using System.Configuration;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace JMS.DVB.NET.Recording.Services.Configuration;

/// <summary>
/// Verwaltet die Konfiguration des VCR.NET Recording Service.
/// </summary>
/// <remarks>LEAF SINGLETON SERVICE.</remarks>
public class VCRConfiguration : IVCRConfiguration
{

    /// <summary>
    /// Beschreibt eine Einstellung und deren Wert.
    /// </summary>
    /// <typeparam name="TValueType">Der Datentyp des Wertes.</typeparam>
    public class SettingDescription<TValueType> : SettingDescription
    {
        /// <summary>
        /// Liest oder setzt den Wert.
        /// </summary>
        public TValueType Value { get; set; } = default!;

        /// <summary>
        /// Der zu verwendende Wert, wenn die Einstellung nicht gefunden wurde.
        /// </summary>
        private readonly TValueType m_Default;

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="name">Der Name der Beschreibung.</param>
        /// <param name="defaultValue">Ein Wert für den Fall, dass der gewünschte Konfigurationswert
        /// nicht belegt ist.</param>
        /// <param name="vcrConfiguration">Die zugehörige Konfiguration.</param>
        internal SettingDescription(SettingNames name, TValueType defaultValue, VCRConfiguration vcrConfiguration)
            : base(name, vcrConfiguration)
        {
            // Remember
            m_Default = defaultValue;
        }

        /// <summary>
        /// Erzeugt eine Kopie des Eintrags.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        protected override SettingDescription CreateClone() => new SettingDescription<TValueType>(Name, m_Default, VCRConfiguration);

        /// <summary>
        /// Erzeugt eine Kopie des Eintrags.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        public new SettingDescription<TValueType> Clone() => (SettingDescription<TValueType>)CreateClone();

        /// <summary>
        /// Liest den Namen der Einstellung.
        /// </summary>
        /// <returns>Der Wert der Einstellung als Zeichenkette.</returns>
        public override object ReadValue()
        {
            // Load as string
            var setting = (string)base.ReadValue();

            // None
            if (string.IsNullOrEmpty(setting))
                return m_Default!;

            // Try to convert
            try
            {
                // Type to use for parsing
                var resultType = typeof(TValueType);

                // Check mode
                if (resultType == typeof(string))
                    return (TValueType)(object)setting;

                // See if type is nullable
                resultType = Nullable.GetUnderlyingType(resultType) ?? resultType;

                // Forward
                if (resultType.IsEnum)
                    return (TValueType)Enum.Parse(resultType, setting);
                else
                    return (TValueType)resultType.InvokeMember("Parse", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, [setting])!;
            }
            catch
            {
                // Report default
                return m_Default!;
            }
        }
    }

    /// <summary>
    /// Beschreibt alle bekannten Konfigurationswerte.
    /// </summary>
    private readonly Dictionary<SettingNames, SettingDescription> m_Settings = [];

    /// <summary>
    /// Enthält alle Konfigurationswerte, deren Veränderung einen Neustart des Dienstes erforderlich machen.
    /// </summary>
    private readonly Dictionary<SettingNames, bool> m_Restart = [];

    /// <summary>
    /// Protokollierungshelfer.
    /// </summary>
    private readonly ILogger<IVCRConfiguration> _logger;

    /// <summary>
    /// Pfad zum Dienst.
    /// </summary>
    private readonly string _configurationExePath;

    /// <summary>
    /// Die aktuell gültige Konfiguration.
    /// </summary>
    internal System.Configuration.Configuration Configuration = null!;

    /// <summary>
    /// Initialisiert eine neue Konfiguration.
    /// </summary>
    /// <param name="configurationExePath">Pfad zum Dienst.</param>
    /// <param name="logger">Protokollierungshelfer.</param>
    public VCRConfiguration(IVCRConfigurationExePathProvider configurationExePath, ILogger<IVCRConfiguration> logger)
    {
        _logger = logger;

        _logger.LogTrace("New Configuration Instance Created");

        // Get the path of the configuration and load the initial configuration.
        _configurationExePath = configurationExePath.Path;

        Reload();

        // Remember all
        Add(SettingNames.AdditionalRecorderPaths);
        Add(SettingNames.ArchiveLifeTime, (uint)5);
        Add(SettingNames.DisablePCRFromH264Generation, false);
        Add(SettingNames.DisablePCRFromMPEG2Generation, false);
        Add(SettingNames.EPGDuration, (uint)15);
        Add(SettingNames.EPGHours);
        Add(SettingNames.EPGIncludeFreeSat, false);
        Add(SettingNames.EPGInterval, (uint?)null);
        Add(SettingNames.EPGJoinThreshold, (uint?)null);
        Add(SettingNames.EPGStations);
        Add(SettingNames.FileNamePattern, "%Job% - %Schedule% - %Start%");
        Add(SettingNames.LoggingLevel, Logging.LoggingLevel.Full);
        Add(SettingNames.LogLifeTime, (uint)5);
        Add(SettingNames.MergeScanResult, true);
        Add(SettingNames.Profiles);
        Add(SettingNames.ScanDuration, (uint)60);
        Add(SettingNames.ScanHours);
        Add(SettingNames.ScanInterval, 0);
        Add(SettingNames.ScanJoinThreshold, (uint?)null);
        Add(SettingNames.TSAudioBufferSize, 0);
        Add(SettingNames.TSHDTVBufferSize, 0);
        Add(SettingNames.TSSDTVBufferSize, 0);
        Add(SettingNames.VideoRecorderDirectory, "Recordings");

        // Set restart items
        m_Restart[SettingNames.Profiles] = true;
    }

    /// <inheritdoc/>
    public void Reload() => Configuration = ConfigurationManager.OpenExeConfiguration(_configurationExePath);

    /// <inheritdoc/>
    public Dictionary<SettingNames, SettingDescription> BeginUpdate(params SettingNames[] names) =>
        names == null ? [] : names.ToDictionary(n => n, n => m_Settings[n].Clone());

    /// <inheritdoc/>
    public bool CommitUpdate(IEnumerable<SettingDescription> settings)
    {
        // Validate
        if (settings == null)
            return false;

        // Clone the current configuration
        var newConfiguration = ConfigurationManager.OpenExeConfiguration(_configurationExePath);

        // See if we changed at all
        bool changed = false, restart = false;

        // Process all
        foreach (var setting in settings)
            if (setting.Update(newConfiguration))
            {
                // Remember
                changed = true;

                // See if this requires a restart
                if (m_Restart.ContainsKey(setting.Name))
                    restart = true;
            }

        // Nothing changed
        if (!changed)
            return false;

        // All names
        string origName = _configurationExePath + ".config", tempName = origName + ".new";

        // Write back to primary
        newConfiguration.SaveAs(tempName);

        // Be safe
        try
        {
            // Try to overwrite
            File.Copy(tempName, origName, true);

            // Write back to backup for upgrade installation
            File.Copy(tempName, origName + ".cpy", true);

            // Force reload
            if (!restart) Reload();

            // Report
            return restart;
        }
        finally
        {
            // Cleanup
            File.Delete(tempName);
        }
    }

    /// <summary>
    /// Vermerkt eine Einstellung.
    /// </summary>
    /// <param name="name">Der Name der Einstellung.</param>
    private void Add(SettingNames name) => Add(name, (string)null!);

    /// <summary>
    /// Vermerkt eine Einstellung.
    /// </summary>
    /// <typeparam name="TValueType">Der Datentyp des zugehörigen Wertes.</typeparam>
    /// <param name="name">Der Name der Einstellung.</param>
    /// <param name="defaultValue">Der voreingestellt Wert.</param>
    private void Add<TValueType>(SettingNames name, TValueType defaultValue)
        => m_Settings[name] = new SettingDescription<TValueType>(name, defaultValue, this);

    /// <summary>
    /// Ermittelt eine einzelne Einstellung.
    /// </summary>
    /// <typeparam name="T">Datentyp der Einstellung,</typeparam>
    /// <param name="name">Name der Einstellung.</param>
    /// <returns>Wert der Einstellung.</returns>
    private T ReadSetting<T>(SettingNames name)
        => m_Settings.TryGetValue(name, out var settings) ? (T)settings.ReadValue() : default!;

    /// <summary>
    /// Ermittelt eine einzelne Einstellung der Art Zeichenkette,
    /// </summary>
    /// <param name="name">Name der Einstellung.</param>
    /// <returns>Wert der Einstellung.</returns>
    private string ReadStringSetting(SettingNames name) => ReadSetting<string>(name);

    /// <inheritdoc/>
    public bool HasRecordedSomething { get; set; }

    /// <inheritdoc/>
    public int? AudioBufferSize
    {
        get
        {
            // Process
            var buffer = ReadSetting<int>(SettingNames.TSAudioBufferSize);

            return (buffer < 1) ? null : Math.Max(1000, buffer);
        }
    }

    /// <inheritdoc/>
    public int? StandardVideoBufferSize
    {
        get
        {
            // Process
            var buffer = ReadSetting<int>(SettingNames.TSSDTVBufferSize);

            return (buffer < 1) ? null : Math.Max(1000, buffer);
        }
    }

    /// <inheritdoc/>
    public int? HighDefinitionVideoBufferSize
    {
        get
        {
            // Process
            var buffer = ReadSetting<int>(SettingNames.TSHDTVBufferSize);

            return (buffer < 1) ? null : Math.Max(1000, buffer);
        }
    }

    /// <inheritdoc/>
    public bool DisablePCRFromH264Generation => ReadSetting<bool>(SettingNames.DisablePCRFromH264Generation);

    /// <inheritdoc/>
    public bool DisablePCRFromMPEG2Generation => ReadSetting<bool>(SettingNames.DisablePCRFromMPEG2Generation);

    /// <inheritdoc/>
    public bool EnableFreeSat => ReadSetting<bool>(SettingNames.EPGIncludeFreeSat);

    /// <summary>
    /// Die Liste der Quellen, die in der Programmzeitschrift berücksichtigt werden sollen.
    /// </summary>
    private string ProgramGuideSourcesRaw => ReadStringSetting(SettingNames.EPGStations);

    /// <inheritdoc/>
    public string[] ProgramGuideSourcesAsArray => ProgramGuideSources.ToArray();

    /// <inheritdoc/>
    public IEnumerable<string> ProgramGuideSources
    {
        get
        {
            // Load from settings
            var sources = ProgramGuideSourcesRaw;
            if (string.IsNullOrEmpty(sources))
                yield break;

            // Process all
            foreach (var source in sources.Split(','))
            {
                // Cleanup
                var trimmed = source.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    yield return trimmed;
            }
        }
    }

    /// <inheritdoc/>
    public bool ProgramGuideUpdateEnabled
    {
        get
        {
            // Ask in the cheapest order
            if (!ProgramGuideSources.Any())
                return false;
            if (ProgramGuideUpdateDuration < 1)
                return false;
            if (ProgramGuideUpdateHours.Any())
                return true;
            if (ProgramGuideUpdateInterval.GetValueOrDefault(TimeSpan.Zero).TotalDays > 0)
                return true;

            return false;
        }
    }

    /// <inheritdoc/>
    public uint[] ProgramGuideUpdateHours => GetHourList(ProgramGuideUpdateHoursRaw).ToArray();

    /// <inheritdoc/>
    public uint ProgramGuideUpdateDuration => ReadSetting<uint>(SettingNames.EPGDuration);

    /// <inheritdoc/>
    public TimeSpan? ProgramGuideUpdateInterval
    {
        get
        {
            // Load
            var interval = ReadSetting<uint?>(SettingNames.EPGInterval);

            return interval.HasValue ? TimeSpan.FromHours(interval.Value) : null;
        }
    }

    /// <inheritdoc/>
    public TimeSpan? ProgramGuideJoinThreshold
    {
        get
        {
            // Load
            var interval = ReadSetting<uint?>(SettingNames.EPGJoinThreshold);

            return interval.HasValue ? TimeSpan.FromHours(interval.Value) : null;
        }
    }

    /// <inheritdoc/>
    public TimeSpan? SourceListJoinThreshold
    {
        get
        {
            // Load
            var interval = ReadSetting<uint?>(SettingNames.ScanJoinThreshold);

            return interval.HasValue ? TimeSpan.FromDays(interval.Value) : null;
        }
    }

    /// <inheritdoc/>
    public uint SourceListUpdateDuration => ReadSetting<uint>(SettingNames.ScanDuration);

    /// <inheritdoc/>
    public int SourceListUpdateInterval => ReadSetting<int>(SettingNames.ScanInterval);

    /// <inheritdoc/>
    public bool MergeSourceListUpdateResult => ReadSetting<bool>(SettingNames.MergeScanResult);

    /// <summary>
    /// Meldet die Liste der Stunden, an denen eine Aktualisierung einer
    /// Liste von Quellen stattfinden darf.
    /// </summary>
    private string SourceListUpdateHoursRaw => ReadStringSetting(SettingNames.ScanHours);

    /// <inheritdoc/>
    public uint[] SourceListUpdateHours => GetHourList(SourceListUpdateHoursRaw).ToArray();

    /// <summary>
    /// Meldet die Liste der Stunden, an denen eine Aktualisierung einer
    /// Programmzeitschrift stattfinden darf.
    /// </summary>
    private string ProgramGuideUpdateHoursRaw => ReadStringSetting(SettingNames.EPGHours);

    /// <inheritdoc/>
    public uint LogLifeTime => ReadSetting<uint>(SettingNames.LogLifeTime);

    /// <inheritdoc/>
    public uint ArchiveLifeTime => ReadSetting<uint>(SettingNames.ArchiveLifeTime);

    /// <inheritdoc/>
    public Logging.LoggingLevel LoggingLevel => ReadSetting<Logging.LoggingLevel>(SettingNames.LoggingLevel);

    /// <inheritdoc/>
    public string FileNamePattern => ReadStringSetting(SettingNames.FileNamePattern);

    /// <summary>
    /// Ermittelt den Namen des primären Aufzeichnungsverzeichnisses
    /// </summary>
    private string PrimaryRecordingDirectory => ReadStringSetting(SettingNames.VideoRecorderDirectory);

    /// <summary>
    /// Meldet die zusätzlichen Aufzeichnungsverzeichnisse.
    /// </summary>
    private string AlternateRecordingDirectories => ReadStringSetting(SettingNames.AdditionalRecorderPaths);

    /// <inheritdoc/>
    public string ProfileNames => ReadStringSetting(SettingNames.Profiles);

    /// <inheritdoc/>
    public DirectoryInfo PrimaryTargetDirectory
    {
        get
        {
            // Get the path
            var path = PrimaryRecordingDirectory;

            // Extend it
            if (!string.IsNullOrEmpty(path))
                if (path[^1] != Path.DirectorySeparatorChar)
                    path += Path.DirectorySeparatorChar;

            // Create
            return new DirectoryInfo(path);
        }
    }

    /// <inheritdoc/>
    public string[] TargetDirectoryNames => TargetDirectories.Select(d => d.FullName).ToArray();

    /// <summary>
    /// Meldet alle erlaubten Aufzeichnungsverzeichnisse.
    /// </summary>
    private IEnumerable<DirectoryInfo> TargetDirectories
    {
        get
        {
            // Start with primary
            yield return PrimaryTargetDirectory;

            // All configured
            var dirs = AlternateRecordingDirectories;

            // Process
            if (!string.IsNullOrEmpty(dirs))
                foreach (var dir in dirs.Split(','))
                {
                    // Load the path
                    string path = dir.Trim();

                    // Skip
                    if (string.IsNullOrEmpty(path))
                        continue;

                    // Extend it
                    if (path[^1] != Path.DirectorySeparatorChar)
                        path += Path.DirectorySeparatorChar;

                    // Be safe
                    DirectoryInfo info;

                    // Be safe
                    try
                    {
                        // Load
                        info = new DirectoryInfo(path);
                    }
                    catch (Exception e)
                    {
                        // Report as error
                        _logger.LogError(e.Message);

                        // Just ignore
                        continue;
                    }

                    // Report
                    yield return info;
                }
        }
    }

    /// <inheritdoc/>
    public bool IsValidTarget(string path)
    {
        // Must habe a path
        if (string.IsNullOrEmpty(path))
            return false;

        // Test it
        foreach (var allowed in TargetDirectories)
            if (path.StartsWith(allowed.FullName, StringComparison.InvariantCultureIgnoreCase))
            {
                // Silent create the directory
                try
                {
                    // Create the directory
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                    // Yeah
                    return true;
                }
                catch (Exception e)
                {
                    // Report
                    _logger.LogError(e.Message);

                    // Done
                    return false;
                }
            }

        // No, not possible
        return false;
    }


    /// <summary>
    /// Ermittelt eine Liste von vollen Stunden eines Tages aus der Konfiguration.
    /// </summary>
    /// <param name="hours">Die durch Komma getrennte Liste von vollen Stunden.</param>
    /// <returns>Die Liste der Stunden.</returns>
    private static IEnumerable<uint> GetHourList(string hours)
    {
        // None at all
        if (string.IsNullOrEmpty(hours))
            yield break;

        // Process all
        foreach (var hourAsString in hours.Split(','))
            if (uint.TryParse(hourAsString.Trim(), out uint hour))
                if ((hour >= 0) && (hour <= 23))
                    yield return hour;
    }
}

