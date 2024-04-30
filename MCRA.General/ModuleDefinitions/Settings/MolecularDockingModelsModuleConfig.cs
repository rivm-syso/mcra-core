using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class MolecularDockingModelsModuleConfig {
        [XmlIgnore]
        public bool IncludeAopNetworks => !MultipleEffects && IncludeAopNetwork;
    }
}
