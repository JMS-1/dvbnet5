using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Requests;

public interface ISourceScanProxyFactory
{
    /// <summary>
    /// Erstellt eine neue Sammlung.
    /// </summary>
    /// <param name="state">Das zugehörige Geräteprofil.</param>
    /// <param name="recording">Die Beschreibung der Aufgabe.</param>
    /// <returns>Die gewünschte Steuerung.</returns>
    SourceScanProxy Create(IProfileState state, VCRRecordingInfo recording);
}

