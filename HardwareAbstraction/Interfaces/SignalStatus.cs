

namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt ein Empfangssignal.
    /// </summary>
    public class SignalStatus
    {
        /// <summary>
        /// Gesetzt, wenn ein Signal best�tigt wurde.
        /// </summary>
        public readonly bool Locked;

        /// <summary>
        /// Die St�rke des Signals.
        /// </summary>
        public readonly double Strength;

        /// <summary>
        /// Die Qualit�t des Signals.
        /// </summary>
        public readonly double Quality;

        /// <summary>
        /// Erstellt eine neue Beschreibung.
        /// </summary>
        /// <param name="locked">Gesetzt, wenn das Signal best�tigt ist.</param>
        /// <param name="strength">Die St�rke des Signals.</param>
        /// <param name="quality">Die Qualit�t des Signals.</param>
        public SignalStatus(bool locked, double strength, double quality)
        {
            // Remember
            Locked = locked;
            Strength = strength;
            Quality = quality;
        }
    }
}
