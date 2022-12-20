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
    }
}
