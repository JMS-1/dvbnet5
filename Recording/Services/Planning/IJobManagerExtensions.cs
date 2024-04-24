namespace JMS.DVB.NET.Recording.Services.Planning;

public static class IJobManagerExtensions
{
    /// <summary>
    /// Führt periodische Aufr?umarbeiten aus.
    /// </summary>
    public static void PeriodicCleanup(this IJobManager jobs)
    {
        // Forward
        jobs.CleanupArchivedJobs();
        jobs.CleanupLogEntries();
    }
}