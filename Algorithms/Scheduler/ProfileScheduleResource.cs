﻿using System.Collections.Concurrent;


namespace JMS.DVB.Algorithms.Scheduler
{
    /// <summary>
    /// Stellt ein Geräteprofil zur Verwaltung zur Verfügung.
    /// </summary>
    public static class ProfileScheduleResource
    {
        /// <summary>
        /// Die maximale Anzahl von Quellen, die gleichzeitig aufgezeichnet werden dürfen.
        /// </summary>
        public const string ParallelSourceLimitName = "Scheduler.SourceLimit";

        /// <summary>
        /// Die Voreinstellung für die maximale Anzahl von Quellen.
        /// </summary>
        public const int DefaultParallelSourceLimit = 15;

        /// <summary>
        /// Die maximale Anzahl von Quellen, die gleichzeitig entschlüsselt werden können.
        /// </summary>
        public const string DecryptionLimitName = "Scheduler.DecryptionLimit";

        /// <summary>
        /// Die Voreinstellung für die maximale Anzahl gleichzeitig entschlüsselbarer Quellen.
        /// </summary>
        public const int DefaultDecryptionLimit = 1;

        /// <summary>
        /// Die Priorität des Gerätes bei der Planung von Aufzeichnungen.
        /// </summary>
        public const string SchedulePriorityName = "Scheduler.Priority";

        /// <summary>
        /// Die bevorzugte Priorität eines Gerätes, falls keine explizit angegeben wurde.
        /// </summary>
        public const int DefaultSchedulePriority = 100;

        /// <summary>
        /// Repräsentiert eine Quelle.
        /// </summary>
        private class _Source : IScheduleSource
        {
            /// <summary>
            /// Die zugehörige Quelle.
            /// </summary>
            public SourceSelection Source { get; private set; }

            /// <summary>
            /// Meldet, ob diese Quelle eine Entschlüsselung benötigt.
            /// </summary>
            public bool IsEncrypted => Station.IsEncrypted;

            /// <summary>
            /// Meldet, ob es sich bei der Quelle um einen Radiosender handelt.
            /// </summary>
            public bool IsAudioOnly => Station.SourceType == SourceTypes.Radio;

            /// <summary>
            /// Meldet die vollen Informationen zur Quelle.
            /// </summary>
            public Station Station => (Station)Source.Source;

            /// <summary>
            /// Prüft, ob diese Quelle mit einer anderen parallel aufgezeichnet werden kann.
            /// </summary>
            /// <param name="source">Eine andere Quelle.</param>
            /// <returns>Gesetzt, wenn eine parallele Aufzeichnung theoretisch möglich ist.</returns>
            public bool BelongsToSameSourceGroupAs(IScheduleSource source)
            {
                // Check type first
                if (source is not _Source typedSource)
                    return false;

                // Check groups
                if (Equals(Source.Group, typedSource.Source.Group))
                    return Equals(Source.Location, typedSource.Source.Location);
                else
                    return false;
            }

            /// <summary>
            /// Prüft ob zwei Quellen identisch sind.
            /// </summary>
            /// <param name="source">Eine andere Quelle.</param>
            /// <returns>Gesetzt, wenn die Quellen identisch sind.</returns>
            public bool IsSameAs(IScheduleSource source)
            {
                // Pre-test
                if (!BelongsToSameSourceGroupAs(source))
                    return false;

                // Can now safe cast
                var typedSource = source as _Source;

                // Just test the identification
                return Equals(Source.Source, typedSource!.Source.Source);
            }

            /// <summary>
            /// Erzeugt eine neue Repräsentation.
            /// </summary>
            /// <param name="source">Die zugehörige Quelle.</param>
            /// <exception cref="ArgumentNullException">Es wurde keine Quelle angegeben.</exception>
            public _Source(SourceSelection source)
            {
                // Validate
                ArgumentNullException.ThrowIfNull(source);

                if (source.Source is not DVB.Station) throw new ArgumentException(null, nameof(source));

                // Remember
                Source = source;
            }

            /// <summary>
            /// Erstellt einen Anzeigetext zu Testzwecken.
            /// </summary>
            /// <returns>Der gewünschte Anzeigetext.</returns>
            public override string? ToString() => Source.Source.ToString();
        }

        /// <summary>
        /// Stellt ein Geräteprofil zur Verwaltung zur Verfügung.
        /// </summary>
        private class _Implementation : ScheduleResource<_Implementation, _Source>
        {
            /// <summary>
            /// Alle Quellen, die wir bereits einmal getestet haben.
            /// </summary>
            private readonly ConcurrentDictionary<SourceSelection, bool> m_accessTests = new(ReferenceComparer<SourceSelection>.Default);

            /// <summary>
            /// Der Änderungszähler der Geräteprofile - wir wissen, dass der Zähler immer bei 0 beginnt, i.e. wir erzwingen immer ein einmaliges Zurücksetzen.
            /// </summary>
            private volatile int m_profileVersion = -1;

            /// <summary>
            /// Das zu verwendende Geräteprofil.
            /// </summary>
            public Profile Profile => ProfileManager.FindProfile(Name) ?? throw new ArgumentException(Name, "profileName");

            /// <summary>
            /// Erzeugt eine neue Verwaltung für ein Geräteprofil.
            /// </summary>
            /// <param name="profileName">Das zugehörige Geräteprofil.</param>
            public _Implementation(string profileName)
            {
                // Name first - want to access profile!
                Name = profileName;

                // Copy settings
                Decryption = new DecryptionLimits { MaximumParallelSources = ReadRecordingParameter(DecryptionLimitName, DefaultDecryptionLimit) };
                SourceLimit = ReadRecordingParameter(ParallelSourceLimitName, DefaultParallelSourceLimit);
                AbsolutePriority = ReadRecordingParameter(SchedulePriorityName, DefaultSchedulePriority);
            }

            /// <summary>
            /// Liest eine verfeinerte Einstellung für Aufzeichnungen.
            /// </summary>
            /// <param name="name">Der Name des Parameters.</param>
            /// <param name="defaultValue">Der Wert, falls dieser nicht explizit angegeben ist.</param>
            /// <returns>Der gewünschte Wert.</returns>
            private int ReadRecordingParameter(string name, int defaultValue)
            {
                // Read the value
                uint value;
                if (!uint.TryParse(Profile.GetParameter(name), out value))
                    return defaultValue;
                else
                    return checked((int)value);
            }

            /// <summary>
            /// Meldet zu Testzwecken einen Anzeigenamen.
            /// </summary>
            /// <returns>Der Name des verwendeten Geräteprofils.</returns>
            public override string ToString()
            {
                // Forward
                return Name;
            }

            /// <summary>
            /// Prüft, ob eine bestimmte Quelle über dieses Gerät angesprochen werden kann.
            /// </summary>
            /// <param name="source">Die gewünschte Quelle.</param>
            /// <returns>Gesetzt, wenn die Quelle angesprochen werden kann.</returns>
            protected override bool TestAccess(_Source source)
            {
                // Check for reset - typically after a source scan
                if (m_profileVersion != ProfileManager.RefreshCounter)
                {
                    // Reset
                    m_accessTests.Clear();

                    // Remember
                    m_profileVersion = ProfileManager.RefreshCounter;
                }

                // See if we can provide exactly this source
                return m_accessTests.GetOrAdd(source.Source, selection => Profile.FindSource(selection.Source).Any(s => Equals(s.Group, selection.Group) && Equals(s.Location, selection.Location)));
            }
        }

        /// <summary>
        /// Erzeugt eine neue Verwaltung für ein Geräteprofil.
        /// </summary>
        /// <param name="profileName">Das zugehörige Geräteprofil.</param>
        /// <returns>Die gewünschte neue Verwaltungsinstanz.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Geräteprofil angegeben.</exception>
        /// <exception cref="ArgumentException">Das angegebene Geräteprofil ist nicht bekannt.</exception>
        public static IScheduleResource Create(string profileName)
        {
            // Validate
            ArgumentException.ThrowIfNullOrEmpty(profileName);

            // Look it up
            var profile = ProfileManager.FindProfile(profileName) ?? throw new ArgumentException(null, nameof(profileName));

            // Forward
            return new _Implementation(profile.Name);
        }

        /// <summary>
        /// Erzeugt eine neue Kapselung für eine Quelle.
        /// </summary>
        /// <param name="source">Die Information zur Quelle.</param>
        /// <returns>Die gewünschte Kapselung.</returns>
        public static IScheduleSource CreateSource(SourceSelection source)
        {
            // Forward
            return new _Source(source);
        }
    }
}
