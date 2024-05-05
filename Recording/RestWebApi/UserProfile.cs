namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt die benutezrdefinierten Einstellungen.
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// Die Anzahl von Tagen, die im Aufzeichnungsplan angezeigt werden sollen.
        /// </summary>
        public int PlanDays { get; set; }

        /// <summary>
        /// Die Liste der bisher verwendeten Quellen.
        /// </summary>
        public string[] RecentSources { get; set; } = null!;

        /// <summary>
        /// Die bevorzugte Auswahl der Art einer Quelle.
        /// </summary>
        public string TypeFilter { get; set; } = null!;

        /// <summary>
        /// Die bevorzugte Auswahl Verschlüsselung einer Quelle.
        /// </summary>
        public string EncryptionFilter { get; set; } = null!;

        /// <summary>
        /// Gesetzt, wenn alle Sprachen aufgezeichnet werden sollen.
        /// </summary>
        public bool Languages { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch die <i>Dolby Digital</i> Tonspur aufgezeichnet werden soll.
        /// </summary>
        public bool Dolby { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch der Videotext aufgezeichnet werden soll.
        /// </summary>
        public bool Videotext { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch Untertitel aufgezeichnet werden sollen.
        /// </summary>
        public bool Subtitles { get; set; }

        /// <summary>
        /// Gesetzt, wenn nach dem Anlegen einer neuen Aufzeichnung aus der Programmzeitschrift
        /// heraus zur Programmzeitschrift zurück gekehrt werden soll - und nicht der aktualisierte
        /// Aufzeichnungsplan zur Anzeige kommt.
        /// </summary>
        public bool BackToGuide { get; set; }

        /// <summary>
        /// Die Anzahl der Zeilen auf einer Seite der Programmzeitschrift.
        /// </summary>
        public int GuideRows { get; set; }

        /// <summary>
        /// Die Anzahl von Minuten, die eine Aufzeichnung vorzeitig beginnt, wenn sie über
        /// die Programmzeitschrift angelegt wird.
        /// </summary>
        public int GuideAheadStart { get; set; }

        /// <summary>
        /// Die Anzahl von Minuten, die eine Aufzeichnung verspätet endet, wenn sie über
        /// die Programmzeitschrift angelegt wird.
        /// </summary>
        public int GuideBeyondEnd { get; set; }

        /// <summary>
        /// Die maximale Anzahl von Einträgen in der Liste der zuletzt verwendeten Quellen.
        /// </summary>
        public int RecentSourceLimit { get; set; }

        /// <summary>
        /// Meldet oder ändert die gespeicherten Suchen der Programmzeitschrift.
        /// </summary>
        public string GuideSearches { get; set; } = null!;

        /// <summary>
        /// Erstellt die Informationen des aktuellen Anwenders.
        /// </summary>
        /// <returns>Die gewünschten Einstellungen.</returns>
        public static UserProfile Create()
        {
            // Report
            return
                new UserProfile
                {
                    TypeFilter = UserProfileSettings.Radio ? (UserProfileSettings.Television ? "RT" : "R") : (UserProfileSettings.Television ? "T" : ""),
                    EncryptionFilter = UserProfileSettings.FreeTV ? (UserProfileSettings.PayTV ? "FP" : "F") : (UserProfileSettings.PayTV ? "P" : ""),
                    RecentSources = UserProfileSettings.RecentChannels.Cast<string>().OrderBy(name => name).ToArray(),
                    RecentSourceLimit = UserProfileSettings.MaxRecentChannels,
                    GuideSearches = UserProfileSettings.GuideFavorites,
                    BackToGuide = UserProfileSettings.BackToEPG,
                    Subtitles = UserProfileSettings.UseSubTitles,
                    PlanDays = UserProfileSettings.DaysToShow,
                    GuideAheadStart = UserProfileSettings.EPGPreTime,
                    Languages = UserProfileSettings.UseMP2,
                    GuideBeyondEnd = UserProfileSettings.EPGPostTime,
                    Videotext = UserProfileSettings.UseTTX,
                    GuideRows = UserProfileSettings.EPGEntries,
                    Dolby = UserProfileSettings.UseAC3,
                };
        }

        /// <summary>
        /// Aktualisiert die Einstellungen des Anwenders.
        /// </summary>
        public void Update()
        {
            // Direct copy
            UserProfileSettings.BackToEPG = BackToGuide;
            UserProfileSettings.UseSubTitles = Subtitles;
            UserProfileSettings.UseMP2 = Languages;
            UserProfileSettings.UseTTX = Videotext;
            UserProfileSettings.UseAC3 = Dolby;

            // A bit more work on flag groups
            switch (TypeFilter ?? string.Empty)
            {
                case "R": UserProfileSettings.Radio = true; UserProfileSettings.Television = false; break;
                case "T": UserProfileSettings.Radio = false; UserProfileSettings.Television = true; break;
                case "RT": UserProfileSettings.Radio = true; UserProfileSettings.Television = true; break;
            }
            switch (EncryptionFilter ?? string.Empty)
            {
                case "F": UserProfileSettings.FreeTV = true; UserProfileSettings.PayTV = false; break;
                case "P": UserProfileSettings.FreeTV = false; UserProfileSettings.PayTV = true; break;
                case "FP": UserProfileSettings.FreeTV = true; UserProfileSettings.PayTV = true; break;
            }

            // Numbers are copied after check
            if (PlanDays >= 1)
                if (PlanDays <= 50)
                    UserProfileSettings.DaysToShow = PlanDays;
            if (RecentSourceLimit >= 1)
                if (RecentSourceLimit <= 50)
                    UserProfileSettings.MaxRecentChannels = RecentSourceLimit;
            if (GuideAheadStart >= 0)
                if (GuideAheadStart <= 240)
                    UserProfileSettings.EPGPreTime = GuideAheadStart;
            if (GuideBeyondEnd >= 0)
                if (GuideBeyondEnd <= 240)
                    UserProfileSettings.EPGPostTime = GuideBeyondEnd;
            if (GuideRows >= 10)
                if (GuideRows <= 100)
                    UserProfileSettings.EPGEntries = GuideRows;

            // Store
            UserProfileSettings.Update();
        }
    }
}
