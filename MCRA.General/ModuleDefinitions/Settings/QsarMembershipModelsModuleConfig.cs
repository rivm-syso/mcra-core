using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class QsarMembershipModelsModuleConfig {
        [XmlIgnore]
        public bool IncludeAopNetworks => !MultipleEffects && IncludeAopNetwork;
    }
}
