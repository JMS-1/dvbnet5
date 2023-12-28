

namespace JMS.DVB.DeviceAccess.Interfaces
{
    /// <summary>
    /// Beschreibt ein Empfangssignal.
    /// </summary>
    /// <param name="locked">Gesetzt, wenn das Signal best�tigt ist.</param>
    /// <param name="strength">Die St�rke des Signals.</param>
    /// <param name="quality">Die Qualit�t des Signals.</param>
    public class SignalStatus(bool locked, double strength, double quality)
    {
        /// <summary>
        /// Gesetzt, wenn ein Signal best�tigt wurde.
        /// </summary>
        public readonly bool Locked = locked;

        /// <summary>
        /// Die St�rke des Signals.
        /// </summary>
        public readonly double Strength = strength;

        /// <summary>
        /// Die Qualit�t des Signals.
        /// </summary>
        public readonly double Quality = quality;
    }
}
