using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;

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
    public partial class LegacyVCRServer(IVCRConfiguration configuration, ILogger logger, IVCRProfiles profiles, IJobManager jobManager, IVCRServer states)
    {
        /// <summary>
        /// Wird beim Bauen automatisch eingemischt.
        /// </summary>
        private const string CURRENTDATE = "2024/04/23";

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
        internal IVCRServer Profiles { get; private set; } = states;

        /// <summary>
        /// Die aktive Konfiguration.
        /// </summary>
        private readonly IVCRConfiguration _configuration = configuration;

        private readonly IVCRProfiles _profiles = profiles;

        /// <summary>
        /// Die Verwaltung der Aufträge.
        /// </summary>
        private IJobManager _jobs = jobManager;

        /// <summary>
        /// L?dt Verwaltungsinstanzen für alle freigeschalteten DVB.NET Ger?teprofile.
        /// </summary>
        static LegacyVCRServer()
        {
            // Report
            Tools.ExtendedLogging("VCRServer static Initialisation started");

            // Set the default directory
            Environment.CurrentDirectory = Tools.ApplicationDirectory.FullName;

            // Report
            Tools.ExtendedLogging("VCRServer static Initialisation completed");
        }

        public readonly ILogger _logger = logger;

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
    }
}
