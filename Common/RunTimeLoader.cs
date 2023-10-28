using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;


namespace JMS.DVB
{
    /// <summary>
    /// Übernimmt das dynamische Laden der DVB.NET Laufzeitbibliotheken.
    /// </summary>
    public class RunTimeLoader
    {
        /// <summary>
        /// Stellt die einzige Instanz zur Verfügung.
        /// </summary>
        private static readonly RunTimeLoader m_Instance = new RunTimeLoader();

        /// <summary>
        /// Alle bereits dynamisch geladenen Bibliotheken.
        /// </summary>
        private Dictionary<string, Assembly?> m_Loaded = new();

        /// <summary>
        /// Gesetzt, wenn keine DVB.NET Installation vorhanden ist.
        /// </summary>
        public static bool InStandAloneMode { get; private set; }

        /// <summary>
        /// Der volle Pfad zur aktuellen Anwendung.
        /// </summary>
        private static volatile FileInfo? m_ApplicationPath;

        /// <summary>
        /// Prüft, ob die Laufzeitumgebung korrekt aufgesetzt ist.
        /// </summary>
        public static void Startup()
        {
            // Test
            if (m_Instance == null)
                throw new InvalidOperationException("Startup");
        }

        /// <summary>
        /// Meldet das DVB.NET Installationsverzeichnis.
        /// </summary>
        public static DirectoryInfo RootDirectory => GetInstallationDirectory("Root");

        /// <summary>
        /// Meldet das Installationsverzeichnis des <i>Card Servers</i>.
        /// </summary>
        public static DirectoryInfo ServerDirectory => GetInstallationDirectory("Server");

        /// <summary>
        /// Meldet den Pfad zur Anwendung.
        /// </summary>
        public static FileInfo ExecutablePath
        {
            get
            {
                // See if we already did it
                if (m_ApplicationPath == null)
                    m_ApplicationPath = CurrentApplication.Executable;

                // Report
                return m_ApplicationPath;
            }
        }

        /// <summary>
        /// Meldet ein Installationsverzeichnis.
        /// </summary>
        /// <param name="scope">Der Name der zugehörigen Installation.</param>
        /// <returns>Das gewünschte Verzeichnis, das über die Windows Registrierung ermittelt wird.</returns>
        /// <exception cref="InvalidOperationException">Es existiert keine entsprechende Installation.</exception>
        private static DirectoryInfo GetInstallationDirectory(string scope) => new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!;

        /// <summary>
        /// Ermittelt ein Unterverzeichnis des Installationsverzeichnisses.
        /// </summary>
        /// <param name="relative">Der Name des Unterverzeichnisses.</param>
        /// <returns>Der gewünschte volle Pfad.</returns>
        public static DirectoryInfo GetDirectory(string relative) => RootDirectory;


        /// <summary>
        /// Meldet das Verzeichnis, an dem die Laufzeitbibiotheken erwartet werden.
        /// </summary>
        public static DirectoryInfo RunTimePath => GetDirectory("RunTime");

        /// <summary>
        /// Meldet das Verzeichnis, an dem die spezifischen Geräteimplementierungen erwartet werden.
        /// </summary>
        public static DirectoryInfo AdapterPath => GetDirectory("Adapter");

    }
}
