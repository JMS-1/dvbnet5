﻿using System.Xml;
using System.Text;
using System.Xml.Serialization;

namespace JMS.DVB;

/// <summary>
/// Wird als Datei abgespeichert und enthält Ursprünge für den Sendersuchlauf. Üblicherweise
/// wird für jeden Ursprung nur eine einzige Gruppe vermerkt, da sich daraus die anderen Gruppen
/// des selben Ursprungs ermitteln lassen (<i>Network Information Table</i>).
/// </summary>
[Serializable]
public class ScanLocations : ICloneable
{
    /// <summary>
    /// Die Standardkonfiguration für den Sendersuchlauf.
    /// </summary>
    private static ScanLocations? m_Default;

    /// <summary>
    /// Schützt den Zugriff auf die Standardkonfiguration.
    /// </summary>
    private static readonly object m_DefaultLock = new();

    /// <summary>
    /// Der XML Namensraum für diese .NET Klasse.
    /// </summary>
    public const string Namespace = "http://psimarron.net/DVBNET/ScanLocations";

    /// <summary>
    /// Die aktuelle DVB.NET Version für diese Art von Listen.
    /// </summary>
    public const string CurrentVersion = "4.0";

    /// <summary>
    /// Die DVB.NET Version, mit der diese Instanz erstellt wurde.
    /// </summary>
    [XmlAttribute("version")]
    public string Version { get; set; }

    /// <summary>
    /// Die Ursprünge aller Art.
    /// </summary>
    [XmlArray("List")]
    [XmlArrayItem(typeof(SatelliteScanLocation))]
    [XmlArrayItem(typeof(CableScanLocation))]
    [XmlArrayItem(typeof(TerrestrialScanLocation))]
    public readonly List<ScanLocation> Locations = [];

    /// <summary>
    /// Erzeugt eine neue Liste.
    /// </summary>
    public ScanLocations()
    {
        // Initialize
        Version = CurrentVersion;
    }

    /// <summary>
    /// Legt diese Informationen in einem Datenstrom ab.
    /// </summary>
    /// <param name="stream">Der gewünschte Datenstrom.</param>
    public void Save(Stream stream)
    {
        // Create serializer
        var serializer = new XmlSerializer(GetType(), Namespace);

        // Configure format
        var settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = true
        };

        // Store
        using var writer = XmlWriter.Create(stream, settings);

        serializer.Serialize(writer, this);
    }

    /// <summary>
    /// Legt dieser Informationen in einer Datei ab.
    /// </summary>
    /// <param name="path">Der volle Pfad zu Datei.</param>
    /// <param name="mode">Die Art, wie die Datei zu erzeugen ist.</param>
    public void Save(string path, FileMode mode)
    {
        // Create stream and forward
        using var stream = new FileStream(path, mode, FileAccess.Write, FileShare.None);

        Save(stream);
    }

    /// <summary>
    /// Legt dieser Informationen in einer Datei ab.
    /// </summary>
    /// <param name="path">Der volle Pfad zu Datei.</param>
    public void Save(string path) => Save(path, FileMode.Create);

    /// <summary>
    /// Legt dieser Informationen in einer Datei ab.
    /// </summary>
    /// <param name="path">Die Zieldatei.</param>
    /// <param name="mode">Die Art, wie die Datei zu erzeugen ist.</param>
    public void Save(FileInfo path, FileMode mode) => Save(path.FullName, mode);

    /// <summary>
    /// Legt dieser Informationen in einer Datei ab.
    /// </summary>
    /// <param name="path">Die Zieldatei.</param>
    public void Save(FileInfo path) => Save(path.FullName, FileMode.Create);

    /// <summary>
    /// Rekonstruiert eine Instanz aus einem Datenstrom.
    /// </summary>
    /// <param name="stream">Der Datenstrom mit der XML Repräsentation der Instanz.</param>
    /// <returns>Die gewünschte Instanz.</returns>
    public static ScanLocations Load(Stream stream)
    {
        // Create serializer
        var serializer = new XmlSerializer(typeof(ScanLocations), Namespace);

        // Process
        return (ScanLocations)serializer.Deserialize(stream)!;
    }

    /// <summary>
    /// Rekonstruiert eine Instanz aus einer Datei.
    /// </summary>
    /// <param name="path">Der volle Pfad zur Datei mit der XML Repräsentation der Instanz.</param>
    /// <returns>Die gewünschte Instanz.</returns>
    public static ScanLocations Load(string path)
    {
        // Open and forward
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        return Load(stream);
    }

    /// <summary>
    /// Rekonstruiert eine Instanz aus einer Datei.
    /// </summary>
    /// <param name="file">Die Datei mit der XML Repräsentation der Instanz.</param>
    /// <returns>Die gewünschte Instanz.</returns>
    public static ScanLocations Load(FileInfo file) => Load(file.FullName);

    /// <summary>
    /// Erzeugt eine exakte Kopie.
    /// </summary>
    /// <returns>Die exakte Kopie.</returns>
    public ScanLocations Clone()
    {
        // Use helper
        using var stream = new MemoryStream();

        // Put in
        Save(stream);

        // Reset stream to the very beginning
        stream.Seek(0, SeekOrigin.Begin);

        // Recreate
        return Load(stream);
    }

    /// <summary>
    /// Lädt die von DVB.NET selbst angebotenen Quellgruppen für den Sendersuchlauf. Zusätzlich
    /// werden benutzerspezifische Definitionen überlagert.
    /// </summary>
    /// <returns>Die Verwaltung aller Quellgruppen.</returns>
    private static ScanLocations Load()
    {
        // Load build-in resources.
        var me = typeof(ScanLocations);

        using var stream = me.Assembly.GetManifestResourceStream($"{me.Assembly.ManifestModule.Name[..^4]}.ScanLocations.BuiltIn.dss")!;

        var product = Load(stream);

        // Attach to the path of the scan locations
        var locations = RunTimeLoader.GetDirectory("Scan Locations");

        if (locations.Exists)
        {
            // Get all files
            var files = locations.GetFiles("*.dss").ToList();

            // See if default exists and move to head
            var builtIn = files.Find(f => 0 == string.Compare("BuiltIn", Path.GetFileNameWithoutExtension(f.FullName), true));

            if (builtIn != null)
            {
                // Remove
                files.Remove(builtIn);

                // Insert
                files.Insert(0, builtIn);

                // Forget presets
                product = new ScanLocations();
            }

            // Process all
            foreach (var extension in files)
                try
                {
                    // Load file
                    var sources = Load(extension);

                    // Map with all unique identifiers found
                    Dictionary<string, bool> map = [];

                    // Fill in
                    foreach (var location in sources.Locations)
                        if (!string.IsNullOrEmpty(location.UniqueName))
                            map[location.UniqueName] = true;

                    // Remove
                    product.Locations.RemoveAll(l => !string.IsNullOrEmpty(l.UniqueName) && map.ContainsKey(l.UniqueName));

                    // Add all groups
                    product.Locations.AddRange(sources.Locations);
                }
                catch
                {
                    // Ignore any error
                }
        }

        // Report
        return product;
    }

    /// <summary>
    /// Ermittelt einen Ursprung einer bestimmten Art mit einem
    /// bestimmten Namen.
    /// </summary>
    /// <typeparam name="T">Die gewünschte Art des Ursprungs.</typeparam>
    /// <param name="uniqueName">Der eindeutige Name des Ursprungs.</param>
    /// <returns>Der Ursprung oder <i>null</i>, wenn kein Ursprung mit diesem Namen existiert.</returns>
    public T? Find<T>(string uniqueName) where T : ScanLocation => Find<T>(l => Equals(l.UniqueName, uniqueName));

    /// <summary>
    /// Ermittelt einen Ursprung einer bestimmten Art.
    /// </summary>
    /// <typeparam name="T">Die gwünschte Art des Ursprungs.</typeparam>
    /// <param name="predicate">Eine Methode zur Ermittelung des gewünschten Ursprungs.</param>
    /// <returns>Der gewünschte Ursprung oder <i>null</i>, wenn kein Ursprung der gewünschten Art existiert.</returns>
    public T? Find<T>(Predicate<T> predicate) where T : ScanLocation => (T?)Locations.Find(l => (l.GetType() == typeof(T)) && predicate((T)l));

    /// <summary>
    /// Meldet die Standardkonfiguration für den Sendersuchlauf.
    /// </summary>
    public static ScanLocations Default
    {
        get
        {
            // Create once
            lock (m_DefaultLock)
                if (m_Default == null)
                    m_Default = Load();

            // Report
            return m_Default;
        }
    }

    #region ICloneable Members

    /// <summary>
    /// Erzeugt eine exakte Kopie.
    /// </summary>
    /// <returns>Die exakte Kopie dieser Instanz.</returns>
    object ICloneable.Clone() => Clone();

    #endregion
}
