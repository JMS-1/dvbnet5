using JMS.DVB.DeviceAccess.Interfaces;

namespace JMS.DVB.Provider.Legacy
{
    /// <summary>
    /// Diese Klasse vermittelt den Zugriff auf eine vorhandene DVB.NET Abstraktion vor
    /// Version 3.5.1.
    /// </summary>
    /// <param name="profile">Das zugeordnete Geräteprofil.</param>
    public class DVBSLegacy(SatelliteProfile profile) : LegacyHardware<SatelliteProfile, SatelliteLocation, SatelliteGroup>(profile)
    {
        /// <summary>
        /// Meldet die Art des DVB Empfangs.
        /// </summary>
        protected override DVBSystemType SystemType => DVBSystemType.Satellite;
    }
}
