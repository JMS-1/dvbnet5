using JMS.DVB.DeviceAccess.Interfaces;
using JMS.DVB.TS;
using JMS.TechnoTrend;
using System.Collections;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace JMS.DVB.Provider.Ubuntu
{
    /// <summary>
    /// Device provider using the DVB.NET Linux proxy TCP protocol.
    /// </summary>
    public class DeviceProvider : ILegacyDevice
    {
        /// <summary>
        /// Name of IP of the server to 
        /// </summary>
        private readonly string m_server;

        /// <summary>
        /// TCP port to connect to.
        /// </summary>
        private readonly int m_port;

        /// <summary>
        /// The active connection to the proxy.
        /// </summary>
        private TcpClient? m_connection;

        /// <summary>
        /// The transport stream analyser for the connection.
        /// </summary>
        private TSParser m_parser = new(true);

        /// <summary>
        /// Initialize a new provider instance.
        /// </summary>
        /// <param name="args">Configuration of the connection.</param>
        public DeviceProvider(Hashtable args)
        {
            m_server = (string)args["Adapter.Server"]!;
            m_port = ArgumentToNumber(args["Adapter.Port"]!, 29713);
        }

        /// <summary>
        /// Take a configuration parameter and try to make it a number.
        /// </summary>
        /// <param name="arg">Value if applicable - may be null.</param>
        /// <param name="fallback">Default number to use.</param>
        /// <returns>The number from the configuration or the default.</returns>
        private static int ArgumentToNumber(object arg, int fallback) =>
           int.TryParse((string)arg, out int number) ? number : fallback;

        /// <summary>
        /// Send a request with a opaque data block.
        /// </summary>
        /// <typeparam name="TData">Type of the data.</typeparam>
        /// <param name="type">Command to send.</param>
        /// <param name="data">Data for the command.</param>
        private void SendRequest<TData>(FrontendRequestType type, TData data)
        {
            /* Must be connected. */
            Open();

            /* Allocate a buffer for command and data. */
            var size = 4 + Marshal.SizeOf(data);
            var buf = new byte[size];
            var bufptr = GCHandle.Alloc(buf, GCHandleType.Pinned);

            try
            {
                /* Pack it into memory. */
                Marshal.WriteInt32(bufptr.AddrOfPinnedObject() + 0, (Int32)type);
                Marshal.StructureToPtr(data!, bufptr.AddrOfPinnedObject() + 4, true);
            }
            finally
            {
                bufptr.Free();
            }

            /* Send packed byte data. */
            SafeWrite(buf);
        }

        /// <summary>
        /// Send a byte block of data to the device.
        /// </summary>
        /// <param name="buf">The data to send.</param>
        private void SafeWrite(byte[] buf)
        {
            try
            {
                m_connection!.GetStream().Write(buf, 0, buf.Length);
            }
            catch (Exception e)
            {
                /* Currently errors are ignored. */
                Debug.WriteLine("Failed to send request: {0}", e);
            }
        }

        /// <summary>
        /// Send a number to the device.
        /// </summary>
        /// <param name="type">Command to send to the device.</param>
        /// <param name="data">Number to send - typically a stream identifier.</param>
        private void SendRequest(FrontendRequestType type, ushort data)
        {
            /* Must be connected. */
            Open();

            /* Allocate a buffer with command and number. */
            var buf = new byte[6];
            var bufptr = GCHandle.Alloc(buf, GCHandleType.Pinned);

            try
            {
                /* Pack the data. */
                Marshal.WriteInt32(bufptr.AddrOfPinnedObject() + 0, (Int32)type);
                Marshal.WriteInt16(bufptr.AddrOfPinnedObject() + 4, (Int16)data);
            }
            finally
            {
                bufptr.Free();
            }

            /* Send the packed data to the device. */
            SafeWrite(buf);
        }

        /// <summary>
        /// Send a command to the device.
        /// </summary>
        /// <param name="type">Command to send.</param>
        private void SendRequest(FrontendRequestType type)
        {
            /* Must be connected. */
            Open();

            /* Pack the command into memory. */
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

            /* Send the packed data to the device. */
            SafeWrite(buf);
        }

        /// <summary>
        /// Process data from the device.
        /// </summary>
        /// <param name="state">Will be ignored.</param>
        private void StartReader(object? state)
        {
            var buffer = new byte[180000];

            try
            {
                /* Attach to the incoming data stream. */
                var stream = m_connection!.GetStream();

                for (; ; )
                {
                    /* Read the next chunk. */
                    var read = stream.Read(buffer, 0, buffer.Length);

                    if (read <= 0)
                        return;

                    /* Let the transport stream parser inspect the raw data, */
                    m_parser.AddPayload(buffer, 0, read);
                }
            }
            catch (Exception)
            {
                /* In case of error abort reading from the connection. */
                return;
            }
        }

        /// <summary>
        /// Open a connection to the device.
        /// </summary>
        private void Open()
        {
            /* Must be done only once. */
            if (m_connection != null)
                return;

            /* Create the raw TCP connection. */
            m_connection = new TcpClient { ReceiveBufferSize = 10 * 1024 * 1024 };

            try
            {
                /* Connect to the device. */
                m_connection.Connect(m_server, m_port);

                /* Start the stream analyser process. */
                ThreadPool.QueueUserWorkItem(StartReader);

                /* Reserve the next free hardware. */
                SendRequest(FrontendRequestType.connect_adapter);
            }
            catch (Exception)
            {
                /* Disconnect in case of any error. */
                Close();

                throw;
            }
        }

        /// <summary>
        /// Disconnection from the device.
        /// </summary>
        private void Close()
        {
            /* Create a new parser. */
            using (m_parser)
                m_parser = new TSParser(true);

            /* Already closed. */
            if (m_connection == null)
                return;

            /* Get rid of the physical TCP connection. */
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

        /// <summary>
        /// Stop all filters.
        /// </summary>
        public void StopFilters()
        {
            /* Reset the parser. */
            using (m_parser)
                m_parser = new TSParser(true);

            /* Let the device stop all filters. */
            if (m_connection != null)
                SendRequest(FrontendRequestType.del_all_filters);
        }

        /// <summary>
        /// Get the DiSEqC mode in protocol representation.
        /// </summary>
        /// <param name="location">DiSEqC mode in DVB.NET representation.</param>
        /// <returns>DiSEqC mode in protocol representation.</returns>
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

        /// <summary>
        /// Get the error correction in protocol representation.
        /// </summary>
        /// <param name="location">Error correction in DVB.NET representation.</param>
        /// <returns>Error correction in protocol representation.</returns>
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

        /// <summary>
        /// Get the code rate in protocol representation.
        /// </summary>
        /// <param name="location">Code rate in DVB.NET representation.</param>
        /// <returns>Code rate in protocol representation.</returns>
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

        /// <summary>
        /// Get the roll off in protocol representation.
        /// </summary>
        /// <param name="location">Roll off in DVB.NET representation.</param>
        /// <returns>Roll off in protocol representation.</returns>
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

        /// <summary>
        /// Tune the device.
        /// </summary>
        /// <param name="group">Transponder to use.</param>
        /// <param name="location">Dish to use.</param>
        public void Tune(SourceGroup group, GroupLocation location)
        {
            /* Stop all current filters. */
            StopFilters();

            /* Currently proxy only supports DVB-S2 devices. */
            if (group is not SatelliteGroup satGroup)
                return;

            if (location is not SatelliteLocation satLocation)
                return;

            /* Send a tune request to the proxy. */
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

        /// <summary>
        /// Enable video and audio reception.
        /// </summary>
        /// <param name="videoPID">Video stream identifier, will be ignored.</param>
        /// <param name="audioPID">Audi stream identifier, will be ignored.</param>
        public void SetVideoAudio(ushort videoPID, ushort audioPID) => Open();

        /// <summary>
        /// Create a section filter.
        /// </summary>
        /// <param name="pid">Stream identifier of the section.</param>
        /// <param name="callback">Will be called when section data is received.</param>
        /// <param name="filterData">Section filter.</param>
        /// <param name="filterMask">Section filter mask.</param>
        public void StartSectionFilter(ushort pid, Action<byte[]> callback, byte[] filterData, byte[] filterMask)
        {
            /* Remember filter. */
            m_parser.SetFilter(pid, true, callback);

            /* Activate filter on device. */
            SendRequest(FrontendRequestType.add_filter, pid);
        }

        /// <summary>
        /// Register a stream filter.
        /// </summary>
        /// <param name="pid">The stream identifier.</param>
        /// <param name="video">Set for video streams - will be ignored.</param>
        /// <param name="smallBuffer">Set to use small buffers - will be ignored.</param>
        /// <param name="callback">Call whenever data is available.</param>
        public void RegisterPipingFilter(ushort pid, bool video, bool smallBuffer, Action<byte[]> callback)
        {
            /* Remember filter. */
            m_parser.SetFilter(pid, false, callback);

            /* Activate filter on device. */
            SendRequest(FrontendRequestType.add_filter, pid);
        }

        /// <summary>
        /// Start filter - will do nothing, filters are always active.
        /// </summary>
        /// <param name="pid">Stream identifier.</param>
        public void StartFilter(ushort pid)
        {
        }

        /// <summary>
        /// Terminate a filter.
        /// </summary>
        /// <param name="pid">Stream identifier.</param>
        public void StopFilter(ushort pid)
        {
            /* Unregister from parser. */
            m_parser.RemoveFilter(pid);

            /* Tell the device that the stream is no longer needed. */
            if (m_connection != null)
                SendRequest(FrontendRequestType.del_filter, pid);
        }

        /// <summary>
        /// Decrypt a station - not supported.
        /// </summary>
        /// <param name="station">Station identifier.</param>
        public void Decrypt(ushort? station)
        {
        }

        /// <summary>
        /// Create a string represenation for this instance.
        /// </summary>
        /// <returns>The server and port used to connect to the device.</returns>
        public override string ToString() => string.Format("Ubuntu DVB Proxy to {0}:{1}", m_server, m_port);

        /// <summary>
        /// Wakeup the device after resume - does nothing.
        /// </summary>
        public void WakeUp()
        {
        }

        /// <summary>
        /// Terminate the use of this instance.
        /// </summary>
        public virtual void Dispose()
        {
            /* Stop all filters. */
            StopFilters();

            /* Terminate the TCP connection. */
            Close();
        }

        /// <summary>
        /// Report the signal status - not implemented.
        /// </summary>
        public SignalStatus SignalStatus { get; private set; } = new SignalStatus(true, 0, 0);
    }
}
