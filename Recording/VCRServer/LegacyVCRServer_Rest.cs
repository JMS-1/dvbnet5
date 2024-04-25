using JMS.DVB.NET.Recording.Persistence;

namespace JMS.DVB.NET.Recording
{
    partial class LegacyVCRServer
    {
        /// <summary>
        /// Liest einen Auszug aus einem Protokoll.
        /// </summary>
        /// <typeparam name="TEntry">Die Art der Zielinformation.</typeparam>
        /// <param name="profileName">Der Name des betroffenen Geräteprofils.</param>
        /// <param name="start">Das Startdatum.</param>
        /// <param name="end">Das Enddatum.</param>
        /// <param name="factory">Methode zum Erzeugen der externen Darstellung aus den ProtokollEinträgen.</param>
        /// <returns>Die angeforderten ProtokollEinträge.</returns>
        public TEntry[] QueryLog<TEntry>(string profileName, DateTime start, DateTime end, Func<VCRRecordingInfo, TEntry> factory)
        {
            // Locate profile and forward call
            if (string.IsNullOrEmpty(profileName))
                return [];
            var profile = Profiles.FindProfile(profileName);
            if (profile == null)
                return [];
            else
                return _jobs.FindLogEntries(start, end, profile).Select(factory).ToArray();
        }
    }
}

