using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Requests;

public interface IProgramGuideProxyFactory
{
    /// <summary>
    /// Erstellt eine neue Aktualisierung.
    /// </summary>
    /// <param name="state">Das zugehörige Geräteprofil.</param>
    /// <param name="recording">Beschreibt die Aufzeichnung.</param>
    /// <returns>Die gewünschte Steuerung.</returns>
    /// <exception cref="ArgumentNullException">Es wurden nicht alle Parameter angegeben.</exception>
    ProgramGuideProxy Create(IProfileState state, VCRRecordingInfo recording);
}
