using System.Collections;
using System.Xml;

namespace JMS.DVB.Provider.Legacy
{
    /// <summary>
    /// Beschreibt die Parameter einer DVB.NET Hardwareabstraktion.
    /// </summary>
    public class LegacyDeviceInformation
    {
        /// <summary>
        /// Enth�lt die Beschreibung zu allen bekannten Geräten der alten DVB.NET Version.
        /// </summary>
        public static readonly LegacyDeviceInformation[] Devices = Load();

        /// <summary>
        /// Das Wurzelelement der Konfiguration.
        /// </summary>
        private XmlElement Root { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        /// <param name="provider">Das Wurzelelement der Konfiguration.</param>
        public LegacyDeviceInformation(XmlElement provider)
        {
            // Load
            Root = provider;

            // Verifiy
            if (!Root.Name.Equals("DVBNETProvider"))
                throw new ArgumentException("bad provider definition", "file");
        }

        private XmlElement? FindElement(string name) => (XmlElement?)Root.SelectSingleNode(name);

        public XmlNodeList Parameters => FindElement("Parameters")!.ChildNodes;

        private string UniqueIdentifier => (string)Root.GetAttribute("id");

        public override string ToString() => UniqueIdentifier;

        /// <summary>
        /// Meldet den Namen der .NET Klasse zum Zugriff auf die DVB Hardware.
        /// </summary>
        public string DriverType => FindElement("Driver")!.InnerText;

        public string[] Names
        {
            get
            {
                // Helper
                var names = new ArrayList();

                // All my names
                foreach (XmlNode name in Root.SelectNodes("CardNames/CardName")!)
                    names.Add(name.InnerText);

                // Report
                return (string[])names.ToArray(typeof(string));
            }
        }

        private static LegacyDeviceInformation[] Load()
        {
            // Remember
            var settings = new Hashtable();

            // Get the root
            string root = RunTimeLoader.GetDirectory("Providers").FullName;

            // Attach to the provider configuration file
            var path = new FileInfo(Path.Combine(root, "DVBNETProviders.xml"));
            var file = new XmlDocument();

            // Process
            if (path.Exists)
            {
                // Load the DOM from file
                file.Load(path.FullName);
            }
            else
            {
                // Get the scope
                var me = typeof(LegacyDeviceInformation);

                // Load the DOM from resource
                using var providers = me.Assembly.GetManifestResourceStream(me.Assembly.ManifestModule.Name[..^4] + ".DVBNETProviders.xml");

                file.Load(providers!);
            }

            // Verify
            if (!file.DocumentElement!.Name.Equals("DVBNETProviders"))
                throw new ArgumentException("bad provider definition", "file");
            if (!Equals(file.DocumentElement.GetAttribute("SchemaVersion"), "3.9"))
                throw new ArgumentException("invalid schema version", "file");

            // All providers
            return
                file
                    .DocumentElement!
                    .SelectNodes("DVBNETProvider")!
                    .Cast<XmlElement>()
                    .Select(node => new LegacyDeviceInformation(node))
                    .ToDictionary(info => info.UniqueIdentifier)
                    .Values
                    .ToArray();
        }
    }
}
