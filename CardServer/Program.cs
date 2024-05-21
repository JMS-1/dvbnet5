using System.IO.Pipes;
using System.Diagnostics;
using System.ComponentModel;

namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Mit dieser Klasse werden <i>Card Server</i> Instanzen gestartet.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Der Einsprungpunkt für einen <i>Card Server</i>. Die Befehlszeilenparameter beschreiben
        /// die Kommunikationskanäle zum steuernden Client.
        /// </summary>
        /// <param name="args">Befehlsparameter für die Kommunikation.</param>
        public static void Main(string[] args)
        {
            // Be safe
            try
            {
                // Always use the configured language
                UserProfile.ApplyLanguage();

                // Set priority
                try
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
                }
                catch (Win32Exception)
                {
                    Console.Error.WriteLine("Card server must be started as root to set priority");
                }

                // Open the communication channels and attach to the pipe server
                using var reader = new AnonymousPipeClientStream(PipeDirection.In, args[0]);
                using var writer = new AnonymousPipeClientStream(PipeDirection.Out, args[1]);
                using var server = ServerImplementation.CreateInMemory();

                for (Request? request; (request = Request.ReceiveRequest(reader)) != null;)
                {
                    // Process it
                    var response = request.Execute(server);

                    // Send the response
                    response.SendResponse(writer);
                }

                // Allow outgoing data to be processed - esp. program guide.
                Thread.Sleep(2000);
            }
            catch (Exception e)
            {
                // Report error
                Console.Error.WriteLine(e);
            }
        }
    }
}
