﻿using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.ProgramGuide;
using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt die Daten eines Auftrags.
    /// </summary>
    public class EditJob
    {
        /// <summary>
        /// Der Name des Auftrags.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Das Aufzeichnungsverzeichnis zum Auftrag.
        /// </summary>
        public string? RecordingDirectory { get; set; }

        /// <summary>
        /// Das für die Auswahl der Quelle verwendete Gerät.
        /// </summary>
        public string Profile { get; set; } = null!;

        /// <summary>
        /// Die Quelle, von der aufgezeichnet werden soll.
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Gesetzt, wenn die Aufzeichnung auf jeden Fall auf dem für die Auswahl der Quelle
        /// verwendetem Geräte ausgeführt werden soll.
        /// </summary>
        public bool UseProfileForRecording { get; set; }

        /// <summary>
        /// Gesetzt, wenn alle Tonspuren aufgezeichnet werden sollen.
        /// </summary>
        public bool AllLanguages { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch die <i>Dolby Digital</i> Tonspur aufgezeichnet werden soll.
        /// </summary>
        public bool DolbyDigital { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch der Videotext aufgezeichnet werden soll.
        /// </summary>
        public bool Videotext { get; set; }

        /// <summary>
        /// Gesetzt, wenn auch alle DVB Untertitel aufgezeichnet werden sollen.
        /// </summary>
        public bool DVBSubtitles { get; set; }

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="job">Der konkrete Auftag.</param>
        /// <param name="guide">Ein Eintrag der Programmzeitschrift.</param>
        /// <param name="profile">Vorgabe für das Geräteprofil.</param>
        /// <returns>Die zugehörige Beschreibung.</returns>
        public static EditJob? Create(VCRJob job, ProgramGuideEntry guide, string profile, IVCRProfiles profiles, UserProfile userProfile)
        {
            // Process
            if (job == null)
            {
                // No hope
                if (guide == null)
                    return null;

                // Create from program guide            
                return
                    new EditJob
                    {
                        Source = profiles.GetUniqueName(new SourceSelection { ProfileName = profile, Source = guide.Source }),
                        DVBSubtitles = userProfile.Subtitles,
                        DolbyDigital = userProfile.Dolby,
                        AllLanguages = userProfile.Languages,
                        Videotext = userProfile.Videotext,
                        UseProfileForRecording = false,
                        Name = guide.Name.MakeValid(),
                        Profile = profile,
                    };
            }

            // Optionen ermitteln
            var streams = job.Streams;
            var sourceName = profiles.GetUniqueName(job.Source);

            // Report            
            return
                new EditJob
                {
                    UseProfileForRecording = !job.AutomaticResourceSelection,
                    DolbyDigital = streams.GetUsesDolbyAudio(),
                    AllLanguages = streams.GetUsesAllAudio(),
                    DVBSubtitles = streams.GetUsesSubtitles(),
                    Videotext = streams.GetUsesVideotext(),
                    RecordingDirectory = job.Directory,
                    Profile = job.Source.ProfileName,
                    Source = sourceName,
                    Name = job.Name,
                };
        }

        /// <summary>
        /// Erstellt einen passenden Auftrag für die persistente Ablage.
        /// </summary>
        /// <returns>Der zugehörige Auftrag.</returns>
        public VCRJob CreateJob(IVCRProfiles profiles) => CreateJob(Guid.NewGuid(), profiles);

        /// <summary>
        /// Erstellt einen passenden Auftrag für die persistente Ablage.
        /// </summary>
        /// <param name="jobIdentifier">Die eindeutige Kennung des Auftrags.</param>
        /// <returns>Der zugehörige Auftrag.</returns>
        public VCRJob CreateJob(Guid jobIdentifier, IVCRProfiles profiles)
        {
            // Create core
            var job =
                new VCRJob
                {
                    AutomaticResourceSelection = !UseProfileForRecording,
                    Directory = RecordingDirectory ?? "",
                    UniqueID = jobIdentifier,
                    Name = Name,
                };

            // Check source
            var profile = Profile;
            if (string.IsNullOrEmpty(profile))
                return job;

            // Get the name of the source
            var sourceName = Source;
            if (string.IsNullOrEmpty(sourceName))
            {
                // Create profile reference
                job.Source = new SourceSelection { ProfileName = profile };

                // Done
                return job;
            }

            // Locate the source
            job.Source = profiles.FindSource(profile, sourceName)!;
            if (job.Source == null)
                return job;

            // Configure streams
            job.Streams = new StreamSelection();

            // Set all - oder of audio settings is relevant, dolby MUST come last
            job.Streams.SetUsesAllAudio(AllLanguages);
            job.Streams.SetUsesDolbyAudio(DolbyDigital);
            job.Streams.SetUsesSubtitles(DVBSubtitles);
            job.Streams.SetUsesVideotext(Videotext);
            job.Streams.ProgramGuide = true;

            // Report
            return job;
        }
    }
}
