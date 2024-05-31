using JMS.DVB.NET.Recording.RestWebApi;
using JMS.DVB.NET.Recording.Server;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Actions;

public class LogQuery(IVCRServer server, IJobManager jobs) : ILogQuery
{

    /// <inheritdoc/>
    public ProtocolEntry[] Query(string profileName, DateTime start, DateTime end)
    {
        // Locate profile and forward call
        if (string.IsNullOrEmpty(profileName))
            return [];

        var profile = server.FindProfile(profileName);

        return (profile == null)
            ? []
            : jobs.FindLogEntries(start, end, profile).Select(ProtocolEntry.Create).ToArray();
    }
}