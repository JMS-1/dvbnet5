extern alias oldVersion;

using System.Collections;

using Legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Diese Klasse beschreibt eine Tabelle mit Daten für alle Dienste
    /// einer Quellgruppe - neben normalen Radio- und Fernsehquellen kann
    /// es auch Datendienste geben, die allerdings für DVB.NET nicht
    /// von Interesse sind.
    /// </summary>
    /// <param name="table">Die empfangene Tabelle.</param>
    public class PAT(Legacy.EPG.Tables.PAT table) : WellKnownLegacyTable<Legacy.EPG.Tables.PAT>(table), IEnumerable<KeyValuePair<ushort, ushort>>
    {

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public PAT()
            : this(null!)
        {
        }

        /// <summary>
        /// Meldet die Liste der SI Tabellenarten, die von dieser Klasse
        /// abgedeckt werden.
        /// </summary>
        public override byte[] TableIdentifiers => [0x00];

        /// <summary>
        /// Meldet den Datenstrom, an den dieser Typ von Tabelle fest gebunden ist.
        /// </summary>
        public override ushort WellKnownStream => 0x00;

        /// <summary>
        /// Meldet die Anzahl der verwalteten Assoziationen.
        /// </summary>
        public int Count => Table.ProgramIdentifier.Count;

        /// <summary>
        /// Meldet die Datenstromkennung (PID) der SI Tablle PMT eines Dienstes.
        /// </summary>
        /// <param name="serviceIdentifier">Die eindeutige Kennung des Dienstes.</param>
        /// <returns>Die zugehörige Datenstromkennung oder <i>null</i>, wenn
        /// der Dienst nicht bekannt ist.</returns>
        public ushort? this[ushort serviceIdentifier]
        {
            get
            {
                // Load
                if (Table.ProgramIdentifier.TryGetValue(serviceIdentifier, out var pmt))
                    return pmt;

                // Not found
                return null;
            }
        }

        /// <summary>
        /// Meldet die eindeutigen Kennungen aller Dienste.
        /// </summary>
        public IEnumerable<ushort> Services => Table.ProgramIdentifier.Keys;

        /// <summary>
        /// Meldet die eindeutige Nummer des zugehörigen <i>Transport Streams</i>.
        /// </summary>
        public ushort TransportStream => Table.TransportStreamIdentifier;


        #region IEnumerable<KeyValuePair<ushort,ushort>> Members

        /// <summary>
        /// Meldet eine Auflistung über alle Assoziationen.
        /// </summary>
        /// <returns>Eine neu erzeugte Auflistung.</returns>
        public IEnumerator<KeyValuePair<ushort, ushort>> GetEnumerator() => Table.ProgramIdentifier.GetEnumerator();

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Meldet eine Auflistung über alle Assoziationen.
        /// </summary>
        /// <returns>Eine neu erzeugte Auflistung.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
