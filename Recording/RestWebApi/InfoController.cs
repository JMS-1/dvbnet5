using JMS.DVB.NET.Recording.FTPWrap;
using JMS.DVB.NET.Recording.Server;
using JMS.DVB.NET.Recording.Services;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;
using Microsoft.AspNetCore.Mvc;

namespace JMS.DVB.NET.Recording.RestWebApi;

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
    IExtensionManager extensions,
    IFTPWrap ftpServer
) : ControllerBase
{
    /// <summary>
    /// Wird beim Bauen automatisch eingemischt.
    /// </summary>
    private const string CURRENTDATE = "2024/07/25";

    /// <summary>
    /// Aktuelle Version des VCR.NET Recording Service.
    /// </summary>
    public const string CurrentVersion = "5.0 [" + CURRENTDATE + "]";

    /// <summary>
    /// Meldet Informationen zur Version des VCR.NET Recording Service.
    /// </summary>
    [HttpGet]
    public InfoService VersionInformation()
        => new()
        {
            FTPPort = ftpServer.OuterPort,
            GuideUpdateEnabled = configuration.ProgramGuideUpdateEnabled,
            HasPendingExtensions = extensions.HasActiveProcesses,
            InstalledVersion = "5.0.7",
            IsRunning = server.IsActive,
            ProfilesNames = [.. server.ProfileNames],
            SourceScanEnabled = configuration.SourceListUpdateInterval != 0,
            Version = CurrentVersion,
        };

    /// <summary>
    /// Meldet alle möglichen Aufzeichnungsverzeichnisse.
    /// </summary>
    /// <returns>Die gewünschte Liste.</returns>
    [HttpGet("folder")]
    public string[] GetRecordingDirectories() => configuration.TargetDirectoryNames.SelectMany(ScanDirectory).ToArray();

    /// <summary>
    /// Meldet alle Aufträge.
    /// </summary>
    /// <returns>Die Liste aller Aufträge.</returns>
    [HttpGet("jobs")]
    public InfoJob[] GetJobs()
        => [.. jobs
            .GetJobs(InfoJob.Create, profiles)
            .OrderBy(job => job.Name ?? string.Empty, StringComparer.InvariantCulture)];

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

