using System.Diagnostics;
using JMS.DVB.NET.Recording.Services;

namespace JMS.DVB.NET.Recording
{
    /// <summary>
    /// Verwaltet Erweiterungen und deren Instanzen.
    /// </summary>
    public class ExtensionManager : MarshalByRefObject
    {
        /// <summary>
        /// Eine leere Liste von Prozessen.
        /// </summary>
        private static readonly Process[] s_noProcesses = { };

        /// <summary>
        /// Alle vom <i>VCR.NET Recording Service</i> gestarteten und noch aktiven Prozesse.
        /// </summary>
        private readonly List<Process> m_activeProcesses = new List<Process>();

        /// <summary>
        /// Startet Erweiterungen und ergänzt die zugehörigen Prozesse in der Verwaltung.
        /// </summary>
        /// <param name="extensionName">Der Name der Erweiterung.</param>
        /// <param name="environment">Die für die Erweiterung zu verwendenden Umgebungsvariablen.</param>
        public void AddWithCleanup(string extensionName, Dictionary<string, string> environment, VCRServer server, ILogger logger)
        {
            // Lookup, start and forward
            AddWithCleanup(Tools.RunExtensions(extensionName, environment, server, logger).ToArray());
        }

        /// <summary>
        /// Prüft, ob sich irgendwelchen aktiven Prozesse in der Verwaltung befinden.
        /// </summary>
        public bool HasActiveProcesses
        {
            get
            {
                // Check protected
                lock (m_activeProcesses)
                    return m_activeProcesses.Count > 0;
            }
        }

        /// <summary>
        /// Prüft alle ausstehenden Erweiterungen.
        /// </summary>
        public void Cleanup()
        {
            // Forward
            AddWithCleanup(s_noProcesses);
        }

        /// <summary>
        /// Ergänzt eine Liste von Prozessen nachdem alle bereits beendeten Prozesse aus der 
        /// Verwaltung entfernt wurden.
        /// </summary>
        /// <param name="processes">Eine neue Liste von Processen.</param>
        private void AddWithCleanup(Process[] processes)
        {
            // Synchronize
            lock (m_activeProcesses)
            {
                // Add the list
                m_activeProcesses.AddRange(processes);

                // Do the cleanup
                m_activeProcesses.RemoveAll(process =>
                    {
                        // Be aware of any error
                        try
                        {
                            // Check for it
                            if (!process.HasExited)
                                return false;

                            // Forget it not really necessary but speeds up handle cleanup a bit
                            process.Dispose();
                        }
                        catch
                        {
                            // Ignore any error - especially do not re-add the process
                        }

                        // Get rid of it
                        return true;
                    });
            }
        }
    }
}