using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.Requests;

public interface IZappingProxyFactory
{
    /// <summary>
    /// Erstellt einen neuen Zugriff.
    /// </summary>
    /// <param name="profile">Das zu verwendende Geräteprofil.</param>
    /// <param name="target">Die aktuelle Zieladresse für die Nutzdaten.</param>
    /// <returns>Die gewünschte Steuerung.</returns>
    /// <exception cref="ArgumentNullException">Mindestens ein Parameter wurde nicht angegeben.</exception>
    ZappingProxy Create(IProfileState profile, string target);
}

