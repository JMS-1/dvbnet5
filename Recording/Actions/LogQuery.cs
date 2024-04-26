using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Actions;

public interface ILogQuery
{
    /// <summary>
    /// Liest einen Auszug aus einem Protokoll.
    /// </summary>
    /// <typeparam name="TEntry">Die Art der Zielinformation.</typeparam>
    /// <param name="profileName">Der Name des betroffenen Geräteprofils.</param>
    /// <param name="start">Das Startdatum.</param>
    /// <param name="end">Das Enddatum.</param>
    /// <param name="factory">Methode zum Erzeugen der externen Darstellung aus den ProtokollEinträgen.</param>
    /// <returns>Die angeforderten ProtokollEinträge.</returns>
    TEntry[] Query<TEntry>(string profileName, DateTime start, DateTime end, Func<VCRRecordingInfo, TEntry> factory);
}

public class LogQuery(IVCRServer server, IJobManager jobs) : ILogQuery
{

    /// <inheritdoc/>
    public TEntry[] Query<TEntry>(string profileName, DateTime start, DateTime end, Func<VCRRecordingInfo, TEntry> factory)
    {
        // Locate profile and forward call
        if (string.IsNullOrEmpty(profileName))
            return [];

        var profile = server.FindProfile(profileName);

        return (profile == null) ? [] : jobs.FindLogEntries(start, end, profile).Select(factory).ToArray();
    }
}