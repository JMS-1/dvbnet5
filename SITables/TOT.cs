extern alias oldVersion;

using Legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Beschreibt eine <i>Time Offset Table</i> Tabelle.
    /// </summary>
    /// <param name="table">Die empfangene Tabelle.</param>
    public class TOT(Legacy.EPG.Tables.TOT table) : WellKnownLegacyTable<Legacy.EPG.Tables.TOT>(table)
    {

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public TOT()
            : this(null!)
        {
        }

        /// <summary>
        /// Meldet die Liste der SI Tabellenarten, die von dieser Klasse
        /// abgedeckt werden.
        /// </summary>
        public override byte[] TableIdentifiers => [0x73];

        /// <summary>
        /// Meldet den Datenstrom, an den dieser Typ von Tabelle fest gebunden ist.
        /// </summary>
        public override ushort WellKnownStream => 0x14;

        /// <summary>
        /// Meldet den dieser Tabelle zugeordneten Zeitpunkt.
        /// </summary>
        public DateTime TimeStamp => Table.Time;
    }
}
