using JMS.DVB.NET.Recording.Actions;
using JMS.DVB.NET.Recording.Server;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Logging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace JMS.DVB.NET.Recording.RestWebApi;

/// <summary>
/// Erlaubt den administrativen Zugriff auf den <i>VCR.NET Recording Service</i>.
/// </summary>
[ApiController]
[Route("api/configuration")]
public class ConfigurationController(
    IVCRServer server,
    IVCRConfiguration configuration,
    IVCRProfiles profiles,
    IConfigurationUpdater updateConfig,
    IRuleUpdater updateRules
) : ControllerBase
{
    /// <summary>
    /// Die Einstellungen der Sicherheit.
    /// </summary>
    public class SecuritySettings
    {
        /// <summary>
        /// Die Gruppe der normalen Benutzer.
        /// </summary>
        public string UserRole { get; set; } = null!;

        /// <summary>
        /// Die Gruppe der Administratoren.
        /// </summary>
        public string AdminRole { get; set; } = null!;
    }

    /// <summary>
    /// Die Einstellung der Aufzeichnungsverzeichnisse.
    /// </summary>
    public class DirectorySettings
    {
        /// <summary>
        /// Die aktuelle Liste der erlaubten Verzeichnisse.
        /// </summary>
        public string[] TargetDirectories { get; set; } = null!;

        /// <summary>
        /// Das Muster für die Erstellung der Dateinamen.
        /// </summary>
        public string RecordingPattern { get; set; } = null!;
    }

    /// <summary>
    /// Die Einstellungen zur Programmzeitschrift.
    /// </summary>
    public class GuideSettings
    {
        /// <summary>
        /// Der Schwellwert für vorgezogene Aktualisierungen (in Stunden).
        /// </summary>
        public int? Threshold { get; set; }

        /// <summary>
        /// Der minimale Abstand zwischen Aktualisierungen (in Stunden).
        /// </summary>
        public int? Interval { get; set; }

        /// <summary>
        /// Die maximale Dauer einer Aktualisierung (in Minuten).
        /// </summary>
        public uint Duration { get; set; }

        /// <summary>
        /// Die Stunden, zu denen eine Aktualisierung stattfinden soll.
        /// </summary>
        public uint[] Hours { get; set; } = null!;

        /// <summary>
        /// Die Quellen, die bei der Aktualisierung zu berücksichtigen sind.
        /// </summary>
        public string[] Sources { get; set; } = null!;

        /// <summary>
        /// Gesetzt, wenn auch die britischen Sendungen zu berücksichtigen sind.
        /// </summary>
        public bool WithUKGuide { get; set; }
    }

    /// <summary>
    /// Die Einstellungen zur Aktualisierung der Quellen.
    /// </summary>
    public class SourceScanSettings
    {
        /// <summary>
        /// Der Schwellwert für vorgezogene Aktualisierungen (in Tagen).
        /// </summary>
        public int? Threshold { get; set; }

        /// <summary>
        /// Der minimale Abstand zwischen Aktualisierungen (in Tagen).
        /// </summary>
        public int? Interval { get; set; }

        /// <summary>
        /// Die maximale Dauer einer Aktualisierung (in Minuten).
        /// </summary>
        public uint Duration { get; set; }

        /// <summary>
        /// Die Stunden, zu denen eine Aktualisierung stattfinden soll.
        /// </summary>
        public uint[] Hours { get; set; } = null!;

        /// <summary>
        /// Gesetzt, wenn die neue Liste mit der alten zusammengeführt werden soll.
        /// </summary>
        public bool MergeLists { get; set; }
    }

    /// <summary>
    /// Die Konfiguration der Geräteprofile.
    /// </summary>
    public class ProfileSettings
    {
        /// <summary>
        /// Die Liste aller bekannten Geräteprofile.
        /// </summary>
        public ConfigurationProfile[] SystemProfiles { get; set; } = null!;

        /// <summary>
        /// Der Name des bevorzugten Geräteprofils.
        /// </summary>
        public string DefaultProfile { get; set; } = null!;
    }

    /// <summary>
    /// Alle sonstigen Einstellungen.
    /// </summary>
    public class OtherSettings
    {
        /// <summary>
        /// Verweildauer (in Wochen) von Aufträgen im Archiv.
        /// </summary>
        public uint ArchiveTime { get; set; }

        /// <summary>
        /// Verweildauer (in Wochen) von Protokolleinträgen.
        /// </summary>
        public uint ProtocolTime { get; set; }

        /// <summary>
        /// Gesetzt, wenn für H.264 Aufzeichnungen kein PCR generiert werden soll.
        /// </summary>
        public bool DisablePCRFromH264 { get; set; }

        /// <summary>
        /// Gesetzt, wenn für MPEG-2 Aufzeichnungen kein PCR generiert werden soll.
        /// </summary>
        public bool DisablePCRFromMPEG2 { get; set; }

        /// <summary>
        /// Die Art der Protokollierung.
        /// </summary>
        public LoggingLevel Logging { get; set; }
    }

    /// <summary>
    /// Informationen zum aktuell verwendeten Regelwerk der Aufzeichnungsplanung.
    /// </summary>
    public class SchedulerRules
    {
        /// <summary>
        /// Der Inhalt der Regeldatei.
        /// </summary>
        public string RuleFileContents { get; set; } = null!;
    }

    /// <summary>
    /// Ermittelt eine Verzeichnisstruktur.
    /// </summary>
    /// <param name="toParent">Gesetzt, wenn zum übergeordneten Verzeichnis gewechselt werden soll.</param>
    /// <param name="root">Das Bezugsverzeichnis.</param>
    /// <returns>Die Verzeichnisse innerhalb ober oberhalb des Bezugsverzeichnisses.</returns>
    [HttpGet("browse")]
    public string[] Browse(bool toParent = false, string root = null!)
    {
        // See if we can move up
        if (!string.IsNullOrEmpty(root))
            if (toParent)
                if (StringComparer.InvariantCultureIgnoreCase.Equals(root, Path.GetPathRoot(root)))
                    root = null!;
                else
                    root = Path.GetDirectoryName(root)!;

        // Devices
        var names = string.IsNullOrEmpty(root)
            ? DriveInfo
                .GetDrives()
                .Where(drive => drive.DriveType == DriveType.Fixed)
                .Where(drive => drive.IsReady)
                .Select(drive => drive.RootDirectory.FullName)
            : Directory
                .GetDirectories(root);

        // Report
        return [root, .. names.OrderBy(name => name, StringComparer.InvariantCultureIgnoreCase)];
    }

    /// <summary>
    /// Prüft, ob ein Verzeichnis verfügbar ist.
    /// </summary>
    /// <param name="directory">Das zu prüfende Verzeichnis.</param>
    /// <returns>Gesetzt, wenn das Verzeichnis verwendet werden kann.</returns>
    [HttpGet("validate")]
    public bool Validate(string directory)
    {
        // Be safe
        try
        {
            // Test
            return Directory.Exists(directory);
        }
        catch (Exception)
        {
            // Nope
            return false;
        }
    }

    /// <summary>
    /// Ermittelt die aktuellen Regeln für die Aufzeichnunsplanung.
    /// </summary>
    /// <returns>Die aktuellen Regeln.</returns>
    [HttpGet("rules")]
    public SchedulerRules ReadSchedulerRules() => new() { RuleFileContents = server.SchedulerRules };

    /// <summary>
    /// Aktualisiert das Regelwerk für die Aufzeichnungsplanung.
    /// </summary>
    /// <param name="settings">Die ab sofort zu verwendenden Regeln.</param>
    /// <returns>Meldet, ob ein Neustart erforderlich ist.</returns>
    [HttpPut("rules")]
    public bool? WriteSchedulerRules([FromBody] SchedulerRules settings) => updateRules.UpdateSchedulerRules(settings.RuleFileContents);

    /// <summary>
    /// Meldet die Konfigurationsdaten der Geräte.
    /// </summary>
    /// <returns>Die aktuelle Konfiguration.</returns>
    [HttpGet("profiles")]
    public ProfileSettings ReadProfiles()
    {
        // Helper

        // Create response
        var settings = new ProfileSettings
        {
            SystemProfiles =
                    [.. profiles
                            .GetProfiles(ConfigurationProfile.Create, out string defaultName)
                            .OrderBy(profile => profile.Name, ProfileManager.ProfileNameComparer)]
        };

        // Merge default
        settings.DefaultProfile = defaultName;

        // Report
        return settings;
    }

    /// <summary>
    /// Aktualisiert die Einstellungen zu den Geräteprofilen.
    /// </summary>
    /// <param name="settings">Die gewünschten neuen Einstellungen.</param>
    /// <returns>Das Ergebnis der Änderung.</returns>
    [HttpPut("profiles")]
    public bool? WriteProfiles([FromBody] ProfileSettings settings)
    {
        // List of profiles to use
        var profiles = settings.SystemProfiles.Where(profile => profile.UsedForRecording).Select(profile => profile.Name).ToList();

        // Move default to the front
        var defaultIndex = profiles.IndexOf(settings.DefaultProfile);
        if (defaultIndex >= 0)
        {
            // Insert at the very beginning
            profiles.Insert(0, profiles[defaultIndex]);
            profiles.RemoveAt(defaultIndex + 1);
        }

        // Prepare
        var update = configuration.BeginUpdate(SettingNames.Profiles);

        // Fill
        update[SettingNames.Profiles].NewValue = string.Join("|", profiles);

        // Process
        return updateConfig.UpdateConfiguration(update.Values, IVCRProfilesExtensions.UpdateProfiles(settings.SystemProfiles, profile => profile.Name, (profile, device) => profile.WriteBack(device)));
    }

    /// <summary>
    /// Meldet die aktuellen Einstellungen zu den Verzeichnissen.
    /// </summary>
    /// <returns>Die gewünschten Einstellungen.</returns>
    [HttpGet("folder")]
    public DirectorySettings ReadDirectory()
    {
        // Report
        return
            new DirectorySettings
            {
                TargetDirectories = [.. configuration.TargetDirectoryNames],
                RecordingPattern = configuration.FileNamePattern,
            };
    }

    /// <summary>
    /// Aktualisiert die Konfiguration der Aufzeichnungsdateien.
    /// </summary>
    /// <param name="settings">Die neuen Daten.</param>
    /// <returns>Das Ergebnis der Operation.</returns>
    [HttpPut("folder")]
    public bool? WriteDirectory([FromBody] DirectorySettings settings)
    {
        // Prepare to update
        var update = configuration.BeginUpdate(SettingNames.VideoRecorderDirectory, SettingNames.AdditionalRecorderPaths, SettingNames.FileNamePattern);

        // Change settings
        update[SettingNames.AdditionalRecorderPaths].NewValue = string.Join(", ", settings.TargetDirectories.Skip(1));
        update[SettingNames.VideoRecorderDirectory].NewValue = settings.TargetDirectories.FirstOrDefault()!;
        update[SettingNames.FileNamePattern].NewValue = settings.RecordingPattern;

        // Process
        return updateConfig.UpdateConfiguration(update.Values);
    }

    /// <summary>
    /// Liest die Konfiguration für die Aktualisierung der Quellen.
    /// </summary>
    /// <returns>Die aktuellen Einstellungen.</returns>
    [HttpGet("scan")]
    public SourceScanSettings ReadSoureScan()
    {
        // Load
        var interval = configuration.SourceListUpdateInterval;
        var join = configuration.SourceListJoinThreshold;

        // Report
        return
            new SourceScanSettings
            {
                Hours = [.. configuration.SourceListUpdateHours.OrderBy(hour => hour)],
                Threshold = join.HasValue ? (int)join.Value.TotalDays : default(int?),
                MergeLists = configuration.MergeSourceListUpdateResult,
                Duration = configuration.SourceListUpdateDuration,
                Interval = (interval != 0) ? interval : default(int?),
            };
    }

    /// <summary>
    /// Aktualisiert die Einstellungen für die Aktualisierung der Quellen.
    /// </summary>
    /// <param name="settings">Die neuen Einstellungen.</param>
    /// <returns>Das Ergebnis der Änderung.</returns>
    [HttpPut("scan")]
    public bool? WriteSourceScan([FromBody] SourceScanSettings settings)
    {
        // Check mode
        if (settings.Interval.GetValueOrDefault(0) == 0)
        {
            // Create settings
            var disable = configuration.BeginUpdate(SettingNames.ScanInterval);

            // Store
            disable[SettingNames.ScanInterval].NewValue = "0";

            // Process
            return updateConfig.UpdateConfiguration(disable.Values);
        }

        // Check mode
        if (settings.Interval < 0)
        {
            // Create settings
            var manual = configuration.BeginUpdate(SettingNames.ScanDuration, SettingNames.MergeScanResult, SettingNames.ScanInterval);

            // Store
            manual[SettingNames.MergeScanResult].NewValue = settings.MergeLists.ToString();
            manual[SettingNames.ScanDuration].NewValue = settings.Duration.ToString();
            manual[SettingNames.ScanInterval].NewValue = "-1";

            // Process
            return updateConfig.UpdateConfiguration(manual.Values);
        }

        // Prepare to update
        var update = configuration.BeginUpdate(SettingNames.ScanDuration, SettingNames.MergeScanResult, SettingNames.ScanInterval, SettingNames.ScanHours, SettingNames.ScanJoinThreshold);

        // Fill it
        update[SettingNames.ScanHours].NewValue = string.Join(", ", settings.Hours.Select(hour => hour.ToString()));
        update[SettingNames.ScanJoinThreshold].NewValue = settings.Threshold.ToString()!;
        update[SettingNames.MergeScanResult].NewValue = settings.MergeLists.ToString();
        update[SettingNames.ScanInterval].NewValue = settings.Interval.ToString()!;
        update[SettingNames.ScanDuration].NewValue = settings.Duration.ToString();

        // Process
        return updateConfig.UpdateConfiguration(update.Values);
    }

    /// <summary>
    /// Liest die Konfiguration für die Programmzeitschrift.
    /// </summary>
    /// <returns>Die aktuellen Einstellungen.</returns>
    [HttpGet("guide")]
    public GuideSettings ReadGuide()
    {
        // Load
        var interval = configuration.ProgramGuideUpdateInterval;
        var join = configuration.ProgramGuideJoinThreshold;

        // Report
        return
            new GuideSettings
            {
                Sources = [.. configuration.ProgramGuideSourcesAsArray.OrderBy(name => name, StringComparer.InvariantCultureIgnoreCase)],
                Hours = [.. configuration.ProgramGuideUpdateHours.OrderBy(hour => hour)],
                Interval = interval.HasValue ? (int)interval.Value.TotalHours : default(int?),
                Threshold = join.HasValue ? (int)join.Value.TotalHours : default(int?),
                Duration = configuration.ProgramGuideUpdateDuration,
                WithUKGuide = configuration.EnableFreeSat,
            };
    }

    /// <summary>
    /// Aktualisiert die Einstellungen der Programmzeitschrift.
    /// </summary>
    /// <param name="settings">Die neuen Einstellungen.</param>
    /// <returns>Das Ergebnis der Änderung.</returns>
    [HttpPut("guide")]
    public bool? WriteGuide([FromBody] GuideSettings settings)
    {
        // Check mode
        if (settings.Duration < 1)
        {
            // Create settings
            var disable = configuration.BeginUpdate(SettingNames.EPGDuration);

            // Store
            disable[SettingNames.EPGDuration].NewValue = "0";

            // Process
            return updateConfig.UpdateConfiguration(disable.Values);
        }

        // Prepare to update
        var update = configuration.BeginUpdate(SettingNames.EPGDuration, SettingNames.EPGStations, SettingNames.EPGHours, SettingNames.EPGIncludeFreeSat, SettingNames.EPGInterval, SettingNames.EPGJoinThreshold);

        // Fill it
        update[SettingNames.EPGHours].NewValue = string.Join(", ", settings.Hours.Select(hour => hour.ToString()));
        update[SettingNames.EPGIncludeFreeSat].NewValue = settings.WithUKGuide.ToString();
        update[SettingNames.EPGStations].NewValue = string.Join(", ", settings.Sources);
        update[SettingNames.EPGJoinThreshold].NewValue = settings.Threshold.ToString()!;
        update[SettingNames.EPGInterval].NewValue = settings.Interval.ToString()!;
        update[SettingNames.EPGDuration].NewValue = settings.Duration.ToString();

        // Process
        return updateConfig.UpdateConfiguration(update.Values);
    }

    /// <summary>
    /// Liest die Sicherheitseinstellungen.
    /// </summary>
    /// <returns>Die aktuellen Einstellungen.</returns>
    [HttpGet("security")]
    public SecuritySettings ReadSecurity()
    {
        // Report
        return
            new SecuritySettings
            {
                AdminRole = configuration.AdminRole,
                UserRole = configuration.UserRole,
            };
    }

    /// <summary>
    /// Aktualisiert die Sicherheitseinstellungen.
    /// </summary>
    /// <param name="settings">Die neuen Einstellungen.</param>
    /// <returns><i>null</i> bei Fehlern und ansonsten gesetzt, wenn ein Neustart des Dienstes ausgeführt wird.</returns>
    [HttpPut("security")]
    public bool? WriteSecurity([FromBody] SecuritySettings settings)
    {
        // Prepare to update
        var update = configuration.BeginUpdate(SettingNames.RequiredUserRole, SettingNames.RequiredAdminRole);

        // Change settings
        update[SettingNames.RequiredAdminRole].NewValue = settings.AdminRole;
        update[SettingNames.RequiredUserRole].NewValue = settings.UserRole;

        // Process
        return updateConfig.UpdateConfiguration(update.Values);
    }

    /// <summary>
    /// Meldet sonstige Konfigurationsparameter.
    /// </summary>
    /// <returns>Die aktuellen Einstellungen.</returns>
    [HttpGet("other")]
    public OtherSettings ReadOtherSettings()
    {
        // Create response
        return
            new OtherSettings
            {
                DisablePCRFromMPEG2 = configuration.DisablePCRFromMPEG2Generation,
                DisablePCRFromH264 = configuration.DisablePCRFromH264Generation,
                ArchiveTime = configuration.ArchiveLifeTime,
                ProtocolTime = configuration.LogLifeTime,
                Logging = configuration.LoggingLevel,
            };
    }

    /// <summary>
    /// Aktualisiert die sonstigen Einstellungen.
    /// </summary>
    /// <param name="settings">Die neuen Einstellungen.</param>
    /// <returns>Das Ergebnis der Änderung.</returns>
    [HttpPut("other")]
    public bool? WriteOther([FromBody] OtherSettings settings)
    {
        // Prepare to update
        var update =
            configuration.BeginUpdate
               (
                    SettingNames.DisablePCRFromMPEG2Generation,
                    SettingNames.DisablePCRFromH264Generation,
                    SettingNames.ArchiveLifeTime,
                    SettingNames.LoggingLevel,
                    SettingNames.LogLifeTime
                );

        // Change
        update[SettingNames.DisablePCRFromMPEG2Generation].NewValue = settings.DisablePCRFromMPEG2.ToString();
        update[SettingNames.DisablePCRFromH264Generation].NewValue = settings.DisablePCRFromH264.ToString();
        update[SettingNames.ArchiveLifeTime].NewValue = settings.ArchiveTime.ToString();
        update[SettingNames.LogLifeTime].NewValue = settings.ProtocolTime.ToString();
        update[SettingNames.LoggingLevel].NewValue = settings.Logging.ToString();

        // Process
        return updateConfig.UpdateConfiguration(update.Values);
    }

    /// <summary>
    /// Nur für die Entwicklung.
    /// </summary>
    [HttpPut]
    public void Restart() => server.Restart();
}

