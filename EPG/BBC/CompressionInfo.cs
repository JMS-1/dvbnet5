﻿using System.Xml;
using System.Text;
using System.Xml.Serialization;

namespace JMS.DVB.EPG.BBC
{
    /// <summary>
    /// Beschreibt die Komprimierungsinformationen einer Zeichenkette.
    /// </summary>
    [Serializable]
    public class CompressionInfo
    {
        /// <summary>
        /// Der komprimierte Text.
        /// </summary>
        public string Text { get; set; } = null!;

        /// <summary>
        /// Die zugehörige Bitsequenz.
        /// </summary>
        public string Sequence { get; set; } = null!;

        /// <summary>
        /// Gesetzt, wenn es sich um einen unvollständigen Eintrag handelt.
        /// </summary>
        [XmlAttribute("failed")]
        public bool Failed { get; set; }

        /// <summary>
        /// Der Zeitpunkt, an dem die Ausstrahlung stattfinden sollte.
        /// </summary>
        [XmlAttribute("start")]
        public DateTime ScheduleTime { get; set; }

        /// <summary>
        /// Die eindeutige Senderkennung.
        /// </summary>
        [XmlAttribute("station")]
        public string Station { get; set; } = null!;

        /// <summary>
        /// Erzeugt eine neue Komprimierungsinformation.
        /// </summary>
        public CompressionInfo()
        {
        }
    }

    /// <summary>
    /// Beschreibt eine Liste von Komprimierungsinformationen.
    /// </summary>
    [Serializable]
    public class CompressionInfos
    {
        /// <summary>
        /// Die einzelnen Komprimierungsinformationen.
        /// </summary>
        [XmlElement("Info")]
        public readonly List<CompressionInfo> Infos = [];

        /// <summary>
        /// Die zugehörige Komprimierungsart.
        /// </summary>
        [XmlAttribute("codePage")]
        public int CodePage = 0;

        /// <summary>
        /// Erzeugt eine neue Liste.
        /// </summary>
        public CompressionInfos()
        {
        }

        /// <summary>
        /// Speichert die Liste in der XML Repräsentation.
        /// </summary>
        /// <param name="stream">Das Ziel für die Speicherung.</param>
        public void Save(Stream stream)
        {
            // Create serializer
            XmlSerializer serializer = new(GetType(), "http://jochen-manns.de/DVB.NET/CompressionInfos");

            // Create settings
            XmlWriterSettings settings = new()
            {
                NewLineHandling = NewLineHandling.Entitize,
                Encoding = Encoding.UTF8,
                CheckCharacters = false,
                Indent = true
            };

            // Create writer and process
            using XmlWriter writer = XmlWriter.Create(stream, settings);

            serializer.Serialize(writer, this);
        }

        /// <summary>
        /// Speichert die Liste in eine XML Datei.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        /// <param name="mode">Die gewünschte Art zum Anlegen der Datei.</param>
        public void Save(string path, FileMode mode)
        {
            // Forward
            using FileStream stream = new(path, mode, FileAccess.Write, FileShare.None);

            Save(stream);
        }

        /// <summary>
        /// Speichert die Liste in eine XML Datei.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        public void Save(string path) => Save(path, FileMode.Create);

        /// <summary>
        /// Lädt eine Liste aus einer XML Datei.
        /// </summary>
        /// <param name="path">Der volle Pfad zu XML Datei. Existiere diese nicht, so wird eine
        /// leere Liste gemeldet.</param>
        /// <returns>Die zur XML Datei gehörende Liste.</returns>
        public static CompressionInfos Load(string path)
        {
            // Create new
            if (!File.Exists(path))
                return new CompressionInfos();

            // Forward
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            return Load(stream);
        }

        /// <summary>
        /// Rekonstruiert eine List aus der zugehörigen XML Repräsentation.
        /// </summary>
        /// <param name="stream">Die Quelle für die XML Repräsenatation.</param>
        /// <returns>Die neu erzeugte zugehörige List.</returns>
        public static CompressionInfos Load(Stream stream)
        {
            // Create deserializer
            XmlSerializer deserializer = new(typeof(CompressionInfos), "http://jochen-manns.de/DVB.NET/CompressionInfos");

            // Create settings
            XmlReaderSettings settings = new() { CheckCharacters = false };

            // Process
            using XmlReader reader = XmlReader.Create(stream, settings);

            return (CompressionInfos)deserializer.Deserialize(reader)!;
        }
    }
}
