extern alias oldVersion;

using Legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Über diese Klasse wird die Programmzeitschrift der PREMIERE Dienste empfangen.
    /// </summary>
    /// <param name="table">Die empfangene Tabelle.</param>
    public abstract class CIT(Legacy.EPG.Tables.CITPremiere table) : WellKnownLegacyTable<Legacy.EPG.Tables.CITPremiere>(table)
    {

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public CIT()
            : this(null!)
        {
        }

        /// <summary>
        /// Meldet die Liste der SI Tabellenarten, die von dieser Klasse
        /// abgedeckt werden.
        /// </summary>
        public override byte[] TableIdentifiers => [0xa0];

        /// <summary>
        /// Meldet, ob diese Tabelle nur Tabellenkennungen oberhalb von 0x7f verwendet.
        /// </summary>
        public override bool IsExtendedTable => true;
    }

    /// <summary>
    /// Empfängt die Programmzeitschrift der PREMIERE Direkt Dienste.
    /// </summary>
    /// <param name="table">Die empfangene Tabelle.</param>
    public class DirectCIT(Legacy.EPG.Tables.CITPremiere table) : CIT(table)
    {
        /// <summary>
        /// Die Quelle, in deren Quellgruppe (Transponder) die Programmzeitschrift bereitgestellt wird.
        /// </summary>
        public static readonly SourceIdentifier TriggerSource = new() { Network = 133, TransportStream = 4, Service = 18 };

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public DirectCIT()
            : this(null!)
        {
        }

        /// <summary>
        /// Meldet den Datenstrom, an den dieser Typ von Tabelle fest gebunden ist.
        /// </summary>
        public override ushort WellKnownStream => 0xb11;
    }

    /// <summary>
    /// Empfängt die Programmzeitschrift der PREMIERE Sport Dienste.
    /// </summary>
    /// <param name="table">Die empfangene Tabelle.</param>
    public class SportCIT(Legacy.EPG.Tables.CITPremiere table) : CIT(table)
    {
        /// <summary>
        /// Die Quelle, in deren Quellgruppe (Transponder) die Programmzeitschrift bereitgestellt wird.
        /// </summary>
        public static readonly SourceIdentifier TriggerSource = new() { Network = 133, TransportStream = 3, Service = 17 };

        /// <summary>
        /// Erzeugt eine neue Tabellenbeschreibung.
        /// </summary>
        public SportCIT()
            : this(null!)
        {
        }

        /// <summary>
        /// Meldet den Datenstrom, an den dieser Typ von Tabelle fest gebunden ist.
        /// </summary>
        public override ushort WellKnownStream => 0xb12;
    }
}
