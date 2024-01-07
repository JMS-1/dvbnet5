
using System.Diagnostics;
using JMS.DVB.NET.Recording.Persistence;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Die Basisklasse f�r alle Anwendungen, die vom <i>VCR.NET Recording Service</i> gestartet werden.
    /// </summary>
    public class ApplicationRuntime : MarshalByRefObject
    {
        /// <summary>
        /// Meldet die <see cref="AppDomain"/>, in der ASP.NET l�uft.
        /// </summary>
        public AppDomain AppDomain => AppDomain.CurrentDomain;

        /// <summary>
        /// Instanzen dieser Klasse sind nicht zeitgebunden.
        /// </summary>
        /// <returns>Die Antwort muss immer <i>null</i> sein.</returns>
        [Obsolete]
        public override object InitializeLifetimeService() => null!;

        /// <summary>
        /// Wird periodisch aufgerufen um zu sehen, ob die Anwendung noch verf�gbar ist.
        /// </summary>
        public void Test()
        {
        }
    }

    /// <summary>
    /// Eine Instanz dieser Klasse sorgt f�r die Bereitstellung der ASP.NET
    /// Laufzeitumgebung. Sie wird in einer eigenen <see cref="AppDomain"/>
    /// gestartet und nimmt HTTP Anfragen zur Bearbeitung entgegen.
    /// </summary>
    public class ServerRuntime : ApplicationRuntime
    {
        /// <summary>
        /// Startet die Laufzeitumgebung.
        /// </summary>
        public static void WebStartup()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        private static VCRServer _VCRServer = null!;

        /// <summary>
        /// Erzeugt eine ASP.NET Laufzeitumgebung.
        /// </summary>
        public ServerRuntime()
        {
            // Check for active debugger
            Tools.EnableTracing = Debugger.IsAttached;
            Tools.DomainName = "Virtual Directory";
        }

        /// <summary>
        /// Verbindet die ASP.NET Laufzeitumgebung des aktuellen virtuellen Verzeichnisses
        /// mit der aktiven VCR.NET Instanz.
        /// </summary>
        /// <param name="server">Die aktive VCR.NET Instanz.</param>
        public void SetServer(VCRServer server)
        {
            // Add to permanent cache
            _VCRServer = server;
        }

        /// <summary>
        /// Beendet die ASP.NET Laufzeitumgebung.
        /// </summary>
        /// <remarks>
        /// Der Aufruf kehrt erst wieder zur�ck, wenn alle ausstehenden Anfragen bearbeitet
        /// wurden. Neue Anfragen werden nicht angenommen.
        /// </remarks>
        public void Stop()
        {
            // Reset
            _VCRServer = null!;
        }

        /// <summary>
        /// Referenz auf die <see cref="AppDomain"/> des Dienstes melden.
        /// </summary>
        public static VCRServer VCRServer
        {
            get
            {
                // Report
                return _VCRServer;
            }
        }

        /// <summary>
        /// Ermittelt eine Referenz f�r eine bestimmte Aufzeichung in einem Auftrag, so dass diese
        /// auch in einer URL verwendet werden kann.
        /// </summary>
        /// <param name="job">Ein Auftrag.</param>
        /// <param name="schedule">Eine Aufzeichnung.</param>
        /// <returns>Die eindeutige Referenz.</returns>
        public static string GetUniqueWebId(VCRJob job, VCRSchedule schedule)
        {
            // Forward
            if (job == null)
                return "*";
            else if (schedule == null)
                return $"*{job.UniqueID!.Value:N}";
            else
                return GetUniqueWebId(job.UniqueID!.Value, schedule.UniqueID!.Value);
        }

        /// <summary>
        /// Ermittelt eine Referenz f�r eine bestimmte Aufzeichung in einem Auftrag, so dass diese
        /// auch in einer URL verwendet werden kann.
        /// </summary>
        /// <param name="job">Die eindeutige Kennung eines Auftrags.</param>
        /// <param name="schedule">Die eindeutige Kennung einer Aufzeichnung des Auftrags.</param>
        /// <returns>Die eindeutige Referenz.</returns>
        public static string GetUniqueWebId(string job, string schedule)
        {
            // Use defaults
            if (string.IsNullOrEmpty(job))
                job = Guid.Empty.ToString("N");
            if (string.IsNullOrEmpty(schedule))
                schedule = Guid.Empty.ToString("N");

            // Create
            return $"{job}{schedule}";
        }

        /// <summary>
        /// Ermittelt eine Referenz f�r eine bestimmte Aufzeichung in einem Auftrag, so dass diese
        /// auch in einer URL verwendet werden kann.
        /// </summary>
        /// <param name="job">Die eindeutige Kennung eines Auftrags.</param>
        /// <param name="schedule">Die eindeutige Kennung einer Aufzeichnung des Auftrags.</param>
        /// <returns>Die eindeutige Referenz.</returns>
        public static string GetUniqueWebId(Guid job, Guid schedule) => GetUniqueWebId(job.ToString("N"), schedule.ToString("N"));

        /// <summary>
        /// Rekonstruiert einen Auftrag und eine Aufzeichnung aus einer Textdarstellung.
        /// </summary>
        /// <param name="id">Die Textdarstellung.</param>
        /// <param name="job">Der zugeh�rige Auftrag.</param>
        /// <param name="schedule">Die Aufzeichnung in dem Auftrag.</param>
        public static void ParseUniqueWebId(string id, out Guid job, out Guid schedule)
        {
            // Read all
            schedule = new Guid(id.Substring(32, 32));
            job = new Guid(id.Substring(0, 32));
        }

        /// <summary>
        /// Rekonstruiert einen Auftrag und eine Aufzeichnung aus einer Textdarstellung.
        /// </summary>
        /// <param name="id">Die Textdarstellung.</param>
        /// <param name="job">Der ermittelte Auftrag.</param>
        /// <returns>Die zugeh�rige Aufzeichnung im Auftrag.</returns>
        public static VCRSchedule? ParseUniqueWebId(string id, out VCRJob job)
        {
            ParseUniqueWebId(id, out Guid jobID, out Guid scheduleID);

            // Find the job
            job = VCRServer.FindJob(jobID)!;

            // Report schedule if job exists
            if (job == null)
                return null;
            else
                return job[scheduleID];
        }

        /// <summary>
        /// Aktualisiert die Regeln f�r die Aufzeichnungsplanung.
        /// </summary>
        /// <param name="newRules">Die ab nun zu verwendenden Regeln.</param>
        /// <returns>Meldet, ob ein Neustart erforderlich ist.</returns>
        public static bool? UpdateSchedulerRules(string newRules)
        {
            // Check state
            if (VCRServer.IsActive)
                return null;

            // Process
            VCRServer.SchedulerRules = newRules;

            // Do not restart in debug mode
            if (VCRServer.InDebugMode)
                return null;

            // Create new process to restart the service
            Process.Start(Tools.ExecutablePath, "Restart").Dispose();

            // Finally back to the administration page
            return true;
        }

        /// <summary>
        /// F�hrt eine Aktualisierung von Konfigurationswerten durch.
        /// </summary>
        /// <param name="settings">Die zu aktualisierenden Konfigurationswerte.</param>
        /// <param name="forceRestart">Erzwingt einen Neustart des Dienstes.</param>
        /// <returns>Gesetzt, wenn ein Neustart erforderlich ist.</returns>
        public static bool? Update(IEnumerable<VCRConfigurationOriginal.SettingDescription> settings, bool forceRestart = false)
        {
            // Check state
            if (VCRServer.IsActive)
                return null;

            // Process
            if (VCRConfigurationOriginal.CommitUpdate(settings) || forceRestart)
            {
                // Do not restart in debug mode
                if (VCRServer.InDebugMode)
                    return null;

                // Create new process to restart the service
                Process.Start(Tools.ExecutablePath, "Restart").Dispose();

                // Finally back to the administration page
                return true;
            }
            else
            {
                // Check for new tasks
                _VCRServer.BeginNewPlan();

                // Finally back to the administration page
                return false;
            }
        }
    }

    /// <summary>
    /// Einige Hilfsmethoden zur Vereinfachung der Webanwendung.
    /// </summary>
    public static class ServerRuntimeExtensions
    {
        /// <summary>
        /// Pr�ft, ob eine Datenstromkonfiguration eine Dolby Digital Tonspur nicht 
        /// grunds�tzlich ausschlie�t.
        /// </summary>
        /// <param name="streams">Die Datenstromkonfiguration.</param>
        /// <returns>Gesetzt, wenn die AC3 Tonspur nicht grunds�tzlich deaktiviert ist.</returns>
        public static bool GetUsesDolbyAudio(this StreamSelection streams)
        {
            // Check mode
            if (streams == null)
                return false;
            else if (streams.AC3Tracks.LanguageMode != LanguageModes.Selection)
                return true;
            else
                return (streams.AC3Tracks.Languages.Count > 0);
        }

        /// <summary>
        /// Pr�ft, ob eine Datenstromkonfiguration alle Tonspuren einschlie�t.
        /// </summary>
        /// <param name="streams">Die Datenstromkonfiguration.</param>
        /// <returns>Gesetzt, wenn alle Tonspuren aufgezeichnet werden sollen.</returns>
        public static bool GetUsesAllAudio(this StreamSelection streams) => (streams != null) && (streams.MP2Tracks.LanguageMode == LanguageModes.All);

        /// <summary>
        /// Pr�ft, ob eine Datenstromkonfiguration DVB Untertitel nicht 
        /// grunds�tzlich ausschlie�t.
        /// </summary>
        /// <param name="streams">Die Datenstromkonfiguration.</param>
        /// <returns>Gesetzt, wenn die DVB Untertitel nicht grunds�tzlich deaktiviert sind.</returns>
        public static bool GetUsesSubtitles(this StreamSelection streams)
        {
            // Check mode
            if (streams == null)
                return false;
            else if (streams.SubTitles.LanguageMode != LanguageModes.Selection)
                return true;
            else
                return (streams.SubTitles.Languages.Count > 0);
        }

        /// <summary>
        /// Pr�ft, ob eine Datenstromkonfiguration auch den Videotext umfasst.
        /// </summary>
        /// <param name="streams">Die Datenstromkonfiguration.</param>
        /// <returns>Gesetzt, wenn der Videotext aufgezeichnet werden soll.</returns>
        public static bool GetUsesVideotext(this StreamSelection streams) => (streams != null) && streams.Videotext;

        /// <summary>
        /// Pr�ft, ob eine Datenstromkonfiguration auch einen Extrakt der Programmzeitschrift umfasst.
        /// </summary>
        /// <param name="streams">Die Datenstromkonfiguration.</param>
        /// <returns>Gesetzt, wenn die Programmzeitschrift ber�cksichtigt werden soll.</returns>
        public static bool GetUsesProgramGuide(this StreamSelection streams) => (streams != null) && streams.ProgramGuide;

        /// <summary>
        /// Legt fest, ob die Dolby Digital Tonspur aufgezeichnet werden soll.
        /// </summary>
        /// <param name="streams">Die Konfiguration der zu verwendenden Datenstr�me.</param>
        /// <param name="set">Gesetzt, wenn die Datenspur aktiviert werden soll.</param>
        public static void SetUsesDolbyAudio(this StreamSelection streams, bool set)
        {
            // Reset language list
            streams.AC3Tracks.Languages.Clear();

            // Check mode
            if (set)
                if (streams.MP2Tracks.LanguageMode == LanguageModes.Selection)
                    streams.AC3Tracks.LanguageMode = LanguageModes.Primary;
                else
                    streams.AC3Tracks.LanguageMode = streams.MP2Tracks.LanguageMode;
            else
                streams.AC3Tracks.LanguageMode = LanguageModes.Selection;
        }

        /// <summary>
        /// Legt fest, ob alle Tonspuren aufgezeichnet werden sollen.
        /// </summary>
        /// <param name="streams">Die Konfiguration der zu verwendenden Datenstr�me.</param>
        /// <param name="set">Gesetzt, wenn die Datenspuren aktiviert werden sollen.</param>
        public static void SetUsesAllAudio(this StreamSelection streams, bool set)
        {
            // Clear all
            streams.MP2Tracks.Languages.Clear();
            streams.AC3Tracks.Languages.Clear();

            // Check mode
            if (set)
                streams.MP2Tracks.LanguageMode = LanguageModes.All;
            else
                streams.MP2Tracks.LanguageMode = LanguageModes.Primary;

            // Forward
            if (streams.AC3Tracks.LanguageMode != LanguageModes.Selection)
                streams.AC3Tracks.LanguageMode = streams.MP2Tracks.LanguageMode;
        }

        /// <summary>
        /// Legt fest, ob DVB Untertitel aufgezeichnet werden sollen.
        /// </summary>
        /// <param name="streams">Die Konfiguration der zu verwendenden Datenstr�me.</param>
        /// <param name="set">Gesetzt, wenn die Datenspuren aktiviert werden sollen.</param>
        public static void SetUsesSubtitles(this StreamSelection streams, bool set)
        {
            // Reset language list
            streams.SubTitles.Languages.Clear();

            // Check mode
            if (set)
                streams.SubTitles.LanguageMode = LanguageModes.All;
            else
                streams.SubTitles.LanguageMode = LanguageModes.Selection;
        }

        /// <summary>
        /// Legt fest, ob der Videotext mit aufgezeichnet werden soll.
        /// </summary>
        /// <param name="streams">Die Konfiguration der zu verwendenden Datenstr�me.</param>
        /// <param name="set">Gesetzt, wenn die Datenspur aktiviert werden soll.</param>
        public static void SetUsesVideotext(this StreamSelection streams, bool set) => streams.Videotext = set;
    }
}
