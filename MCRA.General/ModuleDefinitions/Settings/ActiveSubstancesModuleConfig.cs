using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class ActiveSubstancesModuleConfig {
        [XmlIgnore]
        public bool IncludeAopNetworks => !MultipleEffects && IncludeAopNetwork;
    }
}
