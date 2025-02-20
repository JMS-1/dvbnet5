﻿using System.Runtime.Serialization;

namespace JMS.DVB
{
    /// <summary>
    /// Diese Ausnahme zeigt an, dass keine weiteren Verbraucher für Teildatenströme (PID) mehr
    /// angemeldet werden konnten.
    /// </summary>
    [Serializable]
    public class OutOfConsumersException : Exception
    {
        /// <summary>
        /// Optional die Auswahl der Teildatenströme (PID), die angefordert wurden.
        /// </summary>
        public StreamSelection RequestedSelection { get; set; } = null!;

        /// <summary>
        /// Die Anzahl der angeforderten Verbraucher.
        /// </summary>
        public int Requested { get; set; }

        /// <summary>
        /// Die Anzahl der noch zur Verfügung stehenden Verbraucher.
        /// </summary>
        public int Available { get; set; }

        /// <summary>
        /// Wird für die Deserialisierung benötigt.
        /// </summary>
        /// <param name="info">Informationen zur Deserialisierung.</param>
        /// <param name="context">Aktuelle Arbeitsumgebung der Deserialisierung.</param>
        public OutOfConsumersException(SerializationInfo info, StreamingContext context)
#pragma warning disable SYSLIB0051 // Type or member is obsolete
            : base(info, context)
#pragma warning restore SYSLIB0051 // Type or member is obsolete
        {
        }

        /// <summary>
        /// Erzeugt eine neue Ausnahme.
        /// </summary>
        /// <param name="requested">Die Anzahl der angeforderten Verbraucher.</param>
        /// <param name="available">Die Anzahl der noch bereitstehenden Verbraucher.</param>
        public OutOfConsumersException(int requested, int available)
            : base(string.Format("Consumer limit reached: {0} requested, got only {1}", requested, available))
        {
            // Remember
            Requested = requested;
            Available = available;
        }
    }
}
