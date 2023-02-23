using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions {
    public sealed class ModuleTier {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IdTierSelectionSetting { get; set; }

        [XmlArrayItem("TierSetting")]
        public List<ModuleTierSetting> TierSettings { get; set; }

        [XmlArrayItem("InputTier")]
        public List<ModuleTierInputTier> InputTiers { get; set; }
    }
}
