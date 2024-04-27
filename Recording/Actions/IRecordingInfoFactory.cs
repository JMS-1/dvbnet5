using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Persistence;
using JMS.DVB.NET.Recording.Planning;

namespace JMS.DVB.NET.Recording.Actions;

public interface IRecordingInfoFactory
{
    /// <summary>
    /// Erstellt einen neuen Eintrag.
    /// </summary>
    /// <param name="planItem">Die zugehörige Beschreibung der geplanten Aktivität.</param>
    /// <param name="context">Die Abbildung auf die Aufträge.</param>
    /// <returns>Die angeforderte Repräsentation.</returns>
    VCRRecordingInfo? Create(IScheduleInformation planItem, PlanContext context);
}
