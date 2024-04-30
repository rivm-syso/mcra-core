using System.Xml.Serialization;

namespace MCRA.General.SettingsDefinitions {

    [XmlRoot("SettingsItems")]
    [XmlType(TypeName = "SettingsItem")]
    public class SettingsItem {
        public SettingsItemType Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAdvanced { get; set; }
        public string ValueType { get; set; }
        public bool IsList { get; set; } = false;
        public bool IsInteger { get; set; } = false;
        public string SystemType { get; set; } = null;
        public string DefaultValue { get; set; } = null;
    }
}
