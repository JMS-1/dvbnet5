using JMS.DVB.CardServer;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt den aktuellen Zustand.
    /// </summary>
    public class ZappingStatus
    {
        /// <summary>
        /// Eine leere Liste von Diensten.
        /// </summary>
        private static readonly ZappingService[] s_NoServices = [];

        /// <summary>
        /// Das aktuelle Ziel der Nutzdaten.
        /// </summary>
        public string Target { get; set; } = null!;

        /// <summary>
        /// Die aktuelle Quelle.
        /// </summary>
        public string Source { get; set; } = null!;

        /// <summary>
        /// Alle auf der aktuellen Quellgruppe verfügbaren Dienste.
        /// </summary>
        public ZappingService[] Services { get; set; } = null!;

        /// <summary>
        /// Erstellt einen neuen Zustand.
        /// </summary>
        /// <param name="target">Das aktuelle Ziel des Datenversands.</param>
        /// <param name="server">Die zugehörigen Informationen des Aufzeichnungsprozesses.</param>
        /// <returns>Der gwünschte Zustand.</returns>
        public static ZappingStatus Create(string target, ServerInformation server)
        {
            // Create new
            var status = new ZappingStatus { Target = target, Services = s_NoServices };

            // No state 
            if (server == null)
                return status;

            // Attach to the first stream
            var streams = server.Streams;
            if (streams != null)
                if (streams.Count > 0)
                    status.Source = SourceIdentifier.ToString(streams[0].Source)!.Replace(" ", "");

            // Fill in NVOD services in the standard index order
            var services = server.Services;
            if (services != null)
                status.Services =
                    [.. services
                        .Where(service => service != null)
                        .Select(ZappingService.Create)
                        .OrderBy(service => service.Index)];

            // Report
            return status;
        }
    }
}
