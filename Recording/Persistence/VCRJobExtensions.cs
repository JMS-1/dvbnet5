using JMS.DVB.NET.Recording.Services;

namespace JMS.DVB.NET.Recording.Persistence
{
    public static class VCRJobExtensions
    {
        private static IVCRProfiles _profiles = null!;

        private static ILogger _logger = null!;

        public class Initializer
        {
            public Initializer(IVCRProfiles profiles, ILogger logger)
            {
                _logger = logger;
                _profiles = profiles;
            }
        }

        /// <summary>
        /// Speichert diesen Auftrag ab.
        /// </summary>
        /// <param name="target">Der Pfad zu einem Zielverzeichnis.</param>
        /// <returns>Gesetzt, wenn der Speichervorgang erfolgreich war. <i>null</i> wird
        /// gemeldet, wenn diesem Auftrag keine Datei zugeordnet ist.</returns>
        public static bool? Save(this VCRJob job, DirectoryInfo target)
        {
            // Get the file
            var file = job.GetFileName(target);
            if (file == null)
                return null;

            // Be safe
            try
            {
                // Process
                SerializationTools.Save(job, file);
            }
            catch (Exception e)
            {
                // Report
                _logger.Log(e);

                // Done
                return false;
            }

            // Done
            return true;
        }

        /// <summary>
        /// Löschte diesen Auftrag.
        /// </summary>
        /// <param name="target">Der Pfad zu einem Zielverzeichnis.</param>
        /// <returns>Gesetzt, wenn der Löschvorgang erfolgreich war. <i>null</i> wird gemeldet,
        /// wenn die Datei nicht existierte.</returns>
        public static bool? Delete(this VCRJob job, DirectoryInfo target)
        {
            // Get the file
            var file = job.GetFileName(target);
            if (file == null)
                return null;
            if (!file.Exists)
                return null;

            // Be safe
            try
            {
                // Process
                file.Delete();
            }
            catch (Exception e)
            {
                // Report error
                _logger.Log(e);

                // Failed
                return false;
            }

            // Did it
            return true;
        }
        /// <summary>
        /// Ermittelt den Namen dieses Auftrags in einem Zielverzeichnis.
        /// </summary>
        /// <param name="target">Der Pfad zu einem Zielverzeichnis.</param>
        /// <returns>Die zugehörige Datei.</returns>
        private static FileInfo? GetFileName(this VCRJob job, DirectoryInfo target)
            => job.UniqueID.HasValue
                ? new FileInfo(Path.Combine(target.FullName, job.UniqueID.Value.ToString("N").ToUpper() + VCRJob.FileSuffix))
                : null;

        /// <summary>
        /// Prüft, ob ein Auftrag zulässig ist.
        /// </summary>
        /// <param name="scheduleIdentifier">Die eindeutige Kennung der veränderten Aufzeichnung.</param>
        /// <exception cref="InvalidJobDataException">Die Konfiguration dieses Auftrags is ungültig.</exception>
        public static void Validate(this VCRJob job, Guid? scheduleIdentifier)
        {
            // Identifier
            if (!job.UniqueID.HasValue)
                throw new InvalidJobDataException("Die eindeutige Kennung ist ungültig");

            // Name
            if (!job.Name.IsValidName())
                throw new InvalidJobDataException("Der Name enthält ungültige Zeichen");

            // Test the station
            if (job.HasSource)
            {
                // Source
                if (!job.Source.Validate())
                    throw new InvalidJobDataException("Eine Quelle ist ungültig");

                // Streams
                if (!job.Streams.Validate())
                    throw new InvalidJobDataException("Die Aufzeichnungsoptionen sind ungültig");
            }
            else if (job.Streams != null)
                throw new InvalidJobDataException("Die Aufzeichnungsoptionen sind ungültig");

            // List of schedules
            if (job.Schedules.Count < 1)
                throw new InvalidJobDataException("Keine Aufzeichnungen vorhanden");

            // Only validate the schedule we updated
            if (scheduleIdentifier.HasValue)
                foreach (var schedule in job.Schedules)
                    if (!schedule.UniqueID.HasValue || schedule.UniqueID.Value.Equals(scheduleIdentifier))
                        schedule.Validate(job);
        }

        /// <summary>
        /// Stellt sicher, dass für diesen Auftrag ein Geräteprprofil ausgewählt ist.
        /// </summary>
        public static void SetProfile(this VCRJob job)
        {
            // No need
            if (!string.IsNullOrEmpty(job.Source?.ProfileName))
                return;

            // Attach to the default profile
            var defaultProfile = _profiles.DefaultProfile;
            if (defaultProfile == null)
                return;

            // Process
            if (job.Source == null)
                job.Source = new SourceSelection { ProfileName = defaultProfile.Name };
            else
                job.Source.ProfileName = defaultProfile.Name;
        }

        /// <summary>
        /// Prüft, ob eine Quelle gültig ist.
        /// </summary>
        /// <param name="source">Die Auswahl der Quelle oder <i>null</i>.</param>
        /// <returns>Gesetzt, wenn die Auswahl gültig ist.</returns>
        public static bool Validate(this SourceSelection source) => _profiles.FindSource(source) != null;
    }
}