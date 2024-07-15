using System.Xml.Serialization;
using MCRA.General.Action.Settings;

namespace MCRA.General.ActionSettingsTemplates {
    public class SettingsTemplate {
        public string Id { get; set; }
        public SettingsTemplateType Tier { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        [XmlAttribute("deprecated")]
        public bool Deprecated { get; set; }
        [XmlArrayItem("ModuleConfiguration")]
        public ModuleConfiguration[] ModuleConfigurations {
            get => ConfigurationsDictionary.Values.ToArray();
            set => ConfigurationsDictionary = value.ToDictionary(v => v.ActionType);
        }

        [XmlIgnore]
        public Dictionary<ActionType, ModuleConfiguration> ConfigurationsDictionary { get; set; } = [];

        public override string ToString() => $"{Tier} ({Id}): {Name}";
    }
}
