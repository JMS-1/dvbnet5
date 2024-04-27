using System.Xml.Serialization;

namespace JMS.DVB.NET.Recording.ProgramGuide;

/// <summary>
/// This class hold the core EPG information used in the VCR.NET
/// Recording Service.
/// </summary>
[Serializable]
[XmlType("EPGEvent")]
public class ProgramGuideEntry : IComparable
{
    /// <summary>
    /// Wird gesetzt, um eine Umrechnung der Zeiten auf die lokale Zeitzone zu aktivieren.
    /// </summary>
    [XmlIgnore]
    public bool ShowLocalTimes { get; set; }

    /// <summary>
    /// Die Startzeit der Sendung in UTC/GMT Darstellung.
    /// </summary>
    private DateTime m_Start;

    /// <summary>
    /// Start time for the event in UTC/GMT notation.
    /// </summary>
    public DateTime StartTime
    {
        get
        {
            // Report
            if (ShowLocalTimes)
                return m_Start.ToLocalTime();
            else
                return m_Start;
        }
        set
        {
            // Remember
            if (ShowLocalTimes)
                m_Start = value.ToUniversalTime();
            else
                m_Start = value;
        }
    }

    /// <summary>
    /// Transport identifier of the station.
    /// </summary>
    public ushort TransportIdentifier { get; set; }

    /// <summary>
    /// Original netowrk identifier of the station.
    /// </summary>
    public ushort NetworkIdentifier { get; set; }

    /// <summary>
    /// Service identifier of the station.
    /// </summary>
    public ushort ServiceIdentifier { get; set; }

    /// <summary>
    /// Short name of the station.
    /// </summary>
    [XmlElement(IsNullable = true)]
    public string StationName { get; set; } = null!;

    /// <summary>
    /// Der Name der Quell inklusive des Namens des Dienstanbieters.
    /// </summary>
    public string StationAndProviderName { get; set; } = null!;

    /// <summary>
    /// Description of the event.
    /// </summary>
    [XmlElement(IsNullable = true)]
    public string Description { get; set; } = null!;

    /// <summary>
    /// Eine Kurzbeschreibung der Sendung.
    /// </summary>
    [XmlElement(IsNullable = true)]
    public string ShortDescription { get; set; } = null!;

    /// <summary>
    /// Language for the event.
    /// </summary>
    [XmlElement(IsNullable = true)]
    public string Language { get; set; } = null!;

    /// <summary>
    /// All ratings related with this event.
    /// </summary>
    public readonly List<string> Ratings = [];

    /// <summary>
    /// Alle Kategorien der Sendung.
    /// </summary>
    public readonly List<string> Categories = [];

    /// <summary>
    /// The name of the event.
    /// </summary>
    [XmlElement(IsNullable = true)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// The duration of the event in seconds.
    /// </summary>
    /// <remarks>
    /// This is used instead of a <see cref="TimeSpan"/> because the
    /// later one can not be properly serialzed and deserialized to
    /// XML using the .NET 1.1 core functionality.
    /// </remarks>
    public long Duration { get; set; }

    /// <summary>
    /// Alle Zeichen, die SOAP im XML Modus nicht ohne weiteres unterst�tzt.
    /// </summary>
    private static readonly char[] m_Disallowed;

    /// <summary>
    /// Eine eindeutige Kennung für diesen Eintrag - dieser wird in einem
    /// Filter zum Blättern verwendet.
    /// </summary>
    [XmlIgnore]
    public Guid UniqueIdentifier { get; set; }

    /// <summary>
    /// Initialisiert statische Felder.
    /// </summary>
    static ProgramGuideEntry()
    {
        // Helper
        var disallow = new List<char>();

        // Fill
        for (char ch = '\x0020'; ch-- > '\x0000';)
            if (('\x0009' != ch) && ('\x000a' != ch) && ('\x000d' != ch))
                disallow.Add(ch);

        // Use
        m_Disallowed = disallow.ToArray();
    }

    /// <summary>
    /// Create a new event instance.
    /// </summary>
    public ProgramGuideEntry()
    {
        // Setup fields
        UniqueIdentifier = Guid.NewGuid();
        StartTime = DateTime.MinValue;
    }

    /// <summary>
    /// Meldet das Ende der zugehörigen Sendung.
    /// </summary>
    [XmlIgnore]
    public DateTime EndTime
    {
        get
        {
            // Report
            if (ShowLocalTimes)
                return m_Start.AddSeconds(Duration).ToLocalTime();
            else
                return m_Start.AddSeconds(Duration);
        }
    }

    /// <summary>
    /// Meldet die Quelle zu diesem Eintrag.
    /// </summary>
    [XmlIgnore]
    public SourceIdentifier Source => new(NetworkIdentifier, TransportIdentifier, ServiceIdentifier);

    #region IComparable Members

    /// <summary>
    /// Events are ordered by the <see cref="StartTime"/> field.
    /// </summary>
    /// <param name="obj">Some other instance.</param>
    /// <returns><see cref="DateTime.CompareTo(DateTime)"/> of the <see cref="StartTime"/>
    /// or -1 if the parameter is not an <see cref="ProgramGuideEntry"/>.</returns>
    public int CompareTo(object? obj)
    {
        // Not comparable - we are left of these
        if (obj is not ProgramGuideEntry other)
            return -1;

        // Forward
        return StartTime.CompareTo(other.StartTime);
    }

    #endregion
}
