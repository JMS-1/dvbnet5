namespace JMS.DVB.NET.Recording.Services.Configuration;

/// <summary>
/// Beschreibt eine einzelne Einstellung.
/// </summary>
public abstract class SettingDescription : ICloneable
{
    /// <summary>
    /// Meldet den Namen der Einstellung.
    /// </summary>
    public SettingNames Name { get; }

    /// <summary>
    /// Ein eventuell veränderter Wert.
    /// </summary>
    public string NewValue { get; set; } = null!;

    protected readonly VCRConfiguration VCRConfiguration;

    /// <summary>
    /// Erzeugt eine neue Beschreibung.
    /// </summary>
    /// <param name="name">Der Name der Beschreibung.</param>
    /// <param name="vcrConfiguration">Die zugehörige Konfiguration.</param>
    internal SettingDescription(SettingNames name, VCRConfiguration vcrConfiguration)
    {
        VCRConfiguration = vcrConfiguration;
        Name = name;
    }

    /// <summary>
    /// Liest den Namen der Einstellung.
    /// </summary>
    /// <returns>Der Wert der Einstellung als Zeichenkette.</returns>
    public virtual object ReadValue() => GetCurrentValue();

    /// <summary>
    /// Liest den Namen der Einstellung.
    /// </summary>
    /// <returns>Der Wert der Einstellung als Zeichenkette.</returns>
    public string GetCurrentValue() => ReadRawValue(VCRConfiguration.Configuration)!;

    /// <summary>
    /// Liest den Namen der Einstellung.
    /// </summary>
    /// <param name="configuration">Die zu verwendende Konfiguration.</param>
    /// <returns>Der Wert der Einstellung als Zeichenkette.</returns>
    private string ReadRawValue(System.Configuration.Configuration configuration) => configuration.AppSettings.Settings[Name.ToString()]?.Value?.Trim()!;

    /// <summary>
    /// Aktualisiert einen Konfigurationswert.
    /// </summary>
    /// <param name="newConfiguration"></param>
    /// <returns></returns>
    internal bool Update(System.Configuration.Configuration newConfiguration)
    {
        // Corret
        var newValue = string.IsNullOrEmpty(NewValue) ? string.Empty : NewValue.Trim();

        // Not changed
        if (Equals(newValue, ReadRawValue(newConfiguration)))
            return false;

        // Load the setting
        var setting = newConfiguration.AppSettings.Settings[Name.ToString()];

        // Ups, missing
        if (setting == null)
            newConfiguration.AppSettings.Settings.Add(Name.ToString(), newValue);
        else
            setting.Value = newValue;

        // Report
        return true;
    }

    #region ICloneable Members

    /// <summary>
    /// Erzeugt eine Kopie des Eintrags.
    /// </summary>
    /// <returns>Die gewünschte Kopie.</returns>
    protected abstract SettingDescription CreateClone();

    /// <summary>
    /// Erzeugt eine Kopie des Eintrags.
    /// </summary>
    /// <returns>Die gewünschte Kopie.</returns>
    public SettingDescription Clone() => CreateClone();

    /// <summary>
    /// Erzeugt eine Kopie des Eintrags.
    /// </summary>
    /// <returns>Die gewünschte Kopie.</returns>
    object ICloneable.Clone() => Clone();

    #endregion
}

