using System.Text.Json.Serialization;

namespace JMS.DVB.NET.Recording.ProgramGuide;

/// <summary>
/// Die Art der zu suchenden Quelle.
/// </summary>
[Flags, JsonConverter(typeof(JsonStringEnumConverter))]
public enum GuideSourceFilter
{
    /// <summary>
    /// Nur Fernsehsender.
    /// </summary>
    Television = 1,

    /// <summary>
    /// Nur Radiosender.
    /// </summary>
    Radio = 2,

    /// <summary>
    /// Einfach alles.
    /// </summary>
    All = Television | Radio,
}

