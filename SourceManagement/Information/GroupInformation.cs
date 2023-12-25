namespace JMS.DVB
{
    /// <summary>
    /// Beschreibt alle Quellen in einer Gruppe <see cref="SourceGroup"/> von Quellen, i.e. 
    /// im Allgemeinen die Sender auf einem Transponder.
    /// </summary>
    [Serializable]
    public class GroupInformation
    {
        /// <summary>
        /// Alle in dieser Gruppe verfügbaren Quellen, im Allgemeinen jeweils <see cref="Station"/>
        /// Instanzen.
        /// </summary>
        public readonly List<SourceIdentifier> Sources = [];

        /// <summary>
        /// Erzeugt eine neue Beschreibung.
        /// </summary>
        public GroupInformation()
        {
        }
    }
}
