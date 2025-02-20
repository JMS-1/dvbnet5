﻿using System.Net;
using System.Net.Sockets;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.TS;
using Microsoft.AspNetCore.Mvc;

namespace JMS.DVB.NET.Recording.RestWebApi
{
    /// <summary>
    /// Versendet Dateien.
    /// </summary>
    [ApiController]
    [Route("api/file")]
    public class FileController(IVCRConfiguration configuration) : ControllerBase
    {
        /// <summary>
        /// Fordert ein Stück einer Datei an.
        /// </summary>
        /// <param name="path">Der volle Pfad zur Datei.</param>
        /// <param name="offset">Das erste zu sendende Byte.</param>
        /// <param name="length">Die Anzahl der zu sendenden Bytes.</param>
        /// <param name="target">Der Empfänger der Daten.</param>
        /// <param name="port">Der Empfängerport der Daten.</param>
        /// <returns>Die Anzahl der übertragenden Bytes.</returns>
        [HttpGet]
        public long SendPartOfFile(string path, long offset, int length, string target, ushort port)
        {
            // Validate path
            ArgumentException.ThrowIfNullOrEmpty(path);

            if (!path.ToLower().EndsWith(".ts"))
                throw new ArgumentException(path, nameof(path));

            // Check against VCR.NET recording directories
            if (!configuration.IsValidTarget(path))
                throw new ArgumentException(path, nameof(path));

            // Validate the slice
            ArgumentOutOfRangeException.ThrowIfNegative(offset);
            ArgumentOutOfRangeException.ThrowIfLessThan(length, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(length, 100000000);

            // Validate the IP
            ArgumentException.ThrowIfNullOrEmpty(target);

            // We do not support multi-cast
            if (target.StartsWith('*')) throw new ArgumentException(target, nameof(target));

            // Find the first IP4 address
            var host = Dns.GetHostEntry(target);
            var hostIP = host.AddressList.FirstOrDefault(testIP => testIP.AddressFamily == AddressFamily.InterNetwork);
            var endPoint = new IPEndPoint(hostIP!, port);

            // Create socket
            using (var socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp) { Blocking = true })
            {
                // Last size of stream
                long streamSize;

                // Collection ends in 0,5 Seconds
                var endCollect = DateTime.UtcNow.AddMilliseconds(500);

                // As long as necessary
                for (; ; )
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 10000000))
                    {
                        // Get the size
                        streamSize = stream.Length;

                        // And the maximum number of bytes left
                        var rest = streamSize - offset;

                        ArgumentOutOfRangeException.ThrowIfNegative(rest, nameof(offset));

                        // None left
                        if (rest == 0)
                        {
                            // Check for retry
                            if (length < 1)
                                break;
                            if (DateTime.UtcNow >= endCollect)
                                break;

                            // Wait for new data
                            Thread.Sleep(100);

                            // Try again
                            continue;
                        }

                        // Correct
                        if (rest < length)
                            length = (int)rest;

                        // Create buffer
                        var buffer = new byte[length];

                        // Move to position
                        stream.Position = offset;

                        // Read data
                        if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
                            throw new IOException(path);

                        // When we started
                        var start = DateTime.UtcNow;

                        // Process all
                        for (var i = 0; i < buffer.Length;)
                        {
                            // Overall time
                            var delta = DateTime.UtcNow - start;

                            // Did something - at least
                            var run = delta.TotalSeconds;
                            if (run > 0)
                            {
                                // Check against our maximum
                                var rate = i * 8 / run;
                                if (rate >= 80000000)
                                {
                                    // Must delay
                                    Thread.Sleep(1);

                                    // Next try
                                    continue;
                                }
                            }

                            // Get the size
                            var n = Math.Min(buffer.Length - i, UDPStreaming.BufferSize);

                            // Send to endpoint
                            socket.SendTo(buffer, i, n, SocketFlags.None, endPoint);

                            // Adjust
                            i += n;
                        }

                        // Adjust counters
                        offset += buffer.Length;
                        length -= buffer.Length;

                        // Finished
                        if (length < 1)
                            break;
                        if (DateTime.UtcNow >= endCollect)
                            break;
                    }

                // Close
                socket.Close(10);

                // Report size
                return streamSize;
            }
        }
    }
}
