using JMS.DVB.NET.Recording.RestWebApi;

namespace JMS.DVB.NET.Recording.Actions;

public interface ILogQuery
{
    /// <summary>
    /// Liest einen Auszug aus einem Protokoll.
    /// </summary>
    /// <param name="profileName">Der Name des betroffenen Geräteprofils.</param>
    /// <param name="start">Das Startdatum.</param>
    /// <param name="end">Das Enddatum.</param>
    /// <returns>Die angeforderten ProtokollEinträge.</returns>
    ProtocolEntry[] Query(string profileName, DateTime start, DateTime end);
}
