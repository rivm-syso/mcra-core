using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class EffectRepresentationsModuleConfig {
        [XmlIgnore]
        public bool IncludeAopNetworks => !MultipleEffects && IncludeAopNetwork;
    }
}
