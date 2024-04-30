using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class RelativePotencyFactorsModuleConfig {
        [XmlIgnore]
        public bool IncludeAopNetworks => !MultipleEffects && IncludeAopNetwork;
    }
}
