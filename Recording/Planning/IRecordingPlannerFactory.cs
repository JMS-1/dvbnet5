namespace JMS.DVB.NET.Recording.Planning;

public interface IRecordingPlannerFactory
{
    /// <summary>
    /// Erstellt eine neue Planung.
    /// </summary>
    /// <param name="site">Die zugehörige Arbeitsumgebung.</param>
    /// <returns>Die gewünschte Planungsumgebung.</returns>
    IRecordingPlanner Create(IRecordingPlannerSite site);
}

