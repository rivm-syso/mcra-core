using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class PointsOfDepartureModuleConfig {
        [XmlIgnore]
        public bool IncludeAopNetworks => !MultipleEffects && IncludeAopNetwork;
    }
}
