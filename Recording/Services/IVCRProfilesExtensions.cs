namespace JMS.DVB.NET.Recording.Services
{
    public static class IVCRProfilesExtensions
    {
        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="predicate">Methode, die prüft, ob eine Quelle gemeldet werden soll.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
        public static IEnumerable<SourceSelection> GetSources(this IVCRProfiles profiles, string profileName, Func<SourceSelection, bool> predicate)
            => profiles.GetSources(profiles.FindProfile(profileName)!, predicate);

        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
        public static IEnumerable<SourceSelection> GetSources(this IVCRProfiles profiles, string profileName)
            => profiles.GetSources(profileName, (Func<SourceSelection, bool>)null!);

        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profileName">Der Name des Geräteprofils.</param>
        /// <param name="predicate">Methode, die prüft, ob eine Quelle gemeldet werden soll.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
        public static IEnumerable<SourceSelection> GetSources(this IVCRProfiles profiles, string profileName, Func<Station, bool> predicate)
        {
            // Forward
            if (predicate == null)
                return profiles.GetSources(profiles.FindProfile(profileName)!);
            else
                return profiles.GetSources(profiles.FindProfile(profileName)!, s => predicate((Station)s.Source));
        }

        /// <summary>
        /// Ermittelt alle Quellen zu einem DVB.NET Geräteprofil.
        /// </summary>
        /// <param name="profile">Der Name des Geräteprofils.</param>
        /// <returns>Alle Quellen zum Profil.</returns>
        public static IEnumerable<SourceSelection> GetSources(this IVCRProfiles profiles, Profile profile)
            => profiles.GetSources(profile, null!);


        /// <summary>
        /// Ermittelt die aktuelle Konfiguration einer Quelle.
        /// </summary>
        /// <param name="source">Die gewünschte Quelle.</param>
        /// <returns>Die aktuelle Auswahl oder <i>null</i>.</returns>
        public static SourceSelection? FindSource(this IVCRProfiles profiles, SourceSelection source)
        {
            // Never
            if (source == null)
                return null;
            if (source.Source == null)
                return null;

            // Find the source
            return profiles.FindSource(source.ProfileName, source.Source);
        }
    }
}
