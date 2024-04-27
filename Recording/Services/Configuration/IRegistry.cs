namespace JMS.DVB.NET.Recording.Services.Configuration;

public interface IRegistry
{
    /// <summary>
    /// Aktualisiert einen Zeitwert in der Windows Registrierung.
    /// </summary>
    /// <param name="name">Der Name des Wertes.</param>
    /// <param name="value">Der neue Wert oder <i>null</i>, wenn dieser entfernt
    /// werden soll.</param>
    void SetTime(string name, DateTime? value);

    /// <summary>
    /// Ermittelt einen Zeitwert aus der Windows Registrierung.
    /// </summary>
    /// <param name="name">Der Name des Wertes.</param>
    /// <returns>Der aktuelle Wert oder <i>null</i>, wenn dieser nicht existiert oder
    /// ungültig ist.</returns>
    DateTime? GetTime(string name);
}