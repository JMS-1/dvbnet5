using JMS.DVB.CardServer;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt einen Dienst.
    /// </summary>
    public class ZappingService
    {
        /// <summary>
        /// Der Name des Dienstes.
        /// </summary>
        public string NameWithIndex { get; set; } = null!;

        /// <summary>
        /// Die eindeutige Kennung.
        /// </summary>
        public string Source { get; set; } = null!;

        /// <summary>
        /// Die laufende Nummer dieses Dienstes.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Ermittelt die laufende Nummer eines Dienstes.
        /// </summary>
        /// <param name="uniqueName">Ein Dienst.</param>
        /// <returns>Die laufende Nummer des Dienstes.</returns>
        private static int GetServiceIndex(string uniqueName)
        {
            // Check name
            if (string.IsNullOrEmpty(uniqueName))
                return -1;

            // Split
            int i = uniqueName.IndexOf(',');
            if (i < 0)
                return -1;

            // Get the part
            if (uint.TryParse(uniqueName.Substring(0, i), out uint result))
                if (result < int.MaxValue)
                    return (int)result;
                else
                    return -1;
            else
                return -1;
        }

        /// <summary>
        /// Erstellt eine neue Dienstbeschreibung.
        /// </summary>
        /// <param name="service">Die Beschreibung des Dienstes.</param>
        /// <returns>Der gewünschte Dienst.</returns>
        public static ZappingService Create(ServiceInformation service)
        {
            // Validate
            ArgumentNullException.ThrowIfNull(service);

            // Create new
            return
                new ZappingService
                {
                    Source = SourceIdentifier.ToString(service.Service)!.Replace(" ", ""),
                    Index = GetServiceIndex(service.UniqueName),
                    NameWithIndex = service.UniqueName
                };
        }
    }
}
