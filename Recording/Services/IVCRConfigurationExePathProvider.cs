namespace JMS.DVB.NET.Recording.Services;

/// <summary>
/// Schnittstelle zur Ermittelung des Pfades zum Dienst.
/// </summary>
/// <remarks>
/// Wird ausschließlich für die Tests benötigt.
/// </remarks>
public interface IVCRConfigurationExePathProvider
{
    /// <summary>
    /// Der zu verwendende Dateipfad.
    /// </summary>
    string Path { get; }
}

