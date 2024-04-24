using JMS.DVB.NET.Recording.Services.Configuration;

namespace JMS.DVB.NET.Recording.Actions;

public interface IConfigurationUpdater
{
    /// <summary>
    /// Führt eine Aktualisierung von Konfigurationswerten durch.
    /// </summary>
    /// <param name="settings">Die zu aktualisierenden Konfigurationswerte.</param>
    /// <param name="forceRestart">Erzwingt einen Neustart des Dienstes.</param>
    /// <returns>Gesetzt, wenn ein Neustart erforderlich ist.</returns>
    bool? UpdateConfiguration(IEnumerable<SettingDescription> settings, bool forceRestart = false);
}
