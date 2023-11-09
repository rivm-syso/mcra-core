using System.Xml.Serialization;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.ActionSettingsTemplates {

    public class ModuleSetting {

        [XmlAttribute("id")]
        public SettingsItemType Id { get; set; }

        [XmlText]
        public string Value { get; set; }

        [XmlAttribute("readonly")]
        public bool ReadOnly { get; set; } = true;

        public override string ToString() => $"{Id} = {Value}";

    }
}
