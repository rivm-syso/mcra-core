using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.ActionSettingsTemplates {

    public class ModuleSetting {

        [XmlAttribute("id")]
        public SettingsItemType Id { get; set; }

        [XmlText]
        [DefaultValue(null)]
        public string Value { get; set; } = null;

        [XmlAnyElement]
        [DefaultValue(null)]
        public XmlElement[] XmlValues { get; set; } = null;

        [XmlAttribute("readonly")]
        [DefaultValue(true)]
        public bool ReadOnly { get; set; } = true;

        public override string ToString() => $"{Id} = {Value}";
    }
}
