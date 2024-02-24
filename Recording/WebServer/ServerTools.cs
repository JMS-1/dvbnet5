using JMS.DVB.NET.Recording.Persistence;

namespace JMS.DVB.NET.Recording.RestWebApi;

/// <summary>
/// Einige Hilfsmethoden zur Vereinfachung der Webanwendung.
/// </summary>
public static class ServerTools
{
    /// <summary>
    /// Prüft, ob eine Datenstromkonfiguration eine Dolby Digital Tonspur nicht 
    /// grundsätzlich ausschließt.
    /// </summary>
    /// <param name="streams">Die Datenstromkonfiguration.</param>
    /// <returns>Gesetzt, wenn die AC3 Tonspur nicht grundsätzlich deaktiviert ist.</returns>
    public static bool GetUsesDolbyAudio(this StreamSelection streams)
    {
        // Check mode
        if (streams == null)
            return false;

        if (streams.AC3Tracks.LanguageMode != LanguageModes.Selection)
            return true;

        return streams.AC3Tracks.Languages.Count > 0;
    }

    /// <summary>
    /// Prüft, ob eine Datenstromkonfiguration alle Tonspuren einschließt.
    /// </summary>
    /// <param name="streams">Die Datenstromkonfiguration.</param>
    /// <returns>Gesetzt, wenn alle Tonspuren aufgezeichnet werden sollen.</returns>
    public static bool GetUsesAllAudio(this StreamSelection streams) => (streams != null) && (streams.MP2Tracks.LanguageMode == LanguageModes.All);

    /// <summary>
    /// Prüft, ob eine Datenstromkonfiguration DVB Untertitel nicht 
    /// grundsätzlich ausschließt.
    /// </summary>
    /// <param name="streams">Die Datenstromkonfiguration.</param>
    /// <returns>Gesetzt, wenn die DVB Untertitel nicht grundsätzlich deaktiviert sind.</returns>
    public static bool GetUsesSubtitles(this StreamSelection streams)
    {
        // Check mode
        if (streams == null)
            return false;

        if (streams.SubTitles.LanguageMode != LanguageModes.Selection)
            return true;

        return streams.SubTitles.Languages.Count > 0;
    }

    /// <summary>
    /// Prüft, ob eine Datenstromkonfiguration auch den Videotext umfasst.
    /// </summary>
    /// <param name="streams">Die Datenstromkonfiguration.</param>
    /// <returns>Gesetzt, wenn der Videotext aufgezeichnet werden soll.</returns>
    public static bool GetUsesVideotext(this StreamSelection streams) => (streams != null) && streams.Videotext;

    /// <summary>
    /// Prüft, ob eine Datenstromkonfiguration auch einen Extrakt der Programmzeitschrift umfasst.
    /// </summary>
    /// <param name="streams">Die Datenstromkonfiguration.</param>
    /// <returns>Gesetzt, wenn die Programmzeitschrift berücksichtigt werden soll.</returns>
    public static bool GetUsesProgramGuide(this StreamSelection streams) => (streams != null) && streams.ProgramGuide;

    /// <summary>
    /// Legt fest, ob die Dolby Digital Tonspur aufgezeichnet werden soll.
    /// </summary>
    /// <param name="streams">Die Konfiguration der zu verwendenden Datenströme.</param>
    /// <param name="set">Gesetzt, wenn die Datenspur aktiviert werden soll.</param>
    public static void SetUsesDolbyAudio(this StreamSelection streams, bool set)
    {
        // Reset language list
        streams.AC3Tracks.Languages.Clear();

        // Check mode
        if (set)
            if (streams.MP2Tracks.LanguageMode == LanguageModes.Selection)
                streams.AC3Tracks.LanguageMode = LanguageModes.Primary;
            else
                streams.AC3Tracks.LanguageMode = streams.MP2Tracks.LanguageMode;
        else
            streams.AC3Tracks.LanguageMode = LanguageModes.Selection;
    }

    /// <summary>
    /// Legt fest, ob alle Tonspuren aufgezeichnet werden sollen.
    /// </summary>
    /// <param name="streams">Die Konfiguration der zu verwendenden Datenströme.</param>
    /// <param name="set">Gesetzt, wenn die Datenspuren aktiviert werden sollen.</param>
    public static void SetUsesAllAudio(this StreamSelection streams, bool set)
    {
        // Clear all
        streams.MP2Tracks.Languages.Clear();
        streams.AC3Tracks.Languages.Clear();

        // Check mode
        if (set)
            streams.MP2Tracks.LanguageMode = LanguageModes.All;
        else
            streams.MP2Tracks.LanguageMode = LanguageModes.Primary;

        // Forward
        if (streams.AC3Tracks.LanguageMode != LanguageModes.Selection)
            streams.AC3Tracks.LanguageMode = streams.MP2Tracks.LanguageMode;
    }

    /// <summary>
    /// Legt fest, ob DVB Untertitel aufgezeichnet werden sollen.
    /// </summary>
    /// <param name="streams">Die Konfiguration der zu verwendenden Datenströme.</param>
    /// <param name="set">Gesetzt, wenn die Datenspuren aktiviert werden sollen.</param>
    public static void SetUsesSubtitles(this StreamSelection streams, bool set)
    {
        // Reset language list
        streams.SubTitles.Languages.Clear();

        // Check mode
        if (set)
            streams.SubTitles.LanguageMode = LanguageModes.All;
        else
            streams.SubTitles.LanguageMode = LanguageModes.Selection;
    }

    /// <summary>
    /// Legt fest, ob der Videotext mit aufgezeichnet werden soll.
    /// </summary>
    /// <param name="streams">Die Konfiguration der zu verwendenden Datenströme.</param>
    /// <param name="set">Gesetzt, wenn die Datenspur aktiviert werden soll.</param>
    public static void SetUsesVideotext(this StreamSelection streams, bool set) => streams.Videotext = set;

    /// <summary>
    /// Ermittelt eine Referenz für eine bestimmte Aufzeichung in einem Auftrag, so dass diese
    /// auch in einer URL verwendet werden kann.
    /// </summary>
    /// <param name="job">Ein Auftrag.</param>
    /// <param name="schedule">Eine Aufzeichnung.</param>
    /// <returns>Die eindeutige Referenz.</returns>
    public static string GetUniqueWebId(VCRJob job, VCRSchedule schedule)
    {
        // Forward
        if (job == null)
            return "*";

        if (schedule == null)
            return $"*{job.UniqueID!.Value:N}";

        return GetUniqueWebId(job.UniqueID!.Value, schedule.UniqueID!.Value);
    }

    /// <summary>
    /// Ermittelt eine Referenz für eine bestimmte Aufzeichung in einem Auftrag, so dass diese
    /// auch in einer URL verwendet werden kann.
    /// </summary>
    /// <param name="job">Die eindeutige Kennung eines Auftrags.</param>
    /// <param name="schedule">Die eindeutige Kennung einer Aufzeichnung des Auftrags.</param>
    /// <returns>Die eindeutige Referenz.</returns>
    public static string GetUniqueWebId(string job, string schedule)
    {
        // Use defaults
        if (string.IsNullOrEmpty(job))
            job = Guid.Empty.ToString("N");
        if (string.IsNullOrEmpty(schedule))
            schedule = Guid.Empty.ToString("N");

        // Create
        return $"{job}{schedule}";
    }

    /// <summary>
    /// Ermittelt eine Referenz für eine bestimmte Aufzeichung in einem Auftrag, so dass diese
    /// auch in einer URL verwendet werden kann.
    /// </summary>
    /// <param name="job">Die eindeutige Kennung eines Auftrags.</param>
    /// <param name="schedule">Die eindeutige Kennung einer Aufzeichnung des Auftrags.</param>
    /// <returns>Die eindeutige Referenz.</returns>
    public static string GetUniqueWebId(Guid job, Guid schedule) => GetUniqueWebId(job.ToString("N"), schedule.ToString("N"));

    /// <summary>
    /// Rekonstruiert einen Auftrag und eine Aufzeichnung aus einer Textdarstellung.
    /// </summary>
    /// <param name="id">Die Textdarstellung.</param>
    /// <param name="job">Der zugehörige Auftrag.</param>
    /// <param name="schedule">Die Aufzeichnung in dem Auftrag.</param>
    public static void ParseUniqueWebId(string id, out Guid job, out Guid schedule)
    {
        // Read all
        schedule = new Guid(id.Substring(32, 32));
        job = new Guid(id.Substring(0, 32));
    }
}