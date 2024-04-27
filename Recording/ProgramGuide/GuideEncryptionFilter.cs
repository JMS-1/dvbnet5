namespace JMS.DVB.NET.Recording.ProgramGuide;

/// <summary>
/// Die Verschlüsselung der Quelle.
/// </summary>
[Flags]
public enum GuideEncryptionFilter
{
    /// <summary>
    /// Nur kostenlose Quellen.
    /// </summary>
    Free = 1,

    /// <summary>
    /// Nur Bezahlsender.
    /// </summary>
    Encrypted = 2,

    /// <summary>
    /// Alle Quellen.
    /// </summary>
    All = Free | Encrypted,
}

