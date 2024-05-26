using System.Net;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;

namespace JMS.DVB.NET.Recording.FTPWrap;

/// <summary>
/// Beschreibt eine einzelne FTP Sitzung.
/// </summary>
public class FTPClient : IDisposable
{
	/// <summary>
	/// Signatur einer Rückrufmethode, die beim Beenden der Verbindung aktiviert wird.
	/// </summary>
	/// <param name="client">Die Clientverbindung, die beendet wurde.</param>
	public delegate void FinishedHandler(FTPClient client);

	/// <summary>
	/// Signatur einer Methode zur Ausführung eines FTP Befehls.
	/// </summary>
	/// <param name="command">Parameter des Befehls.</param>
	private delegate void Processor(string command);

	/// <summary>
	/// Alle behandelten FTP Befehle.
	/// </summary>
	private Dictionary<string, Processor> m_Processors = new Dictionary<string, Processor>();

	/// <summary>
	/// Sperre zur internen Synchronisation des Datenkanals.
	/// </summary>
	private object m_DataLock = new object();

	/// <summary>
	/// Speicher für den FTP Befehl.
	/// </summary>
	private byte[] m_Buffer = new byte[1000];

	/// <summary>
	/// Methode zur Benachrichtigung über das Beenden der Verbindung.
	/// </summary>
	private readonly FinishedHandler m_OnFinished;

	/// <summary>
	/// Auf dem Datenkanal auszuführender Befehl.
	/// </summary>
	private Processor? m_DelayedProcessor;

	/// <summary>
	/// Parameter zum Befehl, der über den Datenkanal auszuführen ist.
	/// </summary>
	private string? m_DelayedCommand;

	/// <summary>
	/// Aktueller Datenkanal.
	/// </summary>
	private DataChannel? m_Data;

	/// <summary>
	/// Steuerverbindung für den Datenkanal.
	/// </summary>
	private Socket? m_Passive;

	/// <summary>
	/// Verbindung zum FTP Client.
	/// </summary>
	private Socket? m_Socket;

	/// <summary>
	/// Die zugeordnete Datei.
	/// </summary>
	private FileInfo m_File;

	/// <summary>
	/// Gesetzt, wenn eine leere Datei erkannt wurde.
	/// </summary>
	private bool m_EmptyFile = false;

	/// <summary>
	/// Erzeugt eine neue Client Verbindung.
	/// </summary>
	/// <param name="socket">Netzwerkverbindung zum Client.</param>
	/// <param name="file">Die Datei, die diesem Client zugeordnet ist.</param>
	/// <param name="onFinished">Methode zum Rückrufe nach Beenden der Verbindung.</param>
	public FTPClient(Socket socket, FileInfo file, FinishedHandler onFinished)
	{
		// Remember
		m_OnFinished = onFinished;
		m_Socket = socket;
		m_File = file;

		// Asnychronous
		m_Socket.Blocking = false;

		// Start reply
		Send(220, "FTP Wrap");

		// Register
		m_Processors["ABOR"] = ProcessABOR;
		m_Processors["RETR"] = ProcessRETR;
		m_Processors["LIST"] = ProcessLIST;
		m_Processors["PASV"] = ProcessPASV;
		m_Processors["TYPE"] = ProcessTYPE;
		m_Processors["QUIT"] = ProcessQUIT;
		m_Processors["USER"] = ProcessUSER;
		m_Processors["PASS"] = ProcessPASS;
		m_Processors["SYST"] = ProcessSYST;
		m_Processors["CWD"] = ProcessCWD;
	}

	/// <summary>
	/// Aktivieren der Steuerverbindung für den Datenkanal.
	/// </summary>
	/// <param name="command">Wird ignoriert.</param>
	private void ProcessPASV(string command)
	{
		// Close the current passive socket
		ClosePassive();

		// Create a new passive socket
		m_Passive = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { Blocking = false };

		// Bind to some port
		m_Passive.Bind(new IPEndPoint(((IPEndPoint)m_Socket!.LocalEndPoint!).Address, 0));

		// Read the new endpoint
		var endPoint = (IPEndPoint)m_Passive.LocalEndPoint!;

		// As bytes
		byte[] net = endPoint.Address.GetAddressBytes();

		// Read the port
		int port = endPoint.Port;

		// Configure for listening
		m_Passive.Listen(4);

		// Start accepting
		m_Passive.BeginAccept(OnAcceptPassive, null);

		// Report
		Send(227, "Entering Passive Mode ({0},{1},{2},{3},{4},{5})", net[0], net[1], net[2], net[3], (port >> 8) & 0xff, port & 0xff);
	}

	/// <summary>
	/// Erzeugen des Datenkanals.
	/// </summary>
	/// <param name="result">Aktuelle Informationen zum zugehörigen Steuerkanal.</param>
	private void OnAcceptPassive(IAsyncResult result)
	{
		// We are terminating
		if (null == m_Passive) return;

		// What to do
		Processor? processor;

		// Synchronize
		lock (m_DataLock)
		{
			// Get the communication socket
			m_Data = new DataChannel(m_Passive.EndAccept(result), OnDataChannelFinished);

			// Load processor
			processor = m_DelayedProcessor;

			// Reset
			m_DelayedProcessor = null;
		}

		// Nothing to do
		processor?.Invoke(m_DelayedCommand!);
	}

	/// <summary>
	/// Wird vom Datenkanal <see cref="DataChannel"/> aufgerufen, sobald alle Daten übertragen wurden.
	/// </summary>
	/// <param name="data">Der abgeschlossene Datenkanal.</param>
	private void OnDataChannelFinished(DataChannel data)
	{
		// Report
		Send(226, "Transfer Complete");

		// Terminate on empty file
		if (!data.GotData)
		{
			// Remember
			m_EmptyFile = true;

			// Terminate
			Close();
		}
	}

	/// <summary>
	/// Fordert eine Dateiliste an.
	/// </summary>
	/// <param name="command">Wird ignoriert.</param>
	private void ProcessLIST(string command)
	{
		// Schedule for delayed processing
		lock (m_DataLock)
			if (null == m_Data)
			{
				// Wait for connection
				m_DelayedProcessor = ProcessLIST;
				m_DelayedCommand = command;

				// Try again later
				return;
			}

		// Report
		IAsyncResult wait = Send(125, "Data connection open, Transfer starting.");

		// Wait until client ist ready
		if (null != wait) wait.AsyncWaitHandle.WaitOne();

		// Create list
		string response = string.Format("---------- 1 owner group 999999999999 Sep 29 10:18 {0}\r\n", m_File.Name);

		// Send data
		try
		{
			// Process
			if (null != m_Data) m_Data.Send(response);
		}
		catch
		{
			// Ignore any error
		}
	}

	/// <summary>
	/// Beginnt mit der Übertragung einer Datei.
	/// </summary>
	/// <param name="command">Wird ignoriert.</param>
	private void ProcessRETR(string command)
	{
		// Schedule for delayed processing
		lock (m_DataLock)
			if (null == m_Data)
			{
				// Wait for connection
				m_DelayedProcessor = ProcessRETR;
				m_DelayedCommand = command;

				// Try again later
				return;
			}

		// Report
		IAsyncResult wait = Send(125, "Data connection open, Transfer starting.");

		// Wait until client ist ready
		if (null != wait) wait.AsyncWaitHandle.WaitOne();

		// Send data
		try
		{
			// Process
			m_Data?.Send(new FileStream(m_File.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1000000));
		}
		catch
		{
			// Ignore any error
		}
	}

	/// <summary>
	/// Beendet den aktuellen Datenkanal, üblicherweise die Übertragung einer Datei.
	/// </summary>
	/// <param name="command">Wird ignoriert.</param>
	private void ProcessABOR(string command)
	{
		// Process
		Send(226, "Closing data connection");

		// Terminate data channel
		CloseData();
	}

	/// <summary>
	/// Meldet das Betriebssystem.
	/// </summary>
	/// <param name="command">Wird ignoriert.</param>
	private void ProcessSYST(string command) => Send(215, "Windows_NT");

	/// <summary>
	/// Beendet diese Sitzung.
	/// </summary>
	/// <param name="command">Wird ignoriert.</param>
	private void ProcessQUIT(string command) => Send(221, "Good bye");

	/// <summary>
	/// Setzt die Art der Übertragung.
	/// </summary>
	/// <param name="command">Wird ignoriert.</param>
	private void ProcessTYPE(string command) => Send(200, "OK");

	/// <summary>
	/// Setzt den aktuellen Anwender.
	/// </summary>
	/// <param name="command">Wird ignoriert.</param>
	private void ProcessUSER(string command) => Send(331, "Require Password");

	/// <summary>
	/// Setzt das Arbeitsverzeichnis.
	/// </summary>
	/// <param name="command">Wird ignoriert.</param>
	private void ProcessCWD(string command) => Send(250, "OK");

	/// <summary>
	/// Setzt das Kennwort.
	/// </summary>
	/// <param name="command">Wird ignoriert.</param>
	private void ProcessPASS(string command) => Send(230, "OK");

	/// <summary>
	/// Meldet das Ergebnis einer Operation.
	/// </summary>
	/// <param name="code">FTP Ergebniscode.</param>
	/// <param name="format">Format für den Aufbau der Nachricht.</param>
	/// <param name="args">Parameter für den Aufbau der Nachricht.</param>
	/// <returns>Informationen zum gestarteten Netzwerkvorgang.</returns>
	private IAsyncResult Send(int code, string format, params object[] args) => Send(code, string.Format(format, args));

	/// <summary>
	/// Meldet das Ergebnis einer Operation.
	/// </summary>
	/// <param name="code">FTP Ergebniscode.</param>
	/// <param name="message">Die zu übermittelnde Nachricht.</param>
	/// <returns>Informationen zum gestarteten Netzwerkvorgang.</returns>
	private IAsyncResult Send(int code, string message) => Send(string.Format("{0:000} {1}", code, message));

	/// <summary>
	/// Meldet das Ergebnis einer Operation.
	/// </summary>
	/// <param name="message">Die zu übermittelnde Nachricht.</param>
	/// <returns>Informationen zum gestarteten Netzwerkvorgang.</returns>
	private IAsyncResult Send(string message) => Send(Encoding.ASCII.GetBytes(message + "\r\n"));

	/// <summary>
	/// Meldet das Ergebnis einer Operation.
	/// </summary>
	/// <param name="toSend">Die zu übermittelnden Daten.</param>
	/// <returns>Informationen zum gestarteten Netzwerkvorgang.</returns>
	private IAsyncResult Send(byte[] toSend)
	{
		// Be safe
		try
		{
			// Start
			return m_Socket!.BeginSend(toSend, 0, toSend.Length, SocketFlags.None, OnSendCompleteControl, null);
		}
		catch
		{
			// In error
			return null!;
		}
	}

	/// <summary>
	/// Das Übermitteln der Nachricht wurde abgeschlossen.
	/// </summary>
	/// <param name="result">Ergebnis zur gerade abgeschlossenen Operation.</param>
	private void OnSendCompleteControl(IAsyncResult result)
	{
		// Be safe
		try
		{
			// Finish
			m_Socket!.EndSend(result);

			// Primary communication
			m_Socket.BeginReceive(m_Buffer, 0, m_Buffer.Length, SocketFlags.None, OnReceive, null);
		}
		catch
		{
			// All done
			Close();
		}
	}

	/// <summary>
	/// Es wurde ein FTP Befehl empfangen.
	/// </summary>
	/// <param name="result">Informationen zu den Daten des Befehls.</param>
	private void OnReceive(IAsyncResult result)
	{
		// Already done
		if (m_Socket == null) return;

		// The command
		string command;

		// Be safe
		try
		{
			// Get data
			int bytes = m_Socket.EndReceive(result);

			// Finished
			if (bytes < 1)
			{
				// Terminate
				Close();

				// Done
				return;
			}

			// Get the command
			command = Encoding.Default.GetString(m_Buffer, 0, bytes);
		}
		catch
		{
			// Terminate
			Close();

			// Done
			return;
		}

		// Report
		Debug.Write(string.Format("{1}:{2} {0}", command, Thread.CurrentThread.ManagedThreadId, DateTime.Now.Ticks));

		// Dispatch
		string key = command.Split(' ', '\r', '\n')[0];

		// Process
		if (m_Processors.TryGetValue(key, out var processor)) processor(command[4..^2].Trim());
	}

	/// <summary>
	/// Schließt den Datenkanal.
	/// </summary>
	private void CloseData()
	{
		// Reset
		m_DelayedProcessor = null;

		// Start cleanup data channel
		lock (m_DataLock)
			using (var cleanup = m_Data)
				m_Data = null;
	}

	/// <summary>
	/// Beendet den Datenkanal.
	/// </summary>
	public void Abort()
	{
		// Synchronized
		lock (m_DataLock)
			m_Data?.Abort();
	}

	/// <summary>
	/// Schließt die Steuerung des Datenkanals und den Datenkanal.
	/// </summary>
	private void ClosePassive()
	{
		// Start cleanup data channel
		CloseData();

		// Start cleanup
		Close(ref m_Passive, false);
	}

	/// <summary>
	/// Scließt einen <see cref="Socket"/>.
	/// </summary>
	/// <param name="socket">Die Netzwerkverbindung.</param>
	/// <param name="shutdown">Gesetzt, wenn ein <see cref="Socket.Shutdown"/> ausgeführt werden soll (nicht
	/// für Steuerkanäle).</param>
	/// <returns>Gesetzt, wenn die Netzwerkverbindung tatsächlich geschlossen wurde.</returns>
	private bool Close(ref Socket? socket, bool shutdown)
	{
		// Start cleanup
		using (var cleanup = socket)
			if (cleanup != null)
			{
				// Forget
				socket = null;

				// Close
				if (shutdown) cleanup.Shutdown(SocketShutdown.Both);
				cleanup.Close();

				// Did it
				return true;
			}

		// Did nothing
		return false;
	}

	/// <summary>
	/// Schließt die Verbindung zum Client.
	/// </summary>
	public void Close()
	{
		// Close passive socket
		ClosePassive();

		// Start cleanup
		if (Close(ref m_Socket, true))
			m_OnFinished?.Invoke(this);
	}

	/// <summary>
	/// Gesetzt, wenn eine leere Datei erkannt wurde.
	/// </summary>
	public bool EmptyFile => m_EmptyFile;

	#region IDisposable Members

	/// <summary>
	/// Beendet die Nutzung dieser FTP Verbindung endgültig.
	/// </summary>
	public void Dispose()
	{
		// Forward
		Close();
	}

	#endregion
}
