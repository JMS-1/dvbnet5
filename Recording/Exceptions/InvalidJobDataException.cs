namespace JMS.DVB.NET.Recording
{
    /// <summary>
    /// Used to report configuration errors in <see cref="VCRJob"/>
    /// and <see cref="VCRSchedule"/>.
    /// </summary>
    [Serializable]
    public class InvalidJobDataException : Exception
    {
        /// <summary>
        /// Create a new instance of this exception.
        /// </summary>
        /// <param name="reason">Describes the reason for this exception.</param>
        public InvalidJobDataException(string reason)
            : base(reason)
        {
        }
    }
}
