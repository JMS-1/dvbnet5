namespace JMS.DVB
{
    /// <summary>
    /// Übernimmt das dynamische Laden der DVB.NET Laufzeitbibliotheken.
    /// </summary>
    public static class RunTimeLoader
    {
        /// <summary>
        /// Get the global configuration folder.
        /// </summary>
        public static DirectoryInfo ConfigurationDirectory =>
            new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "jmsdvbnet"));

        /// <summary>
        /// Retrieve a globval configuration folder.
        /// </summary>
        /// <param name="scope">Name of the folder.</param>
        /// <returns>Reference to the folder.</returns>
        public static DirectoryInfo GetDirectory(string scope) =>
            new(Path.Combine(ConfigurationDirectory.FullName, scope));
    }
}
