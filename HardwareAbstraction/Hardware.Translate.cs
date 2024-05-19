using System.Reflection;

namespace JMS.DVB
{
    partial class Hardware
    {
        /// <summary>
        /// Der Name der Bibliothek, in der die Abstraktionsimplementierungen vor DVB.NET 4.0 eingebettet
        /// sind.
        /// </summary>
        private const string LegacyAssemblyName = "JMS.DVB.Provider.Legacy";

        /// <summary>
        /// Beschreibt die Abbildung der Abstraktionsklassen vor DVB.NET 4.0 auf die aktuelle Implementierung.
        /// </summary>
        private static readonly Dictionary<string, Type> s_LegacyMapping = [];

        /// <summary>
        /// Methoden zur Umsetzung alter Konfigurationen in die aktuellen.
        /// </summary>
        private static readonly Dictionary<string, Func<Profile, Type, Hardware>> s_Translators = [];

        /// <summary>
        /// Die Parameter des Geräteprofils zum Zeitpunkt der Erzeugung der Abstraktion.
        /// </summary>
        internal List<ProfileParameter> EffectiveProfileParameters { get; private set; } = null!;

        /// <summary>
        /// Wandelt die Parameter von der alten Darstellung vor DVB.NET 4.0 in die aktuelle Darstellung um.
        /// </summary>
        /// <returns>Gesetzt, wenn die Umwandlung erfolgreich war.</returns>
        private bool Translate()
        {
            // Translate parameters
            for (int i = EffectiveProfileParameters.Count; i-- > 0;)
            {
                // Attach to parameter
                var setting = EffectiveProfileParameters[i];
                if (setting == null)
                    continue;
                if (string.IsNullOrEmpty(setting.Name))
                    continue;

                // Dispatch
                switch (setting.Name)
                {
                    case "Type": break;
                    default: continue;
                }

                // Eaten up
                EffectiveProfileParameters.RemoveAt(i);
            }

            // Did it
            return true;
        }

        /// <summary>
        /// Führt eine Standardumwandlung für den Satellitenempfang mit DiSEqC aus.
        /// </summary>
        /// <param name="profile">Das zu verwendende Geräteprofil.</param>
        /// <param name="hardwareType">Die konkrete Art der DVB.NET 4.0ff Implementierung.</param>
        /// <returns>Die gewünschte Abstraktion.</returns>
        private static Hardware? StandardSatelliteTranslationWithDiSEqC(Profile profile, Type hardwareType) => StandardTranslation(profile, hardwareType);

        /// <summary>
        /// Führt eine Standardumwandlung aus.
        /// </summary>
        /// <param name="profile">Das zu verwendende Geräteprofil.</param>
        /// <param name="hardwareType">Die konkrete Art der DVB.NET 4.0ff Implementierung.</param>
        /// <returns>Die gewünschte Abstraktion.</returns>
        private static Hardware? StandardTranslation(Profile profile, Type hardwareType)
        {
            // Result
            Hardware? result = null;

            // Create the hardware
            var hardware = (Hardware)Activator.CreateInstance(hardwareType, profile)!;
            try
            {
                // Report
                if (hardware.Translate())
                {
                    // Reload restrictions - can only use full featured algorithm when fully ported
                    hardware.Restrictions = new HardwareRestriction { ProvidesSignalInformation = true };

                    // Remember
                    result = hardware;
                }
            }
            catch
            {
            }

            // Cleanup
            if (result == null)
                hardware.Dispose();

            // Failed
            return result;
        }

        /// <summary>
        /// Erzeugt eine neue Instanz der Abstraktion zu einem Geräteprofil.
        /// </summary>
        /// <param name="profile">Das zu verwendende Geräteprofil.</param>
        /// <param name="migrateOnly">Gesetzt, wenn nur eine Migration der Einstellungen vorgenommen werden soll.</param>
        /// <returns>Die neu erzeugte Instanz, die mit <see cref="IDisposable.Dispose"/>
        /// freigegeben werden muss.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Geräteprofil angegeben.</exception>
        /// <exception cref="NotSupportedException">Das Geräteprofil verwendet die nicht mehr unterstützten BDA Abstraktion
        /// vor DVB.NET 4.0.</exception>
        internal static Hardware? Create(Profile profile, bool migrateOnly)
        {
            // Validate
            ArgumentNullException.ThrowIfNull(profile);

            // For exception translation
            try
            {
                // Load the primary type
                var primaryType = Type.GetType(profile.HardwareType, true)!;

                // See we we can translate
                if (LegacyAssemblyName.Equals(primaryType.Assembly.GetName().Name))
                    if (s_LegacyMapping.TryGetValue(primaryType.FullName!, out var translationType))
                    {
                        // Load the provider mapping
                        var provider = profile.GetDeviceAspect(null!);
                        if (!string.IsNullOrEmpty(provider))
                        {
                            // Load the translatior
                            if (s_Translators.TryGetValue(provider, out var translator))
                            {
                                // Try translate
                                var hardware = translator(profile, translationType);
                                if (hardware == null)
                                    if (migrateOnly)
                                        return null;
                                    else
                                        throw new NotSupportedException(string.Format("The legacy BDA Driver is no longer supported - please upgrade the Profile '{0}'", profile));
                                else
                                    return hardware;
                            }
                        }
                    }

                // Create and report
                if (migrateOnly)
                    return null;
                else
                    return (Hardware)Activator.CreateInstance(primaryType, profile)!;
            }
            catch (TargetInvocationException e)
            {
                // Forward
                if (migrateOnly)
                    return null;
                else
                    throw e.InnerException ?? e;
            }
        }


        /// <summary>
        /// Ermittelt einen Parameter des Geräteprofils.
        /// </summary>
        /// <param name="name">Der Name des Parameters.</param>
        /// <returns>Die gewünschten Informationen.</returns>
        /// <exception cref="ArgumentNullException">Es wurde kein Name angegeben.</exception>
        protected string GetParameter(string name)
        {
            // Validate
            ArgumentException.ThrowIfNullOrEmpty(name);

            return EffectiveProfileParameters.GetParameter(name);
        }
    }
}
