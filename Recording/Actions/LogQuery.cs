using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Server;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Actions;

public class LogQuery(IVCRServer server, IJobManager jobs) : ILogQuery
{

    /// <inheritdoc/>
    public TEntry[] Query<TEntry>(string profileName, DateTime start, DateTime end, Func<VCRRecordingInfo, TEntry> factory)
    {
        // Locate profile and forward call
        if (string.IsNullOrEmpty(profileName))
            return [];

        var profile = server.FindProfile(profileName);

        return (profile == null)
            ? []
            : jobs.FindLogEntries(start, end, profile).Select(factory).ToArray();
    }
}