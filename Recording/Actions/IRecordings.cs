using JMS.DVB.Algorithms.Scheduler;
using JMS.DVB.NET.Recording.Planning;
using JMS.DVB.NET.Recording.Services.Configuration;
using JMS.DVB.NET.Recording.Services.Planning;
using JMS.DVB.NET.Recording.Status;

namespace JMS.DVB.NET.Recording.Actions;

public interface IRecordings
{
    /// <summary>
    /// Ermittelt eine Übersicht über die aktuellen und anstehenden Aufzeichnungen
    /// aller Geräteprofile.
    /// </summary>
    /// <typeparam name="TInfo">Die Art der Informationen.</typeparam>
    /// <param name="fromActive">Erstellt eine Liste von Beschreibungen zu einer aktuellen Aufzeichnung.</param>
    /// <param name="fromPlan">Erstellt eine einzelne Beschreibung zu einer Aufzeichnung aus dem Aufzeichnungsplan.</param>
    /// <param name="forIdle">Erstellt eine Beschreibung für ein Gerät, für das keine Aufzeichnungen geplant sind.</param>
    /// <returns>Die Liste aller Informationen.</returns>
    TInfo[] GetCurrent<TInfo>(
       Func<FullInfo, IVCRServer, IVCRProfiles, IJobManager, TInfo[]> fromActive,
       Func<IScheduleInformation, PlanContext, IVCRServer, IVCRProfiles, TInfo> fromPlan = null!,
       Func<string, TInfo> forIdle = null!
   );
}
