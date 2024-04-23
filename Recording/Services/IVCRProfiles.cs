namespace JMS.DVB.NET.Recording.Services
{
    /// <summary>
    /// Verwaltet die Geräteprofile des VCR.NET Recording Service.
    /// </summary>
    /// <remarks>LEAF SERVICE</remarks>
    public interface IVCRProfiles
    {
        /// <summary>
        /// Meldet das erste zu verwendende Geräteprofil.
        /// </summary>
        Profile? DefaultProfile { get; }

        /// <summary>
        /// Meldet die Namen alle aktivierten Geräteprofile, das bevorzugte Profil immer zuerst.
        /// </summary>
        IEnumerable<string> ProfileNames { get; }

        /// <summary>
        /// Ermittelt den eindeutigen Namen einer Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Der eindeutige Name oder <i>null</i>, wenn die Quelle nicht
        /// bekannt ist.</returns>
        string GetUniqueName(SourceSelection source);

        /// <summary>
        /// Ermittelt eine Quelle nach ihrem Namen.
        /// </summary>
        /// <param name="profileName">Das zu verwendende Geräteprofil.</param>
        /// <param name="name">Der Anzeigename der Quelle.</param>
        /// <returns>Die eidneutige Auswahl der Quelle oder <i>null</i>.</returns>
        SourceSelection? FindSource(string profileName, string name);

        /// <summary>
        /// Ermittelt eine Quelle nach ihrer eindeutigen Kennung.
        /// </summary>
        /// <param name="profileName">Das zu verwendende Geräteprofil.</param>
        /// <param name="source">Die gewünschte Kennung.</param>
        /// <returns>Die eidneutige Auswahl der Quelle oder <i>null</i>.</returns>
        SourceSelection? FindSource(string profileName, SourceIdentifier source);

        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="predicate">Methode, die prüft, ob eine Quelle gemeldet werden soll.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
        IEnumerable<SourceSelection> GetSources(Profile profile, Func<SourceSelection, bool> predicate);

        /// <summary>
        /// Ermittelt ein Geräteprofil.
        /// </summary>
        /// <param name="name">Der Name des Geräteprofils oder <i>null</i> für das
        /// bevorzugte Profil.</param>
        /// <returns>Das gewünschte Geräteprofil.</returns>
        Profile? FindProfile(string name);

        /// <summary>
        /// Lädt alle Profile erneut.
        /// </summary>
        void Reset();
    }
}
