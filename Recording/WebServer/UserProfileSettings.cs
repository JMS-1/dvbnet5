using System.Collections.Specialized;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    public class ProfileBase
    {
        private readonly Dictionary<string, object> m_dict = [];

        public T Read<T>(string key, T fallback = default!) => m_dict.TryGetValue(key, out var value) ? (T)value : fallback;

        public void Write<T>(string key, T value) => m_dict[key] = value!;

        public void Save() => throw new NotImplementedException("ProfileBase");
    }

    /// <summary>
    /// Kapselt die benutzerspezifischen Einstellungen.
    /// </summary>
    public static class UserProfileSettings
    {
        /// <summary>
        /// Meldet die Liste der zuletzt verwendenden Sendern.
        /// </summary>
        public static StringCollection RecentChannels => Profile.Read<StringCollection>("RecentChannels", []);

        /// <summary>
        /// Meldet die maximal erlaubte Anzahl von Sendern in der Liste zuletzt verwendeter Sender.
        /// </summary>
        public static int MaxRecentChannels
        {
            get { return Profile.Read<int>("MaxRecentChannels"); }
            set { Profile.Write("MaxRecentChannels", value); }
        }

        /// <summary>
        /// Meldet das aktuelle Benutzerprofil.
        /// </summary>
        private static readonly ProfileBase Profile = new();

        /// <summary>
        /// Begrenzt die Anzahl der zuletzt verwendeten Quellen auf die H�chstgrenze.
        /// </summary>
        /// <returns>Gesetzt, wenn eine Reduktion vorgenommen wurde.</returns>
        private static bool LimitChannels()
        {
            // Check delta
            int delta = RecentChannels.Count - MaxRecentChannels;
            if (delta < 1)
                return false;

            // Cut off
            while (delta-- > 0)
                RecentChannels.RemoveAt(MaxRecentChannels);

            // Did it
            return true;
        }

        /// <summary>
        /// Fügt eine Quelle zur Liste der zuletzt verwendeten Quellen hinzu. Ist der
        /// Eintrag bereits vorhanden, so wird er an den Anfang der Liste verschoben.
        /// </summary>
        /// <param name="station">Der Name der Quelle.</param>
        public static void AddRecentChannel(string? station)
        {
            // Not allowed
            if (string.IsNullOrEmpty(station))
                return;

            // Remove first
            RecentChannels.Remove(station);

            // Append to head
            RecentChannels.Insert(0, station);

            // Correct and save
            Update();
        }

        /// <summary>
        /// Aktualisiert das Benutzerprofil auf der Festplatte.
        /// </summary>
        public static void Update()
        {
            // Correct
            LimitChannels();

            // Store
            Profile.Save();
        }

        /// <summary>
        /// Gesetzt, wen verschlüsselte Sender berücksichtigt werden sollen.
        /// </summary>
        public static bool PayTV
        {
            get { return Profile.Read<bool>("PayTV"); }
            set { Profile.Write("PayTV", value); }
        }

        /// <summary>
        /// Meldet, ob Fernsehsender berücksichtigt werden sollen.
        /// </summary>
        public static bool Radio
        {
            get { return Profile.Read<bool>("Radio"); }
            set { Profile.Write("Radio", value); }
        }

        /// <summary>
        /// Meldet, ob unverschlüsselte Sender berücksichtigt werden sollen.
        /// </summary>
        public static bool FreeTV
        {
            get { return Profile.Read<bool>("FreeTV"); }
            set { Profile.Write("FreeTV", value); }
        }

        /// <summary>
        /// Meldet, ob Radiosender berücksichtigt werden sollen.
        /// </summary>
        public static bool Television
        {
            get { return Profile.Read<bool>("Television"); }
            set { Profile.Write("Television", value); }
        }

        /// <summary>
        /// Meldet, ob nach einer Programmierung über die Programmzeitschrift zu dieser zur�ck gekehrt werden soll.
        /// </summary>
        public static bool BackToEPG
        {
            get { return Profile.Read<bool>("BackToEPG"); }
            set { Profile.Write("BackToEPG", value); }
        }

        /// <summary>
        /// Meldet, ob der Videotext mit aufgezeichnet werden soll.
        /// </summary>
        public static bool UseTTX
        {
            get { return Profile.Read<bool>("UseTTX"); }
            set { Profile.Write("UseTTX", value); }
        }

        /// <summary>
        /// Meldet, ob die <i>Dolby Digital</i> Tonspur mit aufgezeichnet werden soll.
        /// </summary>
        public static bool UseAC3
        {
            get { return Profile.Read<bool>("UseAC3"); }
            set { Profile.Write("UseAC3", value); }
        }

        /// <summary>
        /// Meldet, ob alle Sprachen aufzeichnet werden sollen.
        /// </summary>
        public static bool UseMP2
        {
            get { return Profile.Read<bool>("UseMP2"); }
            set { Profile.Write("UseMP2", value); }
        }

        /// <summary>
        /// Meldet, ob auch DVB Untertitel aufgezeichnet werden sollen.
        /// </summary>
        public static bool UseSubTitles
        {
            get { return Profile.Read<bool>("UseSubTitles"); }
            set { Profile.Write("UseSubTitles", value); }
        }

        /// <summary>
        /// Meldet die Nachlaufzeit bei Programmierung über die Programmzeitschrift.
        /// </summary>
        /// <seealso cref="EPGPreTime"/>
        public static int EPGPostTime
        {
            get { return Profile.Read<int>("EPGPostTime"); }
            set { Profile.Write("EPGPostTime", value); }
        }

        /// <summary>
        /// Meldet die Vorlaufzeit bei Programmierung über die Programmzeitschrift.
        /// </summary>
        /// <seealso cref="EPGPostTime"/>
        public static int EPGPreTime
        {
            get { return Profile.Read<int>("EPGPreTime"); }
            set { Profile.Write("EPGPreTime", value); }
        }

        /// <summary>
        /// Meldet die Anzahl von Einträge der Programmzeitschrift pro Seite.
        /// </summary>
        public static int EPGEntries
        {
            get { return Profile.Read<int>("EPGEntries", 25); }
            set { Profile.Write("EPGEntries", value); }
        }

        /// <summary>
        /// Meldet die Anzahl von Tagen, die im Aufzeichnungsplan pro Seite angezeigt werden sollen.
        /// </summary>
        public static int DaysToShow
        {
            get { return Profile.Read<int>("DaysToShow", 7); }
            set { Profile.Write("DaysToShow", value); }
        }

        /// <summary>
        /// Meldet oder ändert die gespeicherten Suchen der Programmzeichschrift.
        /// </summary>
        public static string GuideFavorites
        {
            get { return Profile.Read<string>("GuideFavorites"); }
            set { Profile.Write("GuideFavorites", value); }
        }
    }
}