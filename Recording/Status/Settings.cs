using System.Xml.Serialization;

namespace JMS.DVB.NET.Recording.Status
{
    /// <summary>
    /// Used to report some settings to the client.
    /// </summary>
    [Serializable]
    [XmlType("VCRSettings")]
    public class Settings
    {
        /// <summary>
        /// The profiles that can be used.
        /// </summary>
        public readonly List<string> Profiles = [];
    }
}
