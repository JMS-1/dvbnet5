﻿using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt eine mögliche Datenquelle.
    /// </summary>
    /// <typeparam name="TReal">Die konkrete Art der Klasse.</typeparam>
    public abstract class SourceInformation<TReal> where TReal : SourceInformation<TReal>, new()
    {
        /// <summary>
        /// Der Anzeigename der Quelle.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gesetzt, wenn die Quelle verschlüsselt ist.
        /// </summary>
        public bool IsEncrypted { get; set; }

        /// <summary>
        /// Führt individuelle Initialisierungen aus.
        /// </summary>
        /// <param name="station">Die Informationen zur Quelle.</param>
        protected abstract void OnCreate(Station station);

        /// <summary>
        /// Erstellt eine alternative Repräsentation einer Quelle.
        /// </summary>
        /// <param name="source">Die volle Beschreibung der Quelle.</param>
        /// <returns>Das Transferformat.</returns>
        public static TReal Create(SourceSelection source, IVCRProfiles profiles)
        {
            // Attach to the station
            var station = (Station)source.Source;

            // Construct
            var info = new TReal
            {
                IsEncrypted = station.IsEncrypted || station.IsService,
                Name = profiles.GetUniqueName(source),
            };

            // Finish setup
            info.OnCreate(station);

            // Report
            return info;
        }
    }
}
