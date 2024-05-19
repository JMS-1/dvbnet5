extern alias oldVersion;

using Legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Basisklasse zur Analyse von Rohdatenströmen mit SI Tabellen.
    /// </summary>
    public class TableParser
    {
        /// <summary>
        /// Methode, die für jede empfangene SI Tabelle aufgerufen wird.
        /// </summary>
        private readonly Action<Table> m_Consumer;

        /// <summary>
        /// Alle von dieser Analyseeinheiten auszuwertenden Arten von SI Tabellen - Tabellen
        /// anderer Art werden einfach verworfen.
        /// </summary>
        private readonly Dictionary<byte, Type> m_Types = [];

        /// <summary>
        /// Übergangslösung für DVB.NET 3.5.1: es wird der SI Mechanismus von DVB.NET 3.5
        /// (oder früher) verwendet.
        /// </summary>
        private Legacy.EPG.Parser m_Parser = new();

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <param name="consumer">Methode zur Auswertung der SI Tabellen.</param>
        /// <param name="tableTypes">Die Liste der zu unterstützenden Arten von SI Tabellen.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Verbraucher oder keine Art angegeben.</exception>
        /// <exception cref="ArgumentException">Eine SI Tabellenart wird mehrfach verwendet oder einer der
        /// angegebenen .NET Klassen ist keine <see cref="Table"/>.</exception>
        private TableParser(Action<Table> consumer, List<Type> tableTypes)
        {
            // Validate
            ArgumentNullException.ThrowIfNull(consumer);
            ArgumentNullException.ThrowIfNull(tableTypes);

            if (tableTypes.Count < 1) throw new ArgumentException(null, nameof(tableTypes));

            // Remember
            m_Consumer = consumer;

            // Build up
            foreach (Type tableType in tableTypes)
            {
                // Validate
                ArgumentNullException.ThrowIfNull(tableTypes);

                if (!typeof(Table).IsAssignableFrom(tableType)) throw new ArgumentException(tableType.FullName, nameof(tableTypes));

                // Process each type
                foreach (byte tableIdentifier in Table.GetTableIdentifiers(tableType))
                    if (m_Types.ContainsKey(tableIdentifier))
                        throw new ArgumentException(tableIdentifier.ToString(), nameof(tableTypes));
                    else
                        m_Types[tableIdentifier] = tableType;
            }

            // Attach analyser
            m_Parser.SectionFound += SectionFound;
        }

        /// <summary>
        /// Wird aufgerufen, sobald eine Tabelle vollständig analysiert wurde.
        /// </summary>
        /// <param name="section">Der SI Bereich zur Tabelle.</param>
        private void SectionFound(Legacy.EPG.Section section)
        {
            // Attach to consumer
            Action<Table> consumer = m_Consumer;

            // Disabled
            if (null == consumer)
                return;

            // Skip if not valid
            if ((null == section) || !section.IsValid)
                return;

            // Skip if no table
            if ((null == section.Table) || !section.Table.IsValid)
                return;

            // Find the related type
            if (!m_Types.TryGetValue(section.TableIdentifier, out var tableType))
                return;

            // Be safe
            try
            {
                // Create wrapped version
                var wrapper = (Table)Activator.CreateInstance(tableType, section.Table)!;

                // Forward
                consumer(wrapper);
            }
            catch
            {
                // Ignore any error
            }
        }

        /// <summary>
        /// Erzeugt eine neue Instanz.
        /// </summary>
        /// <param name="consumer">Methode zur Auswertung der SI Tabellen.</param>
        /// <param name="tableType">Die primäre Art der verwendeten SI Tabelle.</param>
        /// <param name="tableTypes">Optional eine Liste weiterer zu unterstützender Arten von SI Tabellen.</param>
        /// <exception cref="ArgumentNullException">Es wurde kein Verbraucher oder keine Art angegeben.</exception>
        /// <returns>Die neu erzeugte Analyseinstanz.</returns>
        public static TableParser Create(Action<Table> consumer, Type tableType, params Type[] tableTypes)
        {
            // Create the list
            List<Type> types = [tableType];

            // Add optional types
            if (null != tableTypes)
                types.AddRange(tableTypes);

            // Forward
            return new TableParser(consumer, types);
        }

        /// <summary>
        /// Erzeugt einen neue Analyseinstanz für genau eine Art von SI Tabelle.
        /// </summary>
        /// <typeparam name="T">Die gewünschte Tabellenart.</typeparam>
        /// <param name="consumer">Die Methode, die mit jeder erkannten Tabelle aufgerufen 
        /// werden soll.</param>
        /// <returns>Die neu erzeugte Instanz.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Verbraucher angegeben.</exception>
        public static TableParser Create<T>(Action<T> consumer) where T : Table
        {
            // Validate
            ArgumentNullException.ThrowIfNull(consumer);

            // Forward
            return Create(t => consumer((T)t), typeof(T));
        }

        /// <summary>
        /// Überträgt Rohdaten zur Analyse in diese Instanz.
        /// </summary>
        /// <param name="payload">Die zu analysierenden Rohdaten.</param>
        /// <exception cref="ArgumentNullException">Es wurden keine Rohdaten angegeben.</exception>
        public void AddPayload(byte[] payload) => AddPayload(payload, 0, payload.Length);

        /// <summary>
        /// Überträgt Rohdaten zur Analyse in diese Instanz.
        /// </summary>
        /// <param name="payload">Die zu analysierenden Rohdaten.</param>
        /// <param name="offset">Die 0-basierte laufende Nummer des ersten zu analysierenden
        /// Bytes.</param>
        /// <param name="length">Die Anzahl der zu analyiserenden Bytes.</param>
        /// <exception cref="ArgumentNullException">Es wurden keine Rohdaten angegeben.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Der gewünschte Bereich innerhalb der Rohdaten
        /// existiert nicht.</exception>
        public void AddPayload(byte[] payload, int offset, int length) => m_Parser.OnData(payload, offset, length);
    }
}
