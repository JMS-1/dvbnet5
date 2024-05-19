using System.Collections;
using System.Xml.Serialization;

namespace JMS.DVB
{
    /// <summary>
    /// Mit einer Instanz dieser Klasse wird der Ursprung einer Gruppe <see cref="SourceGroup"/>
    /// von Quellen beschrieben. Für den Satellitenempfang ist ein Ursprung etwa eine <i>Digital
    /// Satellite Equipment Control (DiSEqC)</i> Einstellung.
    /// </summary>
    [Serializable]
    [XmlType("Location")]
    public abstract class GroupLocation
    {
        /// <summary>
        /// Initialisiert eine Basisklasse.
        /// </summary>
        internal GroupLocation()
        {
        }

        /// <summary>
        /// Wandelt eine Textdarstellung eines Ursprungs in eine Instanz um.
        /// </summary>
        /// <returns>Die rekonstruierte Instanz.</returns>
        /// <exception cref="FormatException">Es wurde keine gültige Textdarstellung angegeben.</exception>
        public static T? FromString<T>(string text) where T : GroupLocation
        {
            // None
            if (string.IsNullOrEmpty(text))
                return null;

            // Only supported for DVB-S
            if (SatelliteLocation.Parse(text) is not T group)
                throw new FormatException(text);

            // Report
            return group;
        }

        /// <summary>
        /// Meldet alle Quellgruppen zu diesem Ursprung.
        /// </summary>
        [XmlIgnore]
        public abstract IList Groups { get; }

        /// <summary>
        /// Erzeugt eine Beschreibung für den Sendersuchlauf.
        /// </summary>
        /// <returns>Die entsprechende Beschreibung.</returns>
        public abstract ScanLocation ToScanLocation();

        /// <summary>
        /// Erzeugt eine Kopie dieses Ursprungs, allerdings ohne die enthaltenen Quellgruppen.
        /// </summary>
        /// <returns>Die Kopie des Ursprungs selbst.</returns>
        public GroupLocation Clone() => CreateClone();

        /// <summary>
        /// Erzeugt eine Kopie dieses Ursprungs, allerdings ohne die enthaltenen Quellgruppen.
        /// </summary>
        /// <returns>Die Kopie des Ursprungs selbst.</returns>
        protected abstract GroupLocation CreateClone();
    }

    /// <summary>
    /// Mit einer Instanz dieser Klasse wird der Ursprung einer Gruppe <see cref="SourceGroup"/>
    /// von Quellen beschrieben. Für den Satellitenempfang ist ein Ursprung etwa eine <i>Digital
    /// Satellite Equipment Control (DiSEqC)</i> Einstellung.
    /// </summary>
    [Serializable]
    public class GroupLocation<T> : GroupLocation where T : SourceGroup, new()
    {
        /// <summary>
        /// Alle Gruppen von Quellen, die über diesen Ursprung empfangen werden können.
        /// </summary>
        public readonly List<T> SourceGroups = [];

        /// <summary>
        /// Initialisiert eine Basisklasse.
        /// </summary>
        public GroupLocation()
        {
        }

        /// <summary>
        /// Erzeugt einen eindeutigen Schlüssel zu einem Ursprung.
        /// </summary>
        /// <returns>Die Basisklasse meldet immer <i>0</i>.</returns>
        public override int GetHashCode() => 0;

        /// <summary>
        /// Vergleicht zwei Instanzen.
        /// </summary>
        /// <param name="obj">Die andere Instanz.</param>
        /// <returns>Von der Basisklasse immer gesetzt, wenn 
        /// es sich um Instanzen gleichen Typs handelt.</returns>
        public override bool Equals(object? obj) => obj is GroupLocation<T>;

        /// <summary>
        /// Erzeugt einen Anzeigenamen.
        /// </summary>
        /// <returns>Der Anzeigename der Basisklasse ist immer <see cref="String.Empty"/>.</returns>
        public override string ToString() => string.Empty;

        /// <summary>
        /// Meldet alle Quellgruppen zu diesem Ursprung.
        /// </summary>
        [XmlIgnore]
        public override IList Groups => SourceGroups;

        /// <summary>
        /// Erzeugt eine Beschreibung für den Sendersuchlauf.
        /// </summary>
        /// <returns>Die entsprechende Beschreibung.</returns>
        /// <exception cref="ArgumentException">Quellgruppen dieser Art werden nicht unterstützt.</exception>
        public override ScanLocation ToScanLocation()
        {
            // Depends on type
            if (typeof(CableGroup) == typeof(T))
                return new CableScanLocation();
            else if (typeof(TerrestrialGroup) == typeof(T))
                return new TerrestrialScanLocation();
            else if (typeof(SatelliteGroup) == typeof(T))
                return new SatelliteScanLocation();

            throw new ArgumentException(typeof(T).FullName, nameof(T));
        }

        /// <summary>
        /// Erzeugt eine Kopie dieses Ursprungs, allerdings ohne die enthaltenen Quellgruppen.
        /// </summary>
        /// <returns>Die Kopie des Ursprungs selbst.</returns>
        public new GroupLocation<T> Clone() => (GroupLocation<T>)CreateClone();

        /// <summary>
        /// Erzeugt eine Kopie dieses Ursprungs, allerdings ohne die enthaltenen Quellgruppen.
        /// </summary>
        /// <returns>Die Kopie des Ursprungs selbst.</returns>
        protected override GroupLocation CreateClone()
        {
            // First try the standard way
            GroupLocation? clone = FromString<GroupLocation>(ToString());
            if (null != clone)
                return clone;

            // Create empty
            return (GroupLocation)Activator.CreateInstance(GetType(), null)!;
        }
    }

    /// <summary>
    /// Beschreibt einen Dummy-Ursprung für den Kabelempfang.
    /// </summary>
    [Serializable]
    [XmlType("Cable")]
    public class CableLocation : GroupLocation<CableGroup>
    {
        /// <summary>
        /// Erzeugt einen neuen Ursprung.
        /// </summary>
        public CableLocation()
        {
        }

        /// <summary>
        /// Erzeugt einen eindeutigen Schlüssel zu einem Ursprung.
        /// </summary>
        /// <returns>Die Basisklasse meldet immer <i>0</i>.</returns>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Vergleicht zwei Instanzen.
        /// </summary>
        /// <param name="obj">Die andere Instanz.</param>
        /// <returns>Von der Basisklasse immer gesetzt, wenn 
        /// es sich um Instanzen gleichen Typs handelt.</returns>
        public override bool Equals(object? obj) => (obj == null) || base.Equals(obj);
    }

    /// <summary>
    /// Beschreibt einen Dummy-Ursprung für den terrestrischen Empfang.
    /// </summary>
    [Serializable]
    [XmlType("Terrestrial")]
    public class TerrestrialLocation : GroupLocation<TerrestrialGroup>
    {
        /// <summary>
        /// Erzeugt einen neuen Ursprung.
        /// </summary>
        public TerrestrialLocation()
        {
        }

        /// <summary>
        /// Erzeugt einen eindeutigen Schlüssel zu einem Ursprung.
        /// </summary>
        /// <returns>Die Basisklasse meldet immer <i>0</i>.</returns>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Vergleicht zwei Instanzen.
        /// </summary>
        /// <param name="obj">Die andere Instanz.</param>
        /// <returns>Von der Basisklasse immer gesetzt, wenn 
        /// es sich um Instanzen gleichen Typs handelt.</returns>
        public override bool Equals(object? obj) => (obj == null) || base.Equals(obj);
    }
}
