namespace JMS.DVB.NET.Recording.Services.Configuration;

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


    /// <summary>
    /// Ermittelt eine Quelle.
    /// </summary>
    /// <param name="profile">Das zu verwendende Ger?teprofil.</param>
    /// <param name="name">Der (hoffentlicH) eindeutige Name der Quelle.</param>
    /// <returns>Die Beschreibung der Quelle.</returns>
    public static SourceSelection? FindSource(this IVCRProfiles profiles, string profile, string name)
        => profiles.FindSource(profile, name);

    /// <summary>
    /// Ermittelt alle Geräteprofile.
    /// </summary>
    /// <typeparam name="TProfile">Die Art der Zielinformation.</typeparam>
    /// <param name="factory">Methode zum Erstellen der Zielinformation.</param>
    /// <param name="defaultProfile">Der Name des bevorzugten Geräteprofils.</param>
    /// <returns>Die gewünschte Liste.</returns>
    public static TProfile[] GetProfiles<TProfile>(this IVCRProfiles profiles, Func<Profile, bool, TProfile> factory, out string defaultProfile)
    {
        // Create map            
        var names = profiles.ProfileNames.ToArray();
        var active = new HashSet<string>(names, ProfileManager.ProfileNameComparer);

        // Set default
        defaultProfile = names.FirstOrDefault()!;

        // From DVB.NET
        return ProfileManager.AllProfiles.Select(profile => factory(profile, active.Contains(profile.Name))).ToArray();
    }

    /// <summary>
    /// Aktualisiert die Daten von Geräteprofilen.
    /// </summary>
    /// <typeparam name="TProfile">Die Art der Daten.</typeparam>
    /// <param name="profiles">Die Liste der Geräteprofile.</param>
    /// <param name="getName">Methode zum Auslesen des Profilnames.</param>
    /// <param name="updater">Die Aktualisierungsmethode.</param>
    /// <returns>Gesetzt, wenn eine Änderung durchgef�hrt wurde.</returns>
    public static bool UpdateProfiles<TProfile>(TProfile[] profiles, Func<TProfile, string> getName, Func<TProfile, Profile, bool> updater)
    {
        // Result
        var changed = false;

        // Process
        Array.ForEach(profiles, profile => changed |= updater(profile, ProfileManager.FindProfile(getName(profile))!));

        // Report
        return changed;
    }

}
