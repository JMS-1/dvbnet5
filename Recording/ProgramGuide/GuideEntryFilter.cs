using System.Text.Json.Serialization;
using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.ProgramGuide;

/// <summary>
/// Diese Klasse beschreibt eine Auswahl auf die Programmzeitschrift.
/// </summary>
public class GuideEntryFilter
{
    /// <summary>
    /// Vergleichsalorithmus zum Anordnen von Einträgen.
    /// </summary>
    private static readonly IComparer<ProgramGuideEntry> Comparer = new EntryComparer();

    /// <summary>
    /// Implementiert einen Vergleichalgorithmus.
    /// </summary>
    private class EntryComparer : IComparer<ProgramGuideEntry>
    {
        /// <summary>
        /// Vergleicht zwei Einträge.
        /// </summary>
        /// <param name="left">Der erste Eintrag.</param>
        /// <param name="right">Der zweite Eintrag.</param>
        /// <returns>Der Unterschied zwischen den Einträgen.</returns>
        public int Compare(ProgramGuideEntry? left, ProgramGuideEntry? right)
        {
            // Test for nothing
            if (left == null)
                if (right == null)
                    return 0;
                else
                    return -1;
            else if (right == null)
                return +1;

            // Start time
            var delta = left.StartTime.CompareTo(right.StartTime);
            if (delta != 0)
                return delta;

            // Name of station - since there is no provider this may be misleading
            delta = StringComparer.InvariantCultureIgnoreCase.Compare(left.StationName, right.StationName);
            if (delta != 0)
                return delta;

            // Try source
            var leftSource = left.Source;
            var rightSource = right.Source;

            // Test for nothing
            if (leftSource == null)
                if (rightSource == null)
                    return 0;
                else
                    return -1;
            else if (rightSource == null)
                return +1;

            // Dive into
            delta = leftSource.Network.CompareTo(rightSource.Network);
            if (delta != 0)
                return delta;
            delta = leftSource.TransportStream.CompareTo(rightSource.TransportStream);
            if (delta != 0)
                return delta;
            delta = leftSource.Service.CompareTo(rightSource.Service);
            if (delta != 0)
                return delta;

            // Duration
            return left.Duration.CompareTo(right.Duration);
        }
    }

    /// <summary>
    /// Der Name des zu verwendenden Geräteprofils.
    /// </summary>
    public string ProfileName { get; set; } = null!;

    /// <summary>
    /// Optional die Quelle.
    /// </summary>
    public SourceIdentifier Source { get; set; } = null!;

    /// <summary>
    /// Optional der Startzeitpunkt.
    /// </summary>
    [JsonIgnore]
    public DateTime? Start { get; set; }

    /// <summary>
    /// Das Suchmuster für den Titel, das erste Zeichen bestimmt den Suchmodus.
    /// </summary>
    public string TitlePattern { get; set; } = null!;

    /// <summary>
    /// Das Suchmuster für den Inhalt, das erste Zeichen bestimmt den Suchmodus.
    /// </summary>
    public string ContentPattern { get; set; } = null!;

    /// <summary>
    /// Die gewünschte Seitengr��e.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Die Art der Quelle.
    /// </summary>
    public GuideSourceFilter SourceType { get; set; }

    /// <summary>
    /// Die Art der Verschlüsselung.
    /// </summary>
    public GuideEncryptionFilter SourceEncryption { get; set; }

    /// <summary>
    /// Die aktuell gewünschte Seite.
    /// </summary>
    public int PageIndex;

    /// <summary>
    /// Wendet die Fiterbedingung an.
    /// </summary>
    /// <param name="entries">Eine Liste von Einträgen.</param>
    /// <returns>Die gefilterte Liste.</returns>
    public IEnumerable<ProgramGuideEntry> Filter(IEnumerable<ProgramGuideEntry> entries, IVCRProfiles profiles)
    {
        // Only use sources available to the target profile
        var entrySet = entries
            .Select(entry => new { e = entry, s = profiles.FindSource(ProfileName, entry.Source) })
            .Where(entry => entry.s != null)
            .Select(entry => new { e = entry.e, s = (Station)entry.s!.Source });

        // Name of the station - best filter first
        if (Source != null)
        {
            // One source - no more filters
            entrySet = entrySet.Where(entry => Source.Equals(entry.e.Source));
        }
        else
        {
            // Apply source type filter
            if (SourceType == GuideSourceFilter.Television)
                entrySet = entrySet.Where(entry => entry.s.SourceType == SourceTypes.TV);
            else if (SourceType == GuideSourceFilter.Radio)
                entrySet = entrySet.Where(entry => entry.s.SourceType == SourceTypes.Radio);

            // Apply encryption filter
            if (SourceEncryption == GuideEncryptionFilter.Free)
                entrySet = entrySet.Where(entry => !entry.s.IsEncrypted);
            else if (SourceEncryption == GuideEncryptionFilter.Encrypted)
                entrySet = entrySet.Where(entry => entry.s.IsEncrypted);
        }

        // Start time
        if (Start.HasValue)
        {
            // Later
            entrySet = entrySet.Where(entry => entry.e.StartTime >= Start.Value);
        }
        else
        {
            // Current
            var now = DateTime.UtcNow;

            // Still active
            entrySet = entrySet.Where(entry => entry.e.EndTime > now);
        }

        // Matcher on content
        Func<ProgramGuideEntry, bool>? matchTitle = null;
        Func<ProgramGuideEntry, bool>? matchContent = null;

        // Title
        if (!string.IsNullOrEmpty(TitlePattern))
        {
            var title = TitlePattern.Substring(1);
            switch (TitlePattern[0])
            {
                case '=': matchTitle = entry => (entry.Name ?? string.Empty).Equals(title, StringComparison.InvariantCultureIgnoreCase); break;
                case '*': matchTitle = entry => (entry.Name ?? string.Empty).IndexOf(title, StringComparison.InvariantCultureIgnoreCase) >= 0; break;
            }
        }

        // Both descriptions
        if (!string.IsNullOrEmpty(ContentPattern))
        {
            var content = ContentPattern.Substring(1);
            switch (ContentPattern[0])
            {
                case '=': matchContent = entry => (entry.Description ?? string.Empty).Equals(content, StringComparison.InvariantCultureIgnoreCase) || (entry.ShortDescription ?? string.Empty).Equals(content, StringComparison.InvariantCultureIgnoreCase); break;
                case '*': matchContent = entry => ((entry.Description ?? string.Empty).IndexOf(content, StringComparison.InvariantCultureIgnoreCase) >= 0) || ((entry.ShortDescription ?? string.Empty).IndexOf(content, StringComparison.InvariantCultureIgnoreCase) >= 0); break;
            }
        }

        // Apply content filter
        if (matchTitle != null)
            if (matchContent != null)
                entrySet = entrySet.Where(entry => matchTitle(entry.e) || matchContent(entry.e));
            else
                entrySet = entrySet.Where(entry => matchTitle(entry.e));
        else if (matchContent != null)
            entrySet = entrySet.Where(entry => matchContent(entry.e));

        // Back mapping
        entries = entrySet.Select(entry => entry.e);

        // Caller will get it all
        if (PageSize < 1)
            return entries;

        // Copy local
        var matches = entries.ToList();

        // Sort in list to improve overall performance
        matches.Sort(Comparer);

        // Adjust extract - report one more if possible to indicate that there is more available
        return matches.Skip(PageIndex * PageSize).Take(PageSize + 1);
    }
}

