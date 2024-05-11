namespace JMS.DVB.NET.Recording.RestWebApi;

/// <summary>
/// Beschreibt die benutezrdefinierten Einstellungen.
/// </summary>
public class UserProfile
{
    /// <summary>
    /// Die Anzahl von Tagen, die im Aufzeichnungsplan angezeigt werden sollen.
    /// </summary>
    public int PlanDays { get; set; } = 7;

    /// <summary>
    /// Die Liste der bisher verwendeten Quellen.
    /// </summary>
    public List<string> RecentSources { get; set; } = [];

    /// <summary>
    /// Die bevorzugte Auswahl der Art einer Quelle.
    /// </summary>
    public string TypeFilter { get; set; } = "RT";

    /// <summary>
    /// Die bevorzugte Auswahl Verschlüsselung einer Quelle.
    /// </summary>
    public string EncryptionFilter { get; set; } = "FP";

    /// <summary>
    /// Gesetzt, wenn alle Sprachen aufgezeichnet werden sollen.
    /// </summary>
    public bool Languages { get; set; } = true;

    /// <summary>
    /// Gesetzt, wenn auch die <i>Dolby Digital</i> Tonspur aufgezeichnet werden soll.
    /// </summary>
    public bool Dolby { get; set; } = true;

    /// <summary>
    /// Gesetzt, wenn auch der Videotext aufgezeichnet werden soll.
    /// </summary>
    public bool Videotext { get; set; } = true;

    /// <summary>
    /// Gesetzt, wenn auch Untertitel aufgezeichnet werden sollen.
    /// </summary>
    public bool Subtitles { get; set; } = true;

    /// <summary>
    /// Gesetzt, wenn nach dem Anlegen einer neuen Aufzeichnung aus der Programmzeitschrift
    /// heraus zur Programmzeitschrift zurück gekehrt werden soll - und nicht der aktualisierte
    /// Aufzeichnungsplan zur Anzeige kommt.
    /// </summary>
    public bool BackToGuide { get; set; } = true;

    /// <summary>
    /// Die Anzahl der Zeilen auf einer Seite der Programmzeitschrift.
    /// </summary>
    public int GuideRows { get; set; } = 25;

    /// <summary>
    /// Die Anzahl von Minuten, die eine Aufzeichnung vorzeitig beginnt, wenn sie über
    /// die Programmzeitschrift angelegt wird.
    /// </summary>
    public int GuideAheadStart { get; set; } = 15;

    /// <summary>
    /// Die Anzahl von Minuten, die eine Aufzeichnung verspätet endet, wenn sie über
    /// die Programmzeitschrift angelegt wird.
    /// </summary>
    public int GuideBeyondEnd { get; set; } = 30;

    /// <summary>
    /// Die maximale Anzahl von Einträgen in der Liste der zuletzt verwendeten Quellen.
    /// </summary>
    public int RecentSourceLimit { get; set; } = 10;

    /// <summary>
    /// Meldet oder ändert die gespeicherten Suchen der Programmzeitschrift.
    /// </summary>
    public string GuideSearches { get; set; } = "";
}
