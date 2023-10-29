using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.TS;
using JMS.TechnoTrend;
using System.Collections;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace JMS.DVB.Provider.Ubuntu
{
    public class DeviceProvider : ILegacyDevice
    {
        private readonly string m_server;

        private readonly int m_port;

        private TcpClient? m_connection;

        private TSParser m_parser = new(true);

        public DeviceProvider(Hashtable args)
        {
            m_server = (string)args["Adapter.Server"]!;
            m_port = ArgumentToNumber(args["Adapter.Port"]!, 29713);
        }

        private static int ArgumentToNumber(object arg, int fallback)
        {
            if (int.TryParse((string)arg, out int number))
                return number;

            return fallback;
        }

        private void SendRequest<TData>(FrontendRequestType type, TData data)
        {
            Open();

            var size = 4 + Marshal.SizeOf(data);
            var buf = new byte[size];
            var bufptr = GCHandle.Alloc(buf, GCHandleType.Pinned);

            try
            {
                Marshal.WriteInt32(bufptr.AddrOfPinnedObject() + 0, (Int32)type);
                Marshal.StructureToPtr(data!, bufptr.AddrOfPinnedObject() + 4, true);
            }
            finally
            {
                bufptr.Free();
            }

            SafeWrite(buf);
        }

        private void SafeWrite(byte[] buf)
        {
            try
            {
                m_connection!.GetStream().Write(buf, 0, buf.Length);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to send request: {0}", e);
            }
        }

        private void SendRequest(FrontendRequestType type, UInt16 data)
        {
            Open();

            var buf = new byte[6];
            var bufptr = GCHandle.Alloc(buf, GCHandleType.Pinned);

            try
            {
                Marshal.WriteInt32(bufptr.AddrOfPinnedObject() + 0, (Int32)type);
                Marshal.WriteInt16(bufptr.AddrOfPinnedObject() + 4, (Int16)data);
            }
            finally
            {
                bufptr.Free();
            }

            SafeWrite(buf);
        }

        private void SendRequest(FrontendRequestType type)
        {
            Open();

            var buf = new byte[4];
            var bufptr = GCHandle.Alloc(buf, GCHandleType.Pinned);

            try
            {
                Marshal.WriteInt32(bufptr.AddrOfPinnedObject() + 0, (Int32)type);
            }
            finally
            {
                bufptr.Free();
            }

            SafeWrite(buf);
        }



        private void StartReader(object? state)
        {
            var buffer = new byte[180000];

            try
            {
                var stream = m_connection!.GetStream();

                for (; ; )
                {
                    var read = stream.Read(buffer, 0, buffer.Length);

                    if (read <= 0)
                        return;

                    m_parser.AddPayload(buffer, 0, read);
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void Open()
        {
            if (m_connection != null)
                return;

            m_connection = new TcpClient { ReceiveBufferSize = 10 * 1024 * 1024 };

            try
            {
                m_connection.Connect(m_server, m_port);

                ThreadPool.QueueUserWorkItem(StartReader);

                SendRequest(FrontendRequestType.connect_adapter);
            }
            catch (Exception)
            {
                Close();

                throw;
            }
        }

        private void Close()
        {
            using (m_parser)
                m_parser = new TSParser(true);


            if (m_connection == null)
                return;

            using (m_connection)
                try
                {
                    m_connection.Close();
                }
                finally
                {
                    m_connection = null;
                }
        }

        public void StopFilters()
        {
            using (m_parser)
                m_parser = new TSParser(true);

            if (m_connection != null)
                SendRequest(FrontendRequestType.del_all_filters);
        }

        private static DiSEqCModes ConvertDiSEqC(DiSEqCLocations location)
        {
            return location switch
            {
                DiSEqCLocations.BurstOff => DiSEqCModes.burst_off,
                DiSEqCLocations.BurstOn => DiSEqCModes.burst_on,
                DiSEqCLocations.DiSEqC1 => DiSEqCModes.diseqc1,
                DiSEqCLocations.DiSEqC2 => DiSEqCModes.diseqc2,
                DiSEqCLocations.DiSEqC3 => DiSEqCModes.diseqc3,
                DiSEqCLocations.DiSEqC4 => DiSEqCModes.diseqc4,
                _ => DiSEqCModes.none,
            };
        }

        private static FeModulation ConvertModulation(SatelliteModulations modulation)
        {
            return modulation switch
            {
                SatelliteModulations.Auto => FeModulation.QAM_AUTO,
                SatelliteModulations.PSK8 => FeModulation.PSK_8,
                SatelliteModulations.QAM16 => FeModulation.QAM_16,
                _ => FeModulation.QPSK,
            };
        }

        private static FeCodeRate ConvertCodeRate(InnerForwardErrorCorrectionModes modulation)
        {
            return modulation switch
            {
                InnerForwardErrorCorrectionModes.Conv1_2 => FeCodeRate.FEC_1_2,
                InnerForwardErrorCorrectionModes.Conv2_3 => FeCodeRate.FEC_2_3,
                InnerForwardErrorCorrectionModes.Conv3_4 => FeCodeRate.FEC_3_4,
                InnerForwardErrorCorrectionModes.Conv3_5 => FeCodeRate.FEC_3_5,
                InnerForwardErrorCorrectionModes.Conv4_5 => FeCodeRate.FEC_4_5,
                InnerForwardErrorCorrectionModes.Conv5_6 => FeCodeRate.FEC_5_6,
                InnerForwardErrorCorrectionModes.Conv7_8 => FeCodeRate.FEC_7_8,
                InnerForwardErrorCorrectionModes.Conv8_9 => FeCodeRate.FEC_8_9,
                InnerForwardErrorCorrectionModes.Conv9_10 => FeCodeRate.FEC_9_10,
                InnerForwardErrorCorrectionModes.NoConv => FeCodeRate.FEC_NONE,
                _ => FeCodeRate.FEC_AUTO,
            };
        }

        private static FeRolloff ConvertRolloff(S2RollOffs modulation)
        {
            return modulation switch
            {
                S2RollOffs.Alpha20 => FeRolloff.ROLLOFF_20,
                S2RollOffs.Alpha25 => FeRolloff.ROLLOFF_25,
                S2RollOffs.Alpha35 => FeRolloff.ROLLOFF_35,
                _ => FeRolloff.ROLLOFF_AUTO,
            };
        }

        public void Tune(SourceGroup group, GroupLocation location)
        {
            StopFilters();

            if (group is not SatelliteGroup satGroup)
                return;

            if (location is not SatelliteLocation satLocation)
                return;

            var tune = new SatelliteTune
            {
                frequency = satGroup.Frequency,
                horizontal = satGroup.Polarization == Polarizations.Horizontal,
                innerFEC = ConvertCodeRate(satGroup.InnerFEC),
                lnb1 = satLocation.Frequency1,
                lnb2 = satLocation.Frequency2,
                lnbMode = ConvertDiSEqC(satLocation.LNB),
                lnbPower = satLocation.UsePower,
                lnbSwitch = satLocation.SwitchFrequency,
                modulation = ConvertModulation(satGroup.Modulation),
                rolloff = ConvertRolloff(satGroup.RollOff),
                s2 = satGroup.UsesS2Modulation,
                symbolrate = satGroup.SymbolRate,
            };

            SendRequest(FrontendRequestType.tune, tune);
        }

        public void SetVideoAudio(ushort videoPID, ushort audioPID) => Open();

        public void StartSectionFilter(ushort pid, Action<byte[]> callback, byte[] filterData, byte[] filterMask)
        {
            m_parser.SetFilter(pid, true, callback);

            SendRequest(FrontendRequestType.add_filter, pid);
        }

        public void RegisterPipingFilter(ushort pid, bool video, bool smallBuffer, Action<byte[]> callback)
        {
            m_parser.SetFilter(pid, false, callback);

            SendRequest(FrontendRequestType.add_filter, pid);
        }

        public void StartFilter(ushort pid)
        {
        }

        public void StopFilter(ushort pid)
        {
            m_parser.RemoveFilter(pid);

            if (m_connection != null)
                SendRequest(FrontendRequestType.del_filter, pid);
        }

        public void Decrypt(ushort? station)
        {
        }

        public override string ToString() => string.Format("Ubuntu DVB Proxy to {0}:{1}", m_server, m_port);

        public void WakeUp()
        {
        }

        public virtual void Dispose()
        {
            StopFilters();

            Close();
        }

        public SignalStatus SignalStatus { get; private set; } = new SignalStatus(true, 0, 0);
    }
}
