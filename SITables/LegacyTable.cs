extern alias oldVersion;

using Legacy = oldVersion::JMS.DVB;

namespace JMS.DVB.SI
{
    /// <summary>
    /// Hilfsklasse, die eine SI Tabelle auf Basis der DVB.NET 3.5 (oder früher)
    /// Implementierung vornimmt.
    /// </summary>
    /// <typeparam name="T">Die Art der SI Tabelle.</typeparam>
    /// <param name="table">Die DVB.NET 3.5 (oder früher) Tabelle.</param>
    public abstract class LegacyTable<T>(T table) : Table where T : Legacy.EPG.Table
    {
        /// <summary>
        /// Liest oder setzt die DVB.NET 3.5 (oder früher) Tabelle.
        /// </summary>
        public T Table { get; private set; } = table;

        /// <summary>
        /// Meldet die aktuelle laufende Nummer der Tabelle in einer Gruppe zusammengehörender Tabellen.
        /// </summary>
        public override int CurrentSection => Table.SectionNumber;

        /// <summary>
        /// Meldet die letzte Nummer einer Tabelle in einer Gruppe zusammengehörender Tabellen
        /// </summary>
        public override int LastSection => Table.LastSectionNumber;

        /// <summary>
        /// Meldet die Versionsnummer dieser Tabelle.
        /// </summary>
        public override int Version => Table.Version;
    }

    /// <summary>
    /// Hilfsklasse, die eine SI Tabelle auf Basis der DVB.NET 3.5 (oder früher)
    /// Implementierung vornimmt.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="table">Die DVB.NET 3.5 (oder früher) Tabelle.</param>
    public abstract class WellKnownLegacyTable<T>(T table) : WellKnownTable where T : Legacy.EPG.Table
    {
        /// <summary>
        /// Liest oder setzt die DVB.NET 3.5 (oder früher) Tabelle.
        /// </summary>
        public T Table { get; private set; } = table;

        /// <summary>
        /// Meldet die aktuelle laufende Nummer der Tabelle in einer Gruppe zusammengehörender Tabellen.
        /// </summary>
        public override int CurrentSection => Table.SectionNumber;

        /// <summary>
        /// Meldet die letzte Nummer einer Tabelle in einer Gruppe zusammengehörender Tabellen
        /// </summary>
        public override int LastSection => Table.LastSectionNumber;

        /// <summary>
        /// Meldet die Versionsnummer dieser Tabelle.
        /// </summary>
        public override int Version => Table.Version;
    }
}
