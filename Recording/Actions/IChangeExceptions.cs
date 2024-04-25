namespace JMS.DVB.NET.Recording.Actions;

public interface IChangeExceptions
{
    /// <summary>
    /// Verändert eine Ausnahme.
    /// </summary>
    /// <param name="jobIdentifier">Die eindeutige Kennung des Auftrags.</param>
    /// <param name="scheduleIdentifier">Die eindeutige Kennung der Aufzeichnung.</param>
    /// <param name="when">Der betroffene Tag.</param>
    /// <param name="startDelta">Die Verschiebung der Startzeit in Minuten.</param>
    /// <param name="durationDelta">Die Änderung der Aufzeichnungsdauer in Minuten.</param>
    void Update(Guid jobIdentifier, Guid scheduleIdentifier, DateTime when, int startDelta, int durationDelta);
}
