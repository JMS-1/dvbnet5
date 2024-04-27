using JMS.DVB.NET.Recording.Persistence;

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
