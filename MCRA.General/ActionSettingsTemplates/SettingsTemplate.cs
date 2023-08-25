using System.Xml.Serialization;

namespace MCRA.General.ActionSettingsTemplates {
    public class SettingsTemplate {
        public ActionType ActionType { get; set; }
        public string Id { get; set; }
        public SettingsTemplateType Tier { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        [XmlAttribute("deprecated")]
        public bool Deprecated { get; set; }
        [XmlArrayItem("Setting")]
        public List<ModuleSetting> Settings { get; set; }

        public override string ToString() => $"{Tier} ({Id}): {Name}";
    }
}
