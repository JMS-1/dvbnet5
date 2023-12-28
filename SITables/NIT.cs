extern alias oldVersion;

using Legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Diese Klasse beschreibt eine Tabelle mit der Beschreibung der
    /// Quellgruppen für ein Netzwerk.
    /// </summary>
    /// <param name="table">Die empfangene Tabelle.</param>
    public class FullNIT(Legacy.EPG.Tables.NIT table) : WellKnownLegacyTable<Legacy.EPG.Tables.NIT>(table)
    {

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public FullNIT()
            : this(null!)
        {
        }

        /// <summary>
        /// Meldet die Liste der SI Tabellenarten, die von dieser Klasse
        /// abgedeckt werden.
        /// </summary>
        public override byte[] TableIdentifiers => [0x40, 0x41];

        /// <summary>
        /// Meldet, ob sich die Daten in der Tabelle auf die aktuell angewählte
        /// Quellgruppe bezieht.
        /// </summary>
        public bool ForCurrentGroup => 0x40 == Table.Section.TableIdentifier;

        /// <summary>
        /// Meldet den Datenstrom, an den dieser Typ von Tabelle fest gebunden ist.
        /// </summary>
        public override ushort WellKnownStream => 0x10;
    }

    /// <summary>
    /// Diese Klasse beschreibt eine Tabelle mit der Beschreibung der
    /// Quellgruppen für das aktuell angewählte Netzwerk.
    /// </summary>
    /// <param name="table">Die empfangene Tabelle.</param>
    public class NIT(Legacy.EPG.Tables.NIT table) : FullNIT(table)
    {

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public NIT()
            : this(null!)
        {
        }

        /// <summary>
        /// Meldet die Liste der SI Tabellenarten, die von dieser Klasse
        /// abgedeckt werden.
        /// </summary>
        public override byte[] TableIdentifiers => [0x40];
    }
}
