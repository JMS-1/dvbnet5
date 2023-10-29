using System.Runtime.InteropServices;

namespace JMS.DVB.Provider.Ubuntu
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SatelliteTune
    {
        public DiSEqCModes lnbMode;
        public uint lnb1;
        public uint lnb2;
        public uint lnbSwitch;
        public bool lnbPower;
        public FeModulation modulation;
        public uint frequency;
        public uint symbolrate;
        public bool horizontal;
        public FeCodeRate innerFEC;
        public bool s2;
        public FeRolloff rolloff;
    }
}