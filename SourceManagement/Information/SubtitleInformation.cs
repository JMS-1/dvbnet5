namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt eine DVB Untertitelspur.
    /// </summary>
    [Serializable]
    public class SubtitleInformation : ICloneable
    {
        /// <summary>
        /// Die Datenstromkennung (PID), in der die Untertitel übertragen werden.
        /// </summary>
        public ushort SubtitleStream { get; set; }

        /// <summary>
        /// Die Sprache, in der diese Untertitel vorliegen.
        /// </summary>
        public string Language { get; set; } = null!;

        /// <summary>
        /// Die Art der Untertitel.
        /// </summary>
        public SubtitleTypes SubtitleType { get; set; }

        /// <summary>
        /// Die primäre Seitennummer.
        /// </summary>
        public ushort CompositionPage { get; set; }

        /// <summary>
        /// Die sekundäre Seitennummer.
        /// </summary>
        public ushort AncillaryPage { get; set; }

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public SubtitleInformation()
        {
        }

        /// <summary>
        /// Erzeugt einen Anzeigetext für diese Untertitelspur.
        /// </summary>
        /// <returns>Ein Anzeigetext gemäß der aktuellen Konfiguration.</returns>
        public override string ToString() =>
            string.Format("{0} [{1}] {2}", Language, SubtitleStream, SubtitleType);

        #region ICloneable Members

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Information.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        public SubtitleInformation Clone() => new()
        {
            CompositionPage = CompositionPage,
            SubtitleStream = SubtitleStream,
            AncillaryPage = AncillaryPage,
            SubtitleType = SubtitleType,
            Language = Language,
        };

        /// <summary>
        /// Erzeugt eine exakte Kopie dieser Information.
        /// </summary>
        /// <returns>Die gewünschte Kopie.</returns>
        object ICloneable.Clone() => Clone();

        #endregion
    }
}
