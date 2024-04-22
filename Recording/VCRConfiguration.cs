using System.Configuration;
using System.Reflection;
using JMS.DVB.NET.Recording.Services;
using Microsoft.Extensions.Logging;

namespace JMS.DVB.NET.Recording
{
    /// <summary>
    /// Schnittstelle zur Ermittelung des Pfades zum Dienst.
    /// </summary>
    /// <remarks>
    /// Wird ausschließlich für die Tests benötigt.
    /// </remarks>
    public interface IVCRConfigurationExePathProvider
    {
        /// <summary>
        /// Der zu verwendende Dateipfad.
        /// </summary>
        string Path { get; }
    }

    /// <summary>
    /// Verwaltet die Konfiguration des VCR.NET Recording Service.
    /// </summary>
    /// <remarks>LEAF SINGLETON SERVICE.</remarks>
    public class VCRConfiguration
    {
        /// <summary>
        /// Beschreibt eine einzelne Einstellung.
        /// </summary>
        public abstract class SettingDescription : ICloneable
        {
            /// <summary>
            /// Meldet den Namen der Einstellung.
            /// </summary>
            public SettingNames Name { get; }

            /// <summary>
            /// Ein eventuell veränderter Wert.
            /// </summary>
            public string NewValue { get; set; } = null!;

            protected readonly VCRConfiguration VCRConfiguration;

            /// <summary>
            /// Erzeugt eine neue Beschreibung.
            /// </summary>
            /// <param name="name">Der Name der Beschreibung.</param>
            /// <param name="vcrConfiguration">Die zugehörige Konfiguration.</param>
            internal SettingDescription(SettingNames name, VCRConfiguration vcrConfiguration)
            {
                VCRConfiguration = vcrConfiguration;
                Name = name;
            }

            /// <summary>
            /// Liest den Namen der Einstellung.
            /// </summary>
            /// <returns>Der Wert der Einstellung als Zeichenkette.</returns>
            public virtual object ReadValue() => GetCurrentValue();

            /// <summary>
            /// Liest den Namen der Einstellung.
            /// </summary>
            /// <returns>Der Wert der Einstellung als Zeichenkette.</returns>
            public string GetCurrentValue() => ReadRawValue(VCRConfiguration._configuration)!;

            /// <summary>
            /// Liest den Namen der Einstellung.
            /// </summary>
            /// <param name="configuration">Die zu verwendende Konfiguration.</param>
            /// <returns>Der Wert der Einstellung als Zeichenkette.</returns>
            private string ReadRawValue(Configuration configuration) => configuration.AppSettings.Settings[Name.ToString()]?.Value?.Trim()!;

            /// <summary>
            /// Aktualisiert einen Konfigurationswert.
            /// </summary>
            /// <param name="newConfiguration"></param>
            /// <returns></returns>
            internal bool Update(Configuration newConfiguration)
            {
                // Corret
                var newValue = string.IsNullOrEmpty(NewValue) ? string.Empty : NewValue.Trim();

                // Not changed
                if (Equals(newValue, ReadRawValue(newConfiguration)))
                    return false;

                // Load the setting
                var setting = newConfiguration.AppSettings.Settings[Name.ToString()];

                // Ups, missing
                if (setting == null)
                    newConfiguration.AppSettings.Settings.Add(Name.ToString(), newValue);
                else
                    setting.Value = newValue;

                // Report
                return true;
            }

            #region ICloneable Members

            /// <summary>
            /// Erzeugt eine Kopie des Eintrags.
            /// </summary>
            /// <returns>Die gewünschte Kopie.</returns>
            protected abstract SettingDescription CreateClone();

            /// <summary>
            /// Erzeugt eine Kopie des Eintrags.
            /// </summary>
            /// <returns>Die gewünschte Kopie.</returns>
            public SettingDescription Clone() => CreateClone();

            /// <summary>
            /// Erzeugt eine Kopie des Eintrags.
            /// </summary>
            /// <returns>Die gewünschte Kopie.</returns>
            object ICloneable.Clone() => Clone();

            #endregion
        }

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
                        return (TValueType)resultType.InvokeMember("Parse", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { setting })!;
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
        private readonly ILogger<VCRConfiguration> _logger;

        /// <summary>
        /// Pfad zum Dienst.
        /// </summary>
        private readonly string _configurationExePath;

        /// <summary>
        /// Die aktuell gültige Konfiguration.
        /// </summary>
        private Configuration _configuration = null!;

        /// <summary>
        /// Initialisiert eine neue Konfiguration.
        /// </summary>
        /// <param name="configurationExePath">Pfad zum Dienst.</param>
        /// <param name="logger">Protokollierungshelfer.</param>
        public VCRConfiguration(IVCRConfigurationExePathProvider configurationExePath, ILogger<VCRConfiguration> logger)
        {
            _logger = logger;

            _logger.LogTrace("New Configuration Instance Created");

            // Get the path of the configuration and load the initial configuration.
            _configurationExePath = configurationExePath.Path;

            Reload();

            // Remember all
            Add(SettingNames.AdditionalRecorderPaths);
            Add(SettingNames.AllowBasic, false);
            Add(SettingNames.ArchiveLifeTime, (uint)5);
            Add(SettingNames.DelayAfterForcedHibernation, (uint)5);
            Add(SettingNames.DisablePCRFromH264Generation, false);
            Add(SettingNames.DisablePCRFromMPEG2Generation, false);
            Add(SettingNames.EPGDuration, (uint)15);
            Add(SettingNames.EPGHours);
            Add(SettingNames.EPGIncludeFreeSat, false);
            Add(SettingNames.EPGInterval, (uint?)null);
            Add(SettingNames.EPGJoinThreshold, (uint?)null);
            Add(SettingNames.EPGStations);
            Add(SettingNames.FileNamePattern, "%Job% - %Schedule% - %Start%");
            Add(SettingNames.HibernationDelay, (uint)60);
            Add(SettingNames.LoggingLevel, LoggingLevel.Full);
            Add(SettingNames.LogLifeTime, (uint)5);
            Add(SettingNames.MayHibernateSystem, false);
            Add(SettingNames.MergeScanResult, true);
            Add(SettingNames.Profiles);
            Add(SettingNames.RequiredAdminRole);
            Add(SettingNames.RequiredUserRole);
            Add(SettingNames.ScanDuration, (uint)60);
            Add(SettingNames.ScanHours);
            Add(SettingNames.ScanInterval, 0);
            Add(SettingNames.ScanJoinThreshold, (uint?)null);
            Add(SettingNames.SSLPort, (ushort)3909);
            Add(SettingNames.SuppressDelayAfterForcedHibernation, false);
            Add(SettingNames.TCPPort, (ushort)2909);
            Add(SettingNames.TSAudioBufferSize, 0);
            Add(SettingNames.TSHDTVBufferSize, 0);
            Add(SettingNames.TSSDTVBufferSize, 0);
            Add(SettingNames.UseExternalCardServer, true);
            Add(SettingNames.UseSSL, false);
            Add(SettingNames.UseStandByForHibernation, false);
            Add(SettingNames.VideoRecorderDirectory, "Recordings");

            // Set restart items
            m_Restart[SettingNames.AllowBasic] = true;
            m_Restart[SettingNames.Profiles] = true;
            m_Restart[SettingNames.SSLPort] = true;
            m_Restart[SettingNames.TCPPort] = true;
            m_Restart[SettingNames.UseSSL] = true;
        }

        /// <summary>
        /// Load the configuration from the provided file.
        /// </summary>
        public void Reload()
            => _configuration = ConfigurationManager.OpenExeConfiguration(_configurationExePath);

        /// <summary>
        /// Bereitet eine Aktualisierung vor.
        /// </summary>
        /// <param name="names">Die zu aktualisierenden Einträge.</param>
        /// <returns>Alle gewünschten Einträge.</returns>
        public Dictionary<SettingNames, SettingDescription> BeginUpdate(params SettingNames[] names) =>
            names == null ? [] : names.ToDictionary(n => n, n => m_Settings[n].Clone());

        /// <summary>
        /// Führt eine Aktualisierung aus.
        /// </summary>
        /// <param name="settings">Die eventuell veränderten Einstellungen.</param>
        /// <returns>Gesetzt, wenn ein Neustart erforderlich war.</returns>
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

        /// <summary>
        /// Meldet den Namen der Kontogruppe der Anwender, die Zugriff auf den
        /// VCR.NET Recording Service haben.
        /// </summary>
        public string UserRole => ReadStringSetting(SettingNames.RequiredUserRole);

        /// <summary>
        /// Meldet oder legt fest, ob bereits einmal eine Aufzeichnung ausgeführt wurde.
        /// </summary>
        private volatile bool m_HasRecordedSomething;

        /// <summary>
        /// Meldet oder legt fest, ob bereits einmal eine Aufzeichnung ausgeführt wurde.
        /// </summary>
        public bool HasRecordedSomething { get { return m_HasRecordedSomething; } set { m_HasRecordedSomething = value; } }

        /// <summary>
        /// Meldet den Namen der Kontogruppe der Anwender, die administrativen Zugriff auf den
        /// VCR.NET Recording Service haben.
        /// </summary>
        public string AdminRole => ReadStringSetting(SettingNames.RequiredAdminRole);

        /// <summary>
        /// Meldet, ob der Schlafzustand S3 (Standby) anstelle von S4 (Hibernate)
        /// verwenden soll.
        /// </summary>
        public bool UseS3ForHibernate => ReadSetting<bool>(SettingNames.UseStandByForHibernation);

        /// <summary>
        /// Meldet die Größe für die Zwischenspeicherung bei Radioaufnahmen.
        /// </summary>
        public int? AudioBufferSize
        {
            get
            {
                // Process
                var buffer = ReadSetting<int>(SettingNames.TSAudioBufferSize);

                return (buffer < 1) ? null : Math.Max(1000, buffer);
            }
        }

        /// <summary>
        /// Meldet die Größe für die Zwischenspeicherung bei Fernsehaufnahmen normaler Qualität.
        /// </summary>
        public int? StandardVideoBufferSize
        {
            get
            {
                // Process
                var buffer = ReadSetting<int>(SettingNames.TSSDTVBufferSize);

                return (buffer < 1) ? null : Math.Max(1000, buffer);
            }
        }

        /// <summary>
        /// Meldet die Größe für die Zwischenspeicherung bei Fernsehaufnahmen mit hoher Auflösung.
        /// </summary>
        public int? HighDefinitionVideoBufferSize
        {
            get
            {
                // Process
                var buffer = ReadSetting<int>(SettingNames.TSHDTVBufferSize);

                return (buffer < 1) ? null : Math.Max(1000, buffer);
            }
        }

        /// <summary>
        /// Meldet, ob der VCR.NET Recording Service den Rechner in einen Schlafzustand
        /// versetzten darf.
        /// </summary>
        public bool MayHibernateSystem => ReadSetting<bool>(SettingNames.MayHibernateSystem);

        /// <summary>
        /// Meldet die geschätzte Zeit, die dieses System maximal braucht, um aus dem
        /// Schlafzustand zu erwachen.
        /// </summary>
        public uint HibernationDelay => ReadSetting<uint>(SettingNames.HibernationDelay);

        /// <summary>
        /// Meldet, ob der <i>Card Server</i> als eigenständiger Prozess gestartet werden soll.
        /// </summary>
        public bool UseExternalCardServer => ReadSetting<bool>(SettingNames.UseExternalCardServer);

        /// <summary>
        /// Gesetzt wenn es nicht gestattet ist, aus einem H.264 Bildsignal die Zeitbasis (PCR)
        /// abzuleiten.
        /// </summary>
        public bool DisablePCRFromH264Generation => ReadSetting<bool>(SettingNames.DisablePCRFromH264Generation);

        /// <summary>
        /// Gesetzt wenn es nicht gestattet ist, aus einem MPEG2 Bildsignal die Zeitbasis (PCR)
        /// abzuleiten.
        /// </summary>
        public bool DisablePCRFromMPEG2Generation => ReadSetting<bool>(SettingNames.DisablePCRFromMPEG2Generation);

        /// <summary>
        /// Meldet, ob die Programmzeitschrift der englischen FreeSat Sender eingeschlossen 
        /// werden soll.
        /// </summary>
        public bool EnableFreeSat => ReadSetting<bool>(SettingNames.EPGIncludeFreeSat);

        /// <summary>
        /// Die Liste der Quellen, die in der Programmzeitschrift berücksichtigt werden sollen.
        /// </summary>
        public string ProgramGuideSourcesRaw => ReadStringSetting(SettingNames.EPGStations);

        /// <summary>
        /// Meldet alle Quellen, für die Daten gesammelt werden sollen.
        /// </summary>
        public string[] ProgramGuideSourcesAsArray => ProgramGuideSources.ToArray();

        /// <summary>
        /// Meldet alle Quellen, für die Daten gesammelt werden sollen.
        /// </summary>
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

        /// <summary>
        /// Meldet, ob eine Aktialisierung der Programmzeitschrift überhaupt stattfinden soll.
        /// </summary>
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

        /// <summary>
        /// Meldet alle vollen Stunden, zu denen eine Sammlung stattfinden soll.
        /// </summary>
        public uint[] ProgramGuideUpdateHours => GetHourList(ProgramGuideUpdateHoursRaw).ToArray();

        /// <summary>
        /// Meldet alle vollen Stunden, zu denen eine Sammlung stattfinden soll.
        /// </summary>
        public uint[] ProgramGuideUpdateHoursAsArray => ProgramGuideUpdateHours.ToArray();

        /// <summary>
        /// Meldet die maximale Laufzeit einer Aktualisierung gemäß der Konfiguration.
        /// </summary>
        public uint ProgramGuideUpdateDuration => ReadSetting<uint>(SettingNames.EPGDuration);

        /// <summary>
        /// Meldet die minimale Zeitspanne zwischen zwei Aktualisierungen der Programmzeitschrift.
        /// </summary>
        public TimeSpan? ProgramGuideUpdateInterval
        {
            get
            {
                // Load
                var interval = ReadSetting<uint?>(SettingNames.EPGInterval);

                return interval.HasValue ? TimeSpan.FromHours(interval.Value) : null;
            }
        }

        /// <summary>
        /// Meldet die Zeitspanne, nachdem eine Aktualisierung der Programmzeitschrift vorgezogen
        /// erfolgen darf.
        /// </summary>
        public TimeSpan? ProgramGuideJoinThreshold
        {
            get
            {
                // Load
                var interval = ReadSetting<uint?>(SettingNames.EPGJoinThreshold);

                return interval.HasValue ? TimeSpan.FromHours(interval.Value) : null;
            }
        }

        /// <summary>
        /// Meldet die Zeitspanne, nachdem eine Aktualisierung der Quellen vorgezogen
        /// erfolgen darf.
        /// </summary>
        public TimeSpan? SourceListJoinThreshold
        {
            get
            {
                // Load
                var interval = ReadSetting<uint?>(SettingNames.ScanJoinThreshold);

                return interval.HasValue ? TimeSpan.FromDays(interval.Value) : null;
            }
        }

        /// <summary>
        /// Meldet die maximale Laufzeit für die Aktualisierung der Quellen eines Geräteprofils.
        /// </summary>
        public uint SourceListUpdateDuration => ReadSetting<uint>(SettingNames.ScanDuration);

        /// <summary>
        /// Meldet, wieviele Tage mindestens zwischen zwei Aktualisierungen der Liste
        /// der Quellen eines Geräteprofils liegen müssen.
        /// </summary>
        public int SourceListUpdateInterval => ReadSetting<int>(SettingNames.ScanInterval);

        /// <summary>
        /// Meldet, ob nach Abschluss der Aktualisierung die Listen der Quellen zusammengeführt
        /// werden sollen.
        /// </summary>
        public bool MergeSourceListUpdateResult => ReadSetting<bool>(SettingNames.MergeScanResult);

        /// <summary>
        /// Meldet die Liste der Stunden, an denen eine Aktualisierung einer
        /// Liste von Quellen stattfinden darf.
        /// </summary>
        public string SourceListUpdateHoursRaw => ReadStringSetting(SettingNames.ScanHours);

        /// <summary>
        /// Meldet alle vollen Stunden, zu denen eine Sammlung stattfinden soll.
        /// </summary>
        public uint[] SourceListUpdateHours => GetHourList(SourceListUpdateHoursRaw).ToArray();

        /// <summary>
        /// Meldet alle vollen Stunden, zu denen eine Sammlung stattfinden soll.
        /// </summary>
        public uint[] SourceListUpdateHoursAsArray => SourceListUpdateHours.ToArray();

        /// <summary>
        /// Meldet die Liste der Stunden, an denen eine Aktualisierung einer
        /// Programmzeitschrift stattfinden darf.
        /// </summary>
        public string ProgramGuideUpdateHoursRaw => ReadStringSetting(SettingNames.EPGHours);

        /// <summary>
        /// Meldet die Zeit in Wochen, die ein Protokolleintrag vorgehalten wird.
        /// </summary>
        public uint LogLifeTime => ReadSetting<uint>(SettingNames.LogLifeTime);

        /// <summary>
        /// Meldet die maximale Verweildauer eines archivierten Auftrags im Archiv, bevor
        /// er gelöscht wird.
        /// </summary>
        public uint ArchiveLifeTime => ReadSetting<uint>(SettingNames.ArchiveLifeTime);

        /// <summary>
        /// Meldet die Zeit die nach einem erzwungenen Schlafzustand verstreichen muss, bevor der
        /// Rechner für eine Aufzeichnung aufgweckt wird.
        /// </summary>
        public uint RawDelayAfterForcedHibernation => ReadSetting<uint>(SettingNames.DelayAfterForcedHibernation);

        /// <summary>
        /// Meldet die Zeit die nach einem erzwungenen Schlafzustand verstreichen muss, bevor der
        /// Rechner für eine Aufzeichnung aufgweckt wird.
        /// </summary>
        public TimeSpan DelayAfterForcedHibernation => TimeSpan.FromMinutes(Math.Max(1, RawDelayAfterForcedHibernation));

        /// <summary>
        /// Gesetzt, wenn beim Schlafzustand keine Sonderbehandlung erwünscht ist.
        /// </summary>
        public bool SuppressDelayAfterForcedHibernation => ReadSetting<bool>(SettingNames.SuppressDelayAfterForcedHibernation);

        /// <summary>
        /// Meldet den aktuellen Umfang der Protokollierung.
        /// </summary>
        public LoggingLevel LoggingLevel => ReadSetting<LoggingLevel>(SettingNames.LoggingLevel);

        /// <summary>
        /// Meldet den TCP/IP Port, an den der Web Server gebunden werden soll.
        /// </summary>
        public ushort WebServerTcpPort => ReadSetting<ushort>(SettingNames.TCPPort);

        /// <summary>
        /// Meldet den TCP/IP Port, an den der Web Server bei einer sichen Verbindung gebunden werden soll.
        /// </summary>
        public ushort WebServerSecureTcpPort => ReadSetting<ushort>(SettingNames.SSLPort);

        /// <summary>
        /// Gesetzt, wenn die Verbindung zu den Web Diensten verschlüsselt werden soll.
        /// </summary>
        public bool EncryptWebCommunication => ReadSetting<bool>(SettingNames.UseSSL);

        /// <summary>
        /// Gesetzt, wenn die Anwender sich auch über das Basic Prototokoll
        /// autorisieren dürfen.
        /// </summary>
        public bool EnableBasicAuthentication => ReadSetting<bool>(SettingNames.AllowBasic);

        /// <summary>
        /// Ermittelt das Ersetzungsmuster für Dateinamen.
        /// </summary>
        public string FileNamePattern => ReadStringSetting(SettingNames.FileNamePattern);

        /// <summary>
        /// Ermittelt den Namen des primären Aufzeichnungsverzeichnisses
        /// </summary>
        public string PrimaryRecordingDirectory => ReadStringSetting(SettingNames.VideoRecorderDirectory);

        /// <summary>
        /// Meldet die zusätzlichen Aufzeichnungsverzeichnisse.
        /// </summary>
        public string AlternateRecordingDirectories => ReadStringSetting(SettingNames.AdditionalRecorderPaths);

        /// <summary>
        /// Meldet die Namen der DVB.NET Geräteprofile, die der VCR.NET Recording Service
        /// verwenden darf.
        /// </summary>
        public string ProfileNames => ReadStringSetting(SettingNames.Profiles);

        /// <summary>
        /// Meldet das primäre Aufzeichnungsverzeichnis.
        /// </summary>
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

        /// <summary>
        /// Meldet alle erlaubten Aufzeichnungsverzeichnisse.
        /// </summary>
        public string[] TargetDirectoriesNames => TargetDirectories.Select(d => d.FullName).ToArray();

        /// <summary>
        /// Meldet alle erlaubten Aufzeichnungsverzeichnisse.
        /// </summary>
        public IEnumerable<DirectoryInfo> TargetDirectories
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
                        if (path[path.Length - 1] != Path.DirectorySeparatorChar)
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

        /// <summary>
        /// Prüft, ob eine Datei in einem zulässigen Aufzeichnungsverzeichnis liegt.
        /// </summary>
        /// <param name="path">Der zu prüfende Dateipfad.</param>
        /// <returns>Gesetzt, wenn der Pfad gültig ist.</returns>
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
        public static IEnumerable<uint> GetHourList(string hours)
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
}
