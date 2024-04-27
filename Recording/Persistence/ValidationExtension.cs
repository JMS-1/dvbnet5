namespace JMS.DVB.NET.Recording.Persistence;

/// <summary>
/// Hilfsmethoden zur Validierung von Aufträgen und Aufzeichnungen.
/// </summary>
public static class ValidationExtension
{
    /// <summary>
    /// Alle in Dateinamen nicht erlaubten Zeichen.
    /// </summary>
    private static readonly char[] m_BadCharacters = Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).Distinct().ToArray();

    /// <summary>
    /// Prüft, ob eine Datenstromauswahl zulässig ist.
    /// </summary>
    /// <param name="streams">Die Auswahl der Datenströme.</param>
    /// <returns>Gesetzt, wenn die Auswahl gültig ist - und mindestens eine Tonspur enthält.</returns>
    public static bool Validate(this StreamSelection streams)
    {
        // Not possible
        if (streams == null)
            return false;

        // Test for wildcards - may fail at runtime!
        if (streams.MP2Tracks.LanguageMode != LanguageModes.Selection)
            return true;
        if (streams.AC3Tracks.LanguageMode != LanguageModes.Selection)
            return true;

        // Test for language selection - may fail at runtime but at least we tried
        if (streams.MP2Tracks.Languages.Count > 0)
            return true;
        if (streams.AC3Tracks.Languages.Count > 0)
            return true;

        // Will definitly fail
        return false;
    }

    /// <summary>
    /// Prüft, ob eine Zeichenkette als Name für einen Auftrag oder eine
    /// Aufzeichnung verwendet werden darf.
    /// </summary>
    /// <param name="name">Der zu prüfenden Name.</param>
    /// <returns>Gesetzt, wenn der Name verwendet werden darf.</returns>
    public static bool IsValidName(this string name) => string.IsNullOrEmpty(name) || (name.IndexOfAny(m_BadCharacters) < 0);

    /// <summary>
    /// Ersetzt alle Zeichen, die nicht in Dateinamen erlaubt sind, durch einen
    /// Unterstrich.
    /// </summary>
    /// <param name="s">Eine Zeichenkette.</param>
    /// <returns>Die korrigierte Zeichenkette.</returns>
    public static string MakeValid(this string s)
    {
        // No at all
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        // Correct all
        if (s.IndexOfAny(m_BadCharacters) >= 0)
            foreach (var ch in m_BadCharacters)
                s = s.Replace(ch, '_');

        // Report
        return s;
    }
}
