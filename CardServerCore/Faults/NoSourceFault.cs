﻿namespace JMS.DVB.CardServer
{
    /// <summary>
    /// Meldet, dass eine Quelle nicht aktiv ist.
    /// </summary>
    [Serializable]
    public class NoSourceFault : CardServerFault
    {
        /// <summary>
        /// Die betroffene Quelle.
        /// </summary>
        public SourceIdentifier Source { get; set; } = null!;

        /// <summary>
        /// Wird für die XML Serialisierung benötigt.
        /// </summary>
        public NoSourceFault()
        {
        }

        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        /// <param name="source">Die unbekannte Quelle.</param>
        public NoSourceFault(SourceIdentifier source)
            : base(string.Format("Source '{0}' is not active", SourceIdentifier.ToString(source)))
        {
            // Remember
            Source = source;
        }
    }
}
