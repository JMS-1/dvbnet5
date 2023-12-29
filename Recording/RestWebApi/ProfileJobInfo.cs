using System.Runtime.Serialization;
using JMS.DVB.NET.Recording.Persistence;


namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Beschreibt einen Auftrag eines Geräteprofils.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ProfileJobInfo
    {
        /// <summary>
        /// Der Name des Auftrags.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Die Kennung des Auftrags.
        /// </summary>
        [DataMember(Name = "id")]
        public string JobIdentifier { get; set; } = null!;

        /// <summary>
        /// Der Name des Geräteprofils.
        /// </summary>
        public string Profile { get; set; } = null!;

        /// <summary>
        /// Erzeugt eine Beschreibung.
        /// </summary>
        /// <param name="job">Ein Auftrag.</param>
        /// <param name="active">Gesetzt, wenn es sich um einen aktiven Auftrag handel.</param>
        /// <returns>Die gewünschte Beschreibung.</returns>
        public static ProfileJobInfo? Create(VCRJob job, bool active)
        {
            // Process
            if (active)
                return new ProfileJobInfo { Profile = job.Source.ProfileName, Name = job.Name, JobIdentifier = job.UniqueID!.Value.ToString("N").ToLower() };
            else
                return null;
        }
    }
}
