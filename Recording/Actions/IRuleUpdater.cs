namespace JMS.DVB.NET.Recording.Actions;

public interface IRuleUpdater
{  /// <summary>
   /// Aktualisiert die Regeln für die Aufzeichnungsplanung.
   /// </summary>
   /// <param name="newRules">Die ab nun zu verwendenden Regeln.</param>
   /// <returns>Meldet, ob ein Neustart erforderlich ist.</returns>
    bool? UpdateSchedulerRules(string newRules);
}
