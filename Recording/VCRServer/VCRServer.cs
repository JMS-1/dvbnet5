using System.Text;
using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Requests;
using JMS.DVB.NET.Recording.Services;

namespace JMS.DVB.NET.Recording
{
    public class RegistryKey
    {
        public object? GetValue(string name) => null;

        public void SetValue(string name, object? value) { }

        public void DeleteValue(string name, bool throwOnMissingValue) { }
    }


    /// <summary>
    /// Von dieser Klasse existiert im Allgemeinen nur eine einzige Instanz. Sie
    /// realisiert die fachlische Logik des VCR.NET Recording Service.
    /// </summary>
    public partial class VCRServer : IDisposable
    {
        /// <summary>
        /// Wird zum Neustart des Dienstes ausgelöst.
        /// </summary>
        public Action? Restart;

        /// <summary>
        /// Wird beim Bauen automatisch eingemischt.
        /// </summary>
        private const string CURRENTDATE = "2024/04/21";

        /// <summary>
        /// Aktuelle Version des VCR.NET Recording Service.
        /// </summary>
        public const string CurrentVersion = "5.0 [" + CURRENTDATE + "]";

        /// <summary>
        /// Konfigurationseintrag in der Registrierung von Windows.
        /// </summary>
        public static readonly RegistryKey ServiceRegistry = new();

        /// <summary>
        /// Die zugeh?rige Verwaltung der aktiven Ger?teprofile.
        /// </summary>
        internal ProfileStateCollection Profiles { get; private set; }

        /// <summary>
        /// Alle Prozesse, die gestartet wurden.
        /// </summary>
        public readonly ExtensionManager ExtensionProcessManager = new ExtensionManager();

        /// <summary>
        /// Die aktive Konfiguration.
        /// </summary>
        private readonly IVCRConfiguration _configuration;

        private readonly IVCRProfiles _profiles;

        /// <summary>
        /// Die Verwaltung der Aufträge.
        /// </summary>
        private IJobManager _jobs = null!;

        /// <summary>
        /// L?dt Verwaltungsinstanzen f?r alle freigeschalteten DVB.NET Ger?teprofile.
        /// </summary>
        static VCRServer()
        {
            // Report
            Tools.ExtendedLogging("VCRServer static Initialisation started");

            // Set the default directory
            Environment.CurrentDirectory = Tools.ApplicationDirectory.FullName;

            // Report
            Tools.ExtendedLogging("VCRServer static Initialisation completed");
        }

        public readonly ILogger _logger;

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        public VCRServer(IVCRConfiguration configuration, ILogger logger, IVCRProfiles profiles, IJobManager jobManager, ServiceFactory factory)
        {
            _configuration = configuration;
            _logger = logger;
            _profiles = profiles;

            _jobs = jobManager;

            // Prepare profiles
            _profiles.Reset(this);

            // Create profile state manager and start it up
            Profiles = new ProfileStateCollection(this, _profiles, logger, _jobs, factory);
        }

        /// <summary>
        /// Meldet die aktuell zu verwendende Konfiguration.
        /// </summary>
        public IVCRConfiguration Configuration { get { return _configuration; } }

        public class _Setting
        {
            public string Value { get; set; } = null!;
        }

        public class _Settings
        {
            public _Setting? this[string key] => throw new NotImplementedException("Configuration");

            public void Add(string key, string value) => throw new NotImplementedException("Configuration");
        }

        /// <summary>
        /// Meldet die aktuellen Einstellungen des VCR.NET Recording Service.
        /// </summary>
        public _Settings Settings
        {
            get
            {
                // Create a new instance
                var result = new _Settings { };

                // Load profile names
                //result.Profiles.AddRange(VCRProfiles.ProfileNames);

                // Report
                return result;
            }
        }

        /// <summary>
        /// Ermittelt den Startmodus in der aktuellen <see cref="AppDomain"/>.
        /// </summary>
        public bool InDebugMode { get { return Tools.DebugMode; } }

        /// <summary>
        /// Ermittelt ein Ger?teprofil und meldet einen Fehler, wenn keins gefunden wurde.
        /// </summary>
        /// <param name="profileName">Der Name des gew?nschten Ger?teprofils.</param>
        /// <returns>Der Zustand des Profils.</returns>
        public IProfileState? FindProfile(string profileName)
        {
            // Forward
            var state = Profiles[profileName];
            if (state == null)
                _logger.LogError("Es gibt kein Geräteprofil '{0}'", profileName);

            // Report
            return state;
        }

        /// <summary>
        /// Ermittelt alle Quellen eines Ger?teprofils f?r die Nutzung durch den <i>LIVE</i> Zugang.
        /// </summary>
        /// <typeparam name="TTarget">Die Art der Zielklasse.</typeparam>
        /// <param name="profileName">Der Name des Ger?teprofils.</param>
        /// <param name="withTV">Gesetzt, wenn Fernsehsender zu ber?cksichtigen sind.</param>
        /// <param name="withRadio">Gesetzt, wenn Radiosender zu ber?cksichtigen sind.</param>
        /// <param name="factory">Eine Methode zum Erzeugen der Zielelemente aus den Daten einer einzelnen Quelle.</param>
        /// <returns></returns>
        public TTarget[] GetSources<TTarget>(string profileName, bool withTV, bool withRadio, Func<SourceSelection, IVCRProfiles, TTarget> factory)
        {
            // Find the profile
            var profile = FindProfile(profileName);
            if (profile == null)
                return [];

            // Create matcher
            Func<Station, bool> matchStation;
            if (withTV)
                if (withRadio)
                    matchStation = station => true;
                else
                    matchStation = station => station.SourceType == SourceTypes.TV;
            else
                if (withRadio)
                matchStation = station => station.SourceType == SourceTypes.Radio;
            else
                return [];

            // Filter all we want
            return
                _profiles
                    .GetSources(profile.ProfileName, matchStation)
                    .Select(s => factory(s, _profiles))
                    .ToArray();
        }

        /// <summary>
        /// Ermittelt eine Quelle.
        /// </summary>
        /// <param name="profile">Das zu verwendende Ger?teprofil.</param>
        /// <param name="name">Der (hoffentlicH) eindeutige Name der Quelle.</param>
        /// <returns>Die Beschreibung der Quelle.</returns>
        public SourceSelection? FindSource(string profile, string name) => _profiles.FindSource(profile, name);

        /// <summary>
        /// Meldet die aktuellen Regeln f?r die Aufzeichnungsplanung.
        /// </summary>
        public string SchedulerRules
        {
            get
            {
                // Attach to the path
                var rulePath = Profiles.ScheduleRulesPath;
                if (File.Exists(rulePath))
                    using (var reader = new StreamReader(rulePath, true))
                        return reader.ReadToEnd().Replace("\r\n", "\n");

                // Not set
                return null!;
            }
            set
            {
                // Check mode
                var rulePath = Profiles.ScheduleRulesPath;
                if (string.IsNullOrWhiteSpace(value))
                {
                    // Back to default
                    if (File.Exists(rulePath))
                        File.Delete(rulePath);
                }
                else
                {
                    // Update line feeds
                    var content = value.Replace("\r\n", "\n").Replace("\n", "\r\n");
                    var scratchFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

                    // Write to scratch file
                    File.WriteAllText(scratchFile, content, Encoding.UTF8);

                    // With cleanup
                    try
                    {
                        // See if resource manager could be created
                        ResourceManager.Create(scratchFile, ProfileManager.ProfileNameComparer).Dispose();

                        // Try to overwrite
                        File.Copy(scratchFile, rulePath, true);
                    }
                    finally
                    {
                        // Get rid of scratch file
                        File.Delete(scratchFile);
                    }
                }
            }
        }

        /// <summary>
        /// F?hrt periodische Aufr?umarbeiten aus.
        /// </summary>
        public void PeriodicCleanup()
        {
            // Forward
            _jobs.CleanupArchivedJobs();
            _jobs.CleanupLogEntries();
        }

        #region IDisposable Members

        /// <summary>
        /// Beendet die Nutzung dieser Instanz endg?ltig.
        /// </summary>
        public void Dispose()
        {
            // Shutdown profiles
            using (Profiles)
                Profiles = null!;

            // Detach from jobs
            _jobs = null!;
        }

        #endregion
    }
}
