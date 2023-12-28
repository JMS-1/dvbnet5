extern alias oldVersion;

using Legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Diese Klasse beschreibt eine Tabelle mit der Beschreibung aller
    /// Dienste, die über die zugehörige Quellgruppe oder eine andere
    /// Quellgruppe mit dem selben Ursprung verfügbar sind.
    /// </summary>
    /// <param name="table">Die empfangene Tabelle.</param>
    public class FullSDT(Legacy.EPG.Tables.SDT table) : WellKnownLegacyTable<Legacy.EPG.Tables.SDT>(table)
    {

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public FullSDT()
            : this(null!)
        {
        }

        /// <summary>
        /// Meldet die Liste der SI Tabellenarten, die von dieser Klasse
        /// abgedeckt werden.
        /// </summary>
        public override byte[] TableIdentifiers => [0x42, 0x46];

        /// <summary>
        /// Meldet den Datenstrom, an den dieser Typ von Tabelle fest gebunden ist.
        /// </summary>
        public override ushort WellKnownStream => 0x11;

        /// <summary>
        /// Meldet die eindeutige Nummer des zugehörigen <i>Transport Streams</i>.
        /// </summary>
        public ushort TransportStream => Table.TransportStreamIdentifier;

        /// <summary>
        /// Meldet die eindeutige Nummer des zugehörigen DVB Ursprungsnetzwerks.
        /// </summary>
        public ushort Network => Table.OriginalNetworkIdentifier;
    }

    /// <summary>
    /// Diese Klasse beschreibt eine Tabelle mit der Beschreibung aller
    /// Dienste, die über die zugehörige Quellgruppe verfügbar sind.
    /// </summary>
    /// <param name="table">Die empfangene Tabelle.</param>
    public class SDT(Legacy.EPG.Tables.SDT table) : FullSDT(table)
    {

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public SDT()
            : this(null!)
        {
        }

        /// <summary>
        /// Meldet die Liste der SI Tabellenarten, die von dieser Klasse
        /// abgedeckt werden.
        /// </summary>
        public override byte[] TableIdentifiers => [0x42];
    }
}
