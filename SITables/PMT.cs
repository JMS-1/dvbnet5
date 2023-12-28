extern alias oldVersion;

using Legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Mit dieser Klasse werden die Datenströme einer einzelnen Quelle beschrieben.
    /// </summary>
    /// <param name="table">Die empfangene Tabelle.</param>
    public class PMT(Legacy.EPG.Tables.PMT table) : LegacyTable<Legacy.EPG.Tables.PMT>(table)
    {

        /// <summary>
        /// Erzeugt eine neue Quellbeschreibung.
        /// </summary>
        public PMT()
            : this(null!)
        {
        }

        /// <summary>
        /// Meldet die Liste der SI Tabellenarten, die von dieser Klasse
        /// abgedeckt werden.
        /// </summary>
        public override byte[] TableIdentifiers => [0x02];
    }
}
