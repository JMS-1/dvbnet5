using System.Net;
using System.Net.Sockets;
using JMS.DVB.NET.Recording.Services.Planning;

namespace JMS.DVB.NET.Recording.FTPWrap;

public class FTPWrap : IFTPWrap, IDisposable
{
    /// <summary>
    /// Alle aktiven und vergangenen FTP Verbindungen.
    /// </summary>
    private readonly List<FTPClient> m_Clients = [];

    /// <summary>
    /// Der FTP Server <see cref="Socket"/>.
    /// </summary>
    private Socket m_Socket;

    /// <summary>
    /// Verwaltung aller Aufträge.
    /// </summary>
    private readonly IJobManager m_Jobs;

    public FTPWrap(ushort port, IJobManager jobs)
    {
        m_Jobs = jobs;

        // Create socket
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { Blocking = false };

        // Make us an FTP server
        m_Socket.Bind(new IPEndPoint(IPAddress.Any, port));

        // Start listening
        m_Socket.Listen(5);

        // Await first connection
        m_Socket.BeginAccept(OnAccept, null);
    }

    /// <summary>
    /// Beginnt eine neue Sitzung mit einem FTP Client.
    /// </summary>
    /// <param name="result">Informationen zur neuen Sitzung.</param>
    private void OnAccept(IAsyncResult result)
    {
        // Already done
        if (m_Socket == null) return;

        // Be safe
        try
        {
            // Add a new client
            lock (m_Clients)
                m_Clients.Add(new FTPClient(m_Socket.EndAccept(result), OnClientFinished, m_Jobs));

            // Await next connection
            m_Socket.BeginAccept(OnAccept, null);
        }
        catch
        {
        }
    }

    /// <summary>
    /// Wird aufgerufen, wenn ein FTP Client die Sitzung beendet.
    /// </summary>
    /// <param name="client">Die zugehörige Sitzung, die beendet wurde.</param>
    private void OnClientFinished(FTPClient client)
    {
        // Terminate
        lock (m_Clients)
            m_Clients.Remove(client);
    }

    public void Dispose()
    {
        // Cleanup LISTENING port
        using (var cleanup = m_Socket)
            if (cleanup != null)
            {
                // Forget
                m_Socket = null!;

                // Close it
                cleanup.Close();
            }

        // Preempty all client activities
        lock (m_Clients)
        {
            foreach (var client in m_Clients) client.Abort();

            // Delay a bit
            Thread.Sleep(500);

            // Finish all client activities
            foreach (var client in m_Clients) client.Dispose();

            // Reset client list
            m_Clients.Clear();
        }
    }
}
