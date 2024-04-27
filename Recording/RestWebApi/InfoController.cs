using JMS.DVB.NET.Recording.Server;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;
using Microsoft.AspNetCore.Mvc;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Web Service für allgemeine Informationen.
    /// </summary>
    [ApiController]
    [Route("api/info")]
    public class InfoController(
        IVCRConfiguration configuration,
        IVCRServer server,
        IVCRProfiles profiles,
        IJobManager jobs,
        IExtensionManager extensions
    ) : ControllerBase
    {
        /// <summary>
        /// Wird beim Bauen automatisch eingemischt.
        /// </summary>
        private const string CURRENTDATE = "2024/04/26";

        /// <summary>
        /// Aktuelle Version des VCR.NET Recording Service.
        /// </summary>
        public const string CurrentVersion = "5.0 [" + CURRENTDATE + "]";

        /// <summary>
        /// Die exakte Version der Installation.
        /// </summary>
        private static volatile string _InstalledVersion = null!;

        /// <summary>
        /// Sorgt dafür, dass die Version nur einmalig ermittelt wird.
        /// </summary>
        private static object _VersionLock = new();

        /// <summary>
        /// Meldet die exakte Version der Installation.
        /// </summary>
        private static string InstalledVersion
        {
            get
            {
                // Load once
                if (_InstalledVersion == null)
                    lock (_VersionLock)
                        if (_InstalledVersion == null)
                        {
                            // Process
                            try
                            {
                                _InstalledVersion = "?";
                            }
                            catch (Exception e)
                            {
                                // Report
                                Tools.ExtendedLogging("Unable to retrieve MSI Product Version: {0}", e.Message);
                            }

                            // Default
                            if (_InstalledVersion == null)
                                _InstalledVersion = "-";
                        }

                // Report
                return _InstalledVersion;
            }
        }

        /// <summary>
        /// Meldet Informationen zur Version des VCR.NET Recording Service.
        /// </summary>
        [HttpGet("info")]
        public InfoService VersionInformation()
        {
            // Report
            return
                new InfoService
                {
                    GuideUpdateEnabled = configuration.ProgramGuideUpdateEnabled,
                    HasPendingExtensions = extensions.HasActiveProcesses,
                    InstalledVersion = InstalledVersion,
                    IsRunning = server.IsActive,
                    ProfilesNames = server.ProfileNames.ToArray(),
                    SourceScanEnabled = configuration.SourceListUpdateInterval != 0,
                    Version = CurrentVersion,
                };
        }

        /// <summary>
        /// Meldet alle möglichen Aufzeichnungsverzeichnisse.
        /// </summary>
        /// <param name="directories">Wird zur Unterscheidung der Methoden verwendet.</param>
        /// <returns>Die gewünschte Liste.</returns>
        [HttpGet("folder")]
        public string[] GetRecordingDirectories(string directories) => configuration.TargetDirectoryNames.SelectMany(ScanDirectory).ToArray();

        /// <summary>
        /// Meldet alle Aufträge.
        /// </summary>
        /// <returns>Die Liste aller Aufträge.</returns>
        [HttpGet("jobs")]
        public InfoJob[] GetJobs()
        {
            // Report
            return
                jobs
                    .GetJobs(InfoJob.Create, profiles)
                    .OrderBy(job => job.Name ?? string.Empty, StringComparer.InvariantCulture)
                    .ToArray();
        }

        /// <summary>
        /// Meldet den Namen eines Verzeichnisses und rekursiv alle Unterverzeichnisse.
        /// </summary>
        /// <param name="directory">Das zu untersuchende Verzeichnis.</param>
        /// <returns>Die Liste der Verzeichnisse.</returns>
        private static IEnumerable<string> ScanDirectory(string directory)
        {
            // See if the directory is valid
            DirectoryInfo? info;
            try
            {
                // Create
                info = new DirectoryInfo(directory);
            }
            catch (Exception)
            {
                // Not valid
                info = null;
            }

            // Done
            if (info == null)
                yield break;

            // Report self - always, even if it does not exist
            yield return info.FullName;

            // Get the sub directories
            string[]? children;
            try
            {
                // Try if directory exists
                if (info.Exists)
                    children = info.GetDirectories().Select(child => child.FullName).ToArray();
                else
                    children = null;
            }
            catch (Exception)
            {
                // None
                children = null;
            }

            // Forward
            if (children != null)
                foreach (var child in children.SelectMany(ScanDirectory))
                    yield return child;
        }
    }
}
